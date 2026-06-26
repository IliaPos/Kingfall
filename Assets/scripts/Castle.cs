using System;
using UnityEngine;

public sealed class Castle : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private Transform attackPoint;

    public event Action<Castle> Destroyed;

    public Health Health => health;
    public Transform AttackPoint => attackPoint != null ? attackPoint : transform;

    public void SetAttackPoint(Transform value)
    {
        attackPoint = value;
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
        if (health != null)
        {
            health.Died += OnHealthDied;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.Died -= OnHealthDied;
        }
    }

    private void OnHealthDied(Health source)
    {
        Destroyed?.Invoke(this);
    }
}
