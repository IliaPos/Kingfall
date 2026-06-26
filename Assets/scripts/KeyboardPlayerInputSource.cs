using UnityEngine;
using UnityEngine.InputSystem;

public sealed class KeyboardPlayerInputSource : MonoBehaviour, IPlayerInputSource
{
    public Vector2 Move => ReadMovementInput();
    public bool AttackPressed => Keyboard.current?.spaceKey.wasPressedThisFrame == true || Mouse.current?.leftButton.wasPressedThisFrame == true;
    public bool BuildPressed => Keyboard.current?.bKey.wasPressedThisFrame == true;

    private static Vector2 ReadMovementInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return Vector2.zero;
        }

        Vector2 input = Vector2.zero;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
        {
            input.x -= 1f;
        }

        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
        {
            input.x += 1f;
        }

        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
        {
            input.y -= 1f;
        }

        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
        {
            input.y += 1f;
        }

        return input.sqrMagnitude > 1f ? input.normalized : input;
    }
}
