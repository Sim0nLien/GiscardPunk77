#if UNITY_EDITOR
using System.IO;
using GiscardPunk77.Gameplay;
using GiscardPunk77.Gameplay.Weapons;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GiscardPunk77.EditorTools
{
    internal static class HitscanSandboxAuthoring
    {
        private const string MenuPath = "Tools/GiscardPunk77/P03/Create or Update Hitscan Sandbox";
        private const string ScenePath = "Assets/_Project/Scenes/Tests/NpcSandbox.unity";
        private const string RigName = "P03 Hitscan Test Rig";

        [MenuItem(MenuPath)]
        private static void CreateOrUpdateSandbox()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            EnsureSceneDirectoryExists();
            var scene = File.Exists(ScenePath)
                ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single)
                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var rig = FindRoot(scene, RigName);
            if (rig == null)
            {
                rig = new GameObject(RigName);
                Undo.RegisterCreatedObjectUndo(rig, "Create P03 Hitscan Test Rig");
            }

            ConfigureFloor(rig.transform);
            ConfigureLight(rig.transform);
            ConfigurePlayer(rig.transform);
            ConfigureStandardTarget(rig.transform, "Target - Before Wall", new Vector3(-4f, 1f, 8f));
            ConfigureBox(rig.transform, "Wall - Behind Target", new Vector3(-4f, 1.5f, 12f), new Vector3(3f, 3f, 0.5f));
            ConfigureBox(rig.transform, "Wall - Blocks Target", new Vector3(0f, 1.5f, 7f), new Vector3(3f, 3f, 0.5f));
            ConfigureStandardTarget(rig.transform, "Target - Behind Wall", new Vector3(0f, 1f, 10f));
            ConfigureBox(rig.transform, "Door Obstacle - Disable To Open", new Vector3(4f, 1.5f, 7f), new Vector3(2.5f, 3f, 0.35f));
            ConfigureStandardTarget(rig.transform, "Target - Behind Door", new Vector3(4f, 1f, 10f));
            ConfigureHitboxTarget(rig.transform, new Vector3(8f, 1f, 8f));

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            Selection.activeGameObject = rig;
            Debug.Log($"P03 sandbox created or updated: {ScenePath}. Press Play, left-click to fire and R to reload.", rig);
        }

        private static void EnsureSceneDirectoryExists()
        {
            var directory = Path.GetDirectoryName(ScenePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private static void ConfigureFloor(Transform parent)
        {
            ConfigureBox(parent, "Floor", new Vector3(2f, -0.25f, 10f), new Vector3(24f, 0.5f, 24f));
        }

        private static void ConfigureLight(Transform parent)
        {
            var lightObject = FindOrCreate(parent, "Directional Light");
            lightObject.transform.SetPositionAndRotation(new Vector3(0f, 8f, 0f), Quaternion.Euler(50f, -30f, 0f));
            var light = GetOrAdd<Light>(lightObject);
            light.type = LightType.Directional;
            light.intensity = 1.2f;
        }

        private static void ConfigurePlayer(Transform parent)
        {
            var player = FindOrCreate(parent, "P03 Test Player");
            player.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            GetOrAdd<CharacterController>(player);

            var cameraObject = FindOrCreate(player.transform, "Player Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.localPosition = Vector3.up * 0.75f;
            cameraObject.transform.localRotation = Quaternion.identity;
            var camera = GetOrAdd<Camera>(cameraObject);
            var cameraController = GetOrAdd<FpsCameraController>(cameraObject);
            cameraController.SetPlayer(player.transform);

            var playerController = GetOrAdd<PlayerController>(player);
            var playerControllerData = new SerializedObject(playerController);
            playerControllerData.FindProperty("playerCamera").objectReferenceValue = camera;
            playerControllerData.ApplyModifiedPropertiesWithoutUndo();

            var weapon = GetOrAdd<PlayerHitscanWeapon>(player);
            weapon.ConfigureAim(camera, player.transform);
            GetOrAdd<PlayerHitscanWeaponInput>(player);
            GetOrAdd<HitscanSandboxDebugDisplay>(player);
        }

        private static void ConfigureStandardTarget(Transform parent, string name, Vector3 position)
        {
            var target = FindOrCreatePrimitive(parent, name, PrimitiveType.Capsule);
            target.transform.SetPositionAndRotation(position, Quaternion.identity);
            target.transform.localScale = Vector3.one;
            var health = GetOrAdd<Health>(target);
            var hitbox = GetOrAdd<DamageableHitbox>(target);
            hitbox.AssignRootHealth(health);
        }

        private static void ConfigureHitboxTarget(Transform parent, Vector3 position)
        {
            var root = FindOrCreate(parent, "Target - Root Health With Child Hitbox");
            root.transform.SetPositionAndRotation(position, Quaternion.identity);
            var health = GetOrAdd<Health>(root);

            var hitboxObject = FindOrCreatePrimitive(root.transform, "Child Damageable Hitbox", PrimitiveType.Capsule);
            hitboxObject.transform.localPosition = Vector3.zero;
            hitboxObject.transform.localRotation = Quaternion.identity;
            hitboxObject.transform.localScale = Vector3.one;
            var hitbox = GetOrAdd<DamageableHitbox>(hitboxObject);
            hitbox.AssignRootHealth(health);
        }

        private static void ConfigureBox(Transform parent, string name, Vector3 position, Vector3 scale)
        {
            var box = FindOrCreatePrimitive(parent, name, PrimitiveType.Cube);
            box.transform.SetPositionAndRotation(position, Quaternion.identity);
            box.transform.localScale = scale;
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
            var child = parent.Find(name);
            if (child != null)
            {
                return child.gameObject;
            }

            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent);
            Undo.RegisterCreatedObjectUndo(gameObject, $"Create {name}");
            return gameObject;
        }

        private static GameObject FindOrCreatePrimitive(Transform parent, string name, PrimitiveType type)
        {
            var child = parent.Find(name);
            if (child != null)
            {
                return child.gameObject;
            }

            var gameObject = GameObject.CreatePrimitive(type);
            gameObject.name = name;
            gameObject.transform.SetParent(parent);
            Undo.RegisterCreatedObjectUndo(gameObject, $"Create {name}");
            return gameObject;
        }

        private static T GetOrAdd<T>(GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            return component != null ? component : Undo.AddComponent<T>(gameObject);
        }
    }
}
#endif
