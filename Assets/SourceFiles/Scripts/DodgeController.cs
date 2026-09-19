using UnityEngine;

[DisallowMultipleComponent]
public sealed class DodgeController : MonoBehaviour
{
    [SerializeField] private float dodgeDistance = 4f;
    [SerializeField] private float dodgeDuration = 0.22f;
    [SerializeField] private float dodgeCooldown = 0.35f;

    private CharacterController characterController;
    private GameInput input;
    private ThirdPersonCameraController cameraController;
    private float cooldownTimer;
    private float remainingTime;
    private Vector3 dodgeDirection;

    public bool IsDodging { get; private set; }
    public bool IsInvulnerable => IsDodging;

    public void Initialize(
        CharacterController controller,
        GameInput gameInput,
        ThirdPersonCameraController camera)
    {
        if (controller == null)
        {
            throw new System.ArgumentNullException(nameof(controller));
        }

        if (gameInput == null)
        {
            throw new System.ArgumentNullException(nameof(gameInput));
        }

        if (camera == null)
        {
            throw new System.ArgumentNullException(nameof(camera));
        }

        characterController = controller;
        input = gameInput;
        cameraController = camera;
    }

    private void Update()
    {
        if (input == null || characterController == null || cameraController == null)
        {
            return;
        }

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }

        if (IsDodging)
        {
            UpdateDodge();
            return;
        }

        if (input.Dodge.WasPressedThisFrame()
            && cooldownTimer <= 0f
            && characterController.isGrounded)
        {
            StartDodge();
        }
    }

    private void StartDodge()
    {
        Vector2 moveInput = input.Move.ReadValue<Vector2>();
        Vector3 direction = cameraController.Right * moveInput.x
            + cameraController.Forward * moveInput.y;

        if (direction.sqrMagnitude < 0.01f)
        {
            direction = -cameraController.Forward;
        }

        direction.y = 0f;
        dodgeDirection = direction.normalized;

        remainingTime = dodgeDuration;
        IsDodging = true;
        cooldownTimer = dodgeCooldown;
    }

    private void UpdateDodge()
    {
        if (remainingTime <= 0f)
        {
            IsDodging = false;
            return;
        }

        float frameDistance = dodgeDistance / Mathf.Max(0.01f, dodgeDuration) * Time.deltaTime;
        characterController.Move(dodgeDirection * frameDistance);

        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f)
        {
            IsDodging = false;
        }
    }
}
