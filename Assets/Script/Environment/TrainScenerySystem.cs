using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Keeps the train stationary and moves pooled scenery past it to simulate travel.
/// All positions are local to this component: X is the distance from the train,
/// Y is the height, and the default travel axis is Z.
/// </summary>
[DisallowMultipleComponent]
public sealed class TrainScenerySystem : MonoBehaviour
{
    public enum SideSelection
    {
        Both,
        Left,
        Right
    }

    public enum GeneratedShape
    {
        Tree,
        UtilityPole,
        Building,
        Rock
    }

    [Serializable]
    public sealed class SceneryLayer
    {
        [Tooltip("Name shown in the Inspector and used for the pool hierarchy.")]
        public string name = "Scenery layer";

        public bool enabled = true;

        [Tooltip("Optional scenery prefabs. One is chosen when the pool grows. Leave empty to use the generated test shape.")]
        public GameObject[] prefabs = Array.Empty<GameObject>();

        [Tooltip("Simple placeholder used when no prefab is assigned.")]
        public GeneratedShape generatedShape = GeneratedShape.Tree;

        [Tooltip("Colour applied only to generated placeholder shapes.")]
        public Color generatedColor = new Color(0.24f, 0.36f, 0.16f, 1f);

        [Min(1)]
        [Tooltip("Maximum number of objects kept by this layer.")]
        public int poolSize = 40;

        [Tooltip("Choose on which side of the train objects can appear.")]
        public SideSelection side = SideSelection.Both;

        [Tooltip("Alternate left/right instead of selecting a random side.")]
        public bool alternateSides = true;

        [Tooltip("Minimum and maximum absolute X distance from the centre of the train.")]
        public Vector2 xDistance = new Vector2(7f, 12f);

        [Tooltip("Minimum and maximum local Y height.")]
        public Vector2 yPosition = new Vector2(0f, 0f);

        [Tooltip("Seconds between two scenery objects. The actual spacing is speed multiplied by this delta time.")]
        public Vector2 intervalSeconds = new Vector2(0.6f, 1.2f);

        [Min(0f)]
        [Tooltip("Time offset before this layer starts spawning. Useful to stagger several types of scenery.")]
        public float startDelay;

        [Range(0.05f, 3f)]
        [Tooltip("Relative movement speed. Lower values make distant layers move more slowly for parallax.")]
        public float speedMultiplier = 1f;

        [Min(0.1f)]
        [Tooltip("Distance in front of the train where objects enter the loop.")]
        public float spawnDistance = 90f;

        [Min(0.1f)]
        [Tooltip("Distance behind the train where objects return to the pool.")]
        public float despawnDistance = 50f;

        [Tooltip("Random local scale multiplier, chosen independently on X, Y and Z.")]
        public Vector3 minimumScale = Vector3.one;

        [Tooltip("Random local scale multiplier, chosen independently on X, Y and Z.")]
        public Vector3 maximumScale = Vector3.one;

        [Tooltip("Random rotation around local Y, in degrees.")]
        public Vector2 yRotation = new Vector2(0f, 360f);

        [Tooltip("Disable prefab colliders because exterior scenery should not interact with the player.")]
        public bool disableColliders = true;
    }

    private sealed class PooledObject
    {
        public GameObject GameObject;
        public Transform Transform;
        public Vector3 BaseScale;
        public Quaternion BaseRotation;
    }

    private sealed class LayerRuntime
    {
        public readonly List<PooledObject> Pool = new List<PooledObject>();
        public Transform Root;
        public float SpawnTimer;
        public bool SpawnRightNext;
    }

    [Header("Train movement illusion")]
    [SerializeField, Min(0f)] private float trainSpeed = 18f;

    [SerializeField]
    [Tooltip("Local direction followed by the exterior. With (0, 0, -1), scenery appears at +Z and exits at -Z.")]
    private Vector3 movementDirection = Vector3.back;

    [SerializeField]
    [Tooltip("Immediately populate the complete visible strip when Play mode starts.")]
    private bool fillVisibleAreaAtStart = true;

    [SerializeField]
    [Tooltip("When enabled, scenery continues moving while Time.timeScale is zero.")]
    private bool useUnscaledTime;

    [Header("Procedural variation")]
    [SerializeField] private int randomSeed = 1977;
    [SerializeField] private bool randomizeSeedAtRuntime;

    [Header("Scenery layers")]
    [SerializeField] private List<SceneryLayer> layers = new List<SceneryLayer>
    {
        new SceneryLayer
        {
            name = "Poteaux proches",
            generatedShape = GeneratedShape.UtilityPole,
            generatedColor = new Color(0.27f, 0.18f, 0.11f, 1f),
            poolSize = 40,
            xDistance = new Vector2(6f, 9f),
            intervalSeconds = new Vector2(0.7f, 1.1f),
            speedMultiplier = 1f,
            spawnDistance = 90f,
            despawnDistance = 50f,
            minimumScale = new Vector3(0.85f, 0.85f, 0.85f),
            maximumScale = new Vector3(1.15f, 1.2f, 1.15f),
            yRotation = Vector2.zero
        },
        new SceneryLayer
        {
            name = "Arbres moyens",
            generatedShape = GeneratedShape.Tree,
            generatedColor = new Color(0.22f, 0.38f, 0.16f, 1f),
            poolSize = 55,
            alternateSides = false,
            xDistance = new Vector2(10f, 20f),
            intervalSeconds = new Vector2(0.35f, 0.9f),
            startDelay = 0.25f,
            speedMultiplier = 0.82f,
            spawnDistance = 100f,
            despawnDistance = 60f,
            minimumScale = new Vector3(0.8f, 0.75f, 0.8f),
            maximumScale = new Vector3(1.5f, 1.8f, 1.5f)
        },
        new SceneryLayer
        {
            name = "Batiments lointains",
            generatedShape = GeneratedShape.Building,
            generatedColor = new Color(0.48f, 0.42f, 0.31f, 1f),
            poolSize = 24,
            alternateSides = false,
            xDistance = new Vector2(28f, 48f),
            intervalSeconds = new Vector2(1.8f, 3.5f),
            startDelay = 0.8f,
            speedMultiplier = 0.48f,
            spawnDistance = 115f,
            despawnDistance = 75f,
            minimumScale = new Vector3(1.5f, 1.2f, 1.5f),
            maximumScale = new Vector3(4f, 4.5f, 4f),
            yRotation = new Vector2(-8f, 8f)
        }
    };

    private readonly List<LayerRuntime> runtimeLayers = new List<LayerRuntime>();
    private System.Random random;
    private Transform poolRoot;
    private Vector3 normalizedMovementDirection;

    public float TrainSpeed
    {
        get => trainSpeed;
        set => trainSpeed = Mathf.Max(0f, value);
    }

    /// <summary>
    /// Local direction followed by scenery moving past the stationary train.
    /// Other exterior systems use this value to remain perfectly synchronized.
    /// </summary>
    public Vector3 MovementDirection => movementDirection.sqrMagnitude > 0.0001f
        ? movementDirection.normalized
        : Vector3.back;

    /// <summary>
    /// Indicates which Unity clock drives the exterior movement.
    /// </summary>
    public bool UsesUnscaledTime => useUnscaledTime;

    private void Awake()
    {
        Initialize();
    }

    private void Update()
    {
        if (runtimeLayers.Count != layers.Count || poolRoot == null)
        {
            RebuildScenery();
            return;
        }

        if (movementDirection.sqrMagnitude > 0.0001f)
        {
            normalizedMovementDirection = movementDirection.normalized;
        }

        float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        if (deltaTime <= 0f)
        {
            return;
        }

        for (int layerIndex = 0; layerIndex < layers.Count; layerIndex++)
        {
            SceneryLayer layer = layers[layerIndex];
            LayerRuntime runtime = runtimeLayers[layerIndex];

            if (!layer.enabled)
            {
                SetLayerActive(runtime, false);
                continue;
            }

            SetLayerActive(runtime, true);
            MoveLayer(layer, runtime, deltaTime);
            UpdateSpawning(layer, runtime, deltaTime);
        }
    }

    [ContextMenu("Rebuild scenery now")]
    public void RebuildScenery()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "Le decor procedural se construit en Play mode. Lancez la scene avant d'utiliser cette commande.",
                this);
            return;
        }

        ClearRuntimeObjects();
        Initialize();
    }

    private void Initialize()
    {
        normalizedMovementDirection = movementDirection.sqrMagnitude > 0.0001f
            ? movementDirection.normalized
            : Vector3.back;

        random = new System.Random(randomizeSeedAtRuntime
            ? Environment.TickCount
            : randomSeed);

        GameObject rootObject = new GameObject("Generated Scenery Pools");
        rootObject.transform.SetParent(transform, false);
        poolRoot = rootObject.transform;

        runtimeLayers.Clear();

        for (int layerIndex = 0; layerIndex < layers.Count; layerIndex++)
        {
            SceneryLayer layer = layers[layerIndex];
            GameObject layerObject = new GameObject(layer.name);
            layerObject.transform.SetParent(poolRoot, false);

            LayerRuntime runtime = new LayerRuntime
            {
                Root = layerObject.transform,
                SpawnTimer = Mathf.Max(0f, layer.startDelay),
                SpawnRightNext = random.NextDouble() >= 0.5
            };

            runtimeLayers.Add(runtime);

            if (layer.enabled && fillVisibleAreaAtStart)
            {
                FillVisibleStrip(layer, runtime);
            }
        }
    }

    private void FillVisibleStrip(SceneryLayer layer, LayerRuntime runtime)
    {
        float layerSpeed = Mathf.Max(0.01f, trainSpeed * layer.speedMultiplier);
        float averageInterval = Mathf.Max(0.05f, Average(layer.intervalSeconds));
        float averageGap = layerSpeed * averageInterval;
        float stripLength = layer.spawnDistance + layer.despawnDistance;
        int count = Mathf.Min(layer.poolSize, Mathf.CeilToInt(stripLength / averageGap));

        if (count <= 0)
        {
            return;
        }

        float evenGap = stripLength / count;
        float progress = -layer.spawnDistance + evenGap * 0.5f;

        for (int index = 0; index < count; index++)
        {
            TrySpawn(layer, runtime, progress);
            progress += evenGap;
        }
    }

    private void MoveLayer(SceneryLayer layer, LayerRuntime runtime, float deltaTime)
    {
        Vector3 movement = normalizedMovementDirection
            * (trainSpeed * layer.speedMultiplier * deltaTime);

        foreach (PooledObject pooledObject in runtime.Pool)
        {
            if (!pooledObject.GameObject.activeSelf)
            {
                continue;
            }

            pooledObject.Transform.localPosition += movement;
            float progress = Vector3.Dot(
                pooledObject.Transform.localPosition,
                normalizedMovementDirection);

            if (progress >= layer.despawnDistance)
            {
                pooledObject.GameObject.SetActive(false);
            }
        }
    }

    private void UpdateSpawning(
        SceneryLayer layer,
        LayerRuntime runtime,
        float deltaTime)
    {
        if (trainSpeed <= 0f)
        {
            return;
        }

        runtime.SpawnTimer -= deltaTime;

        if (runtime.SpawnTimer > 0f)
        {
            return;
        }

        TrySpawn(layer, runtime, -layer.spawnDistance);
        runtime.SpawnTimer = RandomRange(layer.intervalSeconds, 0.05f);
    }

    private bool TrySpawn(
        SceneryLayer layer,
        LayerRuntime runtime,
        float movementProgress)
    {
        PooledObject pooledObject = FindAvailable(runtime);

        if (pooledObject == null)
        {
            if (runtime.Pool.Count >= layer.poolSize)
            {
                return false;
            }

            pooledObject = CreatePooledObject(layer, runtime);
            runtime.Pool.Add(pooledObject);
        }

        float x = PickXPosition(layer, runtime);
        float y = RandomRange(layer.yPosition);
        Vector3 lateralPosition = new Vector3(x, y, 0f);
        pooledObject.Transform.localPosition = lateralPosition
            + normalizedMovementDirection * movementProgress;

        Vector3 scaleMultiplier = new Vector3(
            RandomRange(layer.minimumScale.x, layer.maximumScale.x),
            RandomRange(layer.minimumScale.y, layer.maximumScale.y),
            RandomRange(layer.minimumScale.z, layer.maximumScale.z));
        pooledObject.Transform.localScale = Vector3.Scale(
            pooledObject.BaseScale,
            scaleMultiplier);

        float yRotation = RandomRange(layer.yRotation);
        pooledObject.Transform.localRotation = pooledObject.BaseRotation
            * Quaternion.Euler(0f, yRotation, 0f);
        pooledObject.GameObject.SetActive(true);
        return true;
    }

    private PooledObject CreatePooledObject(
        SceneryLayer layer,
        LayerRuntime runtime)
    {
        GameObject instance;
        GameObject prefab = PickPrefab(layer.prefabs);

        if (prefab != null)
        {
            instance = Instantiate(prefab, runtime.Root);
            instance.name = prefab.name;
        }
        else
        {
            instance = ProceduralSceneryFactory.Create(
                layer.generatedShape,
                layer.generatedColor,
                runtime.Root);
        }

        if (layer.disableColliders)
        {
            DisableColliders(instance);
        }

        instance.SetActive(false);
        return new PooledObject
        {
            GameObject = instance,
            Transform = instance.transform,
            BaseScale = instance.transform.localScale,
            BaseRotation = instance.transform.localRotation
        };
    }

    private GameObject PickPrefab(GameObject[] prefabs)
    {
        if (prefabs == null || prefabs.Length == 0)
        {
            return null;
        }

        int firstIndex = random.Next(0, prefabs.Length);

        for (int offset = 0; offset < prefabs.Length; offset++)
        {
            GameObject candidate = prefabs[(firstIndex + offset) % prefabs.Length];

            if (candidate != null)
            {
                return candidate;
            }
        }

        return null;
    }

    private float PickXPosition(SceneryLayer layer, LayerRuntime runtime)
    {
        float distance = RandomRange(layer.xDistance, 0f);

        switch (layer.side)
        {
            case SideSelection.Left:
                return -distance;

            case SideSelection.Right:
                return distance;

            default:
                bool spawnRight;

                if (layer.alternateSides)
                {
                    spawnRight = runtime.SpawnRightNext;
                    runtime.SpawnRightNext = !runtime.SpawnRightNext;
                }
                else
                {
                    spawnRight = random.NextDouble() >= 0.5;
                }

                return spawnRight ? distance : -distance;
        }
    }

    private static PooledObject FindAvailable(LayerRuntime runtime)
    {
        foreach (PooledObject pooledObject in runtime.Pool)
        {
            if (!pooledObject.GameObject.activeSelf)
            {
                return pooledObject;
            }
        }

        return null;
    }

    private static void DisableColliders(GameObject instance)
    {
        foreach (Collider sceneryCollider in instance.GetComponentsInChildren<Collider>(true))
        {
            sceneryCollider.enabled = false;
        }
    }

    private static void SetLayerActive(LayerRuntime runtime, bool active)
    {
        if (runtime.Root != null && runtime.Root.gameObject.activeSelf != active)
        {
            runtime.Root.gameObject.SetActive(active);
        }
    }

    private float RandomRange(Vector2 range, float absoluteMinimum = float.NegativeInfinity)
    {
        return RandomRange(
            Mathf.Max(absoluteMinimum, Mathf.Min(range.x, range.y)),
            Mathf.Max(absoluteMinimum, Mathf.Max(range.x, range.y)));
    }

    private float RandomRange(float minimum, float maximum)
    {
        if (minimum > maximum)
        {
            (minimum, maximum) = (maximum, minimum);
        }

        return Mathf.Lerp(minimum, maximum, (float)random.NextDouble());
    }

    private static float Average(Vector2 range)
    {
        return (range.x + range.y) * 0.5f;
    }

    private void ClearRuntimeObjects()
    {
        runtimeLayers.Clear();

        if (poolRoot == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            poolRoot.gameObject.SetActive(false);
            Destroy(poolRoot.gameObject);
        }
        else
        {
            DestroyImmediate(poolRoot.gameObject);
        }

        poolRoot = null;
    }

    private void OnDestroy()
    {
        runtimeLayers.Clear();
    }

    private void OnValidate()
    {
        trainSpeed = Mathf.Max(0f, trainSpeed);

        if (movementDirection.sqrMagnitude < 0.0001f)
        {
            movementDirection = Vector3.back;
        }

        if (layers == null)
        {
            layers = new List<SceneryLayer>();
            return;
        }

        foreach (SceneryLayer layer in layers)
        {
            if (layer == null)
            {
                continue;
            }

            layer.poolSize = Mathf.Max(1, layer.poolSize);
            layer.startDelay = Mathf.Max(0f, layer.startDelay);
            layer.speedMultiplier = Mathf.Max(0.05f, layer.speedMultiplier);
            layer.spawnDistance = Mathf.Max(0.1f, layer.spawnDistance);
            layer.despawnDistance = Mathf.Max(0.1f, layer.despawnDistance);
            layer.xDistance.x = Mathf.Max(0f, layer.xDistance.x);
            layer.xDistance.y = Mathf.Max(0f, layer.xDistance.y);
            layer.intervalSeconds.x = Mathf.Max(0.05f, layer.intervalSeconds.x);
            layer.intervalSeconds.y = Mathf.Max(0.05f, layer.intervalSeconds.y);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 direction = movementDirection.sqrMagnitude > 0.0001f
            ? movementDirection.normalized
            : Vector3.back;

        Gizmos.matrix = transform.localToWorldMatrix;

        foreach (SceneryLayer layer in layers)
        {
            if (layer == null || !layer.enabled)
            {
                continue;
            }

            float maxX = Mathf.Max(layer.xDistance.x, layer.xDistance.y);
            float y = Average(layer.yPosition);
            Vector3 spawnCentre = Vector3.up * y - direction * layer.spawnDistance;
            Vector3 despawnCentre = Vector3.up * y + direction * layer.despawnDistance;

            Gizmos.color = new Color(
                layer.generatedColor.r,
                layer.generatedColor.g,
                layer.generatedColor.b,
                0.8f);
            Gizmos.DrawLine(spawnCentre - Vector3.right * maxX, spawnCentre + Vector3.right * maxX);
            Gizmos.DrawLine(despawnCentre - Vector3.right * maxX, despawnCentre + Vector3.right * maxX);
            Gizmos.DrawLine(spawnCentre - Vector3.right * maxX, despawnCentre - Vector3.right * maxX);
            Gizmos.DrawLine(spawnCentre + Vector3.right * maxX, despawnCentre + Vector3.right * maxX);
        }
    }
}
