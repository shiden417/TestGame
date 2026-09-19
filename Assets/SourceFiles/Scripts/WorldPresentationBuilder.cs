using UnityEngine;

[DisallowMultipleComponent]
public sealed class WorldPresentationBuilder : MonoBehaviour
{
    public void Build()
    {
        CreateGround();
        CreateBoundary();
        CreateFutureShrines();
        CreateEnergyRails();
        CreateArenaCore();
        CreateRuinedDetails();
        CreateSkyObjects();
        ConfigureLighting();
    }

    private void CreateGround()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "FutureRuinedGround";
        ground.transform.localScale = new Vector3(7f, 1f, 7f);

        ApplyMaterial(
            ground,
            new Color(0.025f, 0.045f, 0.07f));
    }

    private void CreateBoundary()
    {
        CreateBlock(
            "NorthWall",
            new Vector3(0f, 2f, 31f),
            new Vector3(62f, 4f, 0.7f),
            new Color(0.015f, 0.025f, 0.04f));

        CreateBlock(
            "SouthWall",
            new Vector3(0f, 2f, -31f),
            new Vector3(62f, 4f, 0.7f),
            new Color(0.015f, 0.025f, 0.04f));

        CreateBlock(
            "EastWall",
            new Vector3(31f, 2f, 0f),
            new Vector3(0.7f, 4f, 62f),
            new Color(0.015f, 0.025f, 0.04f));

        CreateBlock(
            "WestWall",
            new Vector3(-31f, 2f, 0f),
            new Vector3(0.7f, 4f, 62f),
            new Color(0.015f, 0.025f, 0.04f));
    }

    private void CreateFutureShrines()
    {
        Vector3[] origins =
        {
            new(-16f, 0f, 16f),
            new(16f, 0f, 16f),
            new(-16f, 0f, -16f),
            new(16f, 0f, -16f)
        };

        foreach (Vector3 origin in origins)
        {
            CreateBlock(
                "ShrinePillar",
                origin + Vector3.left * 1.8f + Vector3.up * 3f,
                new Vector3(0.8f, 6f, 0.8f),
                new Color(0.06f, 0.08f, 0.12f));

            CreateBlock(
                "ShrinePillar",
                origin + Vector3.right * 1.8f + Vector3.up * 3f,
                new Vector3(0.8f, 6f, 0.8f),
                new Color(0.06f, 0.08f, 0.12f));

            CreateBlock(
                "ShrineBeam",
                origin + Vector3.up * 6f,
                new Vector3(5f, 0.8f, 0.8f),
                new Color(0.08f, 0.1f, 0.16f));

            CreateGlow(
                origin + Vector3.up * 5.2f,
                new Vector3(0.12f, 5f, 0.12f),
                new Color(0.1f, 0.8f, 1f));
        }
    }

    private void CreateEnergyRails()
    {
        for (int i = -4; i <= 4; i++)
        {
            float position = i * 6f;

            CreateGlow(
                new Vector3(position, 0.04f, 0f),
                new Vector3(0.06f, 0.08f, 54f),
                new Color(0.05f, 0.65f, 1f));

            CreateGlow(
                new Vector3(0f, 0.045f, position),
                new Vector3(54f, 0.08f, 0.06f),
                new Color(0.55f, 0.12f, 1f));
        }
    }

    private void CreateArenaCore()
    {
        GameObject arena =
            GameObject.CreatePrimitive(PrimitiveType.Cylinder);

        arena.name = "ArenaCore";
        arena.transform.position = new Vector3(0f, 0.015f, 0f);
        arena.transform.localScale = new Vector3(8.5f, 0.025f, 8.5f);

        ApplyMaterial(
            arena,
            new Color(0.035f, 0.055f, 0.09f));

        CreateGlow(
            new Vector3(0f, 0.055f, 0f),
            new Vector3(0.035f, 0.08f, 17f),
            new Color(0.05f, 0.75f, 1f));

        CreateGlow(
            new Vector3(0f, 0.058f, 0f),
            new Vector3(17f, 0.08f, 0.035f),
            new Color(0.45f, 0.12f, 1f));
    }

    private void CreateRuinedDetails()
    {
        Vector3[] positions =
        {
            new(-11f, 0.35f, 4f),
            new(10f, 0.25f, 7f),
            new(-8f, 0.2f, -8f),
            new(9f, 0.3f, -10f),
            new(-20f, 0.25f, -3f),
            new(20f, 0.25f, 2f)
        };

        for (int i = 0; i < positions.Length; i++)
        {
            GameObject ruin =
                GameObject.CreatePrimitive(PrimitiveType.Cube);

            ruin.name = "RuinedStructureDetail";
            ruin.transform.position = positions[i];
            ruin.transform.localScale = new Vector3(
                1.5f + (i % 3) * 0.7f,
                0.35f + (i % 2) * 0.35f,
                1.2f + (i % 2) * 0.8f);
            ruin.transform.rotation =
                Quaternion.Euler(
                    0f,
                    i * 27f,
                    (i % 2 == 0 ? 8f : -6f));

            ApplyMaterial(
                ruin,
                new Color(0.04f, 0.06f, 0.095f));
        }

        for (int i = 0; i < 6; i++)
        {
            float angle = (i * 60f + 22f) * Mathf.Deg2Rad;
            Vector3 position = new Vector3(
                Mathf.Cos(angle) * 13.5f,
                1.2f + (i % 2) * 0.5f,
                Mathf.Sin(angle) * 13.5f);

            CreateBlock(
                "EnergyMonolith",
                position,
                new Vector3(0.28f, 2.4f, 0.28f),
                new Color(0.05f, 0.13f, 0.19f));

            CreateGlow(
                position + Vector3.up * 0.05f,
                new Vector3(0.09f, 2f, 0.09f),
                i % 2 == 0
                    ? new Color(0.05f, 0.75f, 1f)
                    : new Color(0.55f, 0.12f, 1f));
        }
    }

    private void CreateSkyObjects()
    {
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f * Mathf.Deg2Rad;
            Vector3 position = new Vector3(
                Mathf.Cos(angle) * 26f,
                5f + (i % 3) * 3f,
                Mathf.Sin(angle) * 26f);

            GameObject tower = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tower.name = "RuinedSkyline";
            tower.transform.position = position;
            tower.transform.localScale = new Vector3(
                2f + (i % 3),
                6f + (i % 4) * 2f,
                2f + (i % 2));

            ApplyMaterial(
                tower,
                new Color(0.035f, 0.055f, 0.085f));
        }
    }

    private void ConfigureLighting()
    {
        GameObject lightObject = new GameObject("FutureSun");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 0.95f;
        light.color = new Color(0.75f, 0.86f, 1f);
        lightObject.transform.rotation =
            Quaternion.Euler(42f, -32f, 0f);

        RenderSettings.ambientMode =
            UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight =
            new Color(0.025f, 0.04f, 0.075f);

        RenderSettings.fog = true;
        RenderSettings.fogColor =
            new Color(0.015f, 0.025f, 0.05f);
        RenderSettings.fogDensity = 0.009f;
    }

    private static void CreateBlock(
        string objectName,
        Vector3 position,
        Vector3 scale,
        Color color)
    {
        GameObject block =
            GameObject.CreatePrimitive(PrimitiveType.Cube);

        block.name = objectName;
        block.transform.position = position;
        block.transform.localScale = scale;
        ApplyMaterial(block, color);
    }

    private static void CreateGlow(
        Vector3 position,
        Vector3 scale,
        Color color)
    {
        GameObject glow =
            GameObject.CreatePrimitive(PrimitiveType.Cube);

        glow.name = "EnergyRail";
        glow.transform.position = position;
        glow.transform.localScale = scale;

        Collider collider = glow.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        ApplyMaterial(glow, color);
    }

    private static void ApplyMaterial(GameObject target, Color color)
    {
        Renderer renderer = target.GetComponent<Renderer>();
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

        Material material = new Material(shader)
        {
            color = color
        };

        if (material.HasProperty("_Metallic"))
        {
            material.SetFloat("_Metallic", 0.65f);
        }

        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", 0.72f);
        }

        if (color.g > 0.5f || color.b > 0.5f)
        {
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 1.25f);
            }
        }

        renderer.material = material;
    }
}
