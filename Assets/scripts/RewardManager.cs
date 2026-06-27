using UnityEngine;

public enum RewardType
{
    KingDamage,
    TowerDamage,
    TowerRange,
    TowerAttackSpeed,
    Gold,
    CastleMaxHealth,
    CastleHeal
}

public readonly struct RewardOption
{
    public readonly RewardType Type;
    public readonly string Title;
    public readonly string Description;
    public readonly float Value;

    public RewardOption(RewardType type, string title, string description, float value)
    {
        Type = type;
        Title = title;
        Description = description;
        Value = value;
    }
}

public sealed class RewardManager : MonoBehaviour
{
    [SerializeField] private RunStats runStats;
    [SerializeField] private RunEconomy economy;
    [SerializeField] private Castle castle;

    private readonly RewardOption[] activeOptions = new RewardOption[3];
    private IPlayerInputSource inputSource;

    public bool HasPendingReward { get; private set; }

    public void Initialize(RunStats stats, RunEconomy runEconomy, Castle targetCastle, IPlayerInputSource input)
    {
        runStats = stats;
        economy = runEconomy;
        castle = targetCastle;
        inputSource = input;
    }

    public void BeginReward(int completedWave)
    {
        RewardType first = GetRandomRewardType();
        RewardType second = GetRandomRewardTypeExcept(first);
        RewardType third = GetRandomRewardTypeExcept(first, second);

        activeOptions[0] = CreateReward(first, completedWave);
        activeOptions[1] = CreateReward(second, completedWave);
        activeOptions[2] = CreateReward(third, completedWave);
        HasPendingReward = true;
    }

    private void Update()
    {
        if (!HasPendingReward || inputSource == null)
        {
            return;
        }

        int selection = inputSource.RewardSelection;
        if (selection < 1 || selection > activeOptions.Length)
        {
            return;
        }

        Apply(activeOptions[selection - 1]);
        HasPendingReward = false;
    }

    private static RewardType GetRandomRewardType()
    {
        return (RewardType)Random.Range(0, System.Enum.GetValues(typeof(RewardType)).Length);
    }

    private static RewardType GetRandomRewardTypeExcept(params RewardType[] excluded)
    {
        RewardType type;
        do
        {
            type = GetRandomRewardType();
        }
        while (Contains(excluded, type));

        return type;
    }

    private static bool Contains(RewardType[] options, RewardType type)
    {
        for (int i = 0; i < options.Length; i++)
        {
            if (options[i] == type)
            {
                return true;
            }
        }

        return false;
    }

    private RewardOption CreateReward(RewardType type, int wave)
    {
        int scalingGold = 25 + wave * 5;

        switch (type)
        {
            case RewardType.KingDamage:
                return new RewardOption(RewardType.KingDamage, "Sharper Sword", "+20% king damage", 0.2f);
            case RewardType.TowerDamage:
                return new RewardOption(RewardType.TowerDamage, "Better Arrows", "+20% tower damage", 0.2f);
            case RewardType.TowerRange:
                return new RewardOption(RewardType.TowerRange, "Watch Posts", "+15% tower range", 0.15f);
            case RewardType.TowerAttackSpeed:
                return new RewardOption(RewardType.TowerAttackSpeed, "Fast Bowstrings", "+15% tower attack speed", 0.15f);
            case RewardType.Gold:
                return new RewardOption(RewardType.Gold, "Royal Taxes", $"+{scalingGold} gold", scalingGold);
            case RewardType.CastleMaxHealth:
                return new RewardOption(RewardType.CastleMaxHealth, "Stone Reinforcement", "+50 castle max HP", 50f);
            default:
                return new RewardOption(RewardType.CastleHeal, "Repair Crew", "Heal castle for 45 HP", 45f);
        }
    }

    private void Apply(RewardOption option)
    {
        switch (option.Type)
        {
            case RewardType.KingDamage:
                runStats?.AddKingDamage(option.Value);
                break;
            case RewardType.TowerDamage:
                runStats?.AddTowerDamage(option.Value);
                break;
            case RewardType.TowerRange:
                runStats?.AddTowerRange(option.Value);
                break;
            case RewardType.TowerAttackSpeed:
                runStats?.AddTowerAttackSpeed(option.Value);
                break;
            case RewardType.Gold:
                economy?.AddGold(Mathf.RoundToInt(option.Value));
                break;
            case RewardType.CastleMaxHealth:
                if (castle != null && castle.Health != null)
                {
                    castle.Health.SetMaxHealth(castle.Health.MaxHealth + option.Value, false);
                    castle.Health.Heal(option.Value);
                }
                break;
            case RewardType.CastleHeal:
                castle?.Health?.Heal(option.Value);
                break;
        }
    }

    private void OnGUI()
    {
        if (!HasPendingReward)
        {
            return;
        }

        float width = 520f;
        float height = 150f;
        float x = Screen.width * 0.5f - width * 0.5f;
        float y = Screen.height * 0.5f - height * 0.5f;

        GUI.Box(new Rect(x, y, width, height), "Choose Reward");
        for (int i = 0; i < activeOptions.Length; i++)
        {
            RewardOption option = activeOptions[i];
            float rowY = y + 32f + i * 36f;
            GUI.Label(new Rect(x + 20f, rowY, width - 40f, 24f), $"{i + 1}. {option.Title} - {option.Description}");
        }
    }
}
