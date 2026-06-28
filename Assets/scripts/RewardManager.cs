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
    public readonly RewardDefinition Definition;
    public readonly float Value;

    public RewardOption(RewardDefinition definition, float value)
    {
        Definition = definition;
        Value = value;
    }
}

public sealed class RewardManager : MonoBehaviour
{
    [SerializeField] private RunStats runStats;
    [SerializeField] private RunEconomy economy;
    [SerializeField] private Castle castle;
    [SerializeField] private RewardDefinition[] rewardDefinitions;

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

    public void SetRewardDefinitions(RewardDefinition[] definitions)
    {
        rewardDefinitions = definitions;
    }

    public void BeginReward(int completedWave)
    {
        EnsureRewardDefinitions();

        if (CountUniqueRewardTypes() < activeOptions.Length)
        {
            HasPendingReward = false;
            return;
        }

        RewardDefinition first = GetRandomRewardDefinition();
        RewardDefinition second = GetRandomRewardDefinitionExcept(first);
        RewardDefinition third = GetRandomRewardDefinitionExcept(first, second);

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

    private RewardDefinition GetRandomRewardDefinition()
    {
        RewardDefinition definition;
        do
        {
            definition = rewardDefinitions[Random.Range(0, rewardDefinitions.Length)];
        }
        while (definition == null);

        return definition;
    }

    private void EnsureRewardDefinitions()
    {
        if (rewardDefinitions != null && CountUniqueRewardTypes() >= activeOptions.Length)
        {
            return;
        }

        Debug.LogWarning("RewardManager has missing reward definitions. Falling back to runtime defaults.", this);
        rewardDefinitions = RewardDefinition.CreateRuntimeDefaults();
    }

    private RewardDefinition GetRandomRewardDefinitionExcept(params RewardDefinition[] excluded)
    {
        RewardDefinition definition;
        do
        {
            definition = GetRandomRewardDefinition();
        }
        while (Contains(excluded, definition));

        return definition;
    }

    private int CountUniqueRewardTypes()
    {
        if (rewardDefinitions == null)
        {
            return 0;
        }

        int count = 0;

        for (int i = 0; i < rewardDefinitions.Length; i++)
        {
            RewardDefinition definition = rewardDefinitions[i];
            if (definition == null)
            {
                continue;
            }

            bool alreadyCounted = false;
            for (int j = 0; j < i; j++)
            {
                if (rewardDefinitions[j] != null && rewardDefinitions[j].Type == definition.Type)
                {
                    alreadyCounted = true;
                    break;
                }
            }

            if (!alreadyCounted)
            {
                count++;
            }
        }

        return count;
    }

    private static bool Contains(RewardDefinition[] options, RewardDefinition definition)
    {
        for (int i = 0; i < options.Length; i++)
        {
            if (options[i] == definition || (options[i] != null && definition != null && options[i].Type == definition.Type))
            {
                return true;
            }
        }

        return false;
    }

    private static RewardOption CreateReward(RewardDefinition definition, int wave)
    {
        return new RewardOption(definition, definition != null ? definition.GetValue(wave) : 0f);
    }

    private void Apply(RewardOption option)
    {
        if (option.Definition == null)
        {
            return;
        }

        switch (option.Definition.Type)
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
            if (option.Definition != null)
            {
                GUI.Label(new Rect(x + 20f, rowY, width - 40f, 24f), $"{i + 1}. {option.Definition.Title} - {FormatDescription(option)}");
            }
        }
    }

    private static string FormatDescription(RewardOption option)
    {
        if (option.Definition.Type == RewardType.Gold)
        {
            return $"+{Mathf.RoundToInt(option.Value)} gold";
        }

        return option.Definition.Description;
    }
}
