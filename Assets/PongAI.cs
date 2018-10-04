using UnityEngine;

public class PongAI : MonoBehaviour
{
    public Transform TheBall;

    private new Transform transform;
    private Vector3 newPosition;

    private void Start()
    {
        this.transform = GetComponent<Transform>();      
    }

    private void Update()
    {
        newPosition = transform.parent.InverseTransformPoint(TheBall.position);
        newPosition.x = Mathf.Clamp(newPosition.x, -3, 3);
        newPosition.y = transform.localPosition.y;
        newPosition.z = transform.localPosition.z;
        transform.localPosition = newPosition;
    }
}
