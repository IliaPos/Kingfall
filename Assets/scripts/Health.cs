using System;
using UnityEngine;

public sealed class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;

    public event Action<Health> Changed;
    public event Action<Health> Died;

    public float MaxHealth => maxHealth;
    public float CurrentHealth { get; private set; }
    public bool IsAlive => CurrentHealth > 0f;
    public float Normalized => maxHealth <= 0f ? 0f : CurrentHealth / maxHealth;

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void SetMaxHealth(float value, bool refill = true)
    {
        maxHealth = Mathf.Max(1f, value);
        CurrentHealth = refill ? maxHealth : Mathf.Min(CurrentHealth, maxHealth);
        Changed?.Invoke(this);
    }

    public void TakeDamage(float amount)
    {
        if (!IsAlive || amount <= 0f)
        {
            return;
        }

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        Changed?.Invoke(this);

        if (CurrentHealth <= 0f)
        {
            Died?.Invoke(this);
        }
    }

    public void Heal(float amount)
    {
        if (!IsAlive || amount <= 0f)
        {
            return;
        }

        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        Changed?.Invoke(this);
    }
}
