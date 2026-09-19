using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class BattleDirector : MonoBehaviour
{
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

    public int GetActiveAttackerBudget()
    {
        if (player == null)
        {
            return 0;
        }

        int nearby = 0;
        foreach (EnemyController enemy in activeEnemies)
        {
            if (enemy == null || !enemy.IsAlive || enemy.IsBoss)
            {
                continue;
            }

            float distance = Vector3.Distance(enemy.transform.position, player.position);
            if (distance <= 5f)
            {
                nearby++;
            }
        }

        return Mathf.Min(6, nearby);
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

    public float GetBossHealth()
    {
        foreach (EnemyController enemy in activeEnemies)
        {
            if (enemy != null && enemy.IsAlive && enemy.IsBoss)
            {
                return enemy.CurrentHealth;
            }
        }

        return 0f;
    }
}
