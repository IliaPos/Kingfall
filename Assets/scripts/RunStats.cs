using UnityEngine;

public sealed class RunStats : MonoBehaviour
{
    public float KingDamageMultiplier { get; private set; } = 1f;
    public float TowerDamageMultiplier { get; private set; } = 1f;
    public float TowerRangeMultiplier { get; private set; } = 1f;
    public float TowerAttackSpeedMultiplier { get; private set; } = 1f;

    public void AddKingDamage(float percent)
    {
        KingDamageMultiplier += Mathf.Max(0f, percent);
    }

    public void AddTowerDamage(float percent)
    {
        TowerDamageMultiplier += Mathf.Max(0f, percent);
    }

    public void AddTowerRange(float percent)
    {
        TowerRangeMultiplier += Mathf.Max(0f, percent);
    }

    public void AddTowerAttackSpeed(float percent)
    {
        TowerAttackSpeedMultiplier += Mathf.Max(0f, percent);
    }
}
