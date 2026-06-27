using UnityEngine;
using UnityEngine.InputSystem;

public sealed class KeyboardPlayerInputSource : MonoBehaviour, IPlayerInputSource
{
    public Vector2 Move => ReadMovementInput();
    public bool AttackPressed => Keyboard.current?.spaceKey.wasPressedThisFrame == true;
    public bool BuildPressed => Keyboard.current?.bKey.wasPressedThisFrame == true;
    public bool SwitchBuildPressed => Keyboard.current?.qKey.wasPressedThisFrame == true;
    public bool ConfirmPressed => Mouse.current?.leftButton.wasPressedThisFrame == true;
    public bool CancelPressed => Keyboard.current?.escapeKey.wasPressedThisFrame == true;
    public bool StartWavePressed => Keyboard.current?.enterKey.wasPressedThisFrame == true || Keyboard.current?.numpadEnterKey.wasPressedThisFrame == true;
    public int RewardSelection => ReadRewardSelection();

    private static int ReadRewardSelection()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return 0;
        }

        if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame)
        {
            return 1;
        }

        if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame)
        {
            return 2;
        }

        if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame)
        {
            return 3;
        }

        return 0;
    }

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
