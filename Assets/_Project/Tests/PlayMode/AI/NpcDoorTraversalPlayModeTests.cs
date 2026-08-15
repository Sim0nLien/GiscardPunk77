using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using GiscardPunk77.AI.Navigation;
using GiscardPunk77.Core;
using GiscardPunk77.Gameplay;
using GiscardPunk77.Gameplay.Doors;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;

namespace GiscardPunk77.AI.PlayMode.Tests
{
    public sealed class NpcDoorTraversalPlayModeTests
    {
        private readonly List<GameObject> createdObjects = new List<GameObject>();
        private NavMeshDataInstance navMeshInstance;
        private TestDoorPassage door;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            var settings = NavMesh.GetSettingsByID(NpcSandboxTuning.HumanoidAgentTypeId);
            var source = new NavMeshBuildSource
            {
                shape = NavMeshBuildSourceShape.Box,
                size = new Vector3(12f, 0.1f, 20f),
                transform = Matrix4x4.TRS(new Vector3(0f, -0.05f, 0f), Quaternion.identity, Vector3.one),
                area = 0
            };
            var data = NavMeshBuilder.BuildNavMeshData(
                settings,
                new List<NavMeshBuildSource> { source },
                new Bounds(Vector3.zero, new Vector3(14f, 2f, 22f)),
                Vector3.zero,
                Quaternion.identity);
            navMeshInstance = NavMesh.AddNavMeshData(data);

            var doorObject = Track(new GameObject("P06 Test Door"));
            door = doorObject.AddComponent<TestDoorPassage>();
            door.Configure(new Vector3(0f, 0f, -1.5f), new Vector3(0f, 0f, 1.5f), true);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            for (var index = createdObjects.Count - 1; index >= 0; index--)
            {
                if (createdObjects[index] != null)
                {
                    UnityEngine.Object.Destroy(createdObjects[index]);
                }
            }

            createdObjects.Clear();
            navMeshInstance.Remove();
            yield return null;
        }

        [UnityTest]
        public IEnumerator TwoCapsulesFromOppositeSidesCompleteInOrder()
        {
            yield return RunOpposedScenario(2, 12f);
        }

        [UnityTest]
        public IEnumerator FourCapsulesFromOppositeSidesAllComplete()
        {
            yield return RunOpposedScenario(4, 20f);
        }

        [UnityTest]
        public IEnumerator DeathAlwaysReleasesTheReservation()
        {
            door.SetAutomaticPassability(false);
            var traversal = CreateTraversal(new Vector3(0f, 0f, -4f), out _, out var health);
            yield return null;

            Assert.That(traversal.BeginTraversal(), Is.True);
            Assert.That(door.IsReservedBy(traversal), Is.True);
            LogAssert.Expect(LogType.Warning, "NpcDoorTraversal failed: Died.");
            health.TryApplyDamage(CreateFatalDamage());

            Assert.That(door.QueueCount, Is.Zero);
            Assert.That(traversal.State, Is.EqualTo(NpcDoorTraversalState.Failed));
            Assert.That(traversal.FailureReason, Is.EqualTo(NpcDoorTraversalFailureReason.Died));
        }

        [UnityTest]
        public IEnumerator DisableAlwaysReleasesTheReservation()
        {
            door.SetAutomaticPassability(false);
            var traversal = CreateTraversal(new Vector3(0f, 0f, -4f), out _, out _);
            yield return null;

            traversal.BeginTraversal();
            Assert.That(door.IsReservedBy(traversal), Is.True);
            traversal.enabled = false;

            Assert.That(door.QueueCount, Is.Zero);
            Assert.That(traversal.FailureReason, Is.EqualTo(NpcDoorTraversalFailureReason.Disabled));
        }

        [UnityTest]
        public IEnumerator ResetAlwaysReleasesTheReservation()
        {
            door.SetAutomaticPassability(false);
            var traversal = CreateTraversal(new Vector3(0f, 0f, -4f), out _, out _);
            yield return null;

            traversal.BeginTraversal();
            Assert.That(door.IsReservedBy(traversal), Is.True);
            traversal.ResetTraversal();

            Assert.That(door.QueueCount, Is.Zero);
            Assert.That(traversal.State, Is.EqualTo(NpcDoorTraversalState.Idle));
        }

        [UnityTest]
        public IEnumerator TimeoutAlwaysReleasesTheReservation()
        {
            door.SetAutomaticPassability(false);
            var traversal = CreateTraversal(new Vector3(0f, 0f, -2f), out _, out _, 1f);
            yield return null;

            traversal.BeginTraversal();
            LogAssert.Expect(LogType.Warning, "NpcDoorTraversal failed: Timeout.");
            var deadline = Time.time + 2f;
            while (Time.time < deadline && traversal.State != NpcDoorTraversalState.Failed)
            {
                yield return null;
            }

            Assert.That(traversal.FailureReason, Is.EqualTo(NpcDoorTraversalFailureReason.Timeout));
            Assert.That(door.QueueCount, Is.Zero);
        }

        private IEnumerator RunOpposedScenario(int capsuleCount, float timeout)
        {
            var traversals = new NpcDoorTraversal[capsuleCount];
            var motors = new NpcMotor[capsuleCount];
            var completedOrder = new List<int>(capsuleCount);
            for (var index = 0; index < capsuleCount; index++)
            {
                var lateralOffset = (index - (capsuleCount - 1) * 0.5f) * 2.5f;
                var clearanceA = CreatePoint($"Clearance A {index + 1}", new Vector3(lateralOffset, 0f, -4f));
                var clearanceB = CreatePoint($"Clearance B {index + 1}", new Vector3(lateralOffset, 0f, 4f));
                var position = index % 2 == 0 ? clearanceA.position : clearanceB.position;
                traversals[index] = CreateTraversal(position, out motors[index], out _);
                traversals[index].ConfigureClearancePoints(clearanceA, clearanceB);

                var queuedIndex = index;
                traversals[index].StateChanged += (state, _) =>
                {
                    if (state == NpcDoorTraversalState.Completed)
                    {
                        completedOrder.Add(queuedIndex);
                    }
                };
            }

            yield return null;
            for (var index = 0; index < traversals.Length; index++)
            {
                Assert.That(traversals[index].BeginTraversal(), Is.True);
            }

            var deadline = Time.time + timeout;
            while (Time.time < deadline && !AllCompleted(traversals))
            {
                yield return null;
            }

            Assert.That(AllCompleted(traversals), Is.True, DescribeScenario(traversals, motors));
            Assert.That(door.QueueCount, Is.Zero);
            Assert.That(completedOrder, Is.EqualTo(CreateExpectedOrder(capsuleCount)));
        }

        private NpcDoorTraversal CreateTraversal(
            Vector3 position,
            out NpcMotor motor,
            out Health health,
            float timeout = 30f)
        {
            var agentObject = Track(new GameObject($"P06 Test Capsule {createdObjects.Count}"));
            agentObject.transform.position = position;
            agentObject.AddComponent<NavMeshAgent>();
            motor = agentObject.AddComponent<NpcMotor>();
            motor.ApplyInitialAgentSettings();
            health = agentObject.AddComponent<Health>();
            health.ResetHealth();
            var traversal = agentObject.AddComponent<NpcDoorTraversal>();
            traversal.Configure(motor, door, health, false, timeout);
            return traversal;
        }

        private Transform CreatePoint(string pointName, Vector3 position)
        {
            var point = Track(new GameObject(pointName)).transform;
            point.position = position;
            return point;
        }

        private GameObject Track(GameObject gameObject)
        {
            createdObjects.Add(gameObject);
            return gameObject;
        }

        private static bool AllCompleted(NpcDoorTraversal[] traversals)
        {
            for (var index = 0; index < traversals.Length; index++)
            {
                if (traversals[index].State != NpcDoorTraversalState.Completed)
                {
                    return false;
                }
            }

            return true;
        }

        private static int[] CreateExpectedOrder(int capsuleCount)
        {
            var expected = new int[capsuleCount];
            for (var index = 0; index < capsuleCount; index++)
            {
                expected[index] = index;
            }

            return expected;
        }

        private static string DescribeScenario(NpcDoorTraversal[] traversals, NpcMotor[] motors)
        {
            var description = new StringBuilder("Opposed traversal did not complete.");
            for (var index = 0; index < traversals.Length; index++)
            {
                description.Append(" Agent ")
                    .Append(index)
                    .Append(": traversal=")
                    .Append(traversals[index].State)
                    .Append('/')
                    .Append(traversals[index].FailureReason)
                    .Append(", motor=")
                    .Append(motors[index].Status)
                    .Append('/')
                    .Append(motors[index].FailureReason)
                    .Append(", position=")
                    .Append(traversals[index].transform.position)
                    .Append(", destination=")
                    .Append(motors[index].Destination)
                    .Append('.');
            }

            return description.ToString();
        }

        private static DamageInfo CreateFatalDamage()
        {
            return new DamageInfo(
                100f,
                Vector3.zero,
                Vector3.forward,
                new ActorIdentity(ActorKind.Player, TeamId.Neutral),
                DamageCategory.Hitscan);
        }
    }

    public sealed class TestDoorPassage : MonoBehaviour, IDoorPassage
    {
        private readonly DoorReservationQueue queue = new DoorReservationQueue();
        private bool automaticPassability;
        private bool openRequested;
        private bool isPassable;

        public bool CanUse => isActiveAndEnabled && WaitingPointA != null && WaitingPointB != null;

        public bool IsPassable => CanUse && isPassable;

        public Transform WaitingPointA { get; private set; }

        public Transform WaitingPointB { get; private set; }

        public int QueueCount => queue.Count;

        public event Action<DoorPassageState> StateChanged;

        public event Action<object> ReservationChanged;

        public void Configure(Vector3 pointA, Vector3 pointB, bool opensImmediately)
        {
            WaitingPointA = CreatePoint("Waiting A", pointA);
            WaitingPointB = CreatePoint("Waiting B", pointB);
            automaticPassability = opensImmediately;
            openRequested = false;
            isPassable = false;
            Publish();
        }

        public void SetAutomaticPassability(bool value)
        {
            automaticPassability = value;
            isPassable = value && openRequested;
            Publish();
        }

        public bool RequestOpen()
        {
            if (!CanUse)
            {
                return false;
            }

            openRequested = true;
            isPassable = automaticPassability;
            Publish();
            return true;
        }

        public bool TryReserve(object owner)
        {
            var previousOwner = queue.ActiveOwner;
            var granted = queue.TryReserve(owner, Time.time, 2f);
            PublishReservationIfChanged(previousOwner);
            return granted;
        }

        public bool IsReservedBy(object owner)
        {
            return queue.IsReservedBy(owner);
        }

        public void Release(object owner)
        {
            var previousOwner = queue.ActiveOwner;
            if (queue.Release(owner))
            {
                PublishReservationIfChanged(previousOwner);
            }
        }

        private void Update()
        {
            var previousOwner = queue.ActiveOwner;
            if (queue.RemoveExpired(Time.time) > 0)
            {
                PublishReservationIfChanged(previousOwner);
            }
        }

        private void OnDisable()
        {
            queue.Clear();
        }

        private Transform CreatePoint(string pointName, Vector3 position)
        {
            var point = new GameObject(pointName).transform;
            point.SetParent(transform, false);
            point.position = position;
            return point;
        }

        private void PublishReservationIfChanged(object previousOwner)
        {
            Publish();
            if (!ReferenceEquals(previousOwner, queue.ActiveOwner))
            {
                ReservationChanged?.Invoke(queue.ActiveOwner);
            }
        }

        private void Publish()
        {
            StateChanged?.Invoke(new DoorPassageState(
                CanUse,
                openRequested,
                IsPassable,
                queue.ActiveOwner,
                queue.Count));
        }
    }
}
