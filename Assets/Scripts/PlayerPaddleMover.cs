using DigitalRubyShared;
using System;
using System.Linq;
using UnityEngine;

// ReSharper disable SwitchStatementMissingSomeCases
#pragma warning disable 108,114 

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerPaddleMover : MonoBehaviour
{  
    [Tooltip("How far to the left/right can the paddle move?")]
    public float HorizontalPositionThreshold = 4;

    [Tooltip("Horizontal force to act upon the paddle (in Joystick or LeftRight mode).")]
    public int SpeedForce = 500;

    [Tooltip("Minimum distance between player's touch and joystick center required for the joystick to work.")]
    public float JoystickThreshold = 0.05f;

    public ControlMode ControlMode;
    public SpriteRenderer LeftControlZone;
    public SpriteRenderer RightControlZone;
    public DragSlider DragSlider;

    private new Transform transform;
    private Rigidbody2D rigidbody2D;
    private ConstantForce2D constantForce2D;

    private Vector2? paddleReferencePosition;
    private Vector2? joystickReferencePosition;
    private Vector2 gestureFocusPosition = Vector2.zero;
    private Vector2 helperVector2 = Vector2.zero;

    private LongPressGestureRecognizer longPressGesture;
    private GestureTouch? activeTouchState;


    private void Start()
    {
        transform = GetComponent<Transform>();
        rigidbody2D = GetComponent<Rigidbody2D>();
        constantForce2D = GetComponent<ConstantForce2D>();

        SetupLongPressGesture();

        var isLeftRightControlled = ControlMode == ControlMode.LeftRight;
        LeftControlZone.gameObject.SetActive(isLeftRightControlled);
        RightControlZone.gameObject.SetActive(isLeftRightControlled);

        if (ControlMode == ControlMode.Drag)
        {
            DragSlider.PositionChanged.AddListener(OnDragSliderPositionChanged);
        }
    }

    private void OnDragSliderPositionChanged(float newPosition)
    {
        helperVector2.x = newPosition;
        helperVector2.y = transform.position.y;
        transform.position = helperVector2;
    }

    private void FixedUpdate()
    {
        transform.localPosition = Vector2.ClampMagnitude(transform.localPosition, HorizontalPositionThreshold);
    }

    private void SetupLongPressGesture()
    {
        longPressGesture = new LongPressGestureRecognizer
        {
            MaximumNumberOfTouchesToTrack = (ControlMode == ControlMode.LeftRight) ? 2 : 1,
            ThresholdUnits = Mathf.Infinity,
            MinimumDurationSeconds = 0f
        };
        longPressGesture.StateUpdated += LongPressGestureCallback;
        FingersScript.Instance.AddGesture(longPressGesture);
    }

    private void LongPressGestureCallback(GestureRecognizer gesture)
    {      
        if (ControlMode == ControlMode.LeftRight)
        {
            UpdateActiveTouchState(gesture);
        }

        UpdateFocusPosition(gesture);

        switch (gesture.State)
        {
            case GestureRecognizerState.Began:
            {
                //Debug.Log($"Long press began at: ({gestureFocusPosition})");
                BeginDrag(gestureFocusPosition);
                break;
            }

            case GestureRecognizerState.Executing:
            {
                //Debug.Log($"Long press moved to: {gestureFocusPosition}");
                DragTo(gestureFocusPosition);
                break;
            }

            case GestureRecognizerState.Ended:
            {
                //Debug.Log($"Long press ended at: ({gestureFocusPosition})");
                EndDrag(gestureFocusPosition);
                break;
            }
        }
    }

    private void UpdateActiveTouchState(GestureRecognizer gesture)
    {
        var currentTouches = gesture.CurrentTrackedTouches;

        if (activeTouchState.HasValue && currentTouches.Any(t => t.Id == activeTouchState.Value.Id))
        {
            activeTouchState = gesture.CurrentTrackedTouches.First(t => t.Id == activeTouchState.Value.Id);
        }

        if (gesture.ReceivedAdditionalTouches)
        {
            if (currentTouches.Count == 1)
            {
                activeTouchState = currentTouches.First();
            }
            else if (activeTouchState.HasValue)
            {
                var newTouchState = currentTouches.First(t => t.Id != activeTouchState.Value.Id);
                activeTouchState.Value.Invalidate();
                activeTouchState = newTouchState;
            }
            else
            {
                throw new InvalidOperationException("This should not happen!");
            }
        }
    }

    private void UpdateFocusPosition(GestureRecognizer gesture)
    {
        if (ControlMode == ControlMode.LeftRight && activeTouchState.HasValue)
        {
            gestureFocusPosition.x = activeTouchState.Value.X;
            gestureFocusPosition.y = activeTouchState.Value.Y;
        }
        else
        {
            gestureFocusPosition.x = gesture.FocusX;
            gestureFocusPosition.y = gesture.FocusY;
        }
    }

    private void BeginDrag(Vector2 screenPosition)
    {
        switch (ControlMode)
        {
            case ControlMode.JoystickDrag:
            {
                rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
                paddleReferencePosition = this.transform.localPosition;
                joystickReferencePosition = screenPosition.ToWorld();
                break;
            }

            case ControlMode.LeftRight:
            {
                rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
                paddleReferencePosition = null;
                joystickReferencePosition = null;

                MoveBasedOnSelectedControlZone(screenPosition);
                break;
            }
        }

    }

    private void DragTo(Vector2 screenPosition)
    {
        switch (ControlMode)
        {
            case ControlMode.JoystickDrag:
            {
                if (joystickReferencePosition.HasValue && paddleReferencePosition.HasValue)
                {
                    var offset = (screenPosition.ToWorld().x - joystickReferencePosition.Value.x);
                    var newLocalX = paddleReferencePosition.Value.x + offset;
                    helperVector2.x = Mathf.Clamp(newLocalX,
                                                  -HorizontalPositionThreshold,
                                                  HorizontalPositionThreshold);
                    transform.localPosition = helperVector2;
                }
                break;
            }

            case ControlMode.LeftRight:
            {
                MoveBasedOnSelectedControlZone(screenPosition);
                break;
            }
        }
    }

    private void EndDrag(Vector2 screenPosition)
    {
        paddleReferencePosition = null;
        joystickReferencePosition = null;
        constantForce2D.relativeForce = Vector2.zero;     
    }

    private void MoveBasedOnSelectedControlZone(Vector2 screenPosition)
    {
        var zone = GetSelectedControlZone(screenPosition.ToWorld());

        helperVector2.x =
            zone == ControlZone.Left ? -SpeedForce :
            zone == ControlZone.Right ? SpeedForce :
            0f;

        constantForce2D.relativeForce = helperVector2;
    }

    private ControlZone GetSelectedControlZone(Vector2 worldPosition)
    {
        var result = ControlZone.None;
        if (LeftControlZone.bounds.Contains(worldPosition))
        {
            result = ControlZone.Left;
        }
        else if (RightControlZone.bounds.Contains(worldPosition))
        {
            result = ControlZone.Right;
        }
        return result;
    }

    private enum ControlZone
    {
        None,
        Left, 
        Right
    }
}

public enum ControlMode
{
    Drag,         // player drags the paddle by dragging a slider knob beneath it
    JoystickDrag, // player long-presses any point on the screen and drags the paddle around
    LeftRight,    // screen is divided into left-zone and right-zone. Player moves the paddle with constant speed in the direction of the zone he's pressing.
}
