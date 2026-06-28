using UnityEngine;
using System.Collections.Generic;

public sealed class Tower : MonoBehaviour
{
    private const string ArrowPrefabResourcePath = "Prefabs/Gameplay/ArrowProjectile";

    private static readonly List<Tower> ActiveTowers = new List<Tower>(64);

    [SerializeField] private float range = 9f;
    [SerializeField] private float attacksPerSecond = 1f;
    [SerializeField] private float damage = 25f;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private ArrowProjectile arrowPrefab;

    private float nextShotTime;
    private RunStats runStats;

    public static IReadOnlyList<Tower> Active => ActiveTowers;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        ActiveTowers.Clear();
    }

    public void SetShootPoint(Transform value)
    {
        shootPoint = value;
    }

    public void Initialize(RunStats stats)
    {
        runStats = stats;
    }

    private void OnEnable()
    {
        if (!ActiveTowers.Contains(this))
        {
            ActiveTowers.Add(this);
        }
    }

    private void OnDisable()
    {
        ActiveTowers.Remove(this);
    }

    private void Update()
    {
        if (Time.time < nextShotTime)
        {
            return;
        }

        Enemy target = FindTarget();
        if (target == null)
        {
            return;
        }

        Shoot(target);
        float speedMultiplier = runStats != null ? runStats.TowerAttackSpeedMultiplier : 1f;
        nextShotTime = Time.time + 1f / Mathf.Max(0.01f, attacksPerSecond * speedMultiplier);
    }

    private Enemy FindTarget()
    {
        Enemy best = null;
        float bestDistance = float.MaxValue;
        float effectiveRange = range * (runStats != null ? runStats.TowerRangeMultiplier : 1f);
        float rangeSqr = effectiveRange * effectiveRange;
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
            if (sqrDistance > rangeSqr || sqrDistance >= bestDistance)
            {
                continue;
            }

            best = enemy;
            bestDistance = sqrDistance;
        }

        return best;
    }

    private void Shoot(Enemy target)
    {
        ArrowProjectile prefab = GetArrowPrefab();
        if (prefab == null)
        {
            Debug.LogWarning($"Tower cannot shoot because Resources/{ArrowPrefabResourcePath} is missing.", this);
            return;
        }

        Vector3 origin = shootPoint != null ? shootPoint.position : transform.position + Vector3.up * 1.8f;
        ArrowProjectile arrow = Instantiate(prefab, origin, Quaternion.identity);
        float damageMultiplier = runStats != null ? runStats.TowerDamageMultiplier : 1f;
        arrow.Launch(target, damage * damageMultiplier);
    }

    private ArrowProjectile GetArrowPrefab()
    {
        if (arrowPrefab == null)
        {
            arrowPrefab = Resources.Load<ArrowProjectile>(ArrowPrefabResourcePath);
        }

        return arrowPrefab;
    }
}
