using UnityEngine;

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

    private GameFlowController gameFlowController;
    private BattleDirector battleDirector;
    private StageDirector stageDirector;
    private GameHudController hudController;

    private void Start()
    {
        Application.targetFrameRate = 120;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        CreateCoreSystems();
        CreateWorld();
        CreatePlayer();
        CreateCamera();
        WirePlayerSystems();
        CreatePresentation();
        CreateHud();
    }

    private void CreateCoreSystems()
    {
        gameFlowController = CreateComponent<GameFlowController>("GameFlow");
        battleDirector = CreateComponent<BattleDirector>("BattleDirector");
        stageDirector = CreateComponent<StageDirector>("StageDirector");
    }

    private void CreateWorld()
    {
        WorldPresentationBuilder builder =
            CreateComponent<WorldPresentationBuilder>("WorldPresentation");

        builder.Build();
    }

    private void CreatePlayer()
    {
        GameObject playerObject =
            GameObject.CreatePrimitive(PrimitiveType.Capsule);

        playerObject.name = "Player";
        playerObject.transform.position =
            new Vector3(0f, 1.1f, -9f);
        playerObject.transform.localScale =
            Vector3.one;

        ApplyMaterial(
            playerObject,
            new Color(0.08f, 0.34f, 0.55f));

        Collider primitiveCollider =
            playerObject.GetComponent<Collider>();

        if (primitiveCollider != null)
        {
            Destroy(primitiveCollider);
        }

        CharacterController characterController =
            playerObject.AddComponent<CharacterController>();

        characterController.height = 2f;
        characterController.radius = 0.48f;
        characterController.center = Vector3.zero;
        characterController.stepOffset = 0.3f;
        characterController.slopeLimit = 50f;

        gameInput = playerObject.AddComponent<GameInput>();
        targetingSystem = playerObject.AddComponent<TargetingSystem>();
        dodgeController = playerObject.AddComponent<DodgeController>();
        playerHealth = playerObject.AddComponent<PlayerHealth>();
        perfectDodgeSystem =
            playerObject.AddComponent<PerfectDodgeSystem>();
        combatController =
            playerObject.AddComponent<PlayerCombatController>();
        skillController =
            playerObject.AddComponent<PlayerSkillController>();
        playerController =
            playerObject.AddComponent<PlayerController>();

        CreatePlayerArmor(playerObject);

        player = playerObject.transform;
    }

    private void CreatePlayerArmor(GameObject playerObject)
    {
        CreatePlayerPart(
            "ArmorCore",
            playerObject.transform,
            new Vector3(0f, 0.15f, 0f),
            new Vector3(0.9f, 0.95f, 0.7f),
            new Color(0.12f, 0.16f, 0.22f));

        CreatePlayerPart(
            "LeftShoulder",
            playerObject.transform,
            new Vector3(-0.58f, 0.45f, 0f),
            new Vector3(0.28f, 0.45f, 0.5f),
            new Color(0.55f, 0.1f, 0.2f));

        CreatePlayerPart(
            "RightShoulder",
            playerObject.transform,
            new Vector3(0.58f, 0.45f, 0f),
            new Vector3(0.28f, 0.45f, 0.5f),
            new Color(0.55f, 0.1f, 0.2f));

        CreatePlayerPart(
            "BackEmitter",
            playerObject.transform,
            new Vector3(0f, 0.15f, -0.55f),
            new Vector3(0.45f, 0.75f, 0.18f),
            new Color(0.06f, 0.75f, 1f));
    }

    private static void CreatePlayerPart(
        string objectName,
        Transform parent,
        Vector3 localPosition,
        Vector3 localScale,
        Color color)
    {
        GameObject part =
            GameObject.CreatePrimitive(PrimitiveType.Cube);

        part.name = objectName;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = localScale;

        Collider collider = part.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        ApplyMaterial(part, color);
    }

    private void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera");

        Camera camera = cameraObject.AddComponent<Camera>();
        cameraObject.tag = "MainCamera";
        camera.fieldOfView = 64f;
        camera.nearClipPlane = 0.05f;
        camera.farClipPlane = 240f;

        cameraController =
            cameraObject.AddComponent<ThirdPersonCameraController>();

        cameraObject.transform.position =
            new Vector3(0f, 5.8f, -16f);
    }

    private void WirePlayerSystems()
    {
        CharacterController characterController =
            player.GetComponent<CharacterController>();

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
            || playerController == null)
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

        combatController.Initialize(
            gameInput,
            targetingSystem,
            dodgeController,
            perfectDodgeSystem);

        skillController.Initialize(
            gameInput,
            targetingSystem,
            perfectDodgeSystem);

        playerController.Initialize(
            gameInput,
            cameraController,
            targetingSystem,
            dodgeController);

        battleDirector.Initialize(
            player,
            stageDirector);

        stageDirector.Initialize(
            battleDirector,
            player,
            gameFlowController);
    }

    private void CreatePresentation()
    {
        // Presentation is built by WorldPresentationBuilder.
        // This method intentionally keeps the bootstrap responsibility explicit.
    }

    private void CreateHud()
    {
        GameObject hudObject =
            new GameObject("GameHUD");

        hudController =
            hudObject.AddComponent<GameHudController>();

        hudController.Initialize(
            gameFlowController,
            stageDirector,
            battleDirector,
            playerHealth,
            skillController,
            perfectDodgeSystem);
    }

    private static T CreateComponent<T>(string objectName)
        where T : Component
    {
        GameObject gameObject =
            new GameObject(objectName);

        T component = gameObject.AddComponent<T>();

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
            Shader.Find("Universal Render Pipeline/Lit")
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
