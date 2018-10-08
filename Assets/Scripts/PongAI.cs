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
        newPosition.x = Mathf.Clamp(newPosition.x, -3.375f, 3.375f);
        newPosition.y = transform.localPosition.y;
        newPosition.z = transform.localPosition.z;
        transform.localPosition = newPosition;
    }
}
