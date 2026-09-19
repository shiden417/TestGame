using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class GameHudController : MonoBehaviour
{
    private Text statusText;
    private Text combatText;
    private Text playerText;
    private Text bossText;
    private Text messageText;

    private GameFlowController gameFlow;
    private StageDirector stageDirector;
    private BattleDirector battleDirector;
    private PlayerHealth playerHealth;
    private PlayerSkillController skills;
    private PerfectDodgeSystem perfectDodge;

    public void Initialize(
        GameFlowController flow,
        StageDirector stage,
        BattleDirector battle,
        PlayerHealth health,
        PlayerSkillController skillController,
        PerfectDodgeSystem perfect)
    {
        if (flow == null || stage == null || battle == null || health == null || skillController == null || perfect == null)
        {
            throw new System.ArgumentNullException("HUD dependency");
        }

        gameFlow = flow;
        stageDirector = stage;
        battleDirector = battle;
        playerHealth = health;
        skills = skillController;
        perfectDodge = perfect;

        CreateHud();
    }

    private void Update()
    {
        if (statusText == null)
        {
            return;
        }

        statusText.text =
            $"MISSION // {stageDirector.CurrentPhaseLabel}\n" +
            $"WAVE {stageDirector.WaveNumber}    ENEMIES {battleDirector.ActiveEnemyCount}    DEFEATED {battleDirector.TotalDefeated}";

        playerText.text =
            $"HP {Mathf.CeilToInt(playerHealth.CurrentHealth)} / {Mathf.CeilToInt(playerHealth.MaxHealth)}\n" +
            $"ULT {Mathf.RoundToInt(skills.UltimateGauge)}%";

        combatText.text =
            "左スティック 移動   右スティック カメラ\n" +
            "RT/R2 攻撃   RB/R1 回避   A/× ジャンプ\n" +
            "X/□ スキル1   Y/△ スキル2   LB/L1 必殺技\n" +
            "LT/L2 ロックオン   R3 ターゲット切替";

        bossText.text = battleDirector.HasLivingBoss()
            ? $"BOSS HP {Mathf.CeilToInt(battleDirector.GetBossHealth())}"
            : string.Empty;

        if (perfectDodge.CounterWindowActive)
        {
            messageText.text = "PERFECT DODGE // COUNTER READY";
        }
        else if (gameFlow.StageCompleted)
        {
            messageText.text =
                $"MISSION CLEAR\nDEFEATED {gameFlow.LastStageScore}\nLEVEL {gameFlow.PlayerLevel}";
        }
        else
        {
            messageText.text = "FUTURE ASHES";
        }
    }

    private void CreateHud()
    {
        GameObject canvasObject = new GameObject("HUD");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        canvasObject.AddComponent<GraphicRaycaster>();

        statusText = CreateText(canvasObject.transform, "Status", new Vector2(36f, -30f), new Vector2(900f, 100f), 28);
        playerText = CreateText(canvasObject.transform, "Player", new Vector2(36f, -150f), new Vector2(500f, 100f), 26);
        combatText = CreateText(canvasObject.transform, "Combat", new Vector2(36f, 1020f), new Vector2(900f, 180f), 21);
        bossText = CreateText(canvasObject.transform, "Boss", new Vector2(1050f, -40f), new Vector2(700f, 80f), 30);
        messageText = CreateText(canvasObject.transform, "Message", new Vector2(760f, -75f), new Vector2(900f, 180f), 34);

        messageText.alignment = TextAnchor.UpperCenter;
        bossText.alignment = TextAnchor.UpperRight;
    }

    private static Text CreateText(
        Transform parent,
        string objectName,
        Vector2 position,
        Vector2 size,
        int fontSize)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);

        Text text = textObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = TextAnchor.UpperLeft;

        RectTransform rect = text.rectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        return text;
    }
}
