using UnityEngine;
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

    private Rigidbody2D rigidbody2D;
    private float squareMaxSpeed;
    private float squareMinSpeed;

    protected virtual void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();

        squareMaxSpeed = Mathf.Pow(MaxSpeed, 2);
        squareMinSpeed = Mathf.Pow(MinSpeed, 2);

        ForceMove();
    }

    public void ForceMove()
    {
        rigidbody2D.AddForce(
                    InitialForceMode == ForceMode.Random
                    ? GetRandomForceVector()
                    : FixedInitialForce);
    }

    private void FixedUpdate()
    {
        var currentVelocity = rigidbody2D.velocity;
        if (currentVelocity.sqrMagnitude < squareMinSpeed)
        {
            rigidbody2D.velocity = currentVelocity.normalized * MinSpeed;
        }
        else if (currentVelocity.sqrMagnitude > squareMaxSpeed)
        {
            rigidbody2D.velocity = currentVelocity.normalized * MaxSpeed;
        }
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

    public enum ForceMode
    {
        Random,
        Fixed
    }
}
