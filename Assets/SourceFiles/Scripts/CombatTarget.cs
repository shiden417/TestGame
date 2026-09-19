using UnityEngine;

[RequireComponent(typeof(Targetable))]
public sealed class CombatTarget : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float hitFlashDuration = 0.08f;

    private Targetable targetable;
    private Renderer[] renderers = System.Array.Empty<Renderer>();
    private Material[] materials = System.Array.Empty<Material>();
    private Color[] baseColors = System.Array.Empty<Color>();
    private float currentHealth;
    private float hitFlashTimer;

    public Targetable Targetable => targetable;
    public float CurrentHealth => currentHealth;
    public bool IsAlive => currentHealth > 0f && gameObject.activeSelf;

    private void Awake()
    {
        targetable = GetComponent<Targetable>();
        currentHealth = Mathf.Max(1f, maxHealth);

        renderers = GetComponentsInChildren<Renderer>();
        materials = new Material[renderers.Length];
        baseColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null)
            {
                continue;
            }

            Material material = renderer.material;
            materials[i] = material;
            baseColors[i] = material.color;
        }
    }

    private void Update()
    {
        if (hitFlashTimer <= 0f)
        {
            return;
        }

        hitFlashTimer -= Time.deltaTime;

        float normalized = Mathf.Clamp01(hitFlashTimer / Mathf.Max(0.001f, hitFlashDuration));
        Color flashColor = Color.Lerp(Color.white, new Color(1f, 0.25f, 0.25f), normalized);

        for (int i = 0; i < materials.Length; i++)
        {
            Material material = materials[i];
            if (material == null)
            {
                continue;
            }

            material.color = Color.Lerp(baseColors[i], Color.white, normalized);
        }
    }

    public void TakeDamage(float damage)
    {
        if (!IsAlive || damage <= 0f)
        {
            return;
        }

        currentHealth = Mathf.Max(0f, currentHealth - damage);
        hitFlashTimer = hitFlashDuration;

        if (currentHealth <= 0f)
        {
            targetable.SetLocked(false);
            gameObject.SetActive(false);
        }
    }
}
