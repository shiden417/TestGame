using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class BattleDirector : MonoBehaviour
{
    [SerializeField] private int maximumSimultaneousAttackers = 5;

    private readonly List<EnemyController> activeEnemies = new();

    private Transform player;
    private StageDirector stageDirector;
    private int totalDefeated;
    private int totalSpawned;

    public int ActiveEnemyCount => activeEnemies.Count;
    public int TotalDefeated => totalDefeated;
    public int TotalSpawned => totalSpawned;

    public void Initialize(Transform playerTransform, StageDirector stage)
    {
        if (playerTransform == null)
        {
            throw new System.ArgumentNullException(nameof(playerTransform));
        }

        if (stage == null)
        {
            throw new System.ArgumentNullException(nameof(stage));
        }

        player = playerTransform;
        stageDirector = stage;
    }

    public void Register(EnemyController enemy)
    {
        if (enemy == null || activeEnemies.Contains(enemy))
        {
            return;
        }

        activeEnemies.Add(enemy);
        totalSpawned++;
    }

    private void Update()
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            EnemyController enemy = activeEnemies[i];

            if (enemy == null)
            {
                activeEnemies.RemoveAt(i);
                continue;
            }

            if (!enemy.IsAlive)
            {
                activeEnemies.RemoveAt(i);
                totalDefeated++;
                stageDirector?.NotifyEnemyDefeated(enemy);
            }
        }
    }

    public bool CanEnemyAttack(EnemyController candidate)
    {
        if (candidate == null || candidate.IsBoss)
        {
            return true;
        }

        int attackers = 0;

        foreach (EnemyController enemy in activeEnemies)
        {
            if (enemy == null || !enemy.IsAlive || enemy.IsBoss || enemy == candidate)
            {
                continue;
            }

            if (!enemy.IsAttackCommitmentActive)
            {
                continue;
            }

            if (player != null)
            {
                float distance = Vector3.Distance(
                    new Vector3(enemy.transform.position.x, 0f, enemy.transform.position.z),
                    new Vector3(player.position.x, 0f, player.position.z));

                if (distance > 7f)
                {
                    continue;
                }
            }

            attackers++;
            if (attackers >= maximumSimultaneousAttackers)
            {
                return false;
            }
        }

        return true;
    }

    public bool HasLivingBoss()
    {
        foreach (EnemyController enemy in activeEnemies)
        {
            if (enemy != null && enemy.IsAlive && enemy.IsBoss)
            {
                return true;
            }
        }

        return false;
    }

    public EnemyController GetLivingBoss()
    {
        foreach (EnemyController enemy in activeEnemies)
        {
            if (enemy != null && enemy.IsAlive && enemy.IsBoss)
            {
                return enemy;
            }
        }

        return null;
    }

    public float GetBossHealth()
    {
        EnemyController boss = GetLivingBoss();
        return boss != null ? boss.CurrentHealth : 0f;
    }
}
