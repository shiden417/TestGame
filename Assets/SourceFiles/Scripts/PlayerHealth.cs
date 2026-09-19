using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float hitInvulnerability = 0.12f;

    private DodgeController dodgeController;
    private float currentHealth;
    private float invulnerabilityTimer;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsAlive => currentHealth > 0f;
    public bool IsInvulnerable =>
        (dodgeController != null && dodgeController.IsInvulnerable)
        || invulnerabilityTimer > 0f;

    public void Initialize(DodgeController dodge)
    {
        if (dodge == null)
        {
            throw new System.ArgumentNullException(nameof(dodge));
        }

        dodgeController = dodge;
        maxHealth = Mathf.Max(1f, maxHealth);
        currentHealth = maxHealth;
    }

    private void Awake()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
        currentHealth = maxHealth;
    }

    private void Update()
    {
        invulnerabilityTimer = Mathf.Max(0f, invulnerabilityTimer - Time.deltaTime);
    }

    public void TakeDamage(float damage)
    {
        if (!IsAlive || damage <= 0f || IsInvulnerable)
        {
            return;
        }

        currentHealth = Mathf.Max(0f, currentHealth - damage);
        invulnerabilityTimer = hitInvulnerability;
    }

    public void HealFull()
    {
        currentHealth = maxHealth;
        invulnerabilityTimer = 0f;
    }
}
