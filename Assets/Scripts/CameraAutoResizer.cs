using UnityEngine;

[ExecuteInEditMode]
public class CameraAutoResizer : MonoBehaviour
{
    [Tooltip("The desired width of the area visible from the camera (in Unity units).")]
    public float DesiredWidth = 10;

    private new Camera camera;

    private void Awake()
    {
        camera = GetComponent<Camera>();     
    }

    private void Update()
    {
        camera.orthographicSize = ComputeCameraSize();
    }

    private float ComputeCameraSize()
    {
        // assuming portrait orientation
        var aspectRatio = Screen.height * 1.0f / Screen.width;
        var halfSize = DesiredWidth * aspectRatio / 2;
        return halfSize;
    }
}
