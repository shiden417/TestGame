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
    private PerfectDodgeSystem perfectDodgeSystem;
    private float skill1Timer;
    private float skill2Timer;
    private float ultimateTimer;
    private float ultimateGauge;
    private GameObject skillVisual;
    private float visualLifetime;
    private float visualDuration;

    public float Skill1Remaining => skill1Timer;
    public float Skill2Remaining => skill2Timer;
    public float UltimateRemaining => ultimateTimer;
    public float UltimateGauge => ultimateGauge;
    public bool UltimateReady =>
        ultimateGauge >= 100f
        && ultimateTimer <= 0f;

    public void Initialize(
        GameInput gameInput,
        PerfectDodgeSystem perfectDodge)
    {
        if (gameInput == null)
        {
            throw new System.ArgumentNullException(
                nameof(gameInput));
        }

        if (perfectDodge == null)
        {
            throw new System.ArgumentNullException(
                nameof(perfectDodge));
        }

        input = gameInput;
        perfectDodgeSystem = perfectDodge;
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
        skill1Timer =
            skill1Cooldown;

        PerformAreaDamage(
            skill1Range,
            skill1Damage,
            true);

        TriggerSkillVisual(
            new Color(
                0.1f,
                0.85f,
                1f),
            skill1Range,
            0.16f);

        ultimateGauge =
            Mathf.Min(
                100f,
                ultimateGauge + 12f);

        AudioDirector.Instance?.PlaySkill();
    }

    private void UseSkill2()
    {
        skill2Timer =
            skill2Cooldown;

        Vector3 direction =
            transform.forward;

        Vector3 center =
            transform.position
            + Vector3.up * 0.9f
            + direction * 3.2f;

        PerformDamageAt(
            center,
            skill2Range,
            skill2Damage,
            true);

        TriggerSkillVisual(
            new Color(
                0.7f,
                0.2f,
                1f),
            skill2Range,
            0.2f);

        ultimateGauge =
            Mathf.Min(
                100f,
                ultimateGauge + 18f);

        AudioDirector.Instance?.PlaySkill();
    }

    private void UseUltimate()
    {
        ultimateTimer =
            ultimateCooldown;

        ultimateGauge = 0f;

        PerformAreaDamage(
            ultimateRange,
            ultimateDamage,
            true);

        TriggerSkillVisual(
            new Color(
                1f,
                0.7f,
                0.1f),
            ultimateRange,
            0.42f);

        AudioDirector.Instance?.PlayUltimate();

        Time.timeScale = 0.2f;
        Time.fixedDeltaTime =
            0.02f * Time.timeScale;

        CancelInvoke(
            nameof(RestoreTimeScale));

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
                collider.GetComponentInParent<
                    EnemyController>();

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

        skillVisual = new GameObject("SkillEffect");

        if (skillVisual == null)
        {
            throw new System.InvalidOperationException(
                "Failed to create skill visual.");
        }

        skillVisual.transform.position =
            transform.position
            + Vector3.up * 0.08f;

        CreateSkillSegment(
            "Front",
            skillVisual.transform,
            new Vector3(0f, 0f, range * 0.5f),
            new Vector3(range, 0.05f, 0.09f),
            color);

        CreateSkillSegment(
            "Back",
            skillVisual.transform,
            new Vector3(0f, 0f, -range * 0.5f),
            new Vector3(range, 0.05f, 0.09f),
            color);

        CreateSkillSegment(
            "Left",
            skillVisual.transform,
            new Vector3(-range * 0.5f, 0f, 0f),
            new Vector3(0.09f, 0.05f, range),
            color);

        CreateSkillSegment(
            "Right",
            skillVisual.transform,
            new Vector3(range * 0.5f, 0f, 0f),
            new Vector3(0.09f, 0.05f, range),
            color);

        skillVisual.transform.localScale =
            Vector3.one * 0.45f;

        visualDuration =
            Mathf.Max(0.01f, duration);

        visualLifetime =
            visualDuration;
    }

    private static void CreateSkillSegment(
        string objectName,
        Transform parent,
        Vector3 localPosition,
        Vector3 localScale,
        Color color)
    {
        GameObject segment =
            GameObject.CreatePrimitive(
                PrimitiveType.Cube);

        if (segment == null)
        {
            throw new System.InvalidOperationException(
                $"Failed to create skill segment: {objectName}");
        }

        segment.name = objectName;
        segment.transform.SetParent(parent, false);
        segment.transform.localPosition = localPosition;
        segment.transform.localScale = localScale;

        Collider collider =
            segment.GetComponent<Collider>();

        if (collider != null)
        {
            Destroy(collider);
        }

        Renderer renderer =
            segment.GetComponent<Renderer>();

        if (renderer == null)
        {
            throw new System.InvalidOperationException(
                $"Skill segment renderer was not created: {objectName}");
        }

        Shader shader =
            Shader.Find("Universal Render Pipeline/Lit")
            ?? Shader.Find("Standard");

        if (shader == null)
        {
            throw new System.InvalidOperationException(
                "No compatible Unity material shader was found.");
        }

        Material material =
            new Material(shader)
            {
                color = color
            };

        if (material.HasProperty("_Metallic"))
        {
            material.SetFloat("_Metallic", 0.15f);
        }

        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", 0.9f);
        }

        if (material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor(
                "_EmissionColor",
                color * 1.8f);
        }

        renderer.material = material;
    }

    private void UpdateVisualLifetime()
    {
        if (skillVisual == null)
        {
            return;
        }

        skillVisual.transform.position =
            transform.position
            + Vector3.up * 0.08f;

        float elapsed01 =
            1f
            - Mathf.Clamp01(
                visualLifetime
                / Mathf.Max(0.01f, visualDuration));

        float scale =
            Mathf.Lerp(
                0.45f,
                1.12f,
                elapsed01);

        float pulse =
            1f
            + Mathf.Sin(
                elapsed01 * Mathf.PI * 4f)
            * 0.08f;

        skillVisual.transform.localScale =
            Vector3.one
            * scale
            * pulse;

        skillVisual.transform.Rotate(
            0f,
            420f * Time.unscaledDeltaTime,
            0f,
            Space.Self);

        visualLifetime -=
            Time.unscaledDeltaTime;

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
        CancelInvoke(
            nameof(RestoreTimeScale));

        if (skillVisual != null)
        {
            Destroy(skillVisual);
        }

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
}
