using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[DisallowMultipleComponent]
public sealed class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 7.5f;
    [SerializeField] private float acceleration = 38f;
    [SerializeField] private float deceleration = 48f;
    [SerializeField] private float rotationSpeed = 24f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -22f;

    private CharacterController characterController;
    private GameInput input;
    private ThirdPersonCameraController cameraController;
    private TargetingSystem targetingSystem;
    private DodgeController dodgeController;
    private Vector3 horizontalVelocity;
    private float verticalVelocity;

    public bool IsGrounded => characterController != null && characterController.isGrounded;\n    public float CurrentSpeed => new Vector3(horizontalVelocity.x, 0f, horizontalVelocity.z).magnitude;\n    public float MaxMoveSpeed => moveSpeed;\n    public Vector3 MovementVelocity => horizontalVelocity;

    public void Initialize(
        GameInput gameInput,
        ThirdPersonCameraController camera,
        TargetingSystem targeting,
        DodgeController dodge)
    {
        if (gameInput == null)
        {
            throw new System.ArgumentNullException(nameof(gameInput));
        }

        if (camera == null)
        {
            throw new System.ArgumentNullException(nameof(camera));
        }

        if (targeting == null)
        {
            throw new System.ArgumentNullException(nameof(targeting));
        }

        if (dodge == null)
        {
            throw new System.ArgumentNullException(nameof(dodge));
        }

        characterController = GetComponent<CharacterController>();
        input = gameInput;
        cameraController = camera;
        targetingSystem = targeting;
        dodgeController = dodge;
    }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (characterController == null
            || input == null
            || cameraController == null
            || targetingSystem == null
            || dodgeController == null)
        {
            return;
        }

        if (dodgeController.IsDodging)
        {
            return;
        }

        UpdateVerticalVelocity();
        UpdateHorizontalMovement();
        ApplyMovement();
        UpdateRotation();
    }

    private void UpdateVerticalVelocity()
    {
        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        if (input.Jump.WasPressedThisFrame() && characterController.isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity += gravity * Time.deltaTime;
    }

    private void UpdateHorizontalMovement()
    {
        Vector2 movementInput = input.Move.ReadValue<Vector2>();
        Vector3 desiredDirection = cameraController.Right * movementInput.x
            + cameraController.Forward * movementInput.y;

        desiredDirection.y = 0f;

        if (desiredDirection.sqrMagnitude > 1f)
        {
            desiredDirection.Normalize();
        }

        float targetSpeed = desiredDirection.magnitude * moveSpeed;
        Vector3 targetVelocity = desiredDirection.normalized * targetSpeed;

        float rate = targetSpeed > 0.01f ? acceleration : deceleration;
        horizontalVelocity = Vector3.MoveTowards(
            horizontalVelocity,
            targetVelocity,
            rate * Time.deltaTime);
    }

    private void ApplyMovement()
    {
        Vector3 movement = horizontalVelocity;
        movement.y = verticalVelocity;
        characterController.Move(movement * Time.deltaTime);
    }

    private void UpdateRotation()
    {
        Transform lockTarget = targetingSystem.CurrentTarget;

        Vector3 lookDirection;
        if (lockTarget != null)
        {
            lookDirection = lockTarget.position - transform.position;
            lookDirection.y = 0f;
        }
        else
        {
            lookDirection = horizontalVelocity;
            lookDirection.y = 0f;
        }

        if (lookDirection.sqrMagnitude < 0.01f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime);
    }
}
