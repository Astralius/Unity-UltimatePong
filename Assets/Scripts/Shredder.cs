using System.Collections;
using UnityEngine;

public class Shredder : MonoBehaviour
{
    public Object Owner;
    public Transform RespawnPoint;
    [Range(0f, 10f)]
    public float RespawnDelay;
   

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // TODO: Add points for Owner
        if (collision.HasComponent<Ball>())
        {
            StartCoroutine(Respawn(collision.GetComponent<Ball>()));
        }
    }

    private IEnumerator Respawn(Ball ball)
    {
        ball.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        ball.transform.position = RespawnPoint.position;
        yield return new WaitForSeconds(RespawnDelay);
        ball.ForceMove();
    }
}
