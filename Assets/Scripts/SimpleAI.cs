using UnityEngine;

public class SimpleAI : MonoBehaviour
{
    public Ball TheBall;

    [Tooltip("How far to the left/right can the paddle move?")]
    public float HorizontalPositionThreshold = 3.75f;

    private new Transform transform;
    private Vector3 position;

    private void Start()
    {
        this.transform = GetComponent<Transform>();
    }

    private void Update()
    {
        FollowTheBall();
    }

    private void FollowTheBall()
    {
        position = transform.parent.InverseTransformPoint(TheBall.CurrentPosition);
        position.x = Mathf.Clamp(position.x, -HorizontalPositionThreshold, HorizontalPositionThreshold);
        position.y = transform.localPosition.y;
        position.z = transform.localPosition.z;
        transform.localPosition = position;
    }
}
