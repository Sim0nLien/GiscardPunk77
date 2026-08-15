using System.Collections;
using System.Collections.Generic;
using GiscardPunk77.AI.Navigation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;

namespace GiscardPunk77.AI.PlayMode.Tests
{
    public sealed class NpcMotorPlayModeTests
    {
        private GameObject agentObject;
        private NpcMotor motor;
        private NavMeshDataInstance navMeshInstance;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            var settings = NavMesh.GetSettingsByID(NpcSandboxTuning.HumanoidAgentTypeId);
            var source = new NavMeshBuildSource
            {
                shape = NavMeshBuildSourceShape.Box,
                size = new Vector3(20f, 0.1f, 20f),
                transform = Matrix4x4.TRS(new Vector3(0f, -0.05f, 0f), Quaternion.identity, Vector3.one),
                area = 0
            };
            var data = NavMeshBuilder.BuildNavMeshData(
                settings,
                new List<NavMeshBuildSource> { source },
                new Bounds(Vector3.zero, new Vector3(22f, 2f, 22f)),
                Vector3.zero,
                Quaternion.identity);
            navMeshInstance = NavMesh.AddNavMeshData(data);

            agentObject = new GameObject("NpcMotor PlayMode Test Agent");
            agentObject.transform.position = Vector3.zero;
            agentObject.AddComponent<NavMeshAgent>();
            motor = agentObject.AddComponent<NpcMotor>();
            yield return null;

            Assert.That(agentObject.GetComponent<NavMeshAgent>().isOnNavMesh, Is.True);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (agentObject != null)
            {
                Object.Destroy(agentObject);
            }

            navMeshInstance.Remove();
            yield return null;
        }

        [UnityTest]
        public IEnumerator ValidDestinationStartsAPath()
        {
            Assert.That(motor.TrySetDestination(new Vector3(3f, 0f, 0f)), Is.True);
            yield return null;

            Assert.That(motor.HasActiveDestination, Is.True);
            Assert.That(motor.Status, Is.Not.EqualTo(NpcMotorStatus.Failed));
        }

        [UnityTest]
        public IEnumerator InvalidDestinationIsObservable()
        {
            Assert.That(motor.TrySetDestination(new Vector3(100f, 0f, 100f)), Is.False);
            yield return null;

            Assert.That(motor.Status, Is.EqualTo(NpcMotorStatus.Failed));
            Assert.That(motor.FailureReason, Is.EqualTo(NpcMotorFailureReason.InvalidDestination));
        }

        [UnityTest]
        public IEnumerator CancelClearsTheActivePath()
        {
            Assert.That(motor.TrySetDestination(new Vector3(3f, 0f, 0f)), Is.True);
            motor.Cancel();
            yield return null;

            Assert.That(motor.Status, Is.EqualTo(NpcMotorStatus.Idle));
            Assert.That(motor.HasActiveDestination, Is.False);
        }

        [UnityTest]
        public IEnumerator DisableThenEnableAllowsANewDestination()
        {
            Assert.That(motor.TrySetDestination(new Vector3(3f, 0f, 0f)), Is.True);
            motor.enabled = false;
            yield return null;

            Assert.That(motor.Status, Is.EqualTo(NpcMotorStatus.Idle));
            motor.enabled = true;
            yield return null;

            Assert.That(motor.TrySetDestination(new Vector3(-3f, 0f, 0f)), Is.True);
            Assert.That(motor.Status, Is.Not.EqualTo(NpcMotorStatus.Failed));
        }
    }
}
