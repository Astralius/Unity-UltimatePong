using DigitalRubyShared;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPaddleMover : MonoBehaviour
{
    public float Speed;
    public ControlMode ControlMode;
    public Image LeftControlZone;
    public Image RightControlZone;

    private Vector2 gestureFocusPosition;
    private Vector2 paddleReferencePosition;

    private TapGestureRecognizer tapGesture;
    private TapGestureRecognizer doubleTapGesture;
    private SwipeGestureRecognizer swipeGesture;
    private LongPressGestureRecognizer longPressGesture;

    private void Start()
    {
        SetupDoubleTapGesture();
        SetupTapGesture();
        SetupSwipeGesture();
        SetupLongPressGesture();
    }

    #region Gestures Setup

    private void SetupDoubleTapGesture()
    {
        doubleTapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
        doubleTapGesture.StateUpdated += DoubleTapGestureCallback;
        FingersScript.Instance.AddGesture(doubleTapGesture);
    }

    private void SetupTapGesture()
    {
        tapGesture = new TapGestureRecognizer { RequireGestureRecognizerToFail = doubleTapGesture };
        tapGesture.StateUpdated += TapGestureCallback;
        FingersScript.Instance.AddGesture(tapGesture);
    }

    private void SetupSwipeGesture()
    {
        swipeGesture = new SwipeGestureRecognizer
        {
            Direction = SwipeGestureRecognizerDirection.Any,
            DirectionThreshold = 1.0f // allow a swipe, regardless of slope
        };
        swipeGesture.StateUpdated += SwipeGestureCallback;
        FingersScript.Instance.AddGesture(swipeGesture);
    }

    private void SetupLongPressGesture()
    {
        longPressGesture = new LongPressGestureRecognizer { MaximumNumberOfTouchesToTrack = 1 };
        longPressGesture.StateUpdated += LongPressGestureCallback;
        FingersScript.Instance.AddGesture(longPressGesture);
    }

    #endregion

    #region Gestures' Callbacks

    private void DoubleTapGestureCallback(GestureRecognizer gesture)
    {
        UpdateFocusPosition(gesture);

        if (gesture.State == GestureRecognizerState.Ended)
        {
            Debug.Log($"Double-tapped at {gestureFocusPosition}");
            // TODO: Take action on double tap
        }
    }

    private void TapGestureCallback(GestureRecognizer gesture)
    {
        UpdateFocusPosition(gesture);

        if (gesture.State == GestureRecognizerState.Ended)
        {
            Debug.Log($"Tapped at {gestureFocusPosition}");
            // TODO: Take action on tap
        }
    }

    private void SwipeGestureCallback(GestureRecognizer gesture)
    {
        UpdateFocusPosition(gesture);

        if (gesture.State == GestureRecognizerState.Ended)
        {
            Debug.Log($"Swiped from {gesture.StartFocusX},{gesture.StartFocusY} to " +
                      $"{gesture.FocusX},{gesture.FocusY}; " +
                      $"velocity: {swipeGesture.VelocityX}, {swipeGesture.VelocityY}");
            // TODO: Take action on swipe
        }
    }

    private void LongPressGestureCallback(GestureRecognizer gesture)
    {        
        switch (gesture.State)
        {
            case GestureRecognizerState.Began:
                Debug.Log($"Long press began: {gestureFocusPosition}");
                BeginDrag(gestureFocusPosition);
                break;

            case GestureRecognizerState.Executing:
                Debug.Log($"Long press moved: {gestureFocusPosition}");
                DragTo(gestureFocusPosition);
                break;

            case GestureRecognizerState.Ended:
                Debug.Log($"Long press end: {gestureFocusPosition}, delta: {gesture.DeltaX}, {gesture.DeltaY}");
                EndDrag(gestureFocusPosition);
                break;
        }
    }

    private void UpdateFocusPosition(GestureRecognizer gesture)
    {
        gestureFocusPosition.x = gesture.FocusX;
        gestureFocusPosition.y = gesture.FocusY;
    }

    #endregion

    private void BeginDrag(Vector2 screenPosition)
    {
        var hit = Physics2D.CircleCast(screenPosition.ToWorld(), 2.0f, Vector2.zero);
        if (hit.transform?.tag == "Player")
        {
            paddleReferencePosition = hit.transform.position;
        }
        else
        {
            longPressGesture.Reset();
        }
    }

    private void DragTo(Vector2 screenPosition)
    {
        this.transform.position = screenPosition.ToWorld();
    }

    private void EndDrag(Vector2 screenPosition)
    {

    }
}

public enum ControlMode
{
    Drag,                   // player long-presses the paddle and drags it around
    JoystickDrag,           // player long-presses any point on the screen and drags the paddle around
    Joystick,               // player presses any point on the screen and moves the paddle with constant speed, depending on drag direction
    LeftRightFixedSpeed,    // screen is divided into left-zone and right-zone. Player moves the paddle with constant speed in the direction of the zone he's pressing.
}
