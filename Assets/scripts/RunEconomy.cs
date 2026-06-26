using System;
using UnityEngine;

public sealed class RunEconomy : MonoBehaviour
{
    [SerializeField] private int startingGold = 120;

    public event Action<int> GoldChanged;

    public int Gold { get; private set; }

    private void Awake()
    {
        Gold = Mathf.Max(0, startingGold);
    }

    public void AddGold(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        Gold += amount;
        GoldChanged?.Invoke(Gold);
    }

    public bool TrySpend(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (Gold < amount)
        {
            return false;
        }

        Gold -= amount;
        GoldChanged?.Invoke(Gold);
        return true;
    }
}
