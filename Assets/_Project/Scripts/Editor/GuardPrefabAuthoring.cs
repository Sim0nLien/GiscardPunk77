using System.Linq;
using GiscardPunk77.AI.Behavior;
using GiscardPunk77.AI.Behavior.Guard;
using GiscardPunk77.AI.Coordination;
using GiscardPunk77.AI.Navigation;
using GiscardPunk77.AI.Perception;
using GiscardPunk77.Core;
using GiscardPunk77.Gameplay;
using Unity.Behavior;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace GiscardPunk77.Editor
{
    /// <summary>Creates the isolated P11 configuration and capsule prefab without touching a scene.</summary>
    public static class GuardPrefabAuthoring
    {
        public const string ConfigPath = "Assets/_Project/Config/AI/GuardConfig.asset";
        public const string PrefabPath = "Assets/_Project/Prefabs/AI/Guard Capsule.prefab";
        public const string MaterialPath = "Assets/_Project/Art/Debug/Guard Capsule Debug.mat";

        [MenuItem("Tools/GiscardPunk77/P11/Create or Open Guard Capsule Prefab")]
        public static void CreateOrOpenGuardCapsulePrefab()
        {
            NpcBehaviorGraphAuthoring.CreateOrOpenGuardGraph();

            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (existing != null)
            {
                Selection.activeObject = existing;
                AssetDatabase.OpenAsset(existing);
                Debug.Log("P11 Guard capsule prefab already exists; it was opened without being overwritten.", existing);
                return;
            }

            EnsureFolder("Assets/_Project/Prefabs/AI");
            EnsureFolder("Assets/_Project/Art/Debug");
            var guardConfig = GetOrCreateGuardConfig();
            var awarenessConfig = AssetDatabase.LoadAssetAtPath<NpcAwarenessConfig>(
                "Assets/_Project/Config/AI/NpcAwarenessConfig.asset");
            if (awarenessConfig == null)
            {
                Debug.LogError("P11 requires the shared P08 NpcAwarenessConfig asset.");
                return;
            }

            var runtimeGraph = AssetDatabase.LoadAllAssetsAtPath(NpcBehaviorGraphAuthoring.GuardGraphPath)
                .OfType<BehaviorGraph>()
                .FirstOrDefault();
            if (runtimeGraph == null)
            {
                Debug.LogError("P11 Guard runtime graph was not built; open the graph and resolve its first error.");
                return;
            }

            var root = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            root.name = "Guard Capsule";
            try
            {
                var renderer = root.GetComponent<Renderer>();
                renderer.sharedMaterial = GetOrCreateDebugMaterial(guardConfig.GetColor(GuardState.Idle));

                var eyeObject = new GameObject("Vision Eye");
                eyeObject.transform.SetParent(root.transform, false);
                eyeObject.transform.localPosition = new Vector3(0f, 0.65f, 0.35f);

                var identity = root.AddComponent<ActorIdentityComponent>();
                identity.Configure(ActorKind.Guard, new TeamId(2));
                var health = root.AddComponent<Health>();
                var navAgent = root.AddComponent<NavMeshAgent>();
                var motor = root.AddComponent<NpcMotor>();
                motor.ApplyInitialAgentSettings();

                var vision = root.AddComponent<NpcVisionSensor>();
                vision.Configure(eyeObject.transform, null, ~0, 12f, 100f, 0.6f, 0.65f, 1.75f, 8f);
                var awareness = root.AddComponent<NpcAwareness>();
                awareness.Configure(awarenessConfig, vision);
                root.AddComponent<NpcAlertReporter>();

                var npcContext = root.AddComponent<NpcContext>();
                npcContext.Configure(identity, health, motor, vision, awareness, null);
                var route = root.AddComponent<GuardPatrolRoute>();
                route.Configure(
                    new Vector3(0f, 0f, 3f),
                    new Vector3(3f, 0f, 3f),
                    new Vector3(3f, 0f, 0f));
                var guardContext = root.AddComponent<GuardContext>();
                guardContext.Configure(npcContext, guardConfig, route);
                var presenter = root.AddComponent<GuardStatePresenter>();
                presenter.Configure(guardContext, renderer);

                var behaviorAgent = root.AddComponent<BehaviorGraphAgent>();
                behaviorAgent.Graph = runtimeGraph;
                if (!behaviorAgent.SetVariableValue("Guard Context", guardContext))
                {
                    Debug.LogError("P11 graph does not expose the expected 'Guard Context' variable.");
                    return;
                }

                var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                Selection.activeObject = prefab;
                EditorGUIUtility.PingObject(prefab);
                AssetDatabase.SaveAssets();
                Debug.Log(
                    "P11 Guard capsule prefab created. Assign its scene AlertService and vision target after placement; " +
                    "the missing scene reference intentionally remains explicit.",
                    prefab);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        /// <summary>Batch-mode entry point used by repository validation.</summary>
        public static void GenerateP11Assets()
        {
            CreateOrOpenGuardCapsulePrefab();
        }

        private static GuardConfig GetOrCreateGuardConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<GuardConfig>(ConfigPath);
            if (config != null)
            {
                return config;
            }

            EnsureFolder("Assets/_Project/Config/AI");
            config = ScriptableObject.CreateInstance<GuardConfig>();
            config.name = "GuardConfig";
            AssetDatabase.CreateAsset(config, ConfigPath);
            return config;
        }

        private static Material GetOrCreateDebugMaterial(Color initialColor)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material != null)
            {
                return material;
            }

            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            material = new Material(shader) { name = "Guard Capsule Debug" };
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", initialColor);
            if (material.HasProperty("_Color")) material.SetColor("_Color", initialColor);
            AssetDatabase.CreateAsset(material, MaterialPath);
            return material;
        }

        private static void EnsureFolder(string folderPath)
        {
            var segments = folderPath.Split('/');
            var current = segments[0];
            for (var index = 1; index < segments.Length; index++)
            {
                var next = $"{current}/{segments[index]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, segments[index]);
                }

                current = next;
            }
        }
    }
}
