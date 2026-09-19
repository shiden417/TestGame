using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class GameBootstrap : MonoBehaviour
{
    private readonly List<GameObject> targets = new();

    private GameInput gameInput;
    private Transform player;
    private ThirdPersonCameraController cameraController;
    private TargetingSystem targetingSystem;
    private PlayerCombatController combatController;
    private DodgeController dodgeController;
    private Text statusText;

    private void Start()
    {
        CreateWorld();
        CreatePlayer();
        CreateCamera();
        CreateTargets();
        CreateHud();
    }

    private void Update()
    {
        CleanupTargets();
        UpdateHud();
    }

    private void CreateWorld()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "FuturisticArena";
        ground.transform.localScale = new Vector3(2.75f, 1f, 2.75f);
        ApplyMaterial(ground, new Color(0.035f, 0.055f, 0.075f));

        CreateBoundary(new Vector3(0f, 1.25f, 27.5f), new Vector3(55f, 2.5f, 1f));
        CreateBoundary(new Vector3(0f, 1.25f, -27.5f), new Vector3(55f, 2.5f, 1f));
        CreateBoundary(new Vector3(27.5f, 1.25f, 0f), new Vector3(1f, 2.5f, 55f));
        CreateBoundary(new Vector3(-27.5f, 1.25f, 0f), new Vector3(1f, 2.5f, 55f));

        Vector3[] pillarPositions =
        {
            new(-14f, 3f, 12f),
            new(14f, 3f, 12f),
            new(-14f, 3f, -12f),
            new(14f, 3f, -12f)
        };

        foreach (Vector3 position in pillarPositions)
        {
            GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pillar.name = "RuinedFuturePillar";
            pillar.transform.position = position;
            pillar.transform.localScale = new Vector3(1.6f, 6f, 1.6f);
            ApplyMaterial(pillar, new Color(0.08f, 0.11f, 0.15f));
        }

        GameObject lightObject = new GameObject("Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.15f;
        light.color = new Color(0.82f, 0.9f, 1f);
        lightObject.transform.rotation = Quaternion.Euler(48f, -28f, 0f);

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.06f, 0.08f, 0.12f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.025f, 0.04f, 0.065f);
        RenderSettings.fogDensity = 0.012f;
    }

    private void CreatePlayer()
    {
        GameObject playerObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        playerObject.name = "Player";
        playerObject.transform.position = new Vector3(0f, 1.1f, 0f);
        ApplyMaterial(playerObject, new Color(0.12f, 0.52f, 0.8f));

        Collider primitiveCollider = playerObject.GetComponent<Collider>();
        if (primitiveCollider != null)
        {
            Destroy(primitiveCollider);
        }

        CharacterController characterController = playerObject.AddComponent<CharacterController>();
        characterController.height = 2f;
        characterController.radius = 0.48f;
        characterController.center = Vector3.zero;
        characterController.stepOffset = 0.3f;
        characterController.slopeLimit = 50f;

        gameInput = playerObject.AddComponent<GameInput>();
        targetingSystem = playerObject.AddComponent<TargetingSystem>();
        dodgeController = playerObject.AddComponent<DodgeController>();
        combatController = playerObject.AddComponent<PlayerCombatController>();
        PlayerController playerController = playerObject.AddComponent<PlayerController>();

        player = playerObject.transform;
    }

    private void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        cameraObject.tag = "MainCamera";
        camera.fieldOfView = 62f;
        camera.nearClipPlane = 0.05f;
        camera.farClipPlane = 200f;

        cameraController = cameraObject.AddComponent<ThirdPersonCameraController>();
        targetingSystem.Initialize(gameInput, cameraController);
        cameraController.Initialize(player, gameInput, targetingSystem);

        dodgeController.Initialize(player.GetComponent<CharacterController>(), gameInput, cameraController);
        combatController.Initialize(gameInput, targetingSystem, dodgeController);

        PlayerController playerController = player.GetComponent<PlayerController>();
        playerController.Initialize(
            gameInput,
            cameraController,
            targetingSystem,
            dodgeController);
    }

    private void CreateTargets()
    {
        Vector3[] positions =
        {
            new(-6f, 1f, 7f),
            new(0f, 1f, 9f),
            new(6f, 1f, 7f),
            new(-8f, 1f, 0f),
            new(8f, 1f, 0f),
            new(-5f, 1f, -8f),
            new(5f, 1f, -8f),
            new(0f, 1f, -11f)
        };

        for (int i = 0; i < positions.Length; i++)
        {
            GameObject targetObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            targetObject.name = $"TrainingUnit_{i + 1:00}";
            targetObject.transform.position = positions[i];
            targetObject.transform.localScale = new Vector3(1.15f, 2f, 1.15f);
            ApplyMaterial(targetObject, new Color(0.45f, 0.08f, 0.12f));

            targetObject.AddComponent<Targetable>();
            targetObject.AddComponent<CombatTarget>();

            targets.Add(targetObject);
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

        statusText = CreateText(
            canvasObject.transform,
            "Status",
            new Vector2(36f, -30f),
            new Vector2(720f, 180f),
            27);

        statusText.text =
            "Phase 1 Prototype\n" +
            "左スティック: 移動    右スティック: カメラ\n" +
            "RT/R2: 通常攻撃    RB/R1: 回避    A/×: ジャンプ    LT/L2: ロックオン";
    }

    private Text CreateText(
        Transform parent,
        string objectName,
        Vector2 anchoredPosition,
        Vector2 size,
        int fontSize)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);

        Text text = textObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = TextAnchor.UpperLeft;
        text.color = Color.white;

        RectTransform rect = text.rectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        return text;
    }

    private void CleanupTargets()
    {
        for (int i = targets.Count - 1; i >= 0; i--)
        {
            GameObject target = targets[i];
            if (target == null || !target.activeSelf)
            {
                if (target != null)
                {
                    Destroy(target);
                }

                targets.RemoveAt(i);
            }
        }
    }

    private void UpdateHud()
    {
        if (statusText == null)
        {
            return;
        }

        Targetable target = targetingSystem != null
            ? targetingSystem.CurrentTargetable
            : null;

        int activeTargets = 0;
        foreach (GameObject targetObject in targets)
        {
            if (targetObject != null && targetObject.activeSelf)
            {
                activeTargets++;
            }
        }

        string targetStatus = target != null
            ? $"LOCK-ON: {target.DisplayName}"
            : "LOCK-ON: OFF";

        int comboStep = combatController != null ? combatController.ComboStep : 0;
        string attackStatus = combatController != null && combatController.IsAttacking
            ? $"ATTACK COMBO {comboStep}"
            : "READY";

        statusText.text =
            "PHASE 1 // FUTURE ACTION PROTOTYPE\n" +
            "左スティック: 移動    右スティック: カメラ\n" +
            "RT/R2: 攻撃    RB/R1: 回避    A/×: ジャンプ    LT/L2: ロックオン    R3: ターゲット切替\n" +
            $"{targetStatus}    {attackStatus}    Training Units: {activeTargets}";
    }

    private void CreateBoundary(Vector3 position, Vector3 scale)
    {
        GameObject boundary = GameObject.CreatePrimitive(PrimitiveType.Cube);
        boundary.name = "ArenaBoundary";
        boundary.transform.position = position;
        boundary.transform.localScale = scale;
        ApplyMaterial(boundary, new Color(0.02f, 0.03f, 0.05f));
    }

    private static void ApplyMaterial(GameObject target, Color color)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit")
            ?? Shader.Find("Standard");

        if (shader == null)
        {
            throw new System.InvalidOperationException("No compatible Unity material shader was found.");
        }

        Material material = new Material(shader)
        {
            color = color
        };

        renderer.material = material;
    }
}
