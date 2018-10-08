using DigitalRubyShared;
using UnityEngine;

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
    public GameObject LeftControlZone;
    public GameObject RightControlZone;   

    private new Transform transform;
    private new Rigidbody2D rigidbody2D;
    private ConstantForce2D constantForce2D;

    private Vector2? paddleReferencePosition;
    private Vector2? joystickReferencePosition;
    private Vector2 gestureFocusPosition = Vector2.zero;
    private Vector2 helperVector2 = Vector2.zero;

    private LongPressGestureRecognizer longPressGesture;

    private void Start()
    {
        transform = GetComponent<Transform>();
        rigidbody2D = GetComponent<Rigidbody2D>();
        constantForce2D = GetComponent<ConstantForce2D>();

        SetupLongPressGesture();

        var isLeftRightControlled = ControlMode == ControlMode.LeftRight;
        LeftControlZone.SetActive(isLeftRightControlled);
        RightControlZone.SetActive(isLeftRightControlled);
    }

    private void Update()
    {
        transform.localPosition = Vector2.ClampMagnitude(transform.localPosition, HorizontalPositionThreshold);
    }

    private void SetupLongPressGesture()
    {
        longPressGesture = new LongPressGestureRecognizer
        {
            MaximumNumberOfTouchesToTrack = 1,
            ThresholdUnits = Mathf.Infinity,
            MinimumDurationSeconds = 0f
        };
        longPressGesture.StateUpdated += LongPressGestureCallback;
        FingersScript.Instance.AddGesture(longPressGesture);
    }

    private void LongPressGestureCallback(GestureRecognizer gesture)
    {
        UpdateFocusPosition(gesture);

        switch (gesture.State)
        {
            case GestureRecognizerState.Began:
            {
                // Debug.Log($"Long press began: ({gestureFocusPosition})");
                BeginDrag(gestureFocusPosition);
                break;
            }

            case GestureRecognizerState.Executing:
            {
                // Debug.Log($"Long press moved: {gestureFocusPosition}");
                DragTo(gestureFocusPosition);
                break;
            }

            case GestureRecognizerState.Ended:
            {
                // Debug.Log($"Long press end: {gestureFocusPosition}, delta: {gesture.DeltaX}, {gesture.DeltaY}");
                EndDrag(gestureFocusPosition);
                break;
            }
        }
    }

    private void UpdateFocusPosition(GestureRecognizer gesture)
    {
        gestureFocusPosition.x = gesture.FocusX;
        gestureFocusPosition.y = gesture.FocusY;
    }

    private void BeginDrag(Vector2 screenPosition)
    {
        switch (ControlMode)
        {
            case ControlMode.Drag:
            {
                var hit = Physics2D.CircleCast(screenPosition.ToWorld(), 1.0f, Vector2.zero);
                if (hit.transform != null && hit.transform.tag == "Player")
                {
                    rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
                    paddleReferencePosition = hit.transform.localPosition;
                    joystickReferencePosition = null;
                }
                else
                {
                    longPressGesture.Reset();
                }
                break;
            }

            case ControlMode.Joystick:
            {
                rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
                paddleReferencePosition = null;
                joystickReferencePosition = screenPosition.ToWorld();
                break;
            }

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

                var hit = Physics2D.Raycast(screenPosition.ToWorld(), Vector2.zero);
                if (hit.transform?.gameObject != null)
                {
                    switch (hit.transform.gameObject.name)
                    {
                        case "Right":
                        {
                            helperVector2.x = SpeedForce;
                            helperVector2.y = 0;
                            constantForce2D.relativeForce = helperVector2;
                            break;
                        }

                        case "Left":
                        {
                            helperVector2.x = -SpeedForce;
                            helperVector2.y = 0;
                            constantForce2D.relativeForce = helperVector2;
                            break;
                        }

                        default:
                        {
                            Debug.LogWarning("Left/Right control zone(s) not present!");
                            longPressGesture.Reset();
                            break;
                        }
                    }
                }

                break;
            }
        }

    }

    private void DragTo(Vector2 screenPosition)
    {
        switch (ControlMode)
        {
            case ControlMode.Drag:
            {
                if (paddleReferencePosition.HasValue)
                {
                    var newLocalX = transform.parent.InverseTransformPoint(screenPosition.ToWorld()).x;
                    helperVector2.x = Mathf.Clamp(newLocalX,
                                                  -HorizontalPositionThreshold,
                                                  HorizontalPositionThreshold);
                    transform.localPosition = helperVector2;
                }

                break;
            }

            case ControlMode.Joystick:
            {
                if (joystickReferencePosition.HasValue)
                {
                    helperVector2.y = 0;
                    var delta = screenPosition.ToWorld().x - joystickReferencePosition.Value.x;
                    if (Mathf.Abs(delta) > JoystickThreshold)
                    {
                        var isMovingRight = delta > 0;
                        helperVector2.x = isMovingRight ? SpeedForce : -SpeedForce;               
                        constantForce2D.relativeForce = helperVector2;
                    }
                    else
                    {
                        helperVector2.x = 0;
                        constantForce2D.relativeForce = helperVector2;
                    }
                }

                break;
            }

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

            // ControlMode.LeftRight does not need further handling here.
        }
    }

    private void EndDrag(Vector2 screenPosition)
    {
        paddleReferencePosition = null;
        joystickReferencePosition = null;
        constantForce2D.relativeForce = Vector2.zero;

        longPressGesture.Reset();
    }
}

public enum ControlMode
{
    Drag,           // player long-presses the paddle and drags it around
    JoystickDrag,   // player long-presses any point on the screen and drags the paddle around
    Joystick,       // player presses any point on the screen and moves the paddle with constant speed, depending on drag direction
    LeftRight,      // screen is divided into left-zone and right-zone. Player moves the paddle with constant speed in the direction of the zone he's pressing.
}
