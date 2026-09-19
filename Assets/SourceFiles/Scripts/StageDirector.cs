using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class StageDirector : MonoBehaviour
{
    private enum StageState
    {
        Intro,
        Wave1,
        Wave2,
        Elite,
        Boss,
        Clear,
        Defeat
    }

    [SerializeField] private float firstWaveDelay = 1.2f;

    private StageState state;
    private BattleDirector battleDirector;
    private Transform player;
    private PlayerHealth playerHealth;
    private GameFlowController gameFlow;

    private int waveNumber;

    public string CurrentPhaseLabel { get; private set; } =
        "MISSION START";

    public int WaveNumber => waveNumber;

    public bool IsCleared =>
        state == StageState.Clear;

    public bool IsDefeated =>
        state == StageState.Defeat;

    public void Initialize(
        BattleDirector battle,
        Transform playerTransform,
        GameFlowController flow)
    {
        if (battle == null)
        {
            throw new System.ArgumentNullException(
                nameof(battle));
        }

        if (playerTransform == null)
        {
            throw new System.ArgumentNullException(
                nameof(playerTransform));
        }

        if (flow == null)
        {
            throw new System.ArgumentNullException(
                nameof(flow));
        }

        battleDirector = battle;
        player = playerTransform;
        playerHealth =
            player.GetComponent<PlayerHealth>();

        if (playerHealth == null)
        {
            throw new System.InvalidOperationException(
                "StageDirector requires PlayerHealth.");
        }

        gameFlow = flow;
        state = StageState.Intro;
        StartCoroutine(RunStage());
    }

    private IEnumerator RunStage()
    {
        CurrentPhaseLabel =
            "MISSION START // FUTURE ASHES";

        yield return new WaitForSeconds(
            firstWaveDelay);

        if (!playerHealth.IsAlive)
        {
            HandleDefeat();
            yield break;
        }

        state = StageState.Wave1;
        waveNumber = 1;
        CurrentPhaseLabel =
            "WAVE 1 // PURGE SECTOR";

        yield return StartCoroutine(
            RunWave(18, false));

        if (state == StageState.Defeat)
        {
            yield break;
        }

        state = StageState.Wave2;
        waveNumber = 2;
        CurrentPhaseLabel =
            "WAVE 2 // REINFORCEMENT";

        yield return StartCoroutine(
            RunWave(24, true));

        if (state == StageState.Defeat)
        {
            yield break;
        }

        state = StageState.Elite;
        CurrentPhaseLabel =
            "ELITE SIGNATURE // KAGE FRAME";

        SpawnEnemy(
            EnemyController.EnemyType.Elite,
            player.position
            + new Vector3(0f, 1f, 14f),
            1f);

        yield return StartCoroutine(
            WaitForBattlefieldClear());

        if (state == StageState.Defeat)
        {
            yield break;
        }

        state = StageState.Boss;
        CurrentPhaseLabel =
            "BOSS // OROCHI FRAME";

        SpawnEnemy(
            EnemyController.EnemyType.Boss,
            player.position
            + new Vector3(0f, 1.5f, 16f),
            2.4f);

        yield return StartCoroutine(
            WaitForBattlefieldClear());

        if (state == StageState.Defeat)
        {
            yield break;
        }

        state = StageState.Clear;
        CurrentPhaseLabel =
            "MISSION CLEAR";

        AudioDirector.Instance?.PlayClear();

        gameFlow.CompleteStage(
            battleDirector.TotalDefeated);
    }

    private IEnumerator RunWave(
        int count,
        bool includeSpecialEnemies)
    {
        for (int i = 0; i < count; i++)
        {
            if (!playerHealth.IsAlive)
            {
                HandleDefeat();
                yield break;
            }

            EnemyController.EnemyType type =
                EnemyController.EnemyType.Basic;

            if (includeSpecialEnemies
                && i % 6 == 0)
            {
                type =
                    EnemyController.EnemyType.Heavy;
            }
            else if (i % 8 == 0)
            {
                type =
                    EnemyController.EnemyType.Ranged;
            }

            SpawnEnemy(
                type,
                GetSpawnPosition(i),
                1f);

            yield return new WaitForSeconds(
                0.12f);
        }

        yield return StartCoroutine(
            WaitForBattlefieldClear());
    }

    private IEnumerator WaitForBattlefieldClear()
    {
        while (battleDirector.ActiveEnemyCount > 0)
        {
            if (!playerHealth.IsAlive)
            {
                HandleDefeat();
                yield break;
            }

            yield return null;
        }
    }

    private Vector3 GetSpawnPosition(int index)
    {
        float angle =
            index * 137.5f
            * Mathf.Deg2Rad;

        float radius =
            11f
            + (index % 5) * 1.2f;

        Vector3 position =
            player.position
            + new Vector3(
                Mathf.Cos(angle) * radius,
                1f,
                Mathf.Sin(angle) * radius);

        position.x =
            Mathf.Clamp(
                position.x,
                -25f,
                25f);

        position.z =
            Mathf.Clamp(
                position.z,
                -25f,
                25f);

        return position;
    }

    private void SpawnEnemy(
        EnemyController.EnemyType type,
        Vector3 position,
        float scaleMultiplier)
    {
        GameObject enemyObject =
            CreateEnemyVisual(type);

        enemyObject.transform.position =
            position;

        enemyObject.transform.localScale *=
            scaleMultiplier;

        EnemyController enemy =
            enemyObject.AddComponent<EnemyController>();

        switch (type)
        {
            case EnemyController.EnemyType.Basic:
                enemy.Initialize(
                    type,
                    player,
                    battleDirector,
                    60f,
                    3f,
                    8f,
                    1.7f);
                break;

            case EnemyController.EnemyType.Heavy:
                enemy.Initialize(
                    type,
                    player,
                    battleDirector,
                    105f,
                    2f,
                    14f,
                    2.4f);
                break;

            case EnemyController.EnemyType.Ranged:
                enemy.Initialize(
                    type,
                    player,
                    battleDirector,
                    55f,
                    2.4f,
                    7f,
                    2f);
                break;

            case EnemyController.EnemyType.Elite:
                enemy.Initialize(
                    type,
                    player,
                    battleDirector,
                    260f,
                    3.1f,
                    19f,
                    1.45f);
                break;

            case EnemyController.EnemyType.Boss:
                enemy.Initialize(
                    type,
                    player,
                    battleDirector,
                    1200f,
                    2.35f,
                    24f,
                    1.2f);
                break;
        }

        enemyObject.AddComponent<Targetable>();
        battleDirector.Register(enemy);
    }

    private GameObject CreateEnemyVisual(
        EnemyController.EnemyType type)
    {
        PrimitiveType colliderPrimitive =
            type == EnemyController.EnemyType.Boss
                ? PrimitiveType.Sphere
                : PrimitiveType.Capsule;

        GameObject enemyObject =
            GameObject.CreatePrimitive(colliderPrimitive);

        if (enemyObject == null)
        {
            throw new System.InvalidOperationException(
                "Failed to create enemy root.");
        }

        enemyObject.name =
            $"{type}Unit";

        Renderer rootRenderer =
            enemyObject.GetComponent<Renderer>();

        if (rootRenderer != null)
        {
            rootRenderer.enabled = false;
        }

        Transform visualRoot =
            new GameObject("EnemyVisual").transform;

        visualRoot.SetParent(
            enemyObject.transform,
            false);

        visualRoot.localPosition = Vector3.zero;
        visualRoot.localRotation = Quaternion.identity;
        visualRoot.localScale =
            type == EnemyController.EnemyType.Boss
                ? Vector3.one * 1.35f
                : Vector3.one;

        switch (type)
        {
            case EnemyController.EnemyType.Basic:
                BuildBasicEnemy(visualRoot);
                break;

            case EnemyController.EnemyType.Heavy:
                BuildHeavyEnemy(visualRoot);
                break;

            case EnemyController.EnemyType.Ranged:
                BuildRangedEnemy(visualRoot);
                break;

            case EnemyController.EnemyType.Elite:
                BuildEliteEnemy(visualRoot);
                break;

            case EnemyController.EnemyType.Boss:
                BuildBossEnemy(visualRoot);
                break;
        }

        return enemyObject;
    }

    private void BuildBasicEnemy(Transform parent)
    {
        CreateEnemyPart(
            "Core",
            PrimitiveType.Capsule,
            parent,
            new Vector3(0f, 0.05f, 0f),
            new Vector3(0.62f, 0.85f, 0.5f),
            new Color(0.18f, 0.03f, 0.06f));

        CreateEnemyPart(
            "ChestPlate",
            PrimitiveType.Cube,
            parent,
            new Vector3(0f, 0.35f, 0.27f),
            new Vector3(0.55f, 0.3f, 0.12f),
            new Color(0.3f, 0.04f, 0.08f));

        CreateEnemyPart(
            "Visor",
            PrimitiveType.Cube,
            parent,
            new Vector3(0f, 0.52f, 0.3f),
            new Vector3(0.3f, 0.08f, 0.04f),
            new Color(0.98f, 0.12f, 0.28f));

        CreateEnemyPart(
            "BackFin",
            PrimitiveType.Cube,
            parent,
            new Vector3(0f, 0.25f, -0.28f),
            new Vector3(0.14f, 0.6f, 0.12f),
            new Color(0.08f, 0.04f, 0.07f));
    }

    private void BuildHeavyEnemy(Transform parent)
    {
        CreateEnemyPart(
            "Core",
            PrimitiveType.Capsule,
            parent,
            new Vector3(0f, 0.05f, 0f),
            new Vector3(0.78f, 1.02f, 0.62f),
            new Color(0.1f, 0.025f, 0.05f));

        CreateEnemyPart(
            "ChestArmor",
            PrimitiveType.Cube,
            parent,
            new Vector3(0f, 0.36f, 0.3f),
            new Vector3(0.78f, 0.38f, 0.16f),
            new Color(0.25f, 0.035f, 0.07f));

        CreateEnemyPart(
            "LeftShoulder",
            PrimitiveType.Sphere,
            parent,
            new Vector3(-0.5f, 0.52f, 0f),
            new Vector3(0.42f, 0.38f, 0.48f),
            new Color(0.16f, 0.025f, 0.045f));

        CreateEnemyPart(
            "RightShoulder",
            PrimitiveType.Sphere,
            parent,
            new Vector3(0.5f, 0.52f, 0f),
            new Vector3(0.42f, 0.38f, 0.48f),
            new Color(0.16f, 0.025f, 0.045f));

        CreateEnemyPart(
            "WarningCore",
            PrimitiveType.Cube,
            parent,
            new Vector3(0f, 0.37f, 0.39f),
            new Vector3(0.18f, 0.18f, 0.05f),
            new Color(0.95f, 0.14f, 0.12f));
    }

    private void BuildRangedEnemy(Transform parent)
    {
        CreateEnemyPart(
            "Core",
            PrimitiveType.Capsule,
            parent,
            new Vector3(0f, 0.05f, 0f),
            new Vector3(0.52f, 0.92f, 0.44f),
            new Color(0.2f, 0.055f, 0.08f));

        CreateEnemyPart(
            "FocusLens",
            PrimitiveType.Sphere,
            parent,
            new Vector3(0f, 0.22f, 0.34f),
            new Vector3(0.22f, 0.22f, 0.1f),
            new Color(1f, 0.18f, 0.42f));

        CreateEnemyPart(
            "Emitter",
            PrimitiveType.Cube,
            parent,
            new Vector3(0f, 0.03f, 0.55f),
            new Vector3(0.18f, 0.18f, 0.55f),
            new Color(0.22f, 0.08f, 0.14f));

        CreateEnemyPart(
            "EmitterTip",
            PrimitiveType.Sphere,
            parent,
            new Vector3(0f, 0.03f, 0.84f),
            new Vector3(0.22f, 0.22f, 0.22f),
            new Color(1f, 0.28f, 0.6f));
    }

    private void BuildEliteEnemy(Transform parent)
    {
        CreateEnemyPart(
            "Core",
            PrimitiveType.Capsule,
            parent,
            new Vector3(0f, 0.08f, 0f),
            new Vector3(0.72f, 1.05f, 0.58f),
            new Color(0.17f, 0.04f, 0.22f));

        CreateEnemyPart(
            "ChestSigil",
            PrimitiveType.Cube,
            parent,
            new Vector3(0f, 0.38f, 0.34f),
            new Vector3(0.3f, 0.3f, 0.08f),
            new Color(0.72f, 0.16f, 0.96f));

        CreateEnemyPart(
            "Crest",
            PrimitiveType.Cube,
            parent,
            new Vector3(0f, 0.76f, 0f),
            new Vector3(0.12f, 0.55f, 0.12f),
            new Color(0.48f, 0.08f, 0.68f));

        CreateEnemyPart(
            "LeftBlade",
            PrimitiveType.Cube,
            parent,
            new Vector3(-0.62f, 0.05f, 0f),
            new Vector3(0.08f, 0.7f, 0.18f),
            new Color(0.36f, 0.1f, 0.46f));

        CreateEnemyPart(
            "RightBlade",
            PrimitiveType.Cube,
            parent,
            new Vector3(0.62f, 0.05f, 0f),
            new Vector3(0.08f, 0.7f, 0.18f),
            new Color(0.36f, 0.1f, 0.46f));
    }

    private void BuildBossEnemy(Transform parent)
    {
        CreateEnemyPart(
            "Core",
            PrimitiveType.Sphere,
            parent,
            Vector3.zero,
            new Vector3(1.2f, 1.25f, 1.15f),
            new Color(0.14f, 0.025f, 0.045f));

        CreateEnemyPart(
            "CentralEye",
            PrimitiveType.Sphere,
            parent,
            new Vector3(0f, 0.05f, 0.96f),
            new Vector3(0.38f, 0.28f, 0.1f),
            new Color(1f, 0.24f, 0.05f));

        CreateEnemyPart(
            "LeftHorn",
            PrimitiveType.Cube,
            parent,
            new Vector3(-0.72f, 0.65f, 0f),
            new Vector3(0.18f, 0.7f, 0.18f),
            new Color(0.26f, 0.05f, 0.08f));

        CreateEnemyPart(
            "RightHorn",
            PrimitiveType.Cube,
            parent,
            new Vector3(0.72f, 0.65f, 0f),
            new Vector3(0.18f, 0.7f, 0.18f),
            new Color(0.26f, 0.05f, 0.08f));

        CreateEnemyPart(
            "EnergySpine",
            PrimitiveType.Cube,
            parent,
            new Vector3(0f, 0f, -0.82f),
            new Vector3(0.22f, 1.25f, 0.12f),
            new Color(0.9f, 0.18f, 0.04f));

        CreateEnemyPart(
            "AuraCore",
            PrimitiveType.Sphere,
            parent,
            new Vector3(0f, 0f, 0f),
            new Vector3(0.7f, 0.74f, 0.68f),
            new Color(0.42f, 0.08f, 0.1f));
    }

    private static GameObject CreateEnemyPart(
        string objectName,
        PrimitiveType primitiveType,
        Transform parent,
        Vector3 localPosition,
        Vector3 localScale,
        Color color)
    {
        GameObject part =
            GameObject.CreatePrimitive(primitiveType);

        if (part == null)
        {
            throw new System.InvalidOperationException(
                $"Failed to create enemy visual: {objectName}");
        }

        part.name = objectName;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = localScale;

        Collider collider =
            part.GetComponent<Collider>();

        if (collider != null)
        {
            Destroy(collider);
        }

        Renderer renderer =
            part.GetComponent<Renderer>();

        if (renderer != null)
        {
            Shader shader =
                Shader.Find(
                    "Universal Render Pipeline/Lit")
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
                material.SetFloat("_Metallic", 0.35f);
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", 0.82f);
            }

            if (color.g > 0.45f || color.b > 0.45f)
            {
                if (material.HasProperty("_EmissionColor"))
                {
                    material.EnableKeyword("_EMISSION");
                    material.SetColor(
                        "_EmissionColor",
                        color * 1.35f);
                }
            }

            renderer.material = material;
        }

        return part;
    }

    private void HandleDefeat()
    {
        if (state == StageState.Defeat
            || state == StageState.Clear)
        {
            return;
        }

        state = StageState.Defeat;
        CurrentPhaseLabel =
            "MISSION FAILED";

        gameFlow.DefeatStage();
    }

    public void NotifyEnemyDefeated(
        EnemyController enemy)
    {
        // BattleDirector owns defeat counting.
        // StageDirector only owns stage-state transitions.
    }
}
