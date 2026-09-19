using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerCombatController : MonoBehaviour
{
    [SerializeField] private float attackDuration = 0.29f;
    [SerializeField] private float comboWindow = 0.52f;
    [SerializeField] private float hitStartNormalizedTime = 0.3f;
    [SerializeField] private float hitEndNormalizedTime = 0.58f;
    [SerializeField] private float attackRange = 2.2f;
    [SerializeField] private float attackRadius = 0.9f;
    [SerializeField] private float baseDamage = 34f;
    [SerializeField] private float counterDamageMultiplier = 2f;
    [SerializeField] private float ultimateGainPerHit = 4f;

    private GameInput input;
    private TargetingSystem targetingSystem;
    private DodgeController dodgeController;
    private PerfectDodgeSystem perfectDodgeSystem;
    private PlayerSkillController skillController;
    private GameObject weaponVisual;
    private Material weaponMaterial;
    private float attackTimer;
    private float comboTimer;
    private GameObject bladeGlow;
    private Material bladeGlowMaterial;
    private int comboStep;
    private bool queuedAttack;
    private bool hasHitThisAttack;

    public bool IsAttacking { get; private set; }
    public int ComboStep => comboStep;
    public float AttackNormalizedTime => IsAttacking
        ? Mathf.Clamp01(attackTimer / Mathf.Max(0.001f, attackDuration))
        : 0f;

    public void Initialize(
        GameInput gameInput,
        TargetingSystem targeting,
        DodgeController dodge,
        PerfectDodgeSystem perfectDodge,
        PlayerSkillController skills)
    {
        if (gameInput == null)
        {
            throw new System.ArgumentNullException(nameof(gameInput));
        }

        if (targeting == null)
        {
            throw new System.ArgumentNullException(nameof(targeting));
        }

        if (dodge == null)
        {
            throw new System.ArgumentNullException(nameof(dodge));
        }

        if (perfectDodge == null)
        {
            throw new System.ArgumentNullException(nameof(perfectDodge));
        }

        if (skills == null)
        {
            throw new System.ArgumentNullException(nameof(skills));
        }

        input = gameInput;
        targetingSystem = targeting;
        dodgeController = dodge;
        perfectDodgeSystem = perfectDodge;
        skillController = skills;

        CreateWeaponVisual();
    }

    private void Update()
    {
        if (input == null || dodgeController == null)
        {
            return;
        }

        if (dodgeController.IsDodging)
        {
            CancelAttack();
            return;
        }

        if (input.Pause.WasPressedThisFrame())
        {
            return;
        }

        HandleAttackInput();

        if (IsAttacking)
        {
            UpdateAttack();
        }
        else if (comboTimer > 0f)
        {
            comboTimer -= Time.deltaTime;

            if (comboTimer <= 0f)
            {
                comboStep = 0;
            }
        }
    }

    private void HandleAttackInput()
    {
        if (!input.NormalAttack.WasPressedThisFrame())
        {
            return;
        }

        if (IsAttacking)
        {
            queuedAttack = true;
            return;
        }

        int nextStep =
            comboTimer > 0f
                ? Mathf.Clamp(comboStep + 1, 1, 3)
                : 1;

        StartAttack(nextStep);
    }

    private void StartAttack(int step)
    {
        comboStep = step;
        comboTimer = comboWindow;
        attackTimer = 0f;
        hasHitThisAttack = false;
        queuedAttack = false;
        IsAttacking = true;

        AudioDirector.Instance?.PlayAttack();

        if (weaponVisual != null)
        {
            weaponVisual.SetActive(true);
        }
    }

    private void UpdateAttack()
    {
        attackTimer += Time.deltaTime;

        float normalized =
            Mathf.Clamp01(
                attackTimer
                / Mathf.Max(0.001f, attackDuration));

        AnimateWeapon(normalized);

        if (!hasHitThisAttack
            && normalized >= hitStartNormalizedTime
            && normalized <= hitEndNormalizedTime)
        {
            PerformHit();
            hasHitThisAttack = true;
        }

        if (attackTimer < attackDuration)
        {
            return;
        }

        bool shouldContinueCombo = queuedAttack;
        int nextStep =
            Mathf.Clamp(
                comboStep + 1,
                1,
                3);

        IsAttacking = false;

        if (weaponVisual != null)
        {
            weaponVisual.SetActive(false);
        }

        if (shouldContinueCombo)
        {
            StartAttack(nextStep);
        }
    }

    public void CancelAttack()
    {
        IsAttacking = false;
        queuedAttack = false;
        attackTimer = 0f;
        hasHitThisAttack = false;

        if (weaponVisual != null)
        {
            weaponVisual.SetActive(false);
        }
    }

    private void PerformHit()
    {
        Vector3 direction = transform.forward;

        if (targetingSystem != null
            && targetingSystem.CurrentTarget != null)
        {
            direction =
                targetingSystem.CurrentTarget.position
                - transform.position;

            direction.y = 0f;

            if (direction.sqrMagnitude > 0.01f)
            {
                direction.Normalize();
            }
            else
            {
                direction = transform.forward;
            }
        }

        Vector3 center =
            transform.position
            + Vector3.up * 1.1f
            + direction * (attackRange * 0.55f);

        Collider[] colliders =
            Physics.OverlapSphere(
                center,
                attackRadius,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);

        HashSet<EnemyController> uniqueTargets =
            new HashSet<EnemyController>();

        foreach (Collider collider in colliders)
        {
            if (collider == null)
            {
                continue;
            }

            EnemyController target =
                collider.GetComponentInParent<EnemyController>();

            if (target == null
                || !target.IsAlive
                || !uniqueTargets.Add(target))
            {
                continue;
            }

            float damageMultiplier =
                comboStep switch
                {
                    1 => 1f,
                    2 => 1.15f,
                    3 => 1.4f,
                    _ => 1f
                };

            if (perfectDodgeSystem != null
                && perfectDodgeSystem.CounterWindowActive)
            {
                damageMultiplier *=
                    counterDamageMultiplier;
            }

            target.TakeDamage(
                baseDamage * damageMultiplier,
                direction,
                true);

            skillController?.AddUltimateGauge(
                ultimateGainPerHit);

            AudioDirector.Instance?.PlayHit();
        }
    }

    private void CreateWeaponVisual()
    {
        weaponVisual = new GameObject("EnergyBladeWeapon");

        if (weaponVisual == null)
        {
            throw new System.InvalidOperationException(
                "Failed to create the energy blade weapon root.");
        }

        weaponVisual.transform.SetParent(transform, false);
        weaponVisual.transform.localPosition = new Vector3(0.58f, -0.02f, 0.18f);
        weaponVisual.transform.localRotation = Quaternion.Euler(-8f, -20f, 12f);

        Shader shader =
            Shader.Find("Universal Render Pipeline/Lit")
            ?? Shader.Find("Standard");

        if (shader == null)
        {
            throw new System.InvalidOperationException(
                "No compatible Unity material shader was found.");
        }

        GameObject handle = CreateWeaponPart(
            "WeaponHandle",
            weaponVisual.transform,
            new Vector3(0f, 0f, -0.13f),
            new Vector3(0.09f, 0.09f, 0.3f));

        ApplyWeaponMaterial(handle, new Color(0.08f, 0.09f, 0.12f));

        GameObject bladeOuter = CreateWeaponPart(
            "EnergyBladeGlow",
            weaponVisual.transform,
            new Vector3(0f, 0f, 0.76f),
            new Vector3(0.12f, 0.10f, 1.72f));

        bladeGlowMaterial = new Material(shader)
        {
            color = new Color(0.02f, 0.34f, 0.58f)
        };
        ConfigureMaterial(bladeGlowMaterial, true);
        bladeOuter.GetComponent<Renderer>().material = bladeGlowMaterial;
        bladeGlow = bladeOuter;

        GameObject blade = CreateWeaponPart(
            "EnergyBlade",
            weaponVisual.transform,
            new Vector3(0f, 0f, 0.78f),
            new Vector3(0.055f, 0.055f, 1.68f));

        weaponMaterial = new Material(shader)
        {
            color = new Color(0.18f, 0.92f, 1f)
        };
        ConfigureMaterial(weaponMaterial, true);
        blade.GetComponent<Renderer>().material = weaponMaterial;

        GameObject bladeEdge = CreateWeaponPart(
            "EnergyBladeEdge",
            weaponVisual.transform,
            new Vector3(0f, 0.02f, 0.88f),
            new Vector3(0.025f, 0.035f, 1.35f));

        ApplyWeaponMaterial(
            bladeEdge,
            new Color(0.55f, 0.98f, 1f));

        GameObject bladeTip = CreateWeaponPart(
            "EnergyBladeTip",
            weaponVisual.transform,
            new Vector3(0f, 0.02f, 1.68f),
            new Vector3(0.07f, 0.07f, 0.22f));

        ApplyWeaponMaterial(
            bladeTip,
            new Color(0.25f, 0.92f, 1f));

        GameObject guard = CreateWeaponPart(
            "EnergyBladeGuard",
            weaponVisual.transform,
            new Vector3(0f, 0f, 0.06f),
            new Vector3(0.38f, 0.055f, 0.075f));

        ApplyWeaponMaterial(guard, new Color(0.72f, 0.55f, 0.18f));

        weaponVisual.SetActive(false);
    }

    private static GameObject CreateWeaponPart(
        string objectName,
        Transform parent,
        Vector3 localPosition,
        Vector3 localScale)
    {
        GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);

        if (part == null)
        {
            throw new System.InvalidOperationException(
                $"Failed to create weapon part: {objectName}");
        }

        part.name = objectName;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = localScale;

        Collider collider = part.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        return part;
    }

    private static void ConfigureMaterial(Material material, bool emission)
    {
        if (material.HasProperty("_Metallic"))
        {
            material.SetFloat("_Metallic", 0.35f);
        }

        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", 0.88f);
        }

        if (emission && material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", material.color * 2f);
        }
    }

    private static void ApplyWeaponMaterial(
        GameObject target,
        Color color)
    {
        Renderer renderer = target.GetComponent<Renderer>();

        if (renderer == null)
        {
            return;
        }

        Shader shader =
            Shader.Find("Universal Render Pipeline/Lit")
            ?? Shader.Find("Standard");

        if (shader == null)
        {
            return;
        }

        Material material = new Material(shader)
        {
            color = color
        };

        ConfigureMaterial(material, false);
        renderer.material = material;
    }

    private void AnimateWeapon(float normalizedTime)
    {
        if (weaponVisual == null)
        {
            return;
        }

        float eased =
            normalizedTime * normalizedTime
            * (3f - 2f * normalizedTime);

        float startAngle;
        float endAngle;
        float startPitch;
        float endPitch;
        float startRoll;
        float endRoll;

        switch (comboStep)
        {
            case 1:
                startAngle = -102f;
                endAngle = 52f;
                startPitch = -18f;
                endPitch = 8f;
                startRoll = 24f;
                endRoll = -10f;
                break;

            case 2:
                startAngle = 72f;
                endAngle = -96f;
                startPitch = 10f;
                endPitch = -14f;
                startRoll = -16f;
                endRoll = 16f;
                break;

            default:
                startAngle = -118f;
                endAngle = 138f;
                startPitch = -8f;
                endPitch = 18f;
                startRoll = 14f;
                endRoll = -28f;
                break;
        }

        float angle = Mathf.Lerp(startAngle, endAngle, eased);
        float pitch = Mathf.Lerp(startPitch, endPitch, eased);
        float roll = Mathf.Lerp(startRoll, endRoll, eased);

        float lunge = Mathf.Sin(normalizedTime * Mathf.PI) * 0.16f;
        float lift = Mathf.Sin(normalizedTime * Mathf.PI) * 0.06f;
        float side = GetAttackSide() * Mathf.Sin(normalizedTime * Mathf.PI) * 0.055f;

        weaponVisual.transform.localPosition =
            new Vector3(
                0.64f + side,
                -0.04f + lift,
                0.24f + lunge);

        weaponVisual.transform.localRotation =
            Quaternion.Euler(pitch, angle, roll);

        if (bladeGlow != null)
        {
            float glowScale =
                1f + Mathf.Sin(normalizedTime * Mathf.PI) * 0.1f;

            bladeGlow.transform.localScale =
                new Vector3(
                    0.16f * glowScale,
                    0.13f * glowScale,
                    1.62f * glowScale);
        }
    }

    private int GetAttackSide()
    {
        return comboStep == 2 ? -1 : 1;
    }

    private void OnDestroy()
    {
        if (weaponVisual != null)
        {
            Destroy(weaponVisual);
        }

        if (weaponMaterial != null)
        {
            Destroy(weaponMaterial);
        }

        if (bladeGlowMaterial != null)
        {
            Destroy(bladeGlowMaterial);
        }
    }
}
