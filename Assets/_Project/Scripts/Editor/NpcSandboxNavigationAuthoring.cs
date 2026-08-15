#if UNITY_EDITOR
using System;
using System.IO;
using GiscardPunk77.AI.Navigation;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace GiscardPunk77.EditorTools
{
    public static class NpcSandboxNavigationAuthoring
    {
        private const string MenuPath = "Tools/GiscardPunk77/P04/Create or Update Navigation Sandbox";
        private const string DoorStressMenuPath = "Tools/GiscardPunk77/P06/Create or Update Door Stress Sandbox";
        private const string ScenePath = "Assets/_Project/Scenes/Tests/NpcSandbox.unity";
        private const string RootName = "P04 Navigation Sandbox Generated";

        [InitializeOnLoadMethod]
        private static void ScheduleInitialSceneCreation()
        {
            if (!File.Exists(ScenePath))
            {
                EditorApplication.delayCall += TryCreateMissingScene;
            }
        }

        [MenuItem(MenuPath)]
        private static void CreateOrUpdateFromMenu()
        {
            CreateOrUpdateInIsolation();
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            EditorGUIUtility.PingObject(sceneAsset);
        }

        [MenuItem(DoorStressMenuPath)]
        private static void CreateOrUpdateDoorStressFromMenu()
        {
            CreateOrUpdateFromMenu();
        }

        public static void BuildFromCommandLine()
        {
            CreateOrUpdateInIsolation(true);
        }

        private static void TryCreateMissingScene()
        {
            EditorApplication.delayCall -= TryCreateMissingScene;

            if (File.Exists(ScenePath))
            {
                return;
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.delayCall += TryCreateMissingScene;
                return;
            }

            try
            {
                CreateOrUpdateInIsolation();
            }
            catch (Exception exception)
            {
                Debug.LogError($"P04 automatic sandbox creation failed: {exception}");
            }
        }

        private static void CreateOrUpdateInIsolation(bool replaceUntitledScene = false)
        {
            EnsureSceneDirectoryExists();

            var scene = SceneManager.GetSceneByPath(ScenePath);
            var closeAfterSave = !scene.IsValid() || !scene.isLoaded;

            if (closeAfterSave)
            {
                if (File.Exists(ScenePath))
                {
                    scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
                }
                else
                {
                    if (replaceUntitledScene)
                    {
                        scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                        closeAfterSave = false;
                    }
                    else
                    {
                        var activeScene = SceneManager.GetActiveScene();
                        var canReuseEmptyUntitledScene = activeScene.IsValid()
                            && string.IsNullOrEmpty(activeScene.path)
                            && activeScene.rootCount == 0
                            && !activeScene.isDirty;

                        if (canReuseEmptyUntitledScene)
                        {
                            scene = activeScene;
                            closeAfterSave = false;
                        }
                        else
                        {
                            scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                        }
                    }
                }
            }

            var root = FindRoot(scene, RootName);
            if (root == null)
            {
                root = new GameObject(RootName);
                SceneManager.MoveGameObjectToScene(root, scene);
            }

            root.transform.SetPositionAndRotation(new Vector3(0f, 0f, 30f), Quaternion.identity);
            root.transform.localScale = Vector3.one;

            var doorLeaf = ConfigureGeometry(root.transform);
            ConfigureNavigation(root.transform, doorLeaf);

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new InvalidOperationException($"Unity could not save {ScenePath}.");
            }

            if (closeAfterSave)
            {
                EditorSceneManager.CloseScene(scene, true);
            }

            AssetDatabase.SaveAssets();
            Debug.Log(
                $"P04 navigation sandbox created or updated at {ScenePath}. " +
                "Open it, select the NavMeshSurface and press Bake for H2.");
        }

        private static void EnsureSceneDirectoryExists()
        {
            var directory = Path.GetDirectoryName(ScenePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private static GameObject ConfigureGeometry(Transform root)
        {
            var geometry = FindOrCreate(root, "Geometry").transform;
            var sectionA = FindOrCreate(geometry, "Section A - Narrow Wagon").transform;
            var sectionB = FindOrCreate(geometry, "Section B - Passing Bay Wagon").transform;
            var threshold = FindOrCreate(geometry, "Door Threshold").transform;

            ConfigureSectionA(sectionA);
            ConfigureSectionB(sectionB);
            return ConfigureDoorThreshold(threshold);
        }

        private static void ConfigureSectionA(Transform section)
        {
            ConfigureBox(section, "Floor A", new Vector3(0f, -0.15f, -7.575f), new Vector3(5f, 0.3f, 14.85f));
            ConfigureBox(section, "Outer Wall A Left", new Vector3(-2.55f, 1.5f, -7.575f), new Vector3(0.1f, 3f, 14.85f));
            ConfigureBox(section, "Outer Wall A Right", new Vector3(2.55f, 1.5f, -7.575f), new Vector3(0.1f, 3f, 14.85f));
            ConfigureBox(section, "End Wall A", new Vector3(0f, 1.5f, -15f), new Vector3(5.2f, 3f, 0.1f));

            ConfigureBox(section, "Seat Block A Left", new Vector3(-1.65f, 0.6f, -8f), new Vector3(1.8f, 1.2f, 12.5f));
            ConfigureBox(section, "Seat Block A Right", new Vector3(1.65f, 0.6f, -8f), new Vector3(1.8f, 1.2f, 12.5f));
        }

        private static void ConfigureSectionB(Transform section)
        {
            ConfigureBox(section, "Floor B", new Vector3(0f, -0.15f, 7.575f), new Vector3(5f, 0.3f, 14.85f));
            ConfigureBox(section, "Outer Wall B Left", new Vector3(-2.55f, 1.5f, 7.575f), new Vector3(0.1f, 3f, 14.85f));
            ConfigureBox(section, "Outer Wall B Right", new Vector3(2.55f, 1.5f, 7.575f), new Vector3(0.1f, 3f, 14.85f));
            ConfigureBox(section, "End Wall B", new Vector3(0f, 1.5f, 15f), new Vector3(5.2f, 3f, 0.1f));

            ConfigureBox(section, "Seat Block B Entry Left", new Vector3(-1.65f, 0.6f, 2f), new Vector3(1.8f, 1.2f, 3.5f));
            ConfigureBox(section, "Seat Block B Entry Right", new Vector3(1.65f, 0.6f, 2f), new Vector3(1.8f, 1.2f, 3.5f));
            ConfigureBox(section, "Seat Block B End Left", new Vector3(-1.65f, 0.6f, 10f), new Vector3(1.8f, 1.2f, 7f));
            ConfigureBox(section, "Seat Block B End Right", new Vector3(1.65f, 0.6f, 10f), new Vector3(1.8f, 1.2f, 7f));
        }

        private static GameObject ConfigureDoorThreshold(Transform threshold)
        {
            ConfigureBox(threshold, "Door Frame Left", new Vector3(-1.625f, 1.25f, 0f), new Vector3(1.75f, 2.5f, 0.25f));
            ConfigureBox(threshold, "Door Frame Right", new Vector3(1.625f, 1.25f, 0f), new Vector3(1.75f, 2.5f, 0.25f));

            var legacyLeaf = threshold.Find("Door Leaf - Open");
            if (legacyLeaf != null && threshold.Find("Door Leaf") == null)
            {
                legacyLeaf.name = "Door Leaf";
            }

            return ConfigureBox(
                threshold,
                "Door Leaf",
                new Vector3(0f, 1.25f, 0f),
                new Vector3(NpcSandboxTuning.DoorOpeningWidth, 2.5f, 0.25f));
        }

        private static void ConfigureNavigation(Transform root, GameObject doorLeaf)
        {
            var navigation = FindOrCreate(root, "Navigation").transform;
            var markers = FindOrCreate(root, "Markers and Gizmos").transform;

            var surface = GetOrAdd<NavMeshSurface>(root.gameObject);
            surface.agentTypeID = NpcSandboxTuning.HumanoidAgentTypeId;
            surface.collectObjects = CollectObjects.Children;
            surface.layerMask = ~0;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.defaultArea = 0;
            surface.ignoreNavMeshAgent = true;
            surface.ignoreNavMeshObstacle = true;
            surface.overrideVoxelSize = true;
            surface.voxelSize = NpcSandboxTuning.VoxelSize;
            surface.overrideTileSize = true;
            surface.tileSize = NpcSandboxTuning.TileSize;
            surface.minRegionArea = NpcSandboxTuning.MinimumRegionArea;
            surface.buildHeightMesh = false;

            var linkStart = ConfigureMarker(markers, "Link Start A", new Vector3(0f, 0.02f, -0.45f));
            var linkEnd = ConfigureMarker(markers, "Link End B", new Vector3(0f, 0.02f, 0.45f));
            var linkObject = FindOrCreate(navigation, "NavMeshLink - Door Threshold");
            var link = GetOrAdd<NavMeshLink>(linkObject);
            link.agentTypeID = NpcSandboxTuning.HumanoidAgentTypeId;
            link.startTransform = linkStart;
            link.endTransform = linkEnd;
            link.width = NpcSandboxTuning.ThresholdLinkWidth;
            link.costModifier = -1f;
            link.bidirectional = true;
            link.autoUpdate = false;
            link.area = 0;

            var waitingA = ConfigureMarker(markers, "Waiting Point A", new Vector3(0f, 0.02f, -1.7f));
            var waitingB = ConfigureMarker(markers, "Waiting Point B", new Vector3(0f, 0.02f, 1.7f));

            var slidingDoor = GetOrAdd<SlidingDoor>(doorLeaf);
            slidingDoor.ConfigureMovement(Vector3.left, 2.25f, 3f);
            slidingDoor.ConfigurePassage(waitingA, waitingB, 5f);

            var destinationAEnd = ConfigureMarker(markers, "Destination A - End", new Vector3(0f, 0.02f, -12f));
            var destinationAThreshold = ConfigureMarker(markers, "Destination A - Threshold", new Vector3(0f, 0.02f, -1.5f));
            var destinationBThreshold = ConfigureMarker(markers, "Destination B - Threshold", new Vector3(0f, 0.02f, 1.5f));
            var destinationBay = ConfigureMarker(markers, "Destination B - Passing Bay", new Vector3(-1.2f, 0.02f, 5f));
            var destinationBEnd = ConfigureMarker(markers, "Destination B - End", new Vector3(0f, 0.02f, 12f));
            var destinations = new[]
            {
                destinationAEnd,
                destinationAThreshold,
                destinationBThreshold,
                destinationBay,
                destinationBEnd,
                destinationBay,
                destinationBThreshold,
                destinationAThreshold
            };

            var firingA = ConfigureMarker(markers, "Firing Position A", new Vector3(0f, 0.02f, -10f));
            firingA.localRotation = Quaternion.identity;
            var firingB = ConfigureMarker(markers, "Firing Position B", new Vector3(1.2f, 0.02f, 5f));
            firingB.localRotation = Quaternion.Euler(0f, 180f, 0f);

            var agentObject = FindOrCreate(root, "P04 Capsule Navigation Probe");
            agentObject.transform.localPosition = new Vector3(0f, 0f, -12f);
            agentObject.transform.localRotation = Quaternion.identity;
            var agentCollider = GetOrAdd<CapsuleCollider>(agentObject);
            agentCollider.center = Vector3.up;
            agentCollider.radius = NpcSandboxTuning.AgentRadius;
            agentCollider.height = NpcSandboxTuning.AgentHeight;
            var motor = GetOrAdd<NpcMotor>(agentObject);
            motor.ApplyInitialAgentSettings();
            var probe = GetOrAdd<NpcSandboxAgentProbe>(agentObject);
            probe.Configure(motor, destinations);
            probe.enabled = false;

            var visual = FindOrCreatePrimitive(agentObject.transform, "Capsule Visual", PrimitiveType.Capsule);
            visual.transform.localPosition = Vector3.up;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one;
            var visualCollider = visual.GetComponent<Collider>();
            if (visualCollider != null)
            {
                visualCollider.enabled = false;
            }

            var gizmos = GetOrAdd<NpcSandboxNavigationGizmos>(markers.gameObject);
            gizmos.Configure(
                CreateWalkableZones(),
                new[] { linkStart },
                new[] { linkEnd },
                new[] { waitingA, waitingB },
                destinations,
                new[] { firingA, firingB });

            ConfigureDoorStress(root, slidingDoor);
        }

        private static void ConfigureDoorStress(Transform root, SlidingDoor door)
        {
            var stressRoot = FindOrCreate(root, "P06 Door Traversal Stress").transform;
            var lateralOffsets = new[]
            {
                -1.8f,
                -0.6f,
                0.6f,
                1.8f
            };
            var traversals = new NpcDoorTraversal[lateralOffsets.Length];

            for (var index = 0; index < lateralOffsets.Length; index++)
            {
                var clearanceA = ConfigureMarker(
                    stressRoot,
                    $"P06 Clearance A {index + 1}",
                    new Vector3(lateralOffsets[index], 0.02f, -0.75f));
                var clearanceB = ConfigureMarker(
                    stressRoot,
                    $"P06 Clearance B {index + 1}",
                    new Vector3(lateralOffsets[index], 0.02f, 5f));
                var agentObject = FindOrCreate(stressRoot, $"P06 Door Capsule {index + 1}");
                agentObject.transform.localPosition = index % 2 == 0
                    ? clearanceA.localPosition
                    : clearanceB.localPosition;
                agentObject.transform.localRotation = Quaternion.identity;

                var motor = GetOrAdd<NpcMotor>(agentObject);
                motor.ApplyInitialAgentSettings();
                var health = GetOrAdd<GiscardPunk77.Gameplay.Health>(agentObject);
                var traversal = GetOrAdd<NpcDoorTraversal>(agentObject);
                traversal.Configure(motor, door, health, false, 60f);
                traversal.ConfigureClearancePoints(clearanceA, clearanceB);
                traversals[index] = traversal;

                var visual = FindOrCreatePrimitive(agentObject.transform, "Capsule Visual", PrimitiveType.Capsule);
                visual.transform.localPosition = Vector3.up;
                visual.transform.localRotation = Quaternion.identity;
                visual.transform.localScale = Vector3.one;
                var visualCollider = visual.GetComponent<Collider>();
                if (visualCollider != null)
                {
                    visualCollider.enabled = false;
                }
            }

            var stressProbe = GetOrAdd<NpcDoorTraversalStressProbe>(stressRoot.gameObject);
            stressProbe.Configure(traversals, 20);
        }

        private static Bounds[] CreateWalkableZones()
        {
            return new[]
            {
                new Bounds(new Vector3(0f, 0.05f, -8f), new Vector3(NpcSandboxTuning.NarrowCorridorWidth, 0.1f, 12.5f)),
                new Bounds(new Vector3(0f, 0.05f, 2f), new Vector3(NpcSandboxTuning.NarrowCorridorWidth, 0.1f, 3.5f)),
                new Bounds(new Vector3(0f, 0.05f, 5f), new Vector3(NpcSandboxTuning.PassingBayWidth, 0.1f, 3f)),
                new Bounds(new Vector3(0f, 0.05f, 10f), new Vector3(NpcSandboxTuning.NarrowCorridorWidth, 0.1f, 7f))
            };
        }

        private static Transform ConfigureMarker(Transform parent, string name, Vector3 localPosition)
        {
            var marker = FindOrCreate(parent, name).transform;
            marker.localPosition = localPosition;
            marker.localRotation = Quaternion.identity;
            marker.localScale = Vector3.one;
            return marker;
        }

        private static GameObject ConfigureBox(Transform parent, string name, Vector3 localPosition, Vector3 localScale)
        {
            var box = FindOrCreatePrimitive(parent, name, PrimitiveType.Cube);
            box.transform.localPosition = localPosition;
            box.transform.localRotation = Quaternion.identity;
            box.transform.localScale = localScale;
            GetOrAdd<BoxCollider>(box);
            return box;
        }

        private static GameObject FindRoot(Scene scene, string name)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == name)
                {
                    return root;
                }
            }

            return null;
        }

        private static GameObject FindOrCreate(Transform parent, string name)
        {
            var existing = parent.Find(name);
            if (existing != null)
            {
                return existing.gameObject;
            }

            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent, false);
            return gameObject;
        }

        private static GameObject FindOrCreatePrimitive(Transform parent, string name, PrimitiveType type)
        {
            var existing = parent.Find(name);
            if (existing != null)
            {
                return existing.gameObject;
            }

            var gameObject = GameObject.CreatePrimitive(type);
            gameObject.name = name;
            gameObject.transform.SetParent(parent, false);
            return gameObject;
        }

        private static T GetOrAdd<T>(GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            return component != null ? component : gameObject.AddComponent<T>();
        }
    }
}
#endif
