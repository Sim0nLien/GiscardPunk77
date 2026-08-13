using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// Builds cheap placeholder scenery so the train-motion prototype can be tested
/// before final art prefabs exist.
/// </summary>
internal static class ProceduralSceneryFactory
{
    public static GameObject Create(
        TrainScenerySystem.GeneratedShape shape,
        Color colour,
        Transform parent)
    {
        GameObject root = new GameObject($"Generated {shape}");
        root.transform.SetParent(parent, false);

        switch (shape)
        {
            case TrainScenerySystem.GeneratedShape.UtilityPole:
                CreateUtilityPole(root.transform, colour);
                break;

            case TrainScenerySystem.GeneratedShape.Building:
                CreateBuilding(root.transform, colour);
                break;

            case TrainScenerySystem.GeneratedShape.Rock:
                CreateRock(root.transform, colour);
                break;

            default:
                CreateTree(root.transform, colour);
                break;
        }

        return root;
    }

    private static void CreateTree(Transform root, Color foliageColour)
    {
        Color trunkColour = new Color(0.25f, 0.13f, 0.06f, 1f);
        AddPrimitive(
            PrimitiveType.Cylinder,
            "Trunk",
            root,
            new Vector3(0f, 1.25f, 0f),
            new Vector3(0.32f, 1.25f, 0.32f),
            trunkColour);
        AddPrimitive(
            PrimitiveType.Sphere,
            "Foliage lower",
            root,
            new Vector3(0f, 3.05f, 0f),
            new Vector3(2.1f, 2.2f, 2.1f),
            foliageColour);
        AddPrimitive(
            PrimitiveType.Sphere,
            "Foliage upper",
            root,
            new Vector3(0.25f, 4.25f, 0.1f),
            new Vector3(1.45f, 1.7f, 1.45f),
            foliageColour * 0.88f);
    }

    private static void CreateUtilityPole(Transform root, Color colour)
    {
        AddPrimitive(
            PrimitiveType.Cylinder,
            "Pole",
            root,
            new Vector3(0f, 3.5f, 0f),
            new Vector3(0.16f, 3.5f, 0.16f),
            colour);
        AddPrimitive(
            PrimitiveType.Cube,
            "Crossbar",
            root,
            new Vector3(0f, 6.2f, 0f),
            new Vector3(2.3f, 0.16f, 0.18f),
            colour * 0.82f);

        Color insulatorColour = new Color(0.12f, 0.14f, 0.13f, 1f);

        for (int index = -1; index <= 1; index++)
        {
            AddPrimitive(
                PrimitiveType.Sphere,
                $"Insulator {index + 2}",
                root,
                new Vector3(index * 0.85f, 6.45f, 0f),
                new Vector3(0.22f, 0.22f, 0.22f),
                insulatorColour);
        }
    }

    private static void CreateBuilding(Transform root, Color colour)
    {
        AddPrimitive(
            PrimitiveType.Cube,
            "Building",
            root,
            new Vector3(0f, 2.5f, 0f),
            new Vector3(2.8f, 5f, 2.4f),
            colour);
        AddPrimitive(
            PrimitiveType.Cube,
            "Roof",
            root,
            new Vector3(0f, 5.15f, 0f),
            new Vector3(3.1f, 0.3f, 2.7f),
            colour * 0.72f);

        Color windowColour = new Color(0.45f, 0.66f, 0.72f, 1f);

        for (int floor = 0; floor < 3; floor++)
        {
            for (int column = -1; column <= 1; column += 2)
            {
                AddPrimitive(
                    PrimitiveType.Cube,
                    $"Window {floor} {column}",
                    root,
                    new Vector3(column * 0.75f, 1.25f + floor * 1.35f, -1.215f),
                    new Vector3(0.7f, 0.65f, 0.04f),
                    windowColour);
            }
        }
    }

    private static void CreateRock(Transform root, Color colour)
    {
        GameObject rock = AddPrimitive(
            PrimitiveType.Sphere,
            "Rock",
            root,
            new Vector3(0f, 0.65f, 0f),
            new Vector3(1.8f, 1.3f, 1.45f),
            colour);
        rock.transform.localRotation = Quaternion.Euler(12f, 28f, -8f);
    }

    private static GameObject AddPrimitive(
        PrimitiveType primitiveType,
        string objectName,
        Transform parent,
        Vector3 localPosition,
        Vector3 localScale,
        Color colour)
    {
        GameObject primitive = GameObject.CreatePrimitive(primitiveType);
        primitive.name = objectName;
        primitive.transform.SetParent(parent, false);
        primitive.transform.localPosition = localPosition;
        primitive.transform.localScale = localScale;

        Collider primitiveCollider = primitive.GetComponent<Collider>();

        if (primitiveCollider != null)
        {
            primitiveCollider.enabled = false;

            if (Application.isPlaying)
            {
                Object.Destroy(primitiveCollider);
            }
            else
            {
                Object.DestroyImmediate(primitiveCollider);
            }
        }

        Renderer primitiveRenderer = primitive.GetComponent<Renderer>();

        if (primitiveRenderer != null)
        {
            MaterialPropertyBlock properties = new MaterialPropertyBlock();
            properties.SetColor("_BaseColor", colour);
            properties.SetColor("_Color", colour);
            primitiveRenderer.SetPropertyBlock(properties);
            primitiveRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            primitiveRenderer.receiveShadows = false;
        }

        return primitive;
    }
}
