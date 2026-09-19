using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerSkillController : MonoBehaviour
{
    [SerializeField] private float skill1Cooldown = 3f;
    [SerializeField] private float skill2Cooldown = 5f;
    [SerializeField] private float ultimateCooldown = 12f;
    [SerializeField] private float skill1Damage = 55f;
    [SerializeField] private float skill2Damage = 42f;
    [SerializeField] private float ultimateDamage = 140f;
    [SerializeField] private float skill1Range = 4.5f;
    [SerializeField] private float skill2Range = 5.5f;
    [SerializeField] private float ultimateRange = 8f;

    private GameInput input;
    private TargetingSystem targetingSystem;
    private PerfectDodgeSystem perfectDodgeSystem;
    private float skill1Timer;
    private float skill2Timer;
    private float ultimateTimer;
    private float ultimateGauge;
    private GameObject skillVisual;
    private float visualLifetime;

    public float Skill1Remaining => skill1Timer;
    public float Skill2Remaining => skill2Timer;
    public float UltimateRemaining => ultimateTimer;
    public float UltimateGauge => ultimateGauge;
    public bool UltimateReady =>
        ultimateGauge >= 100f
        && ultimateTimer <= 0f;

    public void Initialize(
        GameInput gameInput,
        TargetingSystem targeting,
        PerfectDodgeSystem perfectDodge)
    {
        if (gameInput == null)
        {
            throw new System.ArgumentNullException(nameof(gameInput));
        }

        if (targeting == null)
        {
            throw new System.ArgumentNullException(nameof(targeting));
        }

        if (perfectDodge == null)
        {
            throw new System.ArgumentNullException(nameof(perfectDodge));
        }

        input = gameInput;
        targetingSystem = targeting;
        perfectDodgeSystem = perfectDodge;
        CreateSkillVisual();
    }

    private void Update()
    {
        if (input == null)
        {
            return;
        }

        skill1Timer =
            Mathf.Max(
                0f,
                skill1Timer - Time.deltaTime);

        skill2Timer =
            Mathf.Max(
                0f,
                skill2Timer - Time.deltaTime);

        ultimateTimer =
            Mathf.Max(
                0f,
                ultimateTimer - Time.deltaTime);

        if (perfectDodgeSystem.CounterWindowActive)
        {
            ultimateGauge =
                Mathf.Min(
                    100f,
                    ultimateGauge
                    + 20f * Time.deltaTime);
        }

        if (input.Skill1.WasPressedThisFrame()
            && skill1Timer <= 0f)
        {
            UseSkill1();
        }

        if (input.Skill2.WasPressedThisFrame()
            && skill2Timer <= 0f)
        {
            UseSkill2();
        }

        if (input.Ultimate.WasPressedThisFrame()
            && UltimateReady)
        {
            UseUltimate();
        }

        UpdateVisualLifetime();
    }

    private void UseSkill1()
    {
        skill1Timer = skill1Cooldown;

        PerformAreaDamage(
            skill1Range,
            skill1Damage,
            true);

        TriggerSkillVisual(
            new Color(0.1f, 0.85f, 1f),
            skill1Range,
            0.16f);

        ultimateGauge =
            Mathf.Min(
                100f,
                ultimateGauge + 12f);
    }

    private void UseSkill2()
    {
        skill2Timer = skill2Cooldown;

        Vector3 forward = transform.forward;

        Vector3 center =
            transform.position
            + Vector3.up * 0.9f
            + forward * 3.2f;

        PerformDamageAt(
            center,
            skill2Range,
            skill2Damage,
            true);

        TriggerSkillVisual(
            new Color(0.7f, 0.2f, 1f),
            skill2Range,
            0.2f);

        ultimateGauge =
            Mathf.Min(
                100f,
                ultimateGauge + 18f);
    }

    private void UseUltimate()
    {
        ultimateTimer = ultimateCooldown;
        ultimateGauge = 0f;

        PerformAreaDamage(
            ultimateRange,
            ultimateDamage,
            true);

        TriggerSkillVisual(
            new Color(1f, 0.7f, 0.1f),
            ultimateRange,
            0.42f);

        Time.timeScale = 0.2f;
        Time.fixedDeltaTime =
            0.02f * Time.timeScale;

        CancelInvoke(nameof(RestoreTimeScale));
        Invoke(
            nameof(RestoreTimeScale),
            0.42f);
    }

    private void PerformAreaDamage(
        float range,
        float damage,
        bool knockback)
    {
        Vector3 center =
            transform.position
            + Vector3.up;

        PerformDamageAt(
            center,
            range,
            damage,
            knockback);
    }

    private void PerformDamageAt(
        Vector3 center,
        float range,
        float damage,
        bool knockback)
    {
        Collider[] colliders =
            Physics.OverlapSphere(
                center,
                range,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);

        foreach (Collider collider in colliders)
        {
            if (collider == null)
            {
                continue;
            }

            EnemyController enemy =
                collider.GetComponentInParent<EnemyController>();

            if (enemy == null || !enemy.IsAlive)
            {
                continue;
            }

            Vector3 direction =
                enemy.transform.position
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

            enemy.TakeDamage(
                damage,
                direction,
                knockback);

            AddUltimateGauge(5f);
        }
    }

    public void AddUltimateGauge(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        ultimateGauge =
            Mathf.Min(
                100f,
                ultimateGauge + amount);
    }

    private void TriggerSkillVisual(
        Color color,
        float range,
        float duration)
    {
        if (skillVisual != null)
        {
            Destroy(skillVisual);
        }

        skillVisual =
            GameObject.CreatePrimitive(
                PrimitiveType.Cylinder);

        skillVisual.name = "SkillEffect";
        skillVisual.transform.position =
            transform.position
            + Vector3.up * 0.05f;

        skillVisual.transform.localScale =
            new Vector3(
                range * 2f,
                0.05f,
                range * 2f);

        Collider collider =
            skillVisual.GetComponent<Collider>();

        if (collider != null)
        {
            Destroy(collider);
        }

        Renderer renderer =
            skillVisual.GetComponent<Renderer>();

        if (renderer != null)
        {
            Shader shader =
                Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard");

            if (shader != null)
            {
                renderer.material =
                    new Material(shader)
                    {
                        color = color
                    };
            }
        }

        visualLifetime = duration;
    }

    private void UpdateVisualLifetime()
    {
        if (skillVisual == null)
        {
            return;
        }

        skillVisual.transform.position =
            transform.position
            + Vector3.up * 0.05f;

        skillVisual.transform.Rotate(
            0f,
            420f * Time.unscaledDeltaTime,
            0f);

        visualLifetime -= Time.unscaledDeltaTime;

        if (visualLifetime <= 0f)
        {
            Destroy(skillVisual);
            skillVisual = null;
        }
    }

    private void RestoreTimeScale()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    private void OnDestroy()
    {
        CancelInvoke(nameof(RestoreTimeScale));

        if (skillVisual != null)
        {
            Destroy(skillVisual);
        }

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
}
