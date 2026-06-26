using UnityEngine;

public sealed class EnemyMover : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2.2f;
    [SerializeField] private float stopDistance = 1.6f;
    [SerializeField] private Transform target;

    public Transform Target
    {
        get => target;
        set => target = value;
    }

    public float StopDistance
    {
        get => stopDistance;
        set => stopDistance = Mathf.Max(0.1f, value);
    }

    private void Update()
    {
        if (target == null)
        {
            return;
        }

        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude <= stopDistance * stopDistance)
        {
            return;
        }

        Vector3 direction = toTarget.normalized;
        Vector3 nextPosition = transform.position + direction * moveSpeed * Time.deltaTime;
        nextPosition.y = 0f;
        transform.position = nextPosition;
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }
}
