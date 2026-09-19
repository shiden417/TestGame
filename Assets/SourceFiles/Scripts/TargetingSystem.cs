using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class TargetingSystem : MonoBehaviour
{
    [SerializeField] private float lockOnRange = 18f;

    private GameInput input;
    private ThirdPersonCameraController cameraController;
    private Targetable currentTarget;

    public Transform CurrentTarget => currentTarget != null && currentTarget.IsActive
        ? currentTarget.AimPoint
        : null;

    public Targetable CurrentTargetable => currentTarget != null && currentTarget.IsActive
        ? currentTarget
        : null;

    public bool IsLockedOn => CurrentTarget != null;

    public void Initialize(GameInput gameInput, ThirdPersonCameraController camera)
    {
        if (gameInput == null)
        {
            throw new System.ArgumentNullException(nameof(gameInput));
        }

        if (camera == null)
        {
            throw new System.ArgumentNullException(nameof(camera));
        }

        input = gameInput;
        cameraController = camera;
    }

    private void Update()
    {
        if (input == null || cameraController == null)
        {
            return;
        }

        if (currentTarget != null && !currentTarget.IsActive)
        {
            ClearTarget();
        }

        if (input.LockOn.WasPressedThisFrame())
        {
            if (IsLockedOn)
            {
                ClearTarget();
            }
            else
            {
                AcquireClosestTarget();
            }
        }

        if (IsLockedOn && input.TargetSwitch.WasPressedThisFrame())
        {
            SwitchTarget();
        }
    }

    private void AcquireClosestTarget()
    {
        Targetable[] candidates = GetCandidates();
        if (candidates.Length == 0)
        {
            return;
        }

        Vector3 cameraForward = cameraController.Forward;
        Targetable bestTarget = null;
        float bestScore = float.MaxValue;

        foreach (Targetable candidate in candidates)
        {
            if (candidate == null || !candidate.IsActive)
            {
                continue;
            }

            Vector3 offset = candidate.AimPoint.position - transform.position;
            float distance = offset.magnitude;
            if (distance <= 0.001f)
            {
                continue;
            }

            float angle = Vector3.Angle(cameraForward, offset.normalized);
            float score = distance + angle * 0.12f;

            if (score < bestScore)
            {
                bestScore = score;
                bestTarget = candidate;
            }
        }

        SetTarget(bestTarget);
    }

    private void SwitchTarget()
    {
        Targetable[] candidates = GetCandidates();
        if (candidates.Length < 2 || currentTarget == null)
        {
            return;
        }

        List<Targetable> ordered = new List<Targetable>(candidates);
        ordered.Sort((left, right) =>
        {
            float leftAngle = GetScreenAngle(left);
            float rightAngle = GetScreenAngle(right);
            return leftAngle.CompareTo(rightAngle);
        });

        int currentIndex = ordered.IndexOf(currentTarget);
        if (currentIndex < 0)
        {
            SetTarget(ordered[0]);
            return;
        }

        int nextIndex = (currentIndex + 1) % ordered.Count;
        SetTarget(ordered[nextIndex]);
    }

    private float GetScreenAngle(Targetable targetable)
    {
        Vector3 direction = targetable.AimPoint.position - transform.position;
        if (direction.sqrMagnitude < 0.0001f)
        {
            return 0f;
        }

        return Vector3.SignedAngle(cameraController.Forward, direction.normalized, Vector3.up);
    }

    private Targetable[] GetCandidates()
    {
        Collider[] colliders = Physics.OverlapSphere(
            transform.position,
            lockOnRange,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);

        HashSet<Targetable> uniqueTargets = new HashSet<Targetable>();

        foreach (Collider collider in colliders)
        {
            if (collider == null)
            {
                continue;
            }

            Targetable targetable = collider.GetComponentInParent<Targetable>();
            if (targetable != null && targetable.IsActive)
            {
                uniqueTargets.Add(targetable);
            }
        }

        Targetable[] result = new Targetable[uniqueTargets.Count];
        uniqueTargets.CopyTo(result);
        return result;
    }

    private void SetTarget(Targetable newTarget)
    {
        if (currentTarget == newTarget)
        {
            return;
        }

        if (currentTarget != null)
        {
            currentTarget.SetLocked(false);
        }

        currentTarget = newTarget;

        if (currentTarget != null)
        {
            currentTarget.SetLocked(true);
        }
    }

    private void ClearTarget()
    {
        if (currentTarget != null)
        {
            currentTarget.SetLocked(false);
        }

        currentTarget = null;
    }

    private void OnDisable()
    {
        ClearTarget();
    }
}
