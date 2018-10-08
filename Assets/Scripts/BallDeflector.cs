using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class BallDeflector : MonoBehaviour
{
    public int SideForce = 100;
    private new Transform transform;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.HasComponent<Ball>())
        {
            var collisionLocalX =
                transform.parent.InverseTransformPoint(collision.transform.position).x
                - transform.localPosition.x;
        
            var forceFraction = GetExponentialFraction(transform.localScale.x, 
                                                       collisionLocalX);
            collision.gameObject
                     .GetComponent<Rigidbody2D>()
                     .AddForce(transform.TransformVector(SideForce * forceFraction, 0, 0));
        }
    }

    private static float GetLinearFraction(float paddleWidth, float offsetFromCenter)
    {
        return Mathf.Clamp(offsetFromCenter / (paddleWidth/2), -1f, 1f);
    }

    private static float GetExponentialFraction(float paddleWidth, float offsetFromCenter)
    {
        return Mathf.Sign(offsetFromCenter) * 
               Mathf.Clamp(Mathf.Pow(offsetFromCenter, 2) / 
                           Mathf.Pow(paddleWidth/2, 2), 0f, 1f);
    }

    private void Start()
    {
        transform = GetComponent<Transform>();
    }
}
