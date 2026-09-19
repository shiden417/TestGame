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

    private GameInput input;
    private TargetingSystem targetingSystem;
    private DodgeController dodgeController;
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
        DodgeController dodge)
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

        input = gameInput;
        targetingSystem = targeting;
        dodgeController = dodge;
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

        int nextStep = comboTimer > 0f
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

        if (weaponVisual != null)
        {
            weaponVisual.SetActive(true);
        }
    }

    private void UpdateAttack()
    {
        attackTimer += Time.deltaTime;

        float normalized = Mathf.Clamp01(attackTimer / Mathf.Max(0.001f, attackDuration));
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
        int nextStep = Mathf.Clamp(comboStep + 1, 1, 3);

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

        if (targetingSystem != null && targetingSystem.CurrentTarget != null)
        {
            direction = targetingSystem.CurrentTarget.position - transform.position;
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

        Vector3 center = transform.position
            + Vector3.up * 1.1f
            + direction * (attackRange * 0.55f);

        Collider[] colliders = Physics.OverlapSphere(
            center,
            attackRadius,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);

        HashSet<CombatTarget> uniqueTargets = new HashSet<CombatTarget>();

        foreach (Collider collider in colliders)
        {
            if (collider == null)
            {
                continue;
            }

            CombatTarget target = collider.GetComponentInParent<CombatTarget>();
            if (target == null || !target.IsAlive || !uniqueTargets.Add(target))
            {
                continue;
            }

            float damageMultiplier = comboStep switch
            {
                1 => 1f,
                2 => 1.15f,
                3 => 1.4f,
                _ => 1f
            };

            target.TakeDamage(baseDamage * damageMultiplier);
        }
    }

    private void CreateWeaponVisual()
    {
        weaponVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        weaponVisual.name = "PrototypeEnergyBlade";
        weaponVisual.transform.SetParent(transform, false);
        weaponVisual.transform.localPosition = new Vector3(0f, 1.05f, 0.85f);
        weaponVisual.transform.localScale = new Vector3(0.11f, 0.12f, 1.55f);

        Collider collider = weaponVisual.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        Renderer renderer = weaponVisual.GetComponent<Renderer>();
        if (renderer == null)
        {
            throw new System.InvalidOperationException("Failed to create the prototype weapon renderer.");
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit")
            ?? Shader.Find("Standard");

        if (shader == null)
        {
            throw new System.InvalidOperationException("No compatible Unity material shader was found.");
        }

        weaponMaterial = new Material(shader)
        {
            color = new Color(0.2f, 0.95f, 1f)
        };

        renderer.material = weaponMaterial;
        weaponVisual.SetActive(false);
    }

    private void AnimateWeapon(float normalizedTime)
    {
        if (weaponVisual == null)
        {
            return;
        }

        float angle = Mathf.Lerp(-75f, 75f, normalizedTime);
        float lift = Mathf.Sin(normalizedTime * Mathf.PI) * 0.12f;

        weaponVisual.transform.localPosition = new Vector3(
            0f,
            1.05f + lift,
            0.85f);

        weaponVisual.transform.localRotation = Quaternion.Euler(
            15f,
            angle,
            35f - angle * 0.45f);
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
