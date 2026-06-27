using UnityEngine;

public sealed class EnemyAttack : MonoBehaviour
{
    [SerializeField] private float damage = 8f;
    [SerializeField] private float range = 1.8f;
    [SerializeField] private float attacksPerSecond = 0.6f;
    [SerializeField] private Health target;
    [SerializeField] private float wallAggroRange = 2.0f;

    private float nextAttackTime;

    public Health Target
    {
        get => target;
        set => target = value;
    }

    private void Update()
    {
        if (Time.time < nextAttackTime)
        {
            return;
        }

        Health attackTarget = GetAttackTarget();
        if (attackTarget == null || !attackTarget.IsAlive)
        {
            return;
        }

        Vector3 toTarget = attackTarget.transform.position - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude > range * range)
        {
            return;
        }

        attackTarget.TakeDamage(damage);
        nextAttackTime = Time.time + 1f / Mathf.Max(0.01f, attacksPerSecond);
    }

    private Health GetAttackTarget()
    {
        Wall wall = Wall.FindNearest(transform.position, wallAggroRange);
        if (wall != null && wall.Health != null && wall.Health.IsAlive)
        {
            return wall.Health;
        }

        return target;
    }
}
