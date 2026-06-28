using UnityEngine;

[CreateAssetMenu(menuName = "Kingfall/Reward Definition")]
public sealed class RewardDefinition : ScriptableObject
{
    [SerializeField] private RewardType type;
    [SerializeField] private string title;
    [SerializeField] private string description;
    [SerializeField] private float value;
    [SerializeField] private float valuePerWave;

    public RewardType Type => type;
    public string Title => title;
    public string Description => description;
    public float GetValue(int wave) => value + valuePerWave * Mathf.Max(0, wave);

    internal static RewardDefinition CreateRuntime(RewardType rewardType, string rewardTitle, string rewardDescription, float rewardValue, float rewardValuePerWave = 0f)
    {
        RewardDefinition definition = CreateInstance<RewardDefinition>();
        definition.type = rewardType;
        definition.title = rewardTitle;
        definition.description = rewardDescription;
        definition.value = rewardValue;
        definition.valuePerWave = rewardValuePerWave;
        return definition;
    }

    internal static RewardDefinition[] CreateRuntimeDefaults()
    {
        return new[]
        {
            CreateRuntime(RewardType.KingDamage, "Sharper Sword", "+20% king damage", 0.2f),
            CreateRuntime(RewardType.TowerDamage, "Better Arrows", "+20% tower damage", 0.2f),
            CreateRuntime(RewardType.TowerRange, "Watch Posts", "+15% tower range", 0.15f),
            CreateRuntime(RewardType.TowerAttackSpeed, "Fast Bowstrings", "+15% tower attack speed", 0.15f),
            CreateRuntime(RewardType.Gold, "Royal Taxes", "+gold", 25f, 5f),
            CreateRuntime(RewardType.CastleMaxHealth, "Stone Reinforcement", "+50 castle max HP", 50f),
            CreateRuntime(RewardType.CastleHeal, "Repair Crew", "Heal castle for 45 HP", 45f),
        };
    }
}
