using UnityEngine;

[DisallowMultipleComponent]
public sealed class GameFlowController : MonoBehaviour
{
    private bool stageCompleted;
    private int lastStageScore;
    private int playerLevel = 1;
    private int upgradePoints;

    public bool StageCompleted => stageCompleted;
    public int LastStageScore => lastStageScore;
    public int PlayerLevel => playerLevel;
    public int UpgradePoints => upgradePoints;

    public void CompleteStage(int defeatedEnemies)
    {
        if (stageCompleted)
        {
            return;
        }

        stageCompleted = true;
        lastStageScore = Mathf.Max(0, defeatedEnemies);
        upgradePoints = Mathf.Max(1, lastStageScore / 10);
        playerLevel += Mathf.Max(1, upgradePoints / 2);
    }

    public void ResetStage()
    {
        stageCompleted = false;
        lastStageScore = 0;
        upgradePoints = 0;
    }
}
