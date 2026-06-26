using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class Enemy : MonoBehaviour
{
    private static readonly List<Enemy> ActiveEnemies = new List<Enemy>(128);

    [SerializeField] private Health health;
    [SerializeField] private int goldReward = 10;

    public event Action<Enemy> Died;

    public Health Health => health;
    public int GoldReward => goldReward;
    public static IReadOnlyList<Enemy> Active => ActiveEnemies;

    public void SetGoldReward(int value)
    {
        goldReward = Mathf.Max(0, value);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        ActiveEnemies.Clear();
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
        if (!ActiveEnemies.Contains(this))
        {
            ActiveEnemies.Add(this);
        }

        if (health != null)
        {
            health.Died += OnHealthDied;
        }
    }

    private void OnDisable()
    {
        ActiveEnemies.Remove(this);

        if (health != null)
        {
            health.Died -= OnHealthDied;
        }
    }

    private void OnHealthDied(Health source)
    {
        Died?.Invoke(this);
        Destroy(gameObject);
    }
}
