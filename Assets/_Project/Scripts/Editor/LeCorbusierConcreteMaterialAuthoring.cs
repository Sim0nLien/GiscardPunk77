#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GiscardPunk77.EditorTools
{
    /// <summary>
    /// Creates the project-owned URP Lit concrete material without modifying any scene.
    /// It can be run again safely after replacing the source texture.
    /// </summary>
    internal static class LeCorbusierConcreteMaterialAuthoring
    {
        private const string MenuPath = "Tools/GiscardPunk77/Art/Create or Update Beton Brut Material";
        private const string TexturePath = "Assets/_Project/Art/Textures/LeCorbusier_BetonBrut_Albedo_v1.png";
        private const string MaterialPath = "Assets/_Project/Art/Materials/LeCorbusier_BetonBrut_URP_Lit.mat";

        [MenuItem(MenuPath)]
        private static void CreateOrUpdate()
        {
            if (!File.Exists(TexturePath))
            {
                Debug.LogError(
                    $"The Beton Brut material was not created because its texture is missing: {TexturePath}");
                return;
            }

            ConfigureTextureImporter();
            var albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
            if (albedo == null)
            {
                Debug.LogError($"Unity could not import the Beton Brut texture: {TexturePath}");
                return;
            }

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                Debug.LogError("The Beton Brut material requires the Universal Render Pipeline Lit shader.");
                return;
            }

            EnsureFolder("Assets/_Project/Art/Materials");
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                material = new Material(shader)
                {
                    name = "LeCorbusier_BetonBrut_URP_Lit"
                };
                AssetDatabase.CreateAsset(material, MaterialPath);
            }

            material.shader = shader;
            material.SetTexture("_BaseMap", albedo);
            material.SetColor("_BaseColor", new Color(0.92f, 0.90f, 0.84f, 1f));
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_Smoothness", 0.16f);
            material.SetFloat("_OcclusionStrength", 1f);
            material.SetTextureScale("_BaseMap", Vector2.one);

            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(MaterialPath, ImportAssetOptions.ForceUpdate);
            Selection.activeObject = material;
            EditorGUIUtility.PingObject(material);

            Debug.Log(
                "Beton Brut material is ready. Assign it to a building Mesh Renderer and adjust Base Map tiling per facade.",
                material);
        }

        private static void ConfigureTextureImporter()
        {
            var importer = AssetImporter.GetAtPath(TexturePath) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
            importer.alphaIsTransparency = false;
            importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Trilinear;
            importer.anisoLevel = 4;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
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
#endif
