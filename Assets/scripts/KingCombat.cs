using UnityEngine;

public sealed class KingCombat : MonoBehaviour
{
    [SerializeField] private float damage = 35f;
    [SerializeField] private float range = 3.2f;
    [SerializeField] private float attacksPerSecond = 1.6f;

    private IPlayerInputSource inputSource;
    private float nextAttackTime;

    private void Awake()
    {
        inputSource = GetComponent<IPlayerInputSource>();
    }

    private void Update()
    {
        if (inputSource == null || !inputSource.AttackPressed || Time.time < nextAttackTime)
        {
            return;
        }

        Attack();
        nextAttackTime = Time.time + 1f / Mathf.Max(0.01f, attacksPerSecond);
    }

    private void Attack()
    {
        Enemy target = FindBestTarget();
        if (target == null || target.Health == null)
        {
            return;
        }

        target.Health.TakeDamage(damage);
    }

    private Enemy FindBestTarget()
    {
        Enemy best = null;
        float bestDistance = float.MaxValue;
        Vector3 origin = transform.position;

        for (int i = 0; i < Enemy.Active.Count; i++)
        {
            Enemy enemy = Enemy.Active[i];
            if (enemy == null || enemy.Health == null || !enemy.Health.IsAlive)
            {
                continue;
            }

            Vector3 toEnemy = enemy.transform.position - origin;
            toEnemy.y = 0f;
            float sqrDistance = toEnemy.sqrMagnitude;
            if (sqrDistance > range * range || sqrDistance >= bestDistance)
            {
                continue;
            }

            best = enemy;
            bestDistance = sqrDistance;
        }

        return best;
    }
}
