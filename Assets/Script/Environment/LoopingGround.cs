using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Generates a collider-free ground strip and loops it at exactly the speed of
/// the exterior scenery, keeping the stationary-train illusion seamless.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(TrainScenerySystem))]
public sealed class LoopingGround : MonoBehaviour
{
    [SerializeField]
    private TrainScenerySystem scenerySystem;

    [Header("Ground appearance")]
    [SerializeField]
    private Material groundMaterial;

    [SerializeField]
    private Color groundColour = new Color(0.22f, 0.18f, 0.11f, 1f);

    [SerializeField]
    [Tooltip("Local centre of the generated ground strip.")]
    private Vector3 groundCentre = new Vector3(0f, -1f, 0f);

    [Header("Ground loop")]
    [SerializeField, Min(1f)]
    private float groundWidth = 110f;

    [SerializeField, Min(1f)]
    private float segmentLength = 50f;

    [SerializeField, Min(2)]
    private int segmentCount = 5;

    [SerializeField, Min(0.01f)]
    private float groundThickness = 0.2f;

    private Transform generatedRoot;
    private Transform[] segments;
    private MaterialPropertyBlock colourProperties;

    private void Reset()
    {
        scenerySystem = GetComponent<TrainScenerySystem>();
    }

    private void Start()
    {
        ResolveScenerySystem();
        BuildGround();
    }

    private void Update()
    {
        if (scenerySystem == null || segments == null || segments.Length == 0)
        {
            return;
        }

        float deltaTime = scenerySystem.UsesUnscaledTime
            ? Time.unscaledDeltaTime
            : Time.deltaTime;

        if (deltaTime <= 0f || scenerySystem.TrainSpeed <= 0f)
        {
            return;
        }

        Vector3 direction = scenerySystem.MovementDirection;
        Vector3 movement = direction * (scenerySystem.TrainSpeed * deltaTime);
        float cycleLength = segmentLength * segmentCount;
        float halfCycle = cycleLength * 0.5f;

        foreach (Transform segment in segments)
        {
            if (segment == null)
            {
                continue;
            }

            segment.localPosition += movement;
            float progress = Vector3.Dot(
                segment.localPosition - groundCentre,
                direction);

            while (progress > halfCycle)
            {
                segment.localPosition -= direction * cycleLength;
                progress -= cycleLength;
            }
        }
    }

    [ContextMenu("Rebuild sliding ground now")]
    public void RebuildGround()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "Le sol defilant se construit en Play mode.",
                this);
            return;
        }

        ClearGround();
        ResolveScenerySystem();
        BuildGround();
    }

    private void ResolveScenerySystem()
    {
        if (scenerySystem == null)
        {
            scenerySystem = GetComponent<TrainScenerySystem>();
        }
    }

    private void BuildGround()
    {
        if (scenerySystem == null || generatedRoot != null)
        {
            return;
        }

        GameObject rootObject = new GameObject("Generated Sliding Ground");
        rootObject.transform.SetParent(transform, false);
        generatedRoot = rootObject.transform;
        segments = new Transform[segmentCount];

        Vector3 direction = scenerySystem.MovementDirection;
        Quaternion groundRotation = Quaternion.FromToRotation(
            Vector3.forward,
            direction);

        for (int index = 0; index < segmentCount; index++)
        {
            GameObject segmentObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            segmentObject.name = $"Ground Segment {index + 1}";
            segmentObject.transform.SetParent(generatedRoot, false);

            float axisOffset = (index - (segmentCount - 1) * 0.5f)
                * segmentLength;
            segmentObject.transform.localPosition = groundCentre
                + direction * axisOffset;
            segmentObject.transform.localRotation = groundRotation;
            segmentObject.transform.localScale = new Vector3(
                groundWidth,
                groundThickness,
                segmentLength);

            ConfigureRenderer(segmentObject.GetComponent<MeshRenderer>());

            Collider groundCollider = segmentObject.GetComponent<Collider>();

            if (groundCollider != null)
            {
                groundCollider.enabled = false;
                Destroy(groundCollider);
            }

            segments[index] = segmentObject.transform;
        }
    }

    private void ConfigureRenderer(MeshRenderer groundRenderer)
    {
        if (groundRenderer == null)
        {
            return;
        }

        groundRenderer.shadowCastingMode = ShadowCastingMode.Off;
        groundRenderer.receiveShadows = true;

        if (groundMaterial != null)
        {
            groundRenderer.sharedMaterial = groundMaterial;
            return;
        }

        if (colourProperties == null)
        {
            colourProperties = new MaterialPropertyBlock();
        }

        colourProperties.SetColor("_BaseColor", groundColour);
        colourProperties.SetColor("_Color", groundColour);
        groundRenderer.SetPropertyBlock(colourProperties);
    }

    private void ClearGround()
    {
        segments = null;

        if (generatedRoot == null)
        {
            return;
        }

        generatedRoot.gameObject.SetActive(false);
        Destroy(generatedRoot.gameObject);
        generatedRoot = null;
    }

    private void OnValidate()
    {
        groundWidth = Mathf.Max(1f, groundWidth);
        segmentLength = Mathf.Max(1f, segmentLength);
        segmentCount = Mathf.Max(2, segmentCount);
        groundThickness = Mathf.Max(0.01f, groundThickness);
    }
}
