using System.Collections.Generic;
using UnityEngine;

public sealed class Wall : MonoBehaviour
{
    private static readonly List<Wall> ActiveWalls = new List<Wall>(128);

    [SerializeField] private Health health;

    public Health Health => health;
    public static IReadOnlyList<Wall> Active => ActiveWalls;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        ActiveWalls.Clear();
    }

    private void Awake()
    {
        if (health == null)
        {
            health = GetComponent<Health>();
        }
    }

    private void OnEnable()
    {
        if (!ActiveWalls.Contains(this))
        {
            ActiveWalls.Add(this);
        }

        if (health != null)
        {
            health.Died += OnHealthDied;
        }
    }

    private void OnDisable()
    {
        ActiveWalls.Remove(this);

        if (health != null)
        {
            health.Died -= OnHealthDied;
        }
    }

    public static Wall FindNearest(Vector3 position, float range)
    {
        Wall nearest = null;
        float bestDistance = range * range;

        for (int i = 0; i < ActiveWalls.Count; i++)
        {
            Wall wall = ActiveWalls[i];
            if (wall == null || wall.Health == null || !wall.Health.IsAlive)
            {
                continue;
            }

            Vector3 toWall = wall.transform.position - position;
            toWall.y = 0f;
            float sqrDistance = toWall.sqrMagnitude;
            if (sqrDistance >= bestDistance)
            {
                continue;
            }

            nearest = wall;
            bestDistance = sqrDistance;
        }

        return nearest;
    }

    private void OnHealthDied(Health source)
    {
        Destroy(gameObject);
    }
}
