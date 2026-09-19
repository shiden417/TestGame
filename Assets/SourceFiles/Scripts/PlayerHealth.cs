using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float hitInvulnerability = 0.12f;

    private float currentHealth;
    private float invulnerabilityTimer;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsAlive => currentHealth > 0f;
    public bool IsInvulnerable { get; private set; }

    private void Awake()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (invulnerabilityTimer <= 0f)
        {
            IsInvulnerable = false;
            return;
        }

        invulnerabilityTimer -= Time.deltaTime;
        IsInvulnerable = true;
    }

    public void TakeDamage(float damage)
    {
        if (!IsAlive || damage <= 0f || IsInvulnerable)
        {
            return;
        }

        currentHealth = Mathf.Max(0f, currentHealth - damage);
        invulnerabilityTimer = hitInvulnerability;

        if (currentHealth <= 0f)
        {
            IsInvulnerable = false;
        }
    }

    public void HealFull()
    {
        currentHealth = maxHealth;
        invulnerabilityTimer = 0f;
        IsInvulnerable = false;
    }
}
