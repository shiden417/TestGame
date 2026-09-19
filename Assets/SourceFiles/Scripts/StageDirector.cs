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

    [SerializeField] private float firstWaveDelay = 1f;

    private StageState state;
    private BattleDirector battleDirector;
    private Transform player;
    private GameFlowController gameFlow;
    private int waveNumber;
    private int currentWaveRemaining;

    public string CurrentPhaseLabel { get; private set; } = "MISSION START";
    public int WaveNumber => waveNumber;
    public int CurrentWaveRemaining => currentWaveRemaining;
    public bool IsCleared => state == StageState.Clear;
    public bool IsDefeated => state == StageState.Defeat;

    public void Initialize(
        BattleDirector battle,
        Transform playerTransform,
        GameFlowController flow)
    {
        if (battle == null)
        {
            throw new System.ArgumentNullException(nameof(battle));
        }

        if (playerTransform == null)
        {
            throw new System.ArgumentNullException(nameof(playerTransform));
        }

        if (flow == null)
        {
            throw new System.ArgumentNullException(nameof(flow));
        }

        battleDirector = battle;
        player = playerTransform;
        gameFlow = flow;
        state = StageState.Intro;
        StartCoroutine(RunStage());
    }

    private IEnumerator RunStage()
    {
        CurrentPhaseLabel = "MISSION START";
        yield return new WaitForSeconds(firstWaveDelay);

        state = StageState.Wave1;
        waveNumber = 1;
        CurrentPhaseLabel = "WAVE 1 // URBAN PATROL";
        yield return StartCoroutine(RunWave(12, false));

        state = StageState.Wave2;
        waveNumber = 2;
        CurrentPhaseLabel = "WAVE 2 // REINFORCEMENT";
        yield return StartCoroutine(RunWave(18, true));

        state = StageState.Elite;
        CurrentPhaseLabel = "ELITE UNIT DETECTED";
        SpawnEnemy(EnemyController.EnemyType.Elite, new Vector3(0f, 1f, 13f));
        yield return new WaitUntil(() => !battleDirector.HasLivingBoss() && FindLivingEliteCount() == 0 && battleDirector.ActiveEnemyCount == 0);

        state = StageState.Boss;
        CurrentPhaseLabel = "BOSS // OROCHI FRAME";
        SpawnBoss(new Vector3(0f, 1.5f, 16f));
        yield return new WaitUntil(() => !battleDirector.HasLivingBoss());

        state = StageState.Clear;
        CurrentPhaseLabel = "MISSION CLEAR";
        gameFlow.CompleteStage(battleDirector.TotalDefeated);
    }

    private IEnumerator RunWave(int count, bool includeHeavy)
    {
        currentWaveRemaining = count;

        for (int i = 0; i < count; i++)
        {
            EnemyController.EnemyType type = EnemyController.EnemyType.Basic;

            if (includeHeavy && i % 5 == 0)
            {
                type = EnemyController.EnemyType.Heavy;
            }
            else if (i % 7 == 0)
            {
                type = EnemyController.EnemyType.Ranged;
            }

            SpawnEnemy(type, GetSpawnPosition(i));
            currentWaveRemaining--;
            yield return new WaitForSeconds(0.18f);
        }

        yield return new WaitUntil(() => battleDirector.ActiveEnemyCount == 0);
    }

    private Vector3 GetSpawnPosition(int index)
    {
        float angle = index * 137.5f * Mathf.Deg2Rad;
        float radius = 10f + (index % 5) * 1.2f;
        return player.position + new Vector3(
            Mathf.Cos(angle) * radius,
            1f,
            Mathf.Sin(angle) * radius);
    }

    private void SpawnEnemy(EnemyController.EnemyType type, Vector3 position)
    {
        GameObject enemyObject = CreateEnemyVisual(type);
        enemyObject.transform.position = position;

        EnemyController enemy = enemyObject.AddComponent<EnemyController>();
        enemy.Initialize(
            type,
            player,
            type switch
            {
                EnemyController.EnemyType.Basic => 60f,
                EnemyController.EnemyType.Heavy => 100f,
                EnemyController.EnemyType.Ranged => 55f,
                EnemyController.EnemyType.Elite => 260f,
                _ => 100f
            },
            type switch
            {
                EnemyController.EnemyType.Basic => 2.8f,
                EnemyController.EnemyType.Heavy => 2f,
                EnemyController.EnemyType.Ranged => 2.2f,
                EnemyController.EnemyType.Elite => 3f,
                _ => 2f
            },
            type switch
            {
                EnemyController.EnemyType.Basic => 8f,
                EnemyController.EnemyType.Heavy => 14f,
                EnemyController.EnemyType.Ranged => 7f,
                EnemyController.EnemyType.Elite => 19f,
                _ => 10f
            },
            type switch
            {
                EnemyController.EnemyType.Basic => 1.8f,
                EnemyController.EnemyType.Heavy => 2.6f,
                EnemyController.EnemyType.Ranged => 2.2f,
                EnemyController.EnemyType.Elite => 1.5f,
                _ => 2f
            });

        enemyObject.AddComponent<Targetable>();
        battleDirector.Register(enemy);
    }

    private void SpawnBoss(Vector3 position)
    {
        GameObject bossObject = CreateEnemyVisual(EnemyController.EnemyType.Boss);
        bossObject.transform.position = position;
        bossObject.transform.localScale *= 2.3f;

        EnemyController enemy = bossObject.AddComponent<EnemyController>();
        enemy.Initialize(
            EnemyController.EnemyType.Boss,
            player,
            1100f,
            2.3f,
            24f,
            1.35f);

        bossObject.AddComponent<Targetable>();
        battleDirector.Register(enemy);
    }

    private GameObject CreateEnemyVisual(EnemyController.EnemyType type)
    {
        PrimitiveType primitive = type == EnemyController.EnemyType.Boss
            ? PrimitiveType.Sphere
            : PrimitiveType.Capsule;

        GameObject enemy = GameObject.CreatePrimitive(primitive);
        enemy.name = $"{type}Unit";

        Color color = type switch
        {
            EnemyController.EnemyType.Basic => new Color(0.65f, 0.08f, 0.12f),
            EnemyController.EnemyType.Heavy => new Color(0.38f, 0.08f, 0.12f),
            EnemyController.EnemyType.Ranged => new Color(0.85f, 0.25f, 0.1f),
            EnemyController.EnemyType.Elite => new Color(0.65f, 0.1f, 0.75f),
            EnemyController.EnemyType.Boss => new Color(0.95f, 0.45f, 0.08f),
            _ => Color.red
        };

        Renderer renderer = enemy.GetComponent<Renderer>();
        if (renderer != null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (shader != null)
            {
                renderer.material = new Material(shader) { color = color };
            }
        }

        return enemy;
    }

    private int FindLivingEliteCount()
    {
        EnemyController[] enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        int count = 0;

        foreach (EnemyController enemy in enemies)
        {
            if (enemy != null && enemy.IsAlive && enemy.IsElite && !enemy.IsBoss)
            {
                count++;
            }
        }

        return count;
    }

    public void NotifyEnemyDefeated(EnemyController enemy)
    {
        if (enemy != null && currentWaveRemaining > 0)
        {
            currentWaveRemaining = Mathf.Max(0, currentWaveRemaining - 1);
        }
    }
}
