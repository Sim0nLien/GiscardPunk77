using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace GiscardPunk77.Core.Tests
{
    public sealed class AssemblyBoundaryTests
    {
        [Test]
        public void RuntimeAssemblyReferences_AreUnidirectional()
        {
            AssertReferences(
                "Scripts/Core/GiscardPunk77.Core.asmdef",
                Array.Empty<string>());
            AssertReferences(
                "Scripts/Gameplay/GiscardPunk77.Gameplay.asmdef",
                new[] { "GiscardPunk77.Core" });
            AssertReferences(
                "Scripts/AI/GiscardPunk77.AI.asmdef",
                new[] { "GiscardPunk77.Core", "GiscardPunk77.Gameplay", "Unity.Behavior" });
        }

        [Test]
        public void PlayerController_ImplementsVisibilityTargetContract()
        {
            Type playerControllerType = AppDomain.CurrentDomain.GetAssemblies()
                .Single(assembly => assembly.GetName().Name == "Assembly-CSharp")
                .GetType("PlayerController", true);

            Assert.That(typeof(IVisibilityTarget).IsAssignableFrom(playerControllerType), Is.True);
        }

        private static void AssertReferences(
            string relativePath,
            string[] expectedReferences)
        {
            string absolutePath = Path.Combine(
                Application.dataPath,
                "_Project",
                relativePath.Replace('/', Path.DirectorySeparatorChar));
            AssemblyDefinitionData definition = JsonUtility.FromJson<AssemblyDefinitionData>(
                File.ReadAllText(absolutePath));

            Assert.That(definition.name, Does.StartWith("GiscardPunk77."));
            Assert.That(
                definition.references.OrderBy(name => name).ToArray(),
                Is.EqualTo(expectedReferences.OrderBy(name => name).ToArray()));
        }

        [Serializable]
        private sealed class AssemblyDefinitionData
        {
            public string name = string.Empty;
            public string[] references = Array.Empty<string>();
        }
    }
}
