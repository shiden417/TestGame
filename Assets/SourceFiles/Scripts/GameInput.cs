using UnityEngine.InputSystem;

using UnityEngine;

[DisallowMultipleComponent]
public sealed class GameInput : MonoBehaviour
{
    public InputAction Move { get; private set; } = null!;
    public InputAction Look { get; private set; } = null!;
    public InputAction MouseLook { get; private set; } = null!;
    public InputAction NormalAttack { get; private set; } = null!;
    public InputAction Dodge { get; private set; } = null!;
    public InputAction Jump { get; private set; } = null!;
    public InputAction LockOn { get; private set; } = null!;
    public InputAction TargetSwitch { get; private set; } = null!;
    public InputAction Skill1 { get; private set; } = null!;
    public InputAction Skill2 { get; private set; } = null!;
    public InputAction Ultimate { get; private set; } = null!;
    public InputAction Pause { get; private set; } = null!;

    private InputActionMap actionMap;

    private void Awake()
    {
        CreateActions();
    }

    private void OnEnable()
    {
        actionMap?.Enable();
    }

    private void OnDisable()
    {
        actionMap?.Disable();
    }

    private void OnDestroy()
    {
        if (actionMap == null)
        {
            return;
        }

        actionMap.Disable();
        actionMap.Dispose();
    }

    private void CreateActions()
    {
        actionMap = new InputActionMap("Gameplay");

        Move = actionMap.AddAction("Move", InputActionType.Value, expectedControlType: "Vector2");
        Move.AddBinding("<Gamepad>/leftStick");
        Move.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        Look = actionMap.AddAction("Look", InputActionType.Value, expectedControlType: "Vector2");
        Look.AddBinding("<Gamepad>/rightStick");

        MouseLook = actionMap.AddAction("MouseLook", InputActionType.Value, expectedControlType: "Vector2");
        MouseLook.AddBinding("<Mouse>/delta");

        NormalAttack = CreateButtonAction("NormalAttack", "<Gamepad>/rightTrigger", "<Mouse>/leftButton");
        Dodge = CreateButtonAction("Dodge", "<Gamepad>/rightShoulder", "<Keyboard>/leftShift");
        Jump = CreateButtonAction("Jump", "<Gamepad>/buttonSouth", "<Keyboard>/space");
        LockOn = CreateButtonAction("LockOn", "<Gamepad>/leftTrigger", "<Keyboard>/tab");
        TargetSwitch = CreateButtonAction("TargetSwitch", "<Gamepad>/rightStickPress", "<Keyboard>/t");
        Skill1 = CreateButtonAction("Skill1", "<Gamepad>/buttonWest", "<Keyboard>/q");
        Skill2 = CreateButtonAction("Skill2", "<Gamepad>/buttonNorth", "<Keyboard>/e");
        Ultimate = CreateButtonAction("Ultimate", "<Gamepad>/leftShoulder", "<Keyboard>/r");
        Pause = CreateButtonAction("Pause", "<Gamepad>/start", "<Keyboard>/escape");
    }

    private InputAction CreateButtonAction(string actionName, string gamepadPath, string keyboardPath)
    {
        InputAction action = actionMap.AddAction(actionName, InputActionType.Button);
        action.AddBinding(gamepadPath);
        action.AddBinding(keyboardPath);
        return action;
    }
}
