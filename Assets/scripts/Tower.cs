using UnityEngine;
using System.Collections.Generic;

public sealed class Tower : MonoBehaviour
{
    private static readonly List<Tower> ActiveTowers = new List<Tower>(64);

    [SerializeField] private float range = 9f;
    [SerializeField] private float attacksPerSecond = 1f;
    [SerializeField] private float damage = 25f;
    [SerializeField] private Transform shootPoint;

    private float nextShotTime;
    private Material arrowMaterial;
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

    private void Awake()
    {
        arrowMaterial = CreateMaterial("Prototype Arrow", new Color(0.78f, 0.54f, 0.25f));
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
        Vector3 origin = shootPoint != null ? shootPoint.position : transform.position + Vector3.up * 1.8f;
        GameObject arrowObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        arrowObject.name = "Arrow Projectile";
        arrowObject.transform.position = origin;
        arrowObject.transform.localScale = new Vector3(0.06f, 0.35f, 0.06f);
        arrowObject.GetComponent<Renderer>().sharedMaterial = arrowMaterial;
        RemoveCollider(arrowObject);

        ArrowProjectile arrow = arrowObject.AddComponent<ArrowProjectile>();
        float damageMultiplier = runStats != null ? runStats.TowerDamageMultiplier : 1f;
        arrow.Launch(target, damage * damageMultiplier);
    }

    private static Material CreateMaterial(string materialName, Color color)
    {
        Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.name = materialName;
        material.color = color;
        return material;
    }

    private static void RemoveCollider(GameObject target)
    {
        Collider collider = target.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }
    }
}
