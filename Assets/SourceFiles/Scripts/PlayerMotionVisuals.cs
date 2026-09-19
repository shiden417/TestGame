using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerMotionVisuals : MonoBehaviour
{
    private PlayerController playerController;
    private DodgeController dodgeController;
    private PlayerCombatController combatController;

    private Transform visualRoot;
    private Transform head;
    private Transform torsoPlate;
    private Transform leftShoulder;
    private Transform rightShoulder;
    private Transform leftUpperArm;
    private Transform rightUpperArm;
    private Transform leftThigh;
    private Transform rightThigh;
    private Transform backCore;
    private Transform leftEnergyLine;
    private Transform rightEnergyLine;
    private Transform centerEnergyLine;

    private Vector3 baseRootPosition;
    private Quaternion baseRootRotation;

    private Quaternion baseHeadRotation;
    private Quaternion baseTorsoRotation;
    private Quaternion baseLeftShoulderRotation;
    private Quaternion baseRightShoulderRotation;
    private Quaternion baseLeftArmRotation;
    private Quaternion baseRightArmRotation;
    private Quaternion baseLeftThighRotation;
    private Quaternion baseRightThighRotation;

    private Vector3 baseBackCoreScale;
    private Vector3 baseLeftEnergyScale;
    private Vector3 baseRightEnergyScale;
    private Vector3 baseCenterEnergyScale;

    public void Initialize(
        PlayerController controller,
        DodgeController dodge,
        PlayerCombatController combat)
    {
        if (controller == null)
        {
            throw new System.ArgumentNullException(nameof(controller));
        }

        if (dodge == null)
        {
            throw new System.ArgumentNullException(nameof(dodge));
        }

        if (combat == null)
        {
            throw new System.ArgumentNullException(nameof(combat));
        }

        playerController = controller;
        dodgeController = dodge;
        combatController = combat;

        visualRoot = transform.Find("PlayerVisual");
        if (visualRoot == null)
        {
            throw new System.InvalidOperationException(
                "PlayerVisual root was not created.");
        }

        head = visualRoot.Find("Head");
        torsoPlate = visualRoot.Find("TorsoPlate");
        leftShoulder = visualRoot.Find("LeftShoulderArmor");
        rightShoulder = visualRoot.Find("RightShoulderArmor");
        leftUpperArm = visualRoot.Find("LeftUpperArm");
        rightUpperArm = visualRoot.Find("RightUpperArm");
        leftThigh = visualRoot.Find("LeftThighArmor");
        rightThigh = visualRoot.Find("RightThighArmor");
        backCore = visualRoot.Find("BackEnergyCore");
        leftEnergyLine = visualRoot.Find("LeftEnergyLine");
        rightEnergyLine = visualRoot.Find("RightEnergyLine");
        centerEnergyLine = visualRoot.Find("CenterEnergyLine");

        CaptureBasePose();
    }

    private void CaptureBasePose()
    {
        baseRootPosition = visualRoot.localPosition;
        baseRootRotation = visualRoot.localRotation;

        baseHeadRotation = GetRotation(head);
        baseTorsoRotation = GetRotation(torsoPlate);
        baseLeftShoulderRotation = GetRotation(leftShoulder);
        baseRightShoulderRotation = GetRotation(rightShoulder);
        baseLeftArmRotation = GetRotation(leftUpperArm);
        baseRightArmRotation = GetRotation(rightUpperArm);
        baseLeftThighRotation = GetRotation(leftThigh);
        baseRightThighRotation = GetRotation(rightThigh);

        baseBackCoreScale = GetScale(backCore);
        baseLeftEnergyScale = GetScale(leftEnergyLine);
        baseRightEnergyScale = GetScale(rightEnergyLine);
        baseCenterEnergyScale = GetScale(centerEnergyLine);
    }

    private static Quaternion GetRotation(Transform part)
    {
        return part != null ? part.localRotation : Quaternion.identity;
    }

    private static Vector3 GetScale(Transform part)
    {
        return part != null ? part.localScale : Vector3.one;
    }

    private void Update()
    {
        if (visualRoot == null
            || playerController == null
            || dodgeController == null
            || combatController == null)
        {
            return;
        }

        float speed01 = Mathf.Clamp01(
            playerController.CurrentSpeed / Mathf.Max(
                0.01f,
                playerController.MaxMoveSpeed));

        float time = Time.unscaledTime;
        float gait = time * Mathf.Lerp(5f, 9.5f, speed01);
        float moveBob = Mathf.Sin(gait * 1.55f) * 0.022f * speed01;
        float sideSway = Mathf.Sin(gait) * 0.012f * speed01;

        float attackBlend = combatController.IsAttacking
            ? Mathf.Sin(Mathf.PI * combatController.AttackNormalizedTime)
            : 0f;

        float dodgeBlend = dodgeController.IsDodging
            ? Mathf.Sin(Mathf.PI * dodgeController.DodgeProgress)
            : 0f;

        Vector3 rootPosition = baseRootPosition;
        rootPosition.y += moveBob;
        rootPosition.x += sideSway;

        Vector3 localDodge = transform.InverseTransformDirection(
            dodgeController.DodgeDirection);
        localDodge.y = 0f;

        float dodgeLeanX = -localDodge.z * 16f * dodgeBlend;
        float dodgeLeanZ = -localDodge.x * 18f * dodgeBlend;
        float attackLeanX = -12f * attackBlend;
        float attackLeanZ = GetAttackSide() * 10f * attackBlend;

        visualRoot.localPosition = rootPosition;
        visualRoot.localRotation =
            baseRootRotation
            * Quaternion.Euler(
                dodgeLeanX + attackLeanX - 4f * speed01,
                sideSway * 10f,
                dodgeLeanZ + attackLeanZ);

        AnimatePartRotations(gait, speed01, attackBlend, dodgeBlend);
        AnimateEnergy(time, speed01, attackBlend, dodgeBlend);
    }

    private int GetAttackSide()
    {
        return combatController.ComboStep == 2 ? -1 : 1;
    }

    private void AnimatePartRotations(
        float gait,
        float speed01,
        float attackBlend,
        float dodgeBlend)
    {
        float legSwing = Mathf.Sin(gait) * 18f * speed01;
        float armSwing = Mathf.Sin(gait + Mathf.PI) * 12f * speed01;

        SetRotation(
            leftThigh,
            baseLeftThighRotation
            * Quaternion.Euler(legSwing, 0f, 0f));

        SetRotation(
            rightThigh,
            baseRightThighRotation
            * Quaternion.Euler(-legSwing, 0f, 0f));

        SetRotation(
            leftUpperArm,
            baseLeftArmRotation
            * Quaternion.Euler(armSwing, 0f, 0f));

        SetRotation(
            rightUpperArm,
            baseRightArmRotation
            * Quaternion.Euler(-armSwing, 0f, 0f));

        float shoulderLift = Mathf.Sin(gait * 0.5f) * 2.5f * speed01;

        SetRotation(
            leftShoulder,
            baseLeftShoulderRotation
            * Quaternion.Euler(0f, 0f, -shoulderLift));

        SetRotation(
            rightShoulder,
            baseRightShoulderRotation
            * Quaternion.Euler(0f, 0f, shoulderLift));

        float headCounter = -sideSwayDegrees() * 0.7f;

        SetRotation(
            head,
            baseHeadRotation
            * Quaternion.Euler(
                attackBlend * -5f,
                0f,
                headCounter + dodgeBlend * 4f));

        SetRotation(
            torsoPlate,
            baseTorsoRotation
            * Quaternion.Euler(
                -speed01 * 3f - attackBlend * 6f,
                0f,
                attackBlend * GetAttackSide() * 8f));
    }

    private float sideSwayDegrees()
    {
        return Mathf.Sin(Time.unscaledTime * 7f) * 2f;
    }

    private void AnimateEnergy(
        float time,
        float speed01,
        float attackBlend,
        float dodgeBlend)
    {
        float pulse =
            1f
            + Mathf.Sin(time * 8f) * 0.055f
            + attackBlend * 0.13f
            + dodgeBlend * 0.1f;

        SetScale(
            backCore,
            baseBackCoreScale * pulse);

        SetScale(
            leftEnergyLine,
            baseLeftEnergyScale * (1f + attackBlend * 0.1f));

        SetScale(
            rightEnergyLine,
            baseRightEnergyScale * (1f + speed01 * 0.06f));

        SetScale(
            centerEnergyLine,
            baseCenterEnergyScale
            * (1f + Mathf.Sin(time * 10f) * 0.04f + dodgeBlend * 0.12f));
    }

    private static void SetRotation(Transform part, Quaternion rotation)
    {
        if (part != null)
        {
            part.localRotation = rotation;
        }
    }

    private static void SetScale(Transform part, Vector3 scale)
    {
        if (part != null)
        {
            part.localScale = scale;
        }
    }
}
