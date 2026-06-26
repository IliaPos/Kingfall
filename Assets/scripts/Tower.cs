using UnityEngine;

public sealed class Tower : MonoBehaviour
{
    [SerializeField] private float range = 9f;
    [SerializeField] private float attacksPerSecond = 1f;
    [SerializeField] private float damage = 25f;
    [SerializeField] private Transform shootPoint;

    private float nextShotTime;
    private Material arrowMaterial;

    public void SetShootPoint(Transform value)
    {
        shootPoint = value;
    }

    private void Awake()
    {
        arrowMaterial = CreateMaterial("Prototype Arrow", new Color(0.78f, 0.54f, 0.25f));
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
        nextShotTime = Time.time + 1f / Mathf.Max(0.01f, attacksPerSecond);
    }

    private Enemy FindTarget()
    {
        Enemy best = null;
        float bestDistance = float.MaxValue;
        float rangeSqr = range * range;
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

        ArrowProjectile arrow = arrowObject.AddComponent<ArrowProjectile>();
        arrow.Launch(target, damage);
    }

    private static Material CreateMaterial(string materialName, Color color)
    {
        Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.name = materialName;
        material.color = color;
        return material;
    }
}
