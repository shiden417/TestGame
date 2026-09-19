using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerAppearanceBuilder : MonoBehaviour
{
    private const string PlayerVisualRootName = "PlayerVisual";

    public void Build(Transform playerRoot)
    {
        if (playerRoot == null)
        {
            throw new System.ArgumentNullException(
                nameof(playerRoot));
        }

        Transform existingRoot =
            playerRoot.Find(PlayerVisualRootName);

        if (existingRoot != null)
        {
            Destroy(existingRoot.gameObject);
        }

        GameObject visualRootObject =
            new GameObject(PlayerVisualRootName);

        Transform visualRoot =
            visualRootObject.transform;

        visualRoot.SetParent(
            playerRoot,
            false);

        visualRoot.localPosition = Vector3.zero;
        visualRoot.localRotation = Quaternion.identity;
        visualRoot.localScale = Vector3.one;

        CreateCoreBody(visualRoot);
        CreateHead(visualRoot);
        CreateTorsoArmor(visualRoot);
        CreateShoulderArmor(visualRoot);
        CreateArmArmor(visualRoot);
        CreateLegArmor(visualRoot);
        CreateWaistArmor(visualRoot);
        CreateBackUnit(visualRoot);
        CreateEnergyLines(visualRoot);
    }

    private void CreateCoreBody(Transform parent)
    {
        CreateCapsule(
            "BodyCore",
            parent,
            new Vector3(0f, 0.05f, 0f),
            new Vector3(0.68f, 0.92f, 0.5f),
            new Color(0.08f, 0.1f, 0.14f));

        CreateSphere(
            "ChestCore",
            parent,
            new Vector3(0f, 0.38f, 0.28f),
            new Vector3(0.52f, 0.36f, 0.16f),
            new Color(0.12f, 0.17f, 0.24f));
    }

    private void CreateHead(Transform parent)
    {
        CreateSphere(
            "Head",
            parent,
            new Vector3(0f, 1.15f, 0f),
            new Vector3(0.38f, 0.42f, 0.34f),
            new Color(0.09f, 0.11f, 0.15f));

        CreateSphere(
            "Visor",
            parent,
            new Vector3(0f, 1.17f, 0.31f),
            new Vector3(0.29f, 0.11f, 0.055f),
            new Color(0.08f, 0.9f, 1f));

        CreateCube(
            "HeadGuard",
            parent,
            new Vector3(0f, 1.38f, 0f),
            new Vector3(0.78f, 0.12f, 0.62f),
            new Color(0.14f, 0.17f, 0.23f));

        CreateCube(
            "HeadCrest",
            parent,
            new Vector3(0f, 1.56f, -0.02f),
            new Vector3(0.12f, 0.34f, 0.1f),
            new Color(0.55f, 0.08f, 0.16f));
    }

    private void CreateTorsoArmor(Transform parent)
    {
        CreateCube(
            "TorsoPlate",
            parent,
            new Vector3(0f, 0.5f, 0.12f),
            new Vector3(0.72f, 0.55f, 0.55f),
            new Color(0.13f, 0.16f, 0.21f));

        CreateCube(
            "ChestSeal",
            parent,
            new Vector3(0f, 0.55f, 0.41f),
            new Vector3(0.28f, 0.3f, 0.08f),
            new Color(0.55f, 0.08f, 0.16f));

        CreateCube(
            "CollarGuard",
            parent,
            new Vector3(0f, 0.84f, 0.02f),
            new Vector3(0.56f, 0.16f, 0.42f),
            new Color(0.18f, 0.22f, 0.29f));
    }

    private void CreateShoulderArmor(Transform parent)
    {
        CreateSphere(
            "LeftShoulderArmor",
            parent,
            new Vector3(-0.55f, 0.72f, 0f),
            new Vector3(0.3f, 0.27f, 0.36f),
            new Color(0.18f, 0.2f, 0.25f));

        CreateCube(
            "RightShoulderArmor",
            parent,
            new Vector3(0.55f, 0.73f, 0f),
            new Vector3(0.42f, 0.34f, 0.5f),
            new Color(0.55f, 0.08f, 0.16f));

        CreateCube(
            "RightShoulderEdge",
            parent,
            new Vector3(0.55f, 0.88f, 0.18f),
            new Vector3(0.44f, 0.07f, 0.08f),
            new Color(0.09f, 0.76f, 1f));
    }

    private void CreateArmArmor(Transform parent)
    {
        CreateCapsule(
            "LeftUpperArm",
            parent,
            new Vector3(-0.52f, 0.35f, 0f),
            new Vector3(0.22f, 0.45f, 0.22f),
            new Color(0.12f, 0.15f, 0.2f));

        CreateCapsule(
            "RightUpperArm",
            parent,
            new Vector3(0.52f, 0.35f, 0f),
            new Vector3(0.22f, 0.45f, 0.22f),
            new Color(0.12f, 0.15f, 0.2f));

        CreateCube(
            "LeftForearmGuard",
            parent,
            new Vector3(-0.62f, -0.02f, 0.12f),
            new Vector3(0.2f, 0.42f, 0.28f),
            new Color(0.2f, 0.23f, 0.29f));

        CreateCube(
            "RightForearmGuard",
            parent,
            new Vector3(0.62f, -0.02f, 0.12f),
            new Vector3(0.2f, 0.42f, 0.28f),
            new Color(0.2f, 0.23f, 0.29f));
    }

    private void CreateLegArmor(Transform parent)
    {
        CreateCapsule(
            "LeftThighArmor",
            parent,
            new Vector3(-0.3f, -0.72f, 0f),
            new Vector3(0.27f, 0.58f, 0.27f),
            new Color(0.11f, 0.13f, 0.18f));

        CreateCapsule(
            "RightThighArmor",
            parent,
            new Vector3(0.3f, -0.72f, 0f),
            new Vector3(0.27f, 0.58f, 0.27f),
            new Color(0.11f, 0.13f, 0.18f));

        CreateCube(
            "LeftShinGuard",
            parent,
            new Vector3(-0.3f, -1.35f, 0.08f),
            new Vector3(0.3f, 0.55f, 0.38f),
            new Color(0.19f, 0.22f, 0.28f));

        CreateCube(
            "RightShinGuard",
            parent,
            new Vector3(0.3f, -1.35f, 0.08f),
            new Vector3(0.3f, 0.55f, 0.38f),
            new Color(0.19f, 0.22f, 0.28f));

        CreateCube(
            "LeftBoot",
            parent,
            new Vector3(-0.3f, -1.75f, 0.18f),
            new Vector3(0.34f, 0.22f, 0.48f),
            new Color(0.08f, 0.1f, 0.14f));

        CreateCube(
            "RightBoot",
            parent,
            new Vector3(0.3f, -1.75f, 0.18f),
            new Vector3(0.34f, 0.22f, 0.48f),
            new Color(0.08f, 0.1f, 0.14f));
    }

    private void CreateWaistArmor(Transform parent)
    {
        CreateCube(
            "WaistCore",
            parent,
            new Vector3(0f, -0.25f, 0f),
            new Vector3(0.8f, 0.22f, 0.58f),
            new Color(0.16f, 0.18f, 0.23f));

        CreateCube(
            "FrontWaistPlate",
            parent,
            new Vector3(0f, -0.44f, 0.18f),
            new Vector3(0.56f, 0.36f, 0.12f),
            new Color(0.27f, 0.07f, 0.12f));

        CreateCube(
            "LeftWaistPlate",
            parent,
            new Vector3(-0.43f, -0.45f, 0f),
            new Vector3(0.12f, 0.42f, 0.38f),
            new Color(0.16f, 0.19f, 0.25f));

        CreateCube(
            "RightWaistPlate",
            parent,
            new Vector3(0.43f, -0.45f, 0f),
            new Vector3(0.12f, 0.42f, 0.38f),
            new Color(0.55f, 0.08f, 0.16f));
    }

    private void CreateBackUnit(Transform parent)
    {
        CreateCube(
            "BackEnergyUnit",
            parent,
            new Vector3(0f, 0.2f, -0.52f),
            new Vector3(0.38f, 0.7f, 0.18f),
            new Color(0.05f, 0.12f, 0.18f));

        CreateCube(
            "BackEnergyCore",
            parent,
            new Vector3(0f, 0.2f, -0.64f),
            new Vector3(0.16f, 0.48f, 0.06f),
            new Color(0.04f, 0.78f, 1f));

        CreateCube(
            "LeftBackFin",
            parent,
            new Vector3(-0.3f, 0.33f, -0.45f),
            new Vector3(0.08f, 0.55f, 0.38f),
            new Color(0.16f, 0.19f, 0.25f));

        CreateCube(
            "RightBackFin",
            parent,
            new Vector3(0.3f, 0.33f, -0.45f),
            new Vector3(0.08f, 0.55f, 0.38f),
            new Color(0.16f, 0.19f, 0.25f));
    }

    private void CreateEnergyLines(Transform parent)
    {
        CreateCube(
            "LeftEnergyLine",
            parent,
            new Vector3(-0.37f, 0.48f, 0.36f),
            new Vector3(0.045f, 0.4f, 0.035f),
            new Color(0.1f, 0.82f, 1f));

        CreateCube(
            "RightEnergyLine",
            parent,
            new Vector3(0.37f, 0.48f, 0.36f),
            new Vector3(0.045f, 0.4f, 0.035f),
            new Color(0.55f, 0.08f, 0.16f));

        CreateCube(
            "CenterEnergyLine",
            parent,
            new Vector3(0f, 0.02f, 0.36f),
            new Vector3(0.055f, 0.62f, 0.035f),
            new Color(0.1f, 0.86f, 1f));
    }

    private static void CreateCube(
        string objectName,
        Transform parent,
        Vector3 localPosition,
        Vector3 localScale,
        Color color)
    {
        CreatePrimitive(
            PrimitiveType.Cube,
            objectName,
            parent,
            localPosition,
            localScale,
            color);
    }

    private static void CreateSphere(
        string objectName,
        Transform parent,
        Vector3 localPosition,
        Vector3 localScale,
        Color color)
    {
        CreatePrimitive(
            PrimitiveType.Sphere,
            objectName,
            parent,
            localPosition,
            localScale,
            color);
    }

    private static void CreateCapsule(
        string objectName,
        Transform parent,
        Vector3 localPosition,
        Vector3 localScale,
        Color color)
    {
        CreatePrimitive(
            PrimitiveType.Capsule,
            objectName,
            parent,
            localPosition,
            localScale,
            color);
    }

    private static GameObject CreatePrimitive(
        PrimitiveType primitiveType,
        string objectName,
        Transform parent,
        Vector3 localPosition,
        Vector3 localScale,
        Color color)
    {
        GameObject part =
            GameObject.CreatePrimitive(
                primitiveType);

        if (part == null)
        {
            throw new System.InvalidOperationException(
                $"Failed to create player visual: {objectName}");
        }

        part.name = objectName;
        part.transform.SetParent(
            parent,
            false);
        part.transform.localPosition =
            localPosition;
        part.transform.localScale =
            localScale;

        Collider collider =
            part.GetComponent<Collider>();

        if (collider != null)
        {
            Destroy(collider);
        }

        ApplyMaterial(
            part,
            color);

        return part;
    }

    private static void ApplyMaterial(
        GameObject target,
        Color color)
    {
        Renderer renderer =
            target.GetComponent<Renderer>();

        if (renderer == null)
        {
            throw new System.InvalidOperationException(
                $"Player visual renderer was not created: {target.name}");
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
