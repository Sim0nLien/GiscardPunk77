#if UNITY_EDITOR
using GiscardPunk77.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GiscardPunk77.EditorTools
{
    internal static class DamageableTestTargetAuthoring
    {
        private const string MenuPath = "Tools/GiscardPunk77/Create or Update Damageable Test Target";
        private const string SandboxScenePath = "Assets/_Project/Scenes/Tests/NpcSandbox.unity";
        private const string TargetName = "Damageable Test Target";

        [MenuItem(MenuPath)]
        private static void CreateOrUpdateTarget()
        {
            var sandboxScene = SceneManager.GetActiveScene();
            if (sandboxScene.path != SandboxScenePath)
            {
                Debug.LogError($"Open {SandboxScenePath} before creating the damageable test target.");
                return;
            }

            var target = FindTarget(sandboxScene);
            if (target == null)
            {
                target = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                target.name = TargetName;
                target.transform.position = Vector3.up;
                Undo.RegisterCreatedObjectUndo(target, "Create Damageable Test Target");
            }

            GetOrAdd<CapsuleCollider>(target);
            var health = GetOrAdd<Health>(target);
            var hitbox = GetOrAdd<DamageableHitbox>(target);

            Undo.RecordObject(hitbox, "Configure Damageable Test Target");
            hitbox.AssignRootHealth(health);

            Selection.activeGameObject = target;
            EditorSceneManager.MarkSceneDirty(sandboxScene);
        }

        [MenuItem(MenuPath, true)]
        private static bool CanCreateOrUpdateTarget()
        {
            return SceneManager.GetActiveScene().path == SandboxScenePath;
        }

        private static GameObject FindTarget(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == TargetName)
                {
                    return root;
                }
            }

            return null;
        }

        private static T GetOrAdd<T>(GameObject target) where T : Component
        {
            var component = target.GetComponent<T>();
            return component != null ? component : Undo.AddComponent<T>(target);
        }
    }
}
#endif
