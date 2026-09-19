using UnityEngine;
using UnityEngine.Rendering.Universal;

[DisallowMultipleComponent]
public sealed class GameBootstrap : MonoBehaviour
{
    private Transform player;
    private GameInput gameInput;
    private PlayerController playerController;
    private DodgeController dodgeController;
    private PlayerHealth playerHealth;
    private PerfectDodgeSystem perfectDodgeSystem;
    private PlayerCombatController combatController;
    private PlayerSkillController skillController;
    private TargetingSystem targetingSystem;
    private ThirdPersonCameraController cameraController;
    private PlayerMotionVisuals playerMotionVisuals;

    private GameFlowController gameFlowController;
    private BattleDirector battleDirector;
    private StageDirector stageDirector;

    private void Start()
    {
        Application.targetFrameRate = 120;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        CreateCoreSystems();
        CreateCamera();
        CreateWorld();
        CreatePlayer();
        WirePlayerSystems();
        CreateHud();
    }

    private void CreateCoreSystems()
    {
        CreateComponent<AudioDirector>(
            "AudioDirector");

        gameFlowController =
            CreateComponent<GameFlowController>(
                "GameFlow");

        battleDirector =
            CreateComponent<BattleDirector>(
                "BattleDirector");

        stageDirector =
            CreateComponent<StageDirector>(
                "StageDirector");
    }

    private void CreateWorld()
    {
        WorldPresentationBuilder builder =
            CreateComponent<WorldPresentationBuilder>(
                "WorldPresentation");

        builder.Build();
    }

    private void CreatePlayer()
    {
        GameObject playerObject =
            GameObject.CreatePrimitive(
                PrimitiveType.Capsule);

        playerObject.name = "Player";
        playerObject.transform.position =
            new Vector3(
                0f,
                1.1f,
                -9f);

        ApplyMaterial(
            playerObject,
            new Color(
                0.08f,
                0.34f,
                0.55f));

        Renderer placeholderRenderer =
            playerObject.GetComponent<Renderer>();

        if (placeholderRenderer != null)
        {
            placeholderRenderer.enabled = false;
        }

        Collider primitiveCollider =
            playerObject.GetComponent<Collider>();

        if (primitiveCollider != null)
        {
            Destroy(primitiveCollider);
        }

        CharacterController characterController =
            playerObject.AddComponent<
                CharacterController>();

        characterController.height = 2f;
        characterController.radius = 0.48f;
        characterController.center =
            Vector3.zero;
        characterController.stepOffset = 0.3f;
        characterController.slopeLimit = 50f;

        gameInput =
            playerObject.AddComponent<
                GameInput>();

        targetingSystem =
            playerObject.AddComponent<
                TargetingSystem>();

        dodgeController =
            playerObject.AddComponent<
                DodgeController>();

        playerHealth =
            playerObject.AddComponent<
                PlayerHealth>();

        perfectDodgeSystem =
            playerObject.AddComponent<
                PerfectDodgeSystem>();

        combatController =
            playerObject.AddComponent<
                PlayerCombatController>();

        skillController =
            playerObject.AddComponent<
                PlayerSkillController>();

        playerController =
            playerObject.AddComponent<
                PlayerController>();

        CreatePlayerAppearance(playerObject);

        playerMotionVisuals =
            playerObject.AddComponent<PlayerMotionVisuals>();

        if (playerMotionVisuals == null)
        {
            throw new System.InvalidOperationException(
                "PlayerMotionVisuals could not be created.");
        }

        player =
            playerObject.transform;
    }

    private void CreatePlayerAppearance(
        GameObject playerObject)
    {
        PlayerAppearanceBuilder appearanceBuilder =
            playerObject.AddComponent<
                PlayerAppearanceBuilder>();

        if (appearanceBuilder == null)
        {
            throw new System.InvalidOperationException(
                "PlayerAppearanceBuilder could not be created.");
        }

        appearanceBuilder.Build(
            playerObject.transform);
    }

    private void CreateCamera()
    {
        GameObject cameraObject =
            new GameObject("Main Camera");

        Camera camera =
            cameraObject.AddComponent<Camera>();

        if (camera == null)
        {
            throw new System.InvalidOperationException(
                "Main Camera could not be created.");
        }

        cameraObject.tag =
            "MainCamera";

        camera.enabled = true;
        camera.fieldOfView = 64f;
        camera.nearClipPlane = 0.05f;
        camera.farClipPlane = 240f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.008f, 0.012f, 0.024f);

        UniversalAdditionalCameraData cameraData =
            cameraObject.GetComponent<UniversalAdditionalCameraData>();

        if (cameraData == null)
        {
            cameraData =
                cameraObject.AddComponent<UniversalAdditionalCameraData>();
        }

        if (cameraData == null)
        {
            throw new System.InvalidOperationException(
                "Universal Additional Camera Data could not be created.");
        }

        cameraData.renderType = CameraRenderType.Base;

        cameraController =
            cameraObject.AddComponent<
                ThirdPersonCameraController>();

        cameraObject.transform.position =
            new Vector3(
                0f,
                5.8f,
                -16f);
    }

    private void WirePlayerSystems()
    {
        CharacterController characterController =
            player.GetComponent<
                CharacterController>();

        if (characterController == null)
        {
            throw new System.InvalidOperationException(
                "Player CharacterController was not created.");
        }

        if (cameraController == null
            || gameInput == null
            || targetingSystem == null
            || dodgeController == null
            || playerHealth == null
            || perfectDodgeSystem == null
            || combatController == null
            || skillController == null
            || playerController == null
            || playerMotionVisuals == null)
        {
            throw new System.InvalidOperationException(
                "One or more player systems could not be initialized.");
        }

        targetingSystem.Initialize(
            gameInput,
            cameraController);

        cameraController.Initialize(
            player,
            gameInput,
            targetingSystem);

        dodgeController.Initialize(
            characterController,
            gameInput,
            cameraController);

        playerHealth.Initialize(
            dodgeController);

        perfectDodgeSystem.Initialize(
            dodgeController);

        skillController.Initialize(
            gameInput,
            perfectDodgeSystem);

        combatController.Initialize(
            gameInput,
            targetingSystem,
            dodgeController,
            perfectDodgeSystem,
            skillController);

        playerController.Initialize(
            gameInput,
            cameraController,
            targetingSystem,
            dodgeController);

        playerMotionVisuals.Initialize(
            playerController,
            dodgeController,
            combatController);

        battleDirector.Initialize(
            player,
            stageDirector);

        stageDirector.Initialize(
            battleDirector,
            player,
            gameFlowController);
    }

    private void CreateHud()
    {
        GameObject hudObject =
            new GameObject(
                "GameHUD");

        GameHudController hudController =
            hudObject.AddComponent<
                GameHudController>();

        hudController.Initialize(
            gameFlowController,
            stageDirector,
            battleDirector,
            playerHealth,
            skillController,
            perfectDodgeSystem);
    }

    private static T CreateComponent<T>(
        string objectName)
        where T : Component
    {
        GameObject gameObject =
            new GameObject(objectName);

        T component =
            gameObject.AddComponent<T>();

        if (component == null)
        {
            throw new System.InvalidOperationException(
                $"Failed to create component: {typeof(T).Name}");
        }

        return component;
    }

    private static void ApplyMaterial(
        GameObject target,
        Color color)
    {
        Renderer renderer =
            target.GetComponent<Renderer>();

        if (renderer == null)
        {
            return;
        }

        Shader shader =
            Shader.Find(
                "Universal Render Pipeline/Lit")
            ?? Shader.Find("Standard");

        if (shader == null)
        {
            throw new System.InvalidOperationException(
                "No compatible Unity material shader was found.");
        }

        renderer.material =
            new Material(shader)
            {
                color = color
            };
    }
}
