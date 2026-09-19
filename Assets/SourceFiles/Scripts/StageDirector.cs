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

    public string CurrentPhaseLabel { get; private set; } = "MISSION START";
    public int WaveNumber => waveNumber;
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
        playerHealth = player.GetComponent<PlayerHealth>();

        if (playerHealth == null)
        {
            throw new System.InvalidOperationException(
                "StageDirector requires PlayerHealth on the player.");
        }

        gameFlow = flow;
        state = StageState.Intro;
        StartCoroutine(RunStage());
    }

    private IEnumerator RunStage()
    {
        CurrentPhaseLabel = "MISSION START // FUTURE ASHES";
        yield return new WaitForSeconds(firstWaveDelay);

        if (!playerHealth.IsAlive)
        {
            HandleDefeat();
            yield break;
        }

        state = StageState.Wave1;
        waveNumber = 1;
        CurrentPhaseLabel = "WAVE 1 // PURGE SECTOR";
        yield return StartCoroutine(RunWave(18, false));

        if (state == StageState.Defeat)
        {
            yield break;
        }

        state = StageState.Wave2;
        waveNumber = 2;
        CurrentPhaseLabel = "WAVE 2 // REINFORCEMENT";
        yield return StartCoroutine(RunWave(24, true));

        if (state == StageState.Defeat)
        {
            yield break;
        }

        state = StageState.Elite;
        CurrentPhaseLabel = "ELITE SIGNATURE // KAGE FRAME";
        SpawnEnemy(
            EnemyController.EnemyType.Elite,
            player.position + new Vector3(0f, 1f, 14f),
            1);

        yield return StartCoroutine(WaitForBattlefieldClear());

        if (state == StageState.Defeat)
        {
            yield break;
        }

        state = StageState.Boss;
        CurrentPhaseLabel = "BOSS // OROCHI FRAME";
        SpawnEnemy(
            EnemyController.EnemyType.Boss,
            player.position + new Vector3(0f, 1.5f, 16f),
            2.4f);

        yield return StartCoroutine(WaitForBattlefieldClear());

        if (state == StageState.Defeat)
        {
            yield break;
        }

        state = StageState.Clear;
        CurrentPhaseLabel = "MISSION CLEAR";
        gameFlow.CompleteStage(battleDirector.TotalDefeated);
    }

    private IEnumerator RunWave(int count, bool includeSpecialEnemies)
    {
        for (int i = 0; i < count; i++)
        {
            if (!playerHealth.IsAlive)
            {
                HandleDefeat();
                yield break;
            }

            EnemyController.EnemyType type = EnemyController.EnemyType.Basic;

            if (includeSpecialEnemies && i % 6 == 0)
            {
                type = EnemyController.EnemyType.Heavy;
            }
            else if (i % 8 == 0)
            {
                type = EnemyController.EnemyType.Ranged;
            }

            SpawnEnemy(type, GetSpawnPosition(i), 1f);
            yield return new WaitForSeconds(0.12f);
        }

        yield return StartCoroutine(WaitForBattlefieldClear());
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
        float angle = index * 137.5f * Mathf.Deg2Rad;
        float radius = 11f + (index % 5) * 1.2f;

        return player.position + new Vector3(
            Mathf.Cos(angle) * radius,
            1f,
            Mathf.Sin(angle) * radius);
    }

    private void SpawnEnemy(
        EnemyController.EnemyType type,
        Vector3 position,
        float scaleMultiplier)
    {
        GameObject enemyObject = CreateEnemyVisual(type);
        enemyObject.transform.position = position;
        enemyObject.transform.localScale *= scaleMultiplier;

        EnemyController enemy = enemyObject.AddComponent<EnemyController>();

        switch (type)
        {
            case EnemyController.EnemyType.Basic:
                enemy.Initialize(type, player, battleDirector, 60f, 3f, 8f, 1.7f);
                break;

            case EnemyController.EnemyType.Heavy:
                enemy.Initialize(type, player, battleDirector, 105f, 2f, 14f, 2.4f);
                break;

            case EnemyController.EnemyType.Ranged:
                enemy.Initialize(type, player, battleDirector, 55f, 2.4f, 7f, 2f);
                break;

            case EnemyController.EnemyType.Elite:
                enemy.Initialize(type, player, battleDirector, 260f, 3.1f, 19f, 1.45f);
                break;

            case EnemyController.EnemyType.Boss:
                enemy.Initialize(type, player, battleDirector, 1200f, 2.35f, 24f, 1.2f);
                break;
        }

        enemyObject.AddComponent<Targetable>();
        battleDirector.Register(enemy);
    }

    private GameObject CreateEnemyVisual(EnemyController.EnemyType type)
    {
        PrimitiveType primitive =
            type == EnemyController.EnemyType.Boss
                ? PrimitiveType.Sphere
                : PrimitiveType.Capsule;

        GameObject enemyObject = GameObject.CreatePrimitive(primitive);
        enemyObject.name = $"{type}Unit";

        Color color = type switch
        {
            EnemyController.EnemyType.Basic => new Color(0.62f, 0.06f, 0.12f),
            EnemyController.EnemyType.Heavy => new Color(0.3f, 0.04f, 0.08f),
            EnemyController.EnemyType.Ranged => new Color(0.95f, 0.22f, 0.08f),
            EnemyController.EnemyType.Elite => new Color(0.68f, 0.12f, 0.85f),
            EnemyController.EnemyType.Boss => new Color(0.95f, 0.48f, 0.05f),
            _ => Color.red
        };

        Renderer renderer = enemyObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard");

            if (shader != null)
            {
                renderer.material = new Material(shader)
                {
                    color = color
                };
            }
        }

        return enemyObject;
    }

    public void NotifyEnemyDefeated(EnemyController enemy)
    {
        if (state == StageState.Clear || state == StageState.Defeat)
        {
            return;
        }
    }

    private void HandleDefeat()
    {
        if (state == StageState.Defeat || state == StageState.Clear)
        {
            return;
        }

        state = StageState.Defeat;
        CurrentPhaseLabel = "MISSION FAILED";
        gameFlow.DefeatStage();
    }
}
