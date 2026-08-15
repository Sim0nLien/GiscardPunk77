#if UNITY_EDITOR
using System;
using System.IO;
using GiscardPunk77.AI.Debugging;
using GiscardPunk77.AI.Perception;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GiscardPunk77.EditorTools
{
    /// <summary>Creates only the isolated P08 awareness root and uses the shared config asset.</summary>
    public static class NpcAwarenessSandboxAuthoring
    {
        private const string MenuPath = "Tools/GiscardPunk77/P08/Create or Update Awareness Sandbox";
        private const string ScenePath = "Assets/_Project/Scenes/Tests/NpcSandbox.unity";
        private const string ConfigPath = "Assets/_Project/Config/AI/NpcAwarenessConfig.asset";
        private const string RootName = "P08 Awareness Test Rig";

        [MenuItem(MenuPath)]
        private static void CreateOrUpdateFromMenu()
        {
            CreateOrUpdateInIsolation();
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            EditorGUIUtility.PingObject(sceneAsset);
        }

        private static void CreateOrUpdateInIsolation()
        {
            if (!File.Exists(ScenePath))
            {
                throw new InvalidOperationException($"{ScenePath} does not exist. Create the P04 sandbox first.");
            }

            var config = AssetDatabase.LoadAssetAtPath<NpcAwarenessConfig>(ConfigPath);
            if (config == null)
            {
                throw new InvalidOperationException($"Shared awareness config is missing at {ConfigPath}.");
            }

            var scene = SceneManager.GetSceneByPath(ScenePath);
            var closeAfterSave = !scene.IsValid() || !scene.isLoaded;
            if (closeAfterSave)
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            }

            var root = FindRoot(scene, RootName);
            if (root == null)
            {
                root = new GameObject(RootName);
                SceneManager.MoveGameObjectToScene(root, scene);
            }

            root.transform.SetPositionAndRotation(new Vector3(18f, 0f, 30f), Quaternion.identity);
            root.transform.localScale = Vector3.one;
            ConfigureRig(root.transform, config);

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
                "P08 awareness sandbox created or updated. Enter Play Mode, then move the target behind " +
                "the observer or set the wall X to 0 to observe memory and decay.");
        }

        private static void ConfigureRig(Transform root, NpcAwarenessConfig config)
        {
            var geometry = FindOrCreate(root, "Geometry").transform;
            ConfigureBox(geometry, "P08 Floor", new Vector3(0f, -0.1f, 4f), new Vector3(6f, 0.2f, 20f));
            ConfigureBox(geometry, "P08 Corridor Left", new Vector3(-3.1f, 1.25f, 4f), new Vector3(0.2f, 2.5f, 20f));
            ConfigureBox(geometry, "P08 Corridor Right", new Vector3(3.1f, 1.25f, 4f), new Vector3(0.2f, 2.5f, 20f));
            ConfigureBox(
                geometry,
                "P08 Occlusion Wall - Move X To 0 To Lose Sight",
                new Vector3(2f, 1.25f, 3f),
                new Vector3(2.5f, 2.5f, 0.25f));

            var target = FindOrCreatePrimitive(root, "P08 Visibility Target", PrimitiveType.Capsule);
            target.transform.localPosition = new Vector3(0f, 1f, 7f);
            target.transform.localRotation = Quaternion.identity;
            target.transform.localScale = Vector3.one;
            var targetPoint = ConfigureMarker(target.transform, "P08 Visibility Point", new Vector3(0f, 0.6f, 0f));
            var targetAdapter = GetOrAdd<NpcVisionSandboxTarget>(target);
            targetAdapter.Configure(targetPoint, false);

            var observer = FindOrCreate(root, "P08 Observer");
            observer.transform.localPosition = Vector3.zero;
            observer.transform.localRotation = Quaternion.identity;
            observer.transform.localScale = Vector3.one;
            var eye = ConfigureMarker(observer.transform, "P08 Eye", new Vector3(0f, 1.6f, 0f));
            var sensor = GetOrAdd<NpcVisionSensor>(observer);
            sensor.Configure(eye, targetAdapter, ~0, 12f, 100f, 0.6f, 0.65f, 1.75f, 8f);
            var awareness = GetOrAdd<NpcAwareness>(observer);
            awareness.Configure(config, sensor);

            var camera = ConfigurePresentationCamera(root);
            ConfigureIndicator(observer.transform, awareness, config, camera);

            var positions = FindOrCreate(root, "P08 Test Positions").transform;
            ConfigureMarker(positions, "P08 Visible Position", new Vector3(0f, 1f, 7f));
            ConfigureMarker(positions, "P08 Lost Sight Position", new Vector3(0f, 1f, -4f));
        }

        private static Camera ConfigurePresentationCamera(Transform root)
        {
            var cameraObject = FindOrCreate(root, "P08 Presentation Camera");
            cameraObject.transform.localPosition = new Vector3(0f, 3.5f, -10f);
            cameraObject.transform.LookAt(root.TransformPoint(new Vector3(0f, 1.2f, 4f)));
            var camera = GetOrAdd<Camera>(cameraObject);
            camera.fieldOfView = 55f;
            camera.nearClipPlane = 0.1f;
            camera.enabled = true;

            var light = GetOrAdd<Light>(cameraObject);
            light.type = LightType.Directional;
            light.intensity = 0.7f;
            return camera;
        }

        private static void ConfigureIndicator(
            Transform observer,
            NpcAwareness awareness,
            NpcAwarenessConfig config,
            Camera presentationCamera)
        {
            var controller = FindOrCreate(observer, "P08 Awareness Indicator Controller");
            controller.transform.localPosition = Vector3.zero;
            controller.transform.localRotation = Quaternion.identity;
            controller.transform.localScale = Vector3.one;

            var visualRoot = FindOrCreate(controller.transform, "P08 Awareness Indicator Visual");
            visualRoot.transform.localPosition = Vector3.up * config.IndicatorHeight;
            visualRoot.transform.localRotation = Quaternion.identity;
            visualRoot.transform.localScale = Vector3.one * config.IndicatorScale;

            var suspicious = FindOrCreatePrimitive(visualRoot.transform, "P08 Suspicion Signal", PrimitiveType.Sphere);
            suspicious.transform.localPosition = Vector3.zero;
            suspicious.transform.localScale = Vector3.one * 0.4f;
            DisableCollider(suspicious);

            var alerted = FindOrCreate(visualRoot.transform, "P08 Alert Exclamation");
            alerted.transform.localPosition = Vector3.zero;
            alerted.transform.localRotation = Quaternion.identity;
            alerted.transform.localScale = Vector3.one;
            var text = GetOrAdd<TextMesh>(alerted);
            text.text = "!";
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.characterSize = 0.7f;
            text.fontSize = 96;
            text.color = Color.red;

            var indicator = GetOrAdd<NpcAwarenessIndicator>(controller);
            indicator.Configure(awareness, config, presentationCamera, visualRoot, suspicious, alerted);
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

        private static void DisableCollider(GameObject gameObject)
        {
            var collider = gameObject.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }
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
