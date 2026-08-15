using UnityEngine;

namespace GiscardPunk77.AI.Navigation
{
    /// <summary>Sandbox-only harness that repeats traversals until the H2 target is reached.</summary>
    [DisallowMultipleComponent]
    public sealed class NpcDoorTraversalStressProbe : MonoBehaviour
    {
        [SerializeField] private NpcDoorTraversal[] traversals;
        [SerializeField, Min(1)] private int targetTraversalCount = 20;

        private bool[] completionCounted;
        private bool started;

        public int CompletedTraversalCount { get; private set; }

        public int TargetTraversalCount => targetTraversalCount;

        public bool IsComplete => CompletedTraversalCount >= targetTraversalCount;

        public void Configure(NpcDoorTraversal[] configuredTraversals, int targetCount)
        {
            traversals = configuredTraversals;
            targetTraversalCount = Mathf.Max(1, targetCount);
            PrepareRun();
        }

        private void OnEnable()
        {
            PrepareRun();
        }

        private void Start()
        {
            BeginAll();
        }

        private void Update()
        {
            if (!started)
            {
                BeginAll();
            }

            if (IsComplete || traversals == null)
            {
                return;
            }

            for (var index = 0; index < traversals.Length; index++)
            {
                var traversal = traversals[index];
                if (traversal == null || traversal.State != NpcDoorTraversalState.Completed || completionCounted[index])
                {
                    continue;
                }

                completionCounted[index] = true;
                CompletedTraversalCount++;
                if (IsComplete)
                {
                    CancelUnfinishedTraversals();
                    Debug.Log($"P06 door stress completed {CompletedTraversalCount} traversals.", this);
                    return;
                }

                traversal.ResetTraversal();
                completionCounted[index] = false;
                traversal.BeginTraversal();
            }
        }

        private void OnDisable()
        {
            if (traversals == null)
            {
                return;
            }

            for (var index = 0; index < traversals.Length; index++)
            {
                traversals[index]?.ResetTraversal();
            }
        }

        private void PrepareRun()
        {
            CompletedTraversalCount = 0;
            started = false;
            completionCounted = traversals != null ? new bool[traversals.Length] : null;
        }

        private void BeginAll()
        {
            if (traversals == null || completionCounted == null)
            {
                return;
            }

            started = true;
            for (var index = 0; index < traversals.Length; index++)
            {
                var traversal = traversals[index];
                if (traversal == null)
                {
                    continue;
                }

                completionCounted[index] = false;
                traversal.ResetTraversal();
                traversal.BeginTraversal();
            }
        }

        private void CancelUnfinishedTraversals()
        {
            for (var index = 0; index < traversals.Length; index++)
            {
                var traversal = traversals[index];
                if (traversal != null && traversal.State != NpcDoorTraversalState.Completed)
                {
                    traversal.ResetTraversal();
                }
            }
        }
    }
}
