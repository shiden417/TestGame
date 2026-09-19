using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameBootstrap : MonoBehaviour
{
    private readonly List<GameObject> pickups = new();
    private Transform player;
    private Text scoreText;
    private int score;

    private void Start()
    {
        CreateWorld();
        CreatePlayer();
        CreateCamera();
        CreateUI();
        CreatePickups();
    }

    private void Update()
    {
        if (player == null)
        {
            return;
        }

        if (player.position.y < -5f)
        {
            player.position = new Vector3(0f, 1.5f, 0f);
        }

        for (int i = pickups.Count - 1; i >= 0; i--)
        {
            GameObject pickup = pickups[i];
            if (pickup == null)
            {
                pickups.RemoveAt(i);
                continue;
            }

            pickup.transform.Rotate(0f, 120f * Time.deltaTime, 0f, Space.World);
            pickup.transform.position = new Vector3(
                pickup.transform.position.x,
                1.1f + Mathf.Sin(Time.time * 2f + i) * 0.2f,
                pickup.transform.position.z);

            if (Vector3.Distance(player.position, pickup.transform.position) < 1.2f)
            {
                Destroy(pickup);
                pickups.RemoveAt(i);
                score++;
                UpdateScore();
            }
        }
    }

    private void CreateWorld()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(3f, 1f, 3f);
        ApplyMaterial(ground, new Color(0.18f, 0.22f, 0.28f));

        CreateWall(new Vector3(0f, 1f, 15f), new Vector3(30f, 2f, 1f));
        CreateWall(new Vector3(0f, 1f, -15f), new Vector3(30f, 2f, 1f));
        CreateWall(new Vector3(15f, 1f, 0f), new Vector3(1f, 2f, 30f));
        CreateWall(new Vector3(-15f, 1f, 0f), new Vector3(1f, 2f, 30f));

        GameObject lightObject = new GameObject("Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        RenderSettings.ambientIntensity = 0.8f;
    }

    private void CreatePlayer()
    {
        GameObject playerObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        playerObject.name = "Player";
        playerObject.transform.position = new Vector3(0f, 1.1f, 0f);
        ApplyMaterial(playerObject, new Color(0.15f, 0.65f, 1f));

        Destroy(playerObject.GetComponent<CapsuleCollider>());

        CharacterController controller = playerObject.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.5f;
        controller.center = new Vector3(0f, 0f, 0f);

        playerObject.AddComponent<PlayerController>();
        player = playerObject.transform;
    }

    private void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.tag = "MainCamera";
        camera.fieldOfView = 60f;
        cameraObject.AddComponent<CameraFollow>().Target = player;
        cameraObject.transform.position = new Vector3(0f, 7f, -10f);
    }

    private void CreateUI()
    {
        GameObject canvasObject = new GameObject("HUD");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject textObject = new GameObject("Score");
        textObject.transform.SetParent(canvasObject.transform, false);

        scoreText = textObject.AddComponent<Text>();
        scoreText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        scoreText.fontSize = 28;
        scoreText.alignment = TextAnchor.UpperLeft;
        scoreText.color = Color.white;
        scoreText.text = "スコア: 0 / 10";

        RectTransform rect = scoreText.rectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(24f, -20f);
        rect.sizeDelta = new Vector2(300f, 50f);

        GameObject helpObject = new GameObject("Help");
        helpObject.transform.SetParent(canvasObject.transform, false);

        Text help = helpObject.AddComponent<Text>();
        help.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        help.fontSize = 20;
        help.alignment = TextAnchor.UpperLeft;
        help.color = Color.white;
        help.text = "WASD: 移動    Space: ジャンプ";

        RectTransform helpRect = help.rectTransform;
        helpRect.anchorMin = new Vector2(0f, 1f);
        helpRect.anchorMax = new Vector2(0f, 1f);
        helpRect.pivot = new Vector2(0f, 1f);
        helpRect.anchoredPosition = new Vector2(24f, -58f);
        helpRect.sizeDelta = new Vector2(500f, 40f);
    }

    private void CreatePickups()
    {
        Vector3[] positions =
        {
            new(-8f, 1f, -8f), new(0f, 1f, -8f), new(8f, 1f, -8f),
            new(-8f, 1f, 0f), new(8f, 1f, 0f),
            new(-8f, 1f, 8f), new(0f, 1f, 8f), new(8f, 1f, 8f),
            new(-4f, 1f, 4f), new(4f, 1f, -4f)
        };

        foreach (Vector3 position in positions)
        {
            GameObject pickup = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pickup.name = "Pickup";
            pickup.transform.position = position;
            pickup.transform.localScale = Vector3.one * 0.65f;
            ApplyMaterial(pickup, new Color(1f, 0.8f, 0.15f));
            Destroy(pickup.GetComponent<SphereCollider>());
            pickups.Add(pickup);
        }
    }

    private void CreateWall(Vector3 position, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "Boundary";
        wall.transform.position = position;
        wall.transform.localScale = scale;
        ApplyMaterial(wall, new Color(0.1f, 0.13f, 0.18f));
    }

    private static void ApplyMaterial(GameObject target, Color color)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (material.shader == null)
        {
            material = new Material(Shader.Find("Standard"));
        }

        material.color = color;
        renderer.material = material;
    }

    private void UpdateScore()
    {
        if (scoreText != null)
        {
            scoreText.text = $"スコア: {score} / 10";
        }
    }
}

public class CameraFollow : MonoBehaviour
{
    public Transform Target { get; set; }

    private void LateUpdate()
    {
        if (Target == null)
        {
            return;
        }

        Vector3 desiredPosition = Target.position + new Vector3(0f, 7f, -10f);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, 8f * Time.deltaTime);
        transform.LookAt(Target.position + Vector3.up * 1f);
    }
}
