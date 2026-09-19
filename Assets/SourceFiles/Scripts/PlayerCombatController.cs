using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerCombatController : MonoBehaviour
{
    [SerializeField] private float attackDuration = 0.32f;
    [SerializeField] private float comboWindow = 0.48f;
    [SerializeField] private float hitStartNormalizedTime = 0.34f;
    [SerializeField] private float hitEndNormalizedTime = 0.52f;
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
    private int comboStep;
    private bool queuedAttack;
    private bool hasHitThisAttack;

    public bool IsAttacking { get; private set; }
    public int ComboStep => comboStep;

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
        weaponVisual =
            new GameObject("EnergyBladeWeapon");

        if (weaponVisual == null)
        {
            throw new System.InvalidOperationException(
                "Failed to create the energy blade weapon root.");
        }

        weaponVisual.transform.SetParent(
            transform,
            false);

        weaponVisual.transform.localPosition =
            new Vector3(
                0.58f,
                -0.02f,
                0.28f);

        weaponVisual.transform.localRotation =
            Quaternion.Euler(
                0f,
                -22f,
                15f);

        GameObject blade =
            GameObject.CreatePrimitive(
                PrimitiveType.Cube);

        if (blade == null)
        {
            throw new System.InvalidOperationException(
                "Failed to create the energy blade.");
        }

        blade.name = "EnergyBlade";
        blade.transform.SetParent(
            weaponVisual.transform,
            false);

        blade.transform.localPosition =
            new Vector3(
                0f,
                0f,
                0.82f);

        blade.transform.localScale =
            new Vector3(
                0.1f,
                0.08f,
                1.55f);

        Collider bladeCollider =
            blade.GetComponent<Collider>();

        if (bladeCollider != null)
        {
            Destroy(bladeCollider);
        }

        GameObject guard =
            GameObject.CreatePrimitive(
                PrimitiveType.Cube);

        if (guard == null)
        {
            throw new System.InvalidOperationException(
                "Failed to create the energy blade guard.");
        }

        guard.name = "EnergyBladeGuard";
        guard.transform.SetParent(
            weaponVisual.transform,
            false);

        guard.transform.localPosition =
            new Vector3(
                0f,
                0f,
                0.08f);

        guard.transform.localScale =
            new Vector3(
                0.34f,
                0.06f,
                0.09f);

        Collider guardCollider =
            guard.GetComponent<Collider>();

        if (guardCollider != null)
        {
            Destroy(guardCollider);
        }

        Shader shader =
            Shader.Find(
                "Universal Render Pipeline/Lit")
            ?? Shader.Find("Standard");

        if (shader == null)
        {
            throw new System.InvalidOperationException(
                "No compatible Unity material shader was found.");
        }

        weaponMaterial =
            new Material(shader)
            {
                color =
                    new Color(
                        0.18f,
                        0.92f,
                        1f)
            };

        Renderer bladeRenderer =
            blade.GetComponent<Renderer>();

        if (bladeRenderer == null)
        {
            throw new System.InvalidOperationException(
                "Energy blade renderer was not created.");
        }

        bladeRenderer.material =
            weaponMaterial;

        ApplyWeaponMaterial(
            guard,
            new Color(
                0.72f,
                0.55f,
                0.18f));

        weaponVisual.SetActive(false);
    }

    private static void ApplyWeaponMaterial(
        GameObject target,
        Color color)
    {
        Renderer renderer =
            target.GetComponent<Renderer>();

        if (renderer == null)
        {
            return;
        }

        Shader shader =
            Shader.Find(
                "Universal Render Pipeline/Lit")
            ?? Shader.Find("Standard");

        if (shader == null)
        {
            return;
        }

        renderer.material =
            new Material(shader)
            {
                color = color
            };
    }

    private void AnimateWeapon(float normalizedTime)
    {
        if (weaponVisual == null)
        {
            return;
        }

        float angle =
            Mathf.Lerp(
                -72f,
                92f,
                normalizedTime);

        float lift =
            Mathf.Sin(
                normalizedTime * Mathf.PI)
            * 0.08f;

        weaponVisual.transform.localPosition =
            new Vector3(
                0.58f,
                -0.02f + lift,
                0.28f);

        weaponVisual.transform.localRotation =
            Quaternion.Euler(
                -8f,
                angle,
                12f);
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
    }
}
