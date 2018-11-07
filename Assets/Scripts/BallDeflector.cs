using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class BallDeflector : MonoBehaviour
{
    [Tooltip("The maximum angle that a ball can bounce off with.")]
    [Range(0f, 89.9f)]
    public float MaxDeflectionAngle = 70;

    [Tooltip("The speed that is added to the ball upon bounce-off.")]
    [Range(0f, 1f)]
    public float SpeedIncrement = 0.05f;

    private new Transform transform;
    
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

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.HasComponent<Ball>())
        {
            var collisionLocalX =
                transform.parent.InverseTransformPoint(collision.transform.position).x
                - transform.localPosition.x;
        
            var angleFraction = GetLinearFraction(transform.localScale.x, collisionLocalX);

            var ballRigidbody2D = collision.gameObject
                                           .GetComponent<Rigidbody2D>();

            var currentBallSpeed = ballRigidbody2D.velocity.magnitude;

            ballRigidbody2D.velocity = CalculateNewVelocity(currentBallSpeed, angleFraction);
        }
    }

    private Vector2 CalculateNewVelocity(float currentSpeed, float angleFraction)
    {
        var rotatedNormalizedVelocity =
            transform.TransformDirection(
                Quaternion.AngleAxis(MaxDeflectionAngle * angleFraction, Vector3.back) * 
                Vector2.up).normalized;

        return rotatedNormalizedVelocity * (currentSpeed + SpeedIncrement);
    }
}
