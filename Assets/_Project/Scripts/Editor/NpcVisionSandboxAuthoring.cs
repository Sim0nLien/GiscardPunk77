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
    /// <summary>Creates only the isolated P07 root inside the existing NPC sandbox.</summary>
    public static class NpcVisionSandboxAuthoring
    {
        private const string MenuPath = "Tools/GiscardPunk77/P07/Create or Update Vision Sandbox";
        private const string ScenePath = "Assets/_Project/Scenes/Tests/NpcSandbox.unity";
        private const string RootName = "P07 Vision Test Rig";

        [MenuItem(MenuPath)]
        private static void CreateOrUpdateFromMenu()
        {
            CreateOrUpdateInIsolation();
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            EditorGUIUtility.PingObject(sceneAsset);
        }

        public static void BuildFromCommandLine()
        {
            CreateOrUpdateInIsolation();
        }

        private static void CreateOrUpdateInIsolation()
        {
            if (!File.Exists(ScenePath))
            {
                throw new InvalidOperationException(
                    $"{ScenePath} does not exist. Create the P04 navigation sandbox first.");
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

            root.transform.SetPositionAndRotation(new Vector3(8f, 0f, 30f), Quaternion.identity);
            root.transform.localScale = Vector3.one;
            ConfigureRig(root.transform);

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
                $"P07 vision sandbox created or updated at {ScenePath}. " +
                "Select P07 Observer, enable Gizmos, then enter Play Mode.");
        }

        private static void ConfigureRig(Transform root)
        {
            var geometry = FindOrCreate(root, "Geometry").transform;
            ConfigureBox(geometry, "P07 Floor", new Vector3(0f, -0.1f, 3f), new Vector3(6f, 0.2f, 18f));
            ConfigureBox(geometry, "P07 Corridor Left", new Vector3(-3.1f, 1.25f, 3f), new Vector3(0.2f, 2.5f, 18f));
            ConfigureBox(geometry, "P07 Corridor Right", new Vector3(3.1f, 1.25f, 3f), new Vector3(0.2f, 2.5f, 18f));
            ConfigureBox(
                geometry,
                "P07 Occlusion Wall - Move X To 0 For Wall Test",
                new Vector3(2f, 1.25f, 3f),
                new Vector3(2.5f, 2.5f, 0.25f));

            var observer = FindOrCreate(root, "P07 Observer");
            observer.transform.localPosition = Vector3.zero;
            observer.transform.localRotation = Quaternion.identity;
            observer.transform.localScale = Vector3.one;
            var eye = ConfigureMarker(observer.transform, "P07 Eye", new Vector3(0f, 1.6f, 0f));

            var target = FindOrCreatePrimitive(root, "P07 Visibility Target", PrimitiveType.Capsule);
            target.transform.localPosition = new Vector3(0f, 1f, 8f);
            target.transform.localRotation = Quaternion.identity;
            target.transform.localScale = Vector3.one;
            var targetPoint = ConfigureMarker(target.transform, "P07 Visibility Point", new Vector3(0f, 0.6f, 0f));
            var targetAdapter = GetOrAdd<NpcVisionSandboxTarget>(target);
            targetAdapter.Configure(targetPoint, false);

            var sensor = GetOrAdd<NpcVisionSensor>(observer);
            sensor.Configure(
                eye,
                targetAdapter,
                ~0,
                12f,
                100f,
                0.6f,
                0.65f,
                1.75f,
                8f);

            var positions = FindOrCreate(root, "P07 Test Positions").transform;
            ConfigureMarker(positions, "Face - Standing Start", new Vector3(0f, 1f, 8f));
            ConfigureMarker(positions, "Near Crouching Comparison", new Vector3(0f, 1f, 5f));
            ConfigureMarker(positions, "Behind Observer", new Vector3(0f, 1f, -4f));
            ConfigureMarker(positions, "Wall Test - Set Wall X To 0", new Vector3(0f, 1f, 8f));
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
