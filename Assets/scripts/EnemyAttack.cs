using UnityEngine;

public sealed class EnemyAttack : MonoBehaviour
{
    [SerializeField] private float damage = 8f;
    [SerializeField] private float range = 1.8f;
    [SerializeField] private float attacksPerSecond = 0.6f;
    [SerializeField] private Health target;

    private float nextAttackTime;

    public Health Target
    {
        get => target;
        set => target = value;
    }

    private void Update()
    {
        if (target == null || !target.IsAlive || Time.time < nextAttackTime)
        {
            return;
        }

        Vector3 toTarget = target.transform.position - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude > range * range)
        {
            return;
        }

        target.TakeDamage(damage);
        nextAttackTime = Time.time + 1f / Mathf.Max(0.01f, attacksPerSecond);
    }
}
