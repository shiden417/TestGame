using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerAppearanceBuilder : MonoBehaviour
{
    private const string PlayerVisualRootName = "PlayerVisual";
    private const string CharacterResourcePath = "PlayerCharacter/PlayerCharacter";

    public void Build(Transform playerRoot)
    {
        if (playerRoot == null)
        {
            throw new System.ArgumentNullException(nameof(playerRoot));
        }

        Transform existingRoot = playerRoot.Find(PlayerVisualRootName);
        if (existingRoot != null)
        {
            Destroy(existingRoot.gameObject);
        }

        GameObject generatedCharacter = Resources.Load<GameObject>(CharacterResourcePath);
        if (generatedCharacter != null)
        {
            GameObject instance = Instantiate(generatedCharacter, playerRoot);
            instance.name = PlayerVisualRootName;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            foreach (Collider collider in instance.GetComponentsInChildren<Collider>(true))
            {
                if (collider != null)
                {
                    Destroy(collider);
                }
            }

            Animator animator = instance.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            }

            return;
        }

        BuildProceduralFallback(playerRoot);
    }

    private static void BuildProceduralFallback(Transform playerRoot)
    {
        GameObject visualRootObject = new GameObject(PlayerVisualRootName);
        Transform visualRoot = visualRootObject.transform;
        visualRoot.SetParent(playerRoot, false);
        visualRoot.localPosition = Vector3.zero;
        visualRoot.localRotation = Quaternion.identity;
        visualRoot.localScale = Vector3.one;

        CreateCapsule("Torso", visualRoot, new Vector3(0f, 0.35f, 0f), new Vector3(0.42f, 0.72f, 0.28f), new Color(0.035f, 0.045f, 0.065f));
        CreateCapsule("Neck", visualRoot, new Vector3(0f, 0.84f, 0f), new Vector3(0.12f, 0.18f, 0.12f), new Color(0.045f, 0.05f, 0.07f));
        CreateSphere("Pelvis", visualRoot, new Vector3(0f, -0.32f, 0f), new Vector3(0.44f, 0.28f, 0.30f), new Color(0.045f, 0.055f, 0.08f));

        CreateCapsule("LeftUpperArm", visualRoot, new Vector3(-0.42f, 0.34f, 0f), new Vector3(0.12f, 0.40f, 0.12f), new Color(0.055f, 0.065f, 0.09f));
        CreateCapsule("RightUpperArm", visualRoot, new Vector3(0.42f, 0.34f, 0f), new Vector3(0.12f, 0.40f, 0.12f), new Color(0.055f, 0.065f, 0.09f));
        CreateCapsule("LeftForearm", visualRoot, new Vector3(-0.50f, -0.05f, 0.015f), new Vector3(0.105f, 0.38f, 0.105f), new Color(0.065f, 0.075f, 0.10f));
        CreateCapsule("RightForearm", visualRoot, new Vector3(0.50f, -0.05f, 0.015f), new Vector3(0.105f, 0.38f, 0.105f), new Color(0.065f, 0.075f, 0.10f));
        CreateSphere("LeftHand", visualRoot, new Vector3(-0.50f, -0.30f, 0.02f), new Vector3(0.12f, 0.14f, 0.11f), new Color(0.04f, 0.045f, 0.06f));
        CreateSphere("RightHand", visualRoot, new Vector3(0.50f, -0.30f, 0.02f), new Vector3(0.12f, 0.14f, 0.11f), new Color(0.04f, 0.045f, 0.06f));

        CreateCapsule("LeftThigh", visualRoot, new Vector3(-0.18f, -0.70f, 0f), new Vector3(0.15f, 0.52f, 0.15f), new Color(0.055f, 0.065f, 0.09f));
        CreateCapsule("RightThigh", visualRoot, new Vector3(0.18f, -0.70f, 0f), new Vector3(0.15f, 0.52f, 0.15f), new Color(0.055f, 0.065f, 0.09f));
        CreateCapsule("LeftShin", visualRoot, new Vector3(-0.18f, -1.28f, 0.015f), new Vector3(0.115f, 0.52f, 0.12f), new Color(0.07f, 0.08f, 0.11f));
        CreateCapsule("RightShin", visualRoot, new Vector3(0.18f, -1.28f, 0.015f), new Vector3(0.115f, 0.52f, 0.12f), new Color(0.07f, 0.08f, 0.11f));
        CreateSphere("LeftFoot", visualRoot, new Vector3(-0.18f, -1.62f, 0.12f), new Vector3(0.15f, 0.11f, 0.28f), new Color(0.04f, 0.05f, 0.07f));
        CreateSphere("RightFoot", visualRoot, new Vector3(0.18f, -1.62f, 0.12f), new Vector3(0.15f, 0.11f, 0.28f), new Color(0.04f, 0.05f, 0.07f));

        CreateSphere("Head", visualRoot, new Vector3(0f, 1.10f, 0f), new Vector3(0.28f, 0.34f, 0.25f), new Color(0.035f, 0.045f, 0.065f));
        CreateSphere("FaceShell", visualRoot, new Vector3(0f, 1.07f, 0.16f), new Vector3(0.235f, 0.22f, 0.13f), new Color(0.055f, 0.065f, 0.09f));
        CreateSphere("Visor", visualRoot, new Vector3(0f, 1.14f, 0.245f), new Vector3(0.235f, 0.035f, 0.025f), new Color(0.02f, 0.8f, 0.95f));
        CreateCapsule("HelmetCrest", visualRoot, new Vector3(0f, 1.39f, -0.015f), new Vector3(0.035f, 0.20f, 0.045f), new Color(0.32f, 0.025f, 0.07f));

        CreateSphere("LeftShoulder", visualRoot, new Vector3(-0.46f, 0.62f, 0f), new Vector3(0.19f, 0.17f, 0.21f), new Color(0.10f, 0.12f, 0.16f));
        CreateSphere("RightShoulder", visualRoot, new Vector3(0.49f, 0.64f, 0f), new Vector3(0.25f, 0.21f, 0.27f), new Color(0.11f, 0.13f, 0.17f));
        CreateSphere("LeftKnee", visualRoot, new Vector3(-0.18f, -1.00f, 0.075f), new Vector3(0.145f, 0.13f, 0.12f), new Color(0.11f, 0.13f, 0.17f));
        CreateSphere("RightKnee", visualRoot, new Vector3(0.18f, -1.00f, 0.075f), new Vector3(0.145f, 0.13f, 0.12f), new Color(0.11f, 0.13f, 0.17f));

        CreateSphere("ChestArmor", visualRoot, new Vector3(0f, 0.45f, 0.245f), new Vector3(0.39f, 0.30f, 0.08f), new Color(0.11f, 0.14f, 0.19f));
        CreateSphere("RightShoulderPlate", visualRoot, new Vector3(0.51f, 0.69f, 0.02f), new Vector3(0.24f, 0.18f, 0.25f), new Color(0.13f, 0.15f, 0.20f));
        CreateSphere("LeftShoulderPlate", visualRoot, new Vector3(-0.48f, 0.67f, 0.02f), new Vector3(0.18f, 0.14f, 0.19f), new Color(0.10f, 0.12f, 0.16f));
        CreateCapsule("RearEnergyUnit", visualRoot, new Vector3(0f, 0.22f, -0.29f), new Vector3(0.14f, 0.34f, 0.075f), new Color(0.045f, 0.075f, 0.11f));
        CreateCapsule("RearEnergyCore", visualRoot, new Vector3(0f, 0.22f, -0.365f), new Vector3(0.045f, 0.27f, 0.025f), new Color(0.03f, 0.85f, 1f));
        CreateCapsule("SwordSheath", visualRoot, new Vector3(-0.43f, -0.15f, -0.06f), new Vector3(0.045f, 0.56f, 0.045f), new Color(0.025f, 0.03f, 0.045f));
        CreateCapsule("SwordHandle", visualRoot, new Vector3(-0.43f, 0.23f, 0.13f), new Vector3(0.042f, 0.20f, 0.042f), new Color(0.13f, 0.14f, 0.17f));
        CreateSphere("SwordGuard", visualRoot, new Vector3(-0.43f, 0.01f, 0.15f), new Vector3(0.10f, 0.025f, 0.045f), new Color(0.55f, 0.38f, 0.12f));
        CreateSphere("ChestEnergySeal", visualRoot, new Vector3(0f, 0.48f, 0.335f), new Vector3(0.035f, 0.16f, 0.018f), new Color(0.05f, 0.9f, 1f));
        CreateSphere("RightChestCrimson", visualRoot, new Vector3(0.27f, 0.57f, 0.33f), new Vector3(0.025f, 0.10f, 0.018f), new Color(0.65f, 0.035f, 0.10f));
    }

    private static void CreateSphere(string objectName, Transform parent, Vector3 localPosition, Vector3 localScale, Color color)
    {
        CreatePrimitive(PrimitiveType.Sphere, objectName, parent, localPosition, localScale, color);
    }

    private static void CreateCapsule(string objectName, Transform parent, Vector3 localPosition, Vector3 localScale, Color color)
    {
        CreatePrimitive(PrimitiveType.Capsule, objectName, parent, localPosition, localScale, color);
    }

    private static GameObject CreatePrimitive(PrimitiveType primitiveType, string objectName, Transform parent, Vector3 localPosition, Vector3 localScale, Color color)
    {
        GameObject part = GameObject.CreatePrimitive(primitiveType);
        if (part == null)
        {
            throw new System.InvalidOperationException($"Failed to create player visual: {objectName}");
        }

        part.name = objectName;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = localScale;

        Collider collider = part.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        Renderer renderer = part.GetComponent<Renderer>();
        if (renderer == null)
        {
            throw new System.InvalidOperationException($"Player visual renderer was not created: {objectName}");
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        if (shader == null)
        {
            throw new System.InvalidOperationException("No compatible Unity material shader was found.");
        }

        Material material = new Material(shader)
        {
            color = color
        };

        if (material.HasProperty("_Metallic"))
        {
            material.SetFloat("_Metallic", 0.55f);
        }

        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", 0.76f);
        }

        if (color.g > 0.55f && color.b > 0.55f && material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 2.4f);
        }

        renderer.material = material;
        return part;
    }
}
