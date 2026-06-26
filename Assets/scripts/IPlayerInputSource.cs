using UnityEngine;

public interface IPlayerInputSource
{
    Vector2 Move { get; }
    bool AttackPressed { get; }
    bool BuildPressed { get; }
}
