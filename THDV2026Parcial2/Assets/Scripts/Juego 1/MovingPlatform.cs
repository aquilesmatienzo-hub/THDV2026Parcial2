
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float speed = 2f;

    private Transform target;

    private void Start()
    {
        target = pointB;
    }

    private void FixedUpdate()
    {
        if (pointA == null || pointB == null)
            return;

        Vector3 nextPosition = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.fixedDeltaTime
        );

        transform.position = nextPosition;

        if (Vector3.Distance(transform.position, target.position) < 0.02f)
        {
            target = target == pointB ? pointA : pointB;
        }
    }
}
