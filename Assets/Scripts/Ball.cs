using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Ball : MonoBehaviour
{
    protected virtual void Start()
    {
        AddRandomForce();
    }

    public void AddRandomForce()
    {
        var x = 0f;
        var y = 0f;

        while (Mathf.Abs(x) < 50)
        {
            x = Random.Range(-200f, 200f);
        }

        while (Mathf.Abs(y) < 80)
        {
            y = Random.Range(-300f, 300f);
        }

        GetComponent<Rigidbody2D>()?.AddForce(new Vector2(x, y));
    }
}
