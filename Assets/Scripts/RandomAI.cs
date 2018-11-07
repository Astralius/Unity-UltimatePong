using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomAI : MonoBehaviour
{
    public Ball TheBall;

    [Tooltip("How far to the left/right can the paddle move?")]
    public float HorizontalPositionThreshold = 3.75f;

    [Tooltip("What fraction of the paddle width should take part in random selection of the hit spot?")]
    [Range(0f, 1f)]
    public float SafetyMargin = 0.9f;

    private new Transform transform;
    private Vector3 position;
    private double currentOffset;

    public void OnBallCollided()
    {
        if (gameObject.activeInHierarchy)
        {
            var paddleWidth = transform.localScale.x / 2 * SafetyMargin;
            var targetOffset = Random.Range(-paddleWidth, paddleWidth);
            StartCoroutine(GraduallyChangeOffset(targetOffset));
        }
    }

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
        position.x = Mathf.Clamp(position.x + (float)currentOffset, -HorizontalPositionThreshold, HorizontalPositionThreshold);
        position.y = transform.localPosition.y;
        position.z = transform.localPosition.z;
        transform.localPosition = position;
    }

    private IEnumerator GraduallyChangeOffset(float targetOffset)
    {
        var step = (targetOffset - currentOffset) * TheBall.CurrentSpeed / 2 * Time.deltaTime;
        while (Math.Abs(targetOffset - currentOffset) > Math.Abs(step))
        {
            currentOffset += step;
            yield return new WaitForEndOfFrame();
        }
        currentOffset = targetOffset;
    }
}
