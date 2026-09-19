using UnityEngine;

[DisallowMultipleComponent]
public sealed class ThirdPersonCameraController : MonoBehaviour
{
    [SerializeField] private float followDistance = 7f;
    [SerializeField] private float followHeight = 2.1f;
    [SerializeField] private float lookHeight = 1.2f;
    [SerializeField] private float lookSensitivity = 160f;
    [SerializeField] private float mouseSensitivity = 0.08f;
    [SerializeField] private float minPitch = -15f;
    [SerializeField] private float maxPitch = 55f;
    [SerializeField] private float positionSmoothTime = 0.08f;
    [SerializeField] private float rotationSmoothSpeed = 18f;
    [SerializeField] private float lockOnRotationSpeed = 8f;
    [SerializeField] private float collisionRadius = 0.2f;
    [SerializeField] private float minimumDistance = 1.5f;

    private Transform target;
    private TargetingSystem targetingSystem;
    private GameInput input;
    private Vector3 positionVelocity;
    private readonly RaycastHit[] cameraCollisionHits = new RaycastHit[16];
    private float yaw;
    private float pitch = 12f;

    public Vector3 Forward
    {
        get
        {
            Vector3 forward = transform.forward;
            forward.y = 0f;
            return forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
        }
    }

    public Vector3 Right
    {
        get
        {
            Vector3 right = transform.right;
            right.y = 0f;
            return right.sqrMagnitude > 0.0001f ? right.normalized : Vector3.right;
        }
    }

    public void Initialize(Transform followTarget, GameInput gameInput, TargetingSystem targeting)
    {
        if (followTarget == null)
        {
            throw new System.ArgumentNullException(nameof(followTarget));
        }

        if (gameInput == null)
        {
            throw new System.ArgumentNullException(nameof(gameInput));
        }

        if (targeting == null)
        {
            throw new System.ArgumentNullException(nameof(targeting));
        }

        target = followTarget;
        input = gameInput;
        targetingSystem = targeting;

        Vector3 initialDirection = transform.position - (target.position + Vector3.up * lookHeight);
        Vector3 flatDirection = new Vector3(initialDirection.x, 0f, initialDirection.z);

        if (flatDirection.sqrMagnitude > 0.01f)
        {
            yaw = Mathf.Atan2(flatDirection.x, flatDirection.z) * Mathf.Rad2Deg;
        }
    }

    private void LateUpdate()
    {
        if (target == null || input == null || targetingSystem == null)
        {
            return;
        }

        UpdateOrbit();
        FollowTarget();
    }

    private void UpdateOrbit()
    {
        Vector2 gamepadLook = input.Look.ReadValue<Vector2>();
        Vector2 mouseLook = input.MouseLook.ReadValue<Vector2>();
        Vector2 activeLook = gamepadLook.sqrMagnitude > 0.0001f
            ? gamepadLook
            : mouseLook;

        if (activeLook.sqrMagnitude > 0.0001f)
        {
            float sensitivity = gamepadLook.sqrMagnitude > 0.0001f
                ? lookSensitivity
                : mouseSensitivity;

            yaw += activeLook.x * sensitivity * Time.deltaTime;
            pitch -= activeLook.y * sensitivity * 0.55f * Time.deltaTime;
        }

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Transform lockTarget = targetingSystem.CurrentTarget;
        if (lockTarget != null && activeLook.sqrMagnitude < 0.01f)
        {
            Vector3 direction = lockTarget.position - target.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.01f)
            {
                float targetYaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                yaw = Mathf.LerpAngle(yaw, targetYaw, lockOnRotationSpeed * Time.deltaTime);
            }
        }
    }

    private void FollowTarget()
    {
        Quaternion orbitRotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 orbitDirection = orbitRotation * Vector3.back;
        Vector3 focusPoint = target.position + Vector3.up * lookHeight;

        Transform lockTarget = targetingSystem.CurrentTarget;
        if (lockTarget != null)
        {
            Vector3 lockPoint = lockTarget.position + Vector3.up * lookHeight;
            focusPoint = Vector3.Lerp(focusPoint, lockPoint, 0.42f);
        }

        Vector3 desiredPosition = target.position
            + orbitDirection * followDistance
            + Vector3.up * followHeight;

        Vector3 resolvedPosition = ResolveCameraCollision(focusPoint, desiredPosition);

        transform.position = Vector3.SmoothDamp(
            transform.position,
            resolvedPosition,
            ref positionVelocity,
            positionSmoothTime);

        Vector3 lookDirection = focusPoint - transform.position;
        if (lookDirection.sqrMagnitude < 0.0001f)
        {
            return;
        }

        Quaternion desiredRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            desiredRotation,
            rotationSmoothSpeed * Time.deltaTime);
    }

    private Vector3 ResolveCameraCollision(Vector3 focusPoint, Vector3 desiredPosition)
    {
        Vector3 direction = desiredPosition - focusPoint;
        float distance = direction.magnitude;

        if (distance <= minimumDistance)
        {
            return desiredPosition;
        }

        direction /= distance;

        int hitCount = Physics.SphereCastNonAlloc(
            focusPoint,
            collisionRadius,
            direction,
            cameraCollisionHits,
            distance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);

        float nearestDistance = distance;
        bool hasCollision = false;

        for (int i = 0; i < hitCount; i++)
        {
            Collider collider = cameraCollisionHits[i].collider;
            if (collider == null || IsOwnedByTarget(collider.transform))
            {
                continue;
            }

            float hitDistance = cameraCollisionHits[i].distance;
            if (hitDistance < nearestDistance)
            {
                nearestDistance = hitDistance;
                hasCollision = true;
            }
        }

        if (!hasCollision)
        {
            return desiredPosition;
        }

        float safeDistance = Mathf.Max(
            minimumDistance,
            nearestDistance - collisionRadius);

        return focusPoint + direction * safeDistance;
    }

    private bool IsOwnedByTarget(Transform hitTransform)
    {
        return hitTransform == target || hitTransform.IsChildOf(target);
    }
}
