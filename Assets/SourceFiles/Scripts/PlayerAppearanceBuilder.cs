using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerAppearanceBuilder : MonoBehaviour
{
    private const string PlayerVisualRootName = "PlayerVisual";

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

        GameObject visualRootObject = new GameObject(PlayerVisualRootName);
        Transform visualRoot = visualRootObject.transform;
        visualRoot.SetParent(playerRoot, false);
        visualRoot.localPosition = Vector3.zero;
        visualRoot.localRotation = Quaternion.identity;
        visualRoot.localScale = Vector3.one;

        CreateBody(visualRoot);
        CreateHead(visualRoot);
        CreateArms(visualRoot);
        CreateLegs(visualRoot);
        CreateArmor(visualRoot);
        CreateBackUnit(visualRoot);
        CreateEnergyDetails(visualRoot);
        CreateSwordMount(visualRoot);
    }

    private void CreateBody(Transform parent)
    {
        CreateCapsule("BodySuit", parent, new Vector3(0f, 0.12f, 0f),
            new Vector3(0.46f, 0.78f, 0.32f), new Color(0.045f, 0.055f, 0.075f));

        CreateCapsule("ChestArmor", parent, new Vector3(0f, 0.43f, 0.03f),
            new Vector3(0.58f, 0.5f, 0.4f), new Color(0.10f, 0.13f, 0.18f));

        CreateSphere("ChestPlate", parent, new Vector3(0f, 0.49f, 0.30f),
            new Vector3(0.48f, 0.36f, 0.12f), new Color(0.16f, 0.20f, 0.27f));

        CreateCapsule("Neck", parent, new Vector3(0f, 0.88f, 0f),
            new Vector3(0.15f, 0.22f, 0.15f), new Color(0.055f, 0.065f, 0.085f));

        CreateCapsule("PelvisSuit", parent, new Vector3(0f, -0.38f, 0f),
            new Vector3(0.38f, 0.30f, 0.28f), new Color(0.055f, 0.065f, 0.085f));

        CreateSphere("PelvisArmor", parent, new Vector3(0f, -0.34f, 0.05f),
            new Vector3(0.62f, 0.28f, 0.38f), new Color(0.12f, 0.15f, 0.20f));
    }

    private void CreateHead(Transform parent)
    {
        CreateSphere("Head", parent, new Vector3(0f, 1.16f, 0f),
            new Vector3(0.34f, 0.40f, 0.31f), new Color(0.055f, 0.065f, 0.09f));

        CreateSphere("FaceShell", parent, new Vector3(0f, 1.10f, 0.18f),
            new Vector3(0.28f, 0.25f, 0.20f), new Color(0.075f, 0.09f, 0.12f));

        CreateCube("Visor", parent, new Vector3(0f, 1.18f, 0.30f),
            new Vector3(0.30f, 0.045f, 0.035f), new Color(0.08f, 0.92f, 1f));

        CreateCapsule("HelmetCrest", parent, new Vector3(0f, 1.48f, -0.01f),
            new Vector3(0.055f, 0.25f, 0.065f), new Color(0.42f, 0.035f, 0.09f));

        CreateSphere("HelmetRearShell", parent, new Vector3(0f, 1.19f, -0.14f),
            new Vector3(0.36f, 0.30f, 0.18f), new Color(0.09f, 0.11f, 0.15f));
    }

    private void CreateArms(Transform parent)
    {
        CreateCapsule("LeftUpperArm", parent, new Vector3(-0.47f, 0.38f, 0f),
            new Vector3(0.15f, 0.42f, 0.15f), new Color(0.07f, 0.085f, 0.115f));
        CreateCapsule("RightUpperArm", parent, new Vector3(0.47f, 0.38f, 0f),
            new Vector3(0.15f, 0.42f, 0.15f), new Color(0.07f, 0.085f, 0.115f));

        CreateSphere("LeftShoulder", parent, new Vector3(-0.50f, 0.66f, 0f),
            new Vector3(0.27f, 0.22f, 0.28f), new Color(0.14f, 0.17f, 0.22f));
        CreateSphere("RightShoulder", parent, new Vector3(0.52f, 0.68f, 0f),
            new Vector3(0.32f, 0.27f, 0.34f), new Color(0.12f, 0.15f, 0.20f));

        CreateCapsule("LeftForearm", parent, new Vector3(-0.55f, -0.02f, 0.06f),
            new Vector3(0.14f, 0.40f, 0.14f), new Color(0.13f, 0.15f, 0.20f));
        CreateCapsule("RightForearm", parent, new Vector3(0.55f, -0.02f, 0.06f),
            new Vector3(0.14f, 0.40f, 0.14f), new Color(0.13f, 0.15f, 0.20f));

        CreateSphere("LeftElbow", parent, new Vector3(-0.55f, 0.17f, 0.07f),
            new Vector3(0.17f, 0.16f, 0.17f), new Color(0.10f, 0.12f, 0.16f));
        CreateSphere("RightElbow", parent, new Vector3(0.55f, 0.17f, 0.07f),
            new Vector3(0.17f, 0.16f, 0.17f), new Color(0.10f, 0.12f, 0.16f));

        CreateSphere("LeftHand", parent, new Vector3(-0.55f, -0.28f, 0.06f),
            new Vector3(0.15f, 0.17f, 0.13f), new Color(0.045f, 0.055f, 0.075f));
        CreateSphere("RightHand", parent, new Vector3(0.55f, -0.28f, 0.06f),
            new Vector3(0.15f, 0.17f, 0.13f), new Color(0.045f, 0.055f, 0.075f));
    }

    private void CreateLegs(Transform parent)
    {
        CreateCapsule("LeftThigh", parent, new Vector3(-0.23f, -0.78f, 0f),
            new Vector3(0.19f, 0.55f, 0.19f), new Color(0.07f, 0.085f, 0.115f));
        CreateCapsule("RightThigh", parent, new Vector3(0.23f, -0.78f, 0f),
            new Vector3(0.19f, 0.55f, 0.19f), new Color(0.07f, 0.085f, 0.115f));

        CreateSphere("LeftKnee", parent, new Vector3(-0.23f, -1.16f, 0.10f),
            new Vector3(0.20f, 0.18f, 0.15f), new Color(0.17f, 0.20f, 0.26f));
        CreateSphere("RightKnee", parent, new Vector3(0.23f, -1.16f, 0.10f),
            new Vector3(0.20f, 0.18f, 0.15f), new Color(0.17f, 0.20f, 0.26f));

        CreateCapsule("LeftShin", parent, new Vector3(-0.23f, -1.45f, 0.02f),
            new Vector3(0.15f, 0.48f, 0.16f), new Color(0.12f, 0.14f, 0.19f));
        CreateCapsule("RightShin", parent, new Vector3(0.23f, -1.45f, 0.02f),
            new Vector3(0.15f, 0.48f, 0.16f), new Color(0.12f, 0.14f, 0.19f));

        CreateSphere("LeftBoot", parent, new Vector3(-0.23f, -1.76f, 0.13f),
            new Vector3(0.21f, 0.16f, 0.36f), new Color(0.055f, 0.065f, 0.09f));
        CreateSphere("RightBoot", parent, new Vector3(0.23f, -1.76f, 0.13f),
            new Vector3(0.21f, 0.16f, 0.36f), new Color(0.055f, 0.065f, 0.09f));
    }

    private void CreateArmor(Transform parent)
    {
        CreateSphere("ChestLeftArmor", parent, new Vector3(-0.25f, 0.52f, 0.30f),
            new Vector3(0.30f, 0.25f, 0.09f), new Color(0.16f, 0.19f, 0.25f));
        CreateSphere("ChestRightArmor", parent, new Vector3(0.25f, 0.52f, 0.30f),
            new Vector3(0.30f, 0.25f, 0.09f), new Color(0.16f, 0.19f, 0.25f));

        CreateSphere("CollarGuard", parent, new Vector3(0f, 0.83f, 0.02f),
            new Vector3(0.45f, 0.13f, 0.30f), new Color(0.16f, 0.19f, 0.25f));

        CreateSphere("LeftHipArmor", parent, new Vector3(-0.38f, -0.43f, 0.03f),
            new Vector3(0.22f, 0.32f, 0.27f), new Color(0.12f, 0.14f, 0.19f));
        CreateSphere("RightHipArmor", parent, new Vector3(0.38f, -0.43f, 0.03f),
            new Vector3(0.22f, 0.32f, 0.27f), new Color(0.20f, 0.07f, 0.12f));

        CreateCapsule("RightShoulderPlate", parent, new Vector3(0.53f, 0.70f, 0.02f),
            new Vector3(0.23f, 0.20f, 0.26f), new Color(0.12f, 0.15f, 0.20f));
    }

    private void CreateBackUnit(Transform parent)
    {
        CreateCapsule("BackEnergyUnit", parent, new Vector3(0f, 0.22f, -0.42f),
            new Vector3(0.20f, 0.48f, 0.11f), new Color(0.045f, 0.10f, 0.15f));

        CreateCapsule("BackEnergyCore", parent, new Vector3(0f, 0.22f, -0.52f),
            new Vector3(0.07f, 0.35f, 0.035f), new Color(0.04f, 0.85f, 1f));

        CreateCapsule("LeftBackFin", parent, new Vector3(-0.22f, 0.34f, -0.34f),
            new Vector3(0.055f, 0.34f, 0.12f), new Color(0.12f, 0.15f, 0.20f));
        CreateCapsule("RightBackFin", parent, new Vector3(0.22f, 0.34f, -0.34f),
            new Vector3(0.055f, 0.34f, 0.12f), new Color(0.12f, 0.15f, 0.20f));
    }

    private void CreateEnergyDetails(Transform parent)
    {
        CreateCube("ChestEnergyLine", parent, new Vector3(0f, 0.48f, 0.405f),
            new Vector3(0.035f, 0.42f, 0.025f), new Color(0.08f, 0.9f, 1f));

        CreateCube("RightChestAccent", parent, new Vector3(0.30f, 0.60f, 0.385f),
            new Vector3(0.035f, 0.18f, 0.025f), new Color(0.65f, 0.045f, 0.12f));

        CreateCube("LeftBootLight", parent, new Vector3(-0.23f, -1.76f, 0.39f),
            new Vector3(0.06f, 0.045f, 0.10f), new Color(0.08f, 0.8f, 1f));
        CreateCube("RightBootLight", parent, new Vector3(0.23f, -1.76f, 0.39f),
            new Vector3(0.06f, 0.045f, 0.10f), new Color(0.08f, 0.8f, 1f));
    }

    private void CreateSwordMount(Transform parent)
    {
        CreateCapsule("SwordSheath", parent, new Vector3(-0.48f, -0.12f, -0.04f),
            new Vector3(0.07f, 0.65f, 0.07f), new Color(0.035f, 0.04f, 0.06f));

        CreateCapsule("SwordHandle", parent, new Vector3(-0.48f, 0.25f, 0.20f),
            new Vector3(0.065f, 0.24f, 0.065f), new Color(0.16f, 0.17f, 0.20f));

        CreateCube("SwordGuard", parent, new Vector3(-0.48f, 0.02f, 0.22f),
            new Vector3(0.22f, 0.045f, 0.06f), new Color(0.68f, 0.48f, 0.16f));

        CreateCube("SwordSheathAccent", parent, new Vector3(-0.48f, 0.08f, 0.06f),
            new Vector3(0.10f, 0.07f, 0.025f), new Color(0.55f, 0.045f, 0.12f));
    }

    private static void CreateCube(string objectName, Transform parent, Vector3 localPosition, Vector3 localScale, Color color)
    {
        CreatePrimitive(PrimitiveType.Cube, objectName, parent, localPosition, localScale, color);
    }

    private static void CreateSphere(string objectName, Transform parent, Vector3 localPosition, Vector3 localScale, Color color)
    {
        CreatePrimitive(PrimitiveType.Sphere, objectName, parent, localPosition, localScale, color);
    }

    private static void CreateCapsule(string objectName, Transform parent, Vector3 localPosition, Vector3 localScale, Color color)
    {
        CreatePrimitive(PrimitiveType.Capsule, objectName, parent, localPosition, localScale, color);
    }

    private static GameObject CreatePrimitive(
        PrimitiveType primitiveType,
        string objectName,
        Transform parent,
        Vector3 localPosition,
        Vector3 localScale,
        Color color)
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

        ApplyMaterial(part, color);
        return part;
    }

    private static void ApplyMaterial(GameObject target, Color color)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer == null)
        {
            throw new System.InvalidOperationException($"Player visual renderer was not created: {target.name}");
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

        if (material.HasProperty("_Metallic"))
        {
            material.SetFloat("_Metallic", 0.48f);
        }

        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", 0.72f);
        }

        if (color.g > 0.55f && color.b > 0.55f && material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 2.0f);
        }

        renderer.material = material;
    }
}
