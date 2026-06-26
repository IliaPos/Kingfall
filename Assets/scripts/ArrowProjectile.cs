using UnityEngine;

public sealed class ArrowProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 18f;
    [SerializeField] private float hitDistance = 0.35f;
    [SerializeField] private float lifetime = 3f;

    private Enemy target;
    private float damage;
    private float dieTime;

    public void Launch(Enemy enemy, float projectileDamage)
    {
        target = enemy;
        damage = projectileDamage;
        dieTime = Time.time + lifetime;
    }

    private void Update()
    {
        if (Time.time >= dieTime || target == null || target.Health == null || !target.Health.IsAlive)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 targetPosition = target.transform.position + Vector3.up * 0.9f;
        Vector3 toTarget = targetPosition - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        if (toTarget.magnitude <= Mathf.Max(hitDistance, distanceThisFrame))
        {
            target.Health.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        Vector3 direction = toTarget.normalized;
        transform.position += direction * distanceThisFrame;
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up) * Quaternion.Euler(90f, 0f, 0f);
    }
}
