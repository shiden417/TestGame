using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemyController : MonoBehaviour
{
    public enum EnemyType
    {
        Basic,
        Heavy,
        Ranged,
        Elite,
        Boss
    }

    [SerializeField] private float aggroRange = 35f;
    [SerializeField] private float hitStunDuration = 0.15f;

    private EnemyType enemyType;
    private Transform player;
    private PlayerHealth playerHealth;
    private BattleDirector battleDirector;
    private Renderer targetRenderer;
    private Material targetMaterial;
    private Color baseColor;
    private GameObject attackTelegraph;
    private Transform visualRoot;
    private Vector3 visualBaseScale;
    private float visualSeed;

    private float maxHealth;
    private float currentHealth;
    private float moveSpeed;
    private float attackRange;
    private float attackCooldown;
    private float attackDamage;
    private float attackWindup;
    private float stopDistance;
    private float knockbackResistance;

    private float attackTimer;
    private float attackWindupTimer;
    private float hitStunTimer;
    private float knockbackTimer;
    private Vector3 knockbackVelocity;
    private float hitFlashTimer;

    public EnemyType Type => enemyType;
    public float CurrentHealth => currentHealth;
    public bool IsAlive => currentHealth > 0f && gameObject.activeSelf;
    public bool IsBoss => enemyType == EnemyType.Boss;
    public bool IsElite => enemyType == EnemyType.Elite || enemyType == EnemyType.Boss;
    public bool IsAttackCommitmentActive => attackWindupTimer > 0f;

    public void Initialize(
        EnemyType type,
        Transform target,
        BattleDirector battle,
        float health,
        float speed,
        float damage,
        float cooldown)
    {
        if (target == null)
        {
            throw new System.ArgumentNullException(nameof(target));
        }

        if (battle == null)
        {
            throw new System.ArgumentNullException(nameof(battle));
        }

        enemyType = type;
        player = target;
        battleDirector = battle;
        maxHealth = Mathf.Max(1f, health);
        currentHealth = maxHealth;
        moveSpeed = Mathf.Max(0.1f, speed);
        attackDamage = Mathf.Max(0f, damage);
        attackCooldown = Mathf.Max(0.2f, cooldown);
        attackTimer = Random.Range(0f, attackCooldown);

        ApplyTypeTuning();

        visualRoot = transform.Find("EnemyVisual");
        if (visualRoot != null)
        {
            visualBaseScale = visualRoot.localScale;
        }

        visualSeed = Random.Range(0f, 1000f);

        targetRenderer = GetComponentInChildren<Renderer>();

        if (targetRenderer != null)
        {
            targetMaterial = targetRenderer.material;
            baseColor = targetMaterial.color;
            ConfigureMaterial(targetMaterial);
        }
    }

    private void Awake()
    {
        targetRenderer = GetComponentInChildren<Renderer>();

        if (targetRenderer != null)
        {
            targetMaterial = targetRenderer.material;
            baseColor = targetMaterial.color;
        }
    }

    private void Update()
    {
        if (!IsAlive
            || player == null
            || battleDirector == null)
        {
            return;
        }

        bool attackWasCommitted =
            attackWindupTimer > 0f;

        UpdateTimers();

        float distanceToPlayer = GetDistanceToPlayer();
        AnimateVisual(distanceToPlayer);

        if (attackWasCommitted
            && attackWindupTimer <= 0f)
        {
            ExecuteAttack();
            return;
        }

        if (knockbackTimer > 0f)
        {
            transform.position +=
                knockbackVelocity * Time.deltaTime;

            return;
        }

        if (hitStunTimer > 0f)
        {
            return;
        }

        Vector3 offset =
            player.position - transform.position;

        offset.y = 0f;

        float distance =
            offset.magnitude;

        if (distance > aggroRange)
        {
            return;
        }

        if (attackWindupTimer > 0f)
        {
            FaceDirection(offset);
            return;
        }

        if (distance > attackRange)
        {
            float desiredDistance =
                Mathf.Max(
                    stopDistance,
                    attackRange * 0.7f);

            Vector3 moveDirection =
                distance > desiredDistance
                    ? offset.normalized
                    : Vector3.zero;

            transform.position +=
                moveDirection
                * moveSpeed
                * Time.deltaTime;

            FaceDirection(moveDirection);
            return;
        }

        FaceDirection(offset);

        if (attackTimer <= 0f
            && battleDirector.CanEnemyAttack(this))
        {
            attackWindupTimer = attackWindup;
            CreateAttackTelegraph();
        }
    }

    private void UpdateTimers()
    {
        attackTimer =
            Mathf.Max(
                0f,
                attackTimer - Time.deltaTime);

        attackWindupTimer =
            Mathf.Max(
                0f,
                attackWindupTimer - Time.deltaTime);

        hitStunTimer =
            Mathf.Max(
                0f,
                hitStunTimer - Time.deltaTime);

        if (knockbackTimer > 0f)
        {
            knockbackTimer =
                Mathf.Max(
                    0f,
                    knockbackTimer - Time.deltaTime);

            knockbackVelocity =
                Vector3.Lerp(
                    knockbackVelocity,
                    Vector3.zero,
                    14f * Time.deltaTime);
        }

        if (hitFlashTimer > 0f)
        {
            hitFlashTimer =
                Mathf.Max(
                    0f,
                    hitFlashTimer - Time.deltaTime);

            if (targetMaterial != null)
            {
                float normalized =
                    hitFlashTimer / 0.08f;

                targetMaterial.color =
                    Color.Lerp(
                        baseColor,
                        Color.white,
                        normalized);
            }
        }
    }

    private void AnimateVisual(float distanceToPlayer)
    {
        if (visualRoot == null)
        {
            return;
        }

        float movementCycle =
            Time.unscaledTime
            * (4.5f + moveSpeed * 0.7f)
            + visualSeed;

        float moving01 =
            Mathf.Clamp01(
                (distanceToPlayer - stopDistance)
                / 6f);

        float bob =
            Mathf.Abs(Mathf.Sin(movementCycle))
            * 0.045f
            * moving01;

        float attackBlend =
            attackWindupTimer > 0f
                ? Mathf.Sin(
                    Mathf.PI
                    * Mathf.Clamp01(
                        1f
                        - attackWindupTimer
                        / Mathf.Max(0.01f, attackWindup)))
                : 0f;

        float hitBlend =
            hitStunTimer > 0f
                ? Mathf.Clamp01(
                    hitStunTimer
                    / Mathf.Max(0.01f, hitStunDuration))
                : 0f;

        float squash =
            1f
            + attackBlend * 0.1f
            - hitBlend * 0.08f;

        visualRoot.localPosition =
            new Vector3(
                0f,
                bob
                + attackBlend * 0.025f,
                0f);

        visualRoot.localRotation =
            Quaternion.Euler(
                -attackBlend * 9f
                + hitBlend * 5f,
                Mathf.Sin(movementCycle * 0.5f)
                * 3f
                * moving01,
                attackBlend * GetAttackLean() * 7f);

        visualRoot.localScale =
            new Vector3(
                visualBaseScale.x
                * (1f / Mathf.Sqrt(squash)),
                visualBaseScale.y * squash,
                visualBaseScale.z
                * (1f / Mathf.Sqrt(squash)));

        if (attackTelegraph != null)
        {
            float pulse =
                1f
                + Mathf.Sin(
                    Time.unscaledTime * 18f
                    + visualSeed)
                * 0.08f;

            attackTelegraph.transform.localScale =
                attackTelegraph.transform.localScale
                * 0.96f
                + attackTelegraph.transform.localScale
                * 0.04f
                * pulse;
        }
    }

    private float GetAttackLean()
    {
        return enemyType switch
        {
            EnemyType.Heavy => -1f,
            EnemyType.Ranged => 1f,
            EnemyType.Elite => -1f,
            EnemyType.Boss => 1f,
            _ => 1f
        };
    }

    private float GetDistanceToPlayer()
    {
        if (player == null)
        {
            return 999f;
        }

        Vector3 self = transform.position;
        Vector3 target = player.position;
        self.y = 0f;
        target.y = 0f;
        return Vector3.Distance(self, target);
    }

    private void ExecuteAttack()
    {
        RemoveAttackTelegraph();
        attackTimer = attackCooldown;

        if (playerHealth == null)
        {
            playerHealth =
                player.GetComponent<PlayerHealth>();
        }

        if (playerHealth == null
            || !playerHealth.IsAlive)
        {
            return;
        }

        float distance =
            GetDistanceToPlayer();

        if (IsBoss)
        {
            PerformBossAttack(distance);
        }
        else if (distance <= attackRange * 1.25f)
        {
            playerHealth.TakeDamage(
                attackDamage);
        }

        if (enemyType == EnemyType.Ranged)
        {
            CreateRangedProjectileEffect();
        }

        AudioDirector.Instance?.PlayAttack();
    }

    private void PerformBossAttack(float distance)
    {
        if (distance <= attackRange * 1.35f)
        {
            playerHealth.TakeDamage(
                attackDamage);
        }

        GameObject shockwave =
            GameObject.CreatePrimitive(
                PrimitiveType.Cylinder);

        shockwave.name =
            "BossShockwave";

        shockwave.transform.position =
            transform.position
            + Vector3.up * 0.06f;

        shockwave.transform.localScale =
            new Vector3(
                4.8f,
                0.045f,
                4.8f);

        Collider collider =
            shockwave.GetComponent<Collider>();

        if (collider != null)
        {
            Destroy(collider);
        }

        Renderer renderer =
            shockwave.GetComponent<Renderer>();

        if (renderer != null)
        {
            Shader shader =
                Shader.Find(
                    "Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard");

            if (shader != null)
            {
                Material material =
                    new Material(shader)
                    {
                        color =
                            new Color(
                                1f,
                                0.28f,
                                0.035f)
                    };

                ConfigureMaterial(material);
                renderer.material = material;
            }
        }

        StartCoroutine(
            ExpandShockwave(
                shockwave));
    }

    private IEnumerator ExpandShockwave(GameObject shockwave)
    {
        if (shockwave == null)
        {
            yield break;
        }

        Vector3 start =
            shockwave.transform.localScale;

        const float duration = 0.28f;
        float elapsed = 0f;

        while (elapsed < duration && shockwave != null)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = t * t * (3f - 2f * t);

            shockwave.transform.localScale =
                Vector3.Lerp(
                    start,
                    start * 2.2f,
                    eased);

            yield return null;
        }

        if (shockwave != null)
        {
            Destroy(shockwave);
        }
    }

    private void CreateRangedProjectileEffect()
    {
        if (player == null)
        {
            return;
        }

        GameObject projectile =
            GameObject.CreatePrimitive(
                PrimitiveType.Sphere);

        if (projectile == null)
        {
            return;
        }

        projectile.name = "RangedEnergyBolt";
        projectile.transform.position =
            transform.position
            + Vector3.up * 0.8f
            + transform.forward * 0.8f;
        projectile.transform.localScale =
            new Vector3(0.16f, 0.16f, 0.16f);

        Collider collider =
            projectile.GetComponent<Collider>();

        if (collider != null)
        {
            Destroy(collider);
        }

        Renderer renderer =
            projectile.GetComponent<Renderer>();

        if (renderer != null)
        {
            Shader shader =
                Shader.Find(
                    "Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard");

            if (shader != null)
            {
                Material material =
                    new Material(shader)
                    {
                        color =
                            new Color(
                                1f,
                                0.22f,
                                0.55f)
                    };

                ConfigureMaterial(material);
                renderer.material = material;
            }
        }

        StartCoroutine(
            MoveProjectile(
                projectile));
    }

    private IEnumerator MoveProjectile(GameObject projectile)
    {
        Vector3 start =
            projectile.transform.position;

        Vector3 end =
            player != null
                ? player.position + Vector3.up * 0.9f
                : start + transform.forward * 8f;

        float elapsed = 0f;
        const float duration = 0.16f;

        while (elapsed < duration && projectile != null)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            projectile.transform.position =
                Vector3.Lerp(
                    start,
                    end,
                    t);

            yield return null;
        }

        if (projectile != null)
        {
            Destroy(projectile);
        }
    }

    public void TakeDamage(
        float damage,
        Vector3 hitDirection,
        bool applyKnockback)
    {
        if (!IsAlive || damage <= 0f)
        {
            return;
        }

        currentHealth =
            Mathf.Max(
                0f,
                currentHealth - damage);

        hitStunTimer =
            hitStunDuration;

        hitFlashTimer = 0.08f;

        RemoveAttackTelegraph();

        if (targetMaterial != null)
        {
            targetMaterial.color =
                Color.white;
        }

        if (applyKnockback
            && !IsBoss
            && hitDirection.sqrMagnitude > 0.001f)
        {
            float force =
                Mathf.Max(
                    0f,
                    8f - knockbackResistance);

            if (force > 0f)
            {
                knockbackVelocity =
                    hitDirection.normalized
                    * force;

                knockbackTimer = 0.12f;
            }
        }

        if (currentHealth <= 0f)
        {
            gameObject.SetActive(false);
        }

        AudioDirector.Instance?.PlayHit();
    }

    private void CreateAttackTelegraph()
    {
        RemoveAttackTelegraph();

        PrimitiveType primitive =
            IsBoss
                ? PrimitiveType.Cylinder
                : PrimitiveType.Cube;

        attackTelegraph =
            GameObject.CreatePrimitive(
                primitive);

        attackTelegraph.name =
            "EnemyAttackTelegraph";

        if (IsBoss)
        {
            attackTelegraph.transform.position =
                transform.position
                + Vector3.up * 0.08f;

            attackTelegraph.transform.localScale =
                new Vector3(
                    2.8f,
                    0.05f,
                    2.8f);
        }
        else
        {
            attackTelegraph.transform.position =
                transform.position
                + transform.forward * 1.15f
                + Vector3.up * 0.75f;

            attackTelegraph.transform.localScale =
                new Vector3(
                    enemyType == EnemyType.Ranged ? 0.35f : 0.55f,
                    0.08f,
                    enemyType == EnemyType.Ranged ? 0.95f : 0.82f);
        }

        Collider collider =
            attackTelegraph.GetComponent<Collider>();

        if (collider != null)
        {
            Destroy(collider);
        }

        Renderer renderer =
            attackTelegraph.GetComponent<Renderer>();

        if (renderer != null)
        {
            Shader shader =
                Shader.Find(
                    "Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard");

            if (shader != null)
            {
                Material material =
                    new Material(shader)
                    {
                        color =
                            new Color(
                                1f,
                                0.08f,
                                0.12f)
                    };

                ConfigureMaterial(material);
                renderer.material = material;
            }
        }
    }

    private void RemoveAttackTelegraph()
    {
        if (attackTelegraph == null)
        {
            return;
        }

        Destroy(attackTelegraph);
        attackTelegraph = null;
    }

    private void ApplyTypeTuning()
    {
        switch (enemyType)
        {
            case EnemyType.Basic:
                attackRange = 2.1f;
                stopDistance = 1.6f;
                knockbackResistance = 0f;
                attackWindup = 0.38f;
                break;

            case EnemyType.Heavy:
                moveSpeed *= 0.7f;
                attackRange = 2.5f;
                stopDistance = 1.9f;
                attackDamage *= 1.7f;
                knockbackResistance = 6f;
                attackWindup = 0.62f;
                break;

            case EnemyType.Ranged:
                moveSpeed *= 0.85f;
                attackRange = 7f;
                stopDistance = 5.5f;
                attackDamage *= 0.75f;
                knockbackResistance = 1f;
                attackWindup = 0.5f;
                break;

            case EnemyType.Elite:
                maxHealth *= 2.4f;
                currentHealth = maxHealth;
                moveSpeed *= 1.05f;
                attackRange = 2.7f;
                attackDamage *= 1.5f;
                attackCooldown *= 0.85f;
                knockbackResistance = 7f;
                attackWindup = 0.52f;
                break;

            case EnemyType.Boss:
                maxHealth *= 8f;
                currentHealth = maxHealth;
                moveSpeed *= 0.95f;
                attackRange = 3.4f;
                attackDamage *= 2.2f;
                attackCooldown *= 0.75f;
                knockbackResistance = 999f;
                attackWindup = 0.78f;
                break;
        }
    }

    private void FaceDirection(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion desiredRotation =
            Quaternion.LookRotation(
                direction.normalized,
                Vector3.up);

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                17f * Time.deltaTime);
    }

    private static void ConfigureMaterial(Material material)
    {
        if (material == null)
        {
            return;
        }

        if (material.HasProperty("_Metallic"))
        {
            material.SetFloat("_Metallic", 0.35f);
        }

        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", 0.82f);
        }

        if (material.HasProperty("_EmissionColor")
            && material.color.maxColorComponent > 0.6f)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor(
                "_EmissionColor",
                material.color * 1.5f);
        }
    }

    private void OnDestroy()
    {
        RemoveAttackTelegraph();

        if (targetMaterial != null)
        {
            targetMaterial.color =
                baseColor;
        }
    }
}
