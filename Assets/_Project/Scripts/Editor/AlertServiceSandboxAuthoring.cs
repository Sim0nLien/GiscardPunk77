#if UNITY_EDITOR
using System;
using System.IO;
using GiscardPunk77.AI.Coordination;
using GiscardPunk77.AI.Perception;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GiscardPunk77.EditorTools
{
    /// <summary>Creates only a P09 root and explicitly connects it to the existing P08 observer.</summary>
    public static class AlertServiceSandboxAuthoring
    {
        private const string MenuPath = "Tools/GiscardPunk77/P09/Create or Update Alert Service Sandbox";
        private const string ScenePath = "Assets/_Project/Scenes/Tests/NpcSandbox.unity";
        private const string AwarenessRootName = "P08 Awareness Test Rig";
        private const string RootName = "P09 Alert Service Test Rig";

        [MenuItem(MenuPath)]
        private static void CreateOrUpdateFromMenu()
        {
            if (!File.Exists(ScenePath))
            {
                throw new InvalidOperationException($"{ScenePath} does not exist. Create the P04 sandbox first.");
            }

            var scene = SceneManager.GetSceneByPath(ScenePath);
            var closeAfterSave = !scene.IsValid() || !scene.isLoaded;
            if (closeAfterSave)
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            }

            var awareness = FindAwareness(scene);
            if (awareness == null)
            {
                throw new InvalidOperationException(
                    "P08 observer is missing. Run Tools > GiscardPunk77 > P08 > Create or Update Awareness Sandbox first.");
            }

            var root = FindRoot(scene, RootName);
            if (root == null)
            {
                root = new GameObject(RootName);
                SceneManager.MoveGameObjectToScene(root, scene);
            }

            root.transform.SetPositionAndRotation(new Vector3(26f, 0f, 30f), Quaternion.identity);
            root.transform.localScale = Vector3.one;
            var service = GetOrAdd<AlertService>(root);
            var reporter = GetOrAdd<NpcAlertReporter>(root);
            reporter.Configure(awareness, service);

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new InvalidOperationException($"Unity could not save {ScenePath}.");
            }

            if (closeAfterSave)
            {
                EditorSceneManager.CloseScene(scene, true);
            }

            Debug.Log(
                "P09 alert service sandbox created or updated. Select P09 Alert Service Test Rig in Play Mode: " +
                "the AlertService changes to Alerted once P08 Observer sees its target.");
        }

        private static NpcAwareness FindAwareness(Scene scene)
        {
            var awarenessRoot = FindRoot(scene, AwarenessRootName);
            if (awarenessRoot == null)
            {
                return null;
            }

            var observer = awarenessRoot.transform.Find("P08 Observer");
            return observer == null ? null : observer.GetComponent<NpcAwareness>();
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

        private static T GetOrAdd<T>(GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            return component != null ? component : gameObject.AddComponent<T>();
        }
    }
}
#endif
