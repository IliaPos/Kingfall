using UnityEngine;

public interface IPlayerInputSource
{
    Vector2 Move { get; }
    bool AttackPressed { get; }
    bool BuildPressed { get; }
    bool SwitchBuildPressed { get; }
    bool ConfirmPressed { get; }
    bool CancelPressed { get; }
    bool StartWavePressed { get; }
    int RewardSelection { get; }
}
