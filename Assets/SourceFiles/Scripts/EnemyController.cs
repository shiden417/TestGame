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

        targetRenderer = GetComponentInChildren<Renderer>();

        if (targetRenderer != null)
        {
            targetMaterial = targetRenderer.material;
            baseColor = targetMaterial.color;
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
                    12f * Time.deltaTime);
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
            Vector3.Distance(
                new Vector3(
                    transform.position.x,
                    0f,
                    transform.position.z),
                new Vector3(
                    player.position.x,
                    0f,
                    player.position.z));

        if (IsBoss)
        {
            PerformBossAttack(distance);
        }
        else if (distance <= attackRange * 1.25f)
        {
            playerHealth.TakeDamage(
                attackDamage);
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
                3.6f,
                0.04f,
                3.6f);

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
                renderer.material =
                    new Material(shader)
                    {
                        color =
                            new Color(
                                1f,
                                0.32f,
                                0.04f)
                    };
            }
        }

        Destroy(shockwave, 0.18f);
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
                    2.4f,
                    0.05f,
                    2.4f);
        }
        else
        {
            attackTelegraph.transform.position =
                transform.position
                + transform.forward * 1.1f
                + Vector3.up;

            attackTelegraph.transform.localScale =
                new Vector3(
                    0.45f,
                    0.12f,
                    0.7f);
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
                renderer.material =
                    new Material(shader)
                    {
                        color =
                            new Color(
                                1f,
                                0.12f,
                                0.12f)
                    };
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
                break;

            case EnemyType.Heavy:
                moveSpeed *= 0.7f;
                attackRange = 2.5f;
                stopDistance = 1.9f;
                attackDamage *= 1.7f;
                knockbackResistance = 6f;
                break;

            case EnemyType.Ranged:
                moveSpeed *= 0.85f;
                attackRange = 7f;
                stopDistance = 5.5f;
                attackDamage *= 0.75f;
                knockbackResistance = 1f;
                break;

            case EnemyType.Elite:
                maxHealth *= 2.4f;
                currentHealth = maxHealth;
                moveSpeed *= 1.05f;
                attackRange = 2.7f;
                attackDamage *= 1.5f;
                attackCooldown *= 0.85f;
                knockbackResistance = 7f;
                break;

            case EnemyType.Boss:
                maxHealth *= 8f;
                currentHealth = maxHealth;
                moveSpeed *= 0.95f;
                attackRange = 3.4f;
                attackDamage *= 2.2f;
                attackCooldown *= 0.75f;
                knockbackResistance = 999f;
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
                14f * Time.deltaTime);
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
