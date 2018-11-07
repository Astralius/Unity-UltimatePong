using UnityEngine;
using UnityEngine.Events;

#pragma warning disable 108,114

[RequireComponent(typeof(Rigidbody2D))]
public class Ball : MonoBehaviour
{
    public float MaxSpeed;
    public float MinSpeed;
    public ForceMode InitialForceMode = ForceMode.Fixed;

    [Tooltip("Exact initial force vector (used in Fixed force mode).")]
    public Vector2 FixedInitialForce = new Vector2(200, 300);

    [Tooltip("Minimum force magnitude for the initial force vector (used in Random force mode).")]
    public float MinInitialForce = 60;

    [Tooltip("Maximum force magnitude for the initial force vector (used in Random force mode).")]
    public float MaxInitialForce = 250;

    public UnityEvent Collided;

    private new Transform transform;
    private Rigidbody2D rigidbody2D;
    private float squareMaxSpeed;
    private float squareMinSpeed;

    public Vector3 CurrentPosition => transform.position;
    public Vector3 PreviousPosition { get; private set; }
    public float CurrentSpeed => rigidbody2D.velocity.magnitude;

    public void ForceMove()
    {
        rigidbody2D.AddForce(
                    InitialForceMode == ForceMode.Random
                    ? GetRandomForceVector()
                    : FixedInitialForce);
    }

    protected virtual void Start()
    {
        transform = GetComponent<Transform>();
        rigidbody2D = GetComponent<Rigidbody2D>();

        squareMaxSpeed = Mathf.Pow(MaxSpeed, 2);
        squareMinSpeed = Mathf.Pow(MinSpeed, 2);

        ForceMove();
    }

    private void FixedUpdate()
    {
        PreviousPosition = transform.position;
        LimitVelocity(MinSpeed, MaxSpeed);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        Collided.Invoke();
    }

    private Vector2 GetRandomForceVector()
    {
        var result = Vector2.zero;
        while (result.magnitude < MinInitialForce)
        {
            result = Random.insideUnitCircle * MaxInitialForce;
        }
        return result;
    }

    private void LimitVelocity(float minimum, float maximum)
    {
        var currentVelocity = rigidbody2D.velocity;
        if (currentVelocity.sqrMagnitude < squareMinSpeed)
        {
            rigidbody2D.velocity = currentVelocity.normalized * minimum;
        }
        else if (currentVelocity.sqrMagnitude > squareMaxSpeed)
        {
            rigidbody2D.velocity = currentVelocity.normalized * maximum;
        }
    }

    public enum ForceMode
    {
        Random,
        Fixed
    }
}
