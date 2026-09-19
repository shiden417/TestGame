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

    [SerializeField] private EnemyType enemyType = EnemyType.Basic;
    [SerializeField] private float maxHealth = 60f;
    [SerializeField] private float moveSpeed = 2.8f;
    [SerializeField] private float attackRange = 2.1f;
    [SerializeField] private float attackCooldown = 1.8f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackWindup = 0.55f;
    [SerializeField] private float knockbackResistance;
    [SerializeField] private float aggroRange = 30f;
    [SerializeField] private float stopDistance = 1.6f;
    [SerializeField] private float hitStunDuration = 0.15f;

    private Transform player;
    private PlayerHealth playerHealth;
    private float currentHealth;
    private float attackTimer;
    private float attackWindupTimer;
    private float hitStunTimer;
    private float knockbackTimer;
    private Vector3 knockbackVelocity;

    public EnemyType Type => enemyType;
    public float CurrentHealth => currentHealth;
    public bool IsAlive => currentHealth > 0f && gameObject.activeSelf;
    public bool IsBoss => enemyType == EnemyType.Boss;
    public bool IsElite => enemyType == EnemyType.Elite || enemyType == EnemyType.Boss;

    public void Initialize(
        EnemyType type,
        Transform target,
        float health,
        float speed,
        float damage,
        float cooldown)
    {
        enemyType = type;
        player = target;
        maxHealth = Mathf.Max(1f, health);
        currentHealth = maxHealth;
        moveSpeed = Mathf.Max(0.1f, speed);
        attackDamage = Mathf.Max(0f, damage);
        attackCooldown = Mathf.Max(0.2f, cooldown);
        attackTimer = Random.Range(0f, attackCooldown);
        ApplyTypeTuning();
    }

    private void Update()
    {
        if (!IsAlive || player == null)
        {
            return;
        }

        UpdateTimers();

        if (knockbackTimer > 0f)
        {
            transform.position += knockbackVelocity * Time.deltaTime;
            return;
        }

        Vector3 offset = player.position - transform.position;
        offset.y = 0f;
        float distance = offset.magnitude;

        if (distance > aggroRange)
        {
            return;
        }

        if (hitStunTimer > 0f)
        {
            return;
        }

        if (attackWindupTimer > 0f)
        {
            if (distance <= attackRange * 1.15f)
            {
                FaceDirection(offset);
            }

            if (attackWindupTimer <= 0f)
            {
                ExecuteAttack();
            }

            return;
        }

        if (distance > attackRange)
        {
            float desiredDistance = Mathf.Max(stopDistance, attackRange * 0.7f);
            Vector3 moveDirection = distance > desiredDistance ? offset.normalized : Vector3.zero;
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
            FaceDirection(moveDirection);
            return;
        }

        FaceDirection(offset);

        if (attackTimer <= 0f)
        {
            attackWindupTimer = attackWindup;
        }
    }

    private void UpdateTimers()
    {
        attackTimer = Mathf.Max(0f, attackTimer - Time.deltaTime);
        attackWindupTimer = Mathf.Max(0f, attackWindupTimer - Time.deltaTime);
        hitStunTimer = Mathf.Max(0f, hitStunTimer - Time.deltaTime);

        if (knockbackTimer > 0f)
        {
            knockbackTimer = Mathf.Max(0f, knockbackTimer - Time.deltaTime);
            knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, 10f * Time.deltaTime);
        }
    }

    private void ExecuteAttack()
    {
        attackTimer = attackCooldown;

        if (playerHealth == null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }

        if (playerHealth == null || !playerHealth.IsAlive)
        {
            return;
        }

        float distance = Vector3.Distance(
            new Vector3(transform.position.x, 0f, transform.position.z),
            new Vector3(player.position.x, 0f, player.position.z));

        if (distance <= attackRange * 1.3f)
        {
            playerHealth.TakeDamage(attackDamage);
        }

        CreateAttackFlash();
    }

    public void TakeDamage(float damage, Vector3 hitDirection, bool applyKnockback)
    {
        if (!IsAlive || damage <= 0f)
        {
            return;
        }

        currentHealth = Mathf.Max(0f, currentHealth - damage);
        hitStunTimer = hitStunDuration;

        if (applyKnockback && !IsBoss && hitDirection.sqrMagnitude > 0.001f)
        {
            float force = Mathf.Max(0f, 8f - knockbackResistance);
            knockbackVelocity = hitDirection.normalized * force;
            knockbackTimer = 0.12f;
        }

        FlashHit();

        if (currentHealth <= 0f)
        {
            gameObject.SetActive(false);
        }
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
                attackDamage *= 1.8f;
                knockbackResistance = 6f;
                break;
            case EnemyType.Ranged:
                moveSpeed *= 0.9f;
                attackRange = 7f;
                stopDistance = 6f;
                attackDamage *= 0.8f;
                knockbackResistance = 1f;
                break;
            case EnemyType.Elite:
                maxHealth *= 2.4f;
                currentHealth = maxHealth;
                moveSpeed *= 1.1f;
                attackRange = 2.6f;
                attackDamage *= 1.6f;
                attackCooldown *= 0.8f;
                knockbackResistance = 7f;
                break;
            case EnemyType.Boss:
                maxHealth *= 8f;
                currentHealth = maxHealth;
                moveSpeed *= 0.95f;
                attackRange = 3.2f;
                attackDamage *= 2.2f;
                attackCooldown *= 0.75f;
                knockbackResistance = 999f;
                break;
        }
    }

    private void FaceDirection(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion desiredRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            desiredRotation,
            14f * Time.deltaTime);
    }

    private void FlashHit()
    {
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer == null)
        {
            return;
        }

        Material material = renderer.material;
        Color original = material.color;
        material.color = Color.white;
        Invoke(nameof(RestoreMaterialColor), 0.08f);

        void RestoreMaterialColor()
        {
            if (renderer != null && renderer.material != null)
            {
                renderer.material.color = original;
            }
        }
    }

    private void CreateAttackFlash()
    {
        GameObject flash = GameObject.CreatePrimitive(PrimitiveType.Cube);
        flash.name = "EnemyAttackFlash";
        flash.transform.position = transform.position + transform.forward * 1.1f + Vector3.up;
        flash.transform.localScale = new Vector3(0.45f, 0.15f, 0.6f);

        Collider collider = flash.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        Renderer renderer = flash.GetComponent<Renderer>();
        if (renderer != null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (shader != null)
            {
                renderer.material = new Material(shader) { color = new Color(1f, 0.15f, 0.15f) };
            }
        }

        Destroy(flash, 0.09f);
    }
}
