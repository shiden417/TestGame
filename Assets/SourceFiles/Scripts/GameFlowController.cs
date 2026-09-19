using UnityEngine;

[DisallowMultipleComponent]
public sealed class GameFlowController : MonoBehaviour
{
    private bool stageCompleted;
    private bool stageDefeated;
    private int lastStageScore;
    private int playerLevel = 1;
    private int experience;
    private int upgradePoints;

    public bool StageCompleted => stageCompleted;
    public bool StageDefeated => stageDefeated;
    public bool IsGameOver => stageCompleted || stageDefeated;
    public int LastStageScore => lastStageScore;
    public int PlayerLevel => playerLevel;
    public int Experience => experience;
    public int UpgradePoints => upgradePoints;

    public void CompleteStage(int defeatedEnemies)
    {
        if (IsGameOver)
        {
            return;
        }

        stageCompleted = true;
        lastStageScore = Mathf.Max(0, defeatedEnemies);

        int gainedExperience = Mathf.Max(10, defeatedEnemies * 12);
        experience += gainedExperience;

        while (experience >= ExperienceToNextLevel())
        {
            experience -= ExperienceToNextLevel();
            playerLevel++;
            upgradePoints++;
        }

        if (upgradePoints == 0)
        {
            upgradePoints = 1;
        }
    }

    public void DefeatStage()
    {
        if (IsGameOver)
        {
            return;
        }

        stageDefeated = true;
    }

    public int ExperienceToNextLevel()
    {
        return 100 + (playerLevel - 1) * 50;
    }

    public void ResetStage()
    {
        stageCompleted = false;
        stageDefeated = false;
        lastStageScore = 0;
    }
}
