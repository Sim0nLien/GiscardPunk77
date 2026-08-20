using System;
using UnityEngine;

namespace GiscardPunk77.AI.Behavior.Guard
{
    [Serializable]
    public struct GuardTransitionRecord
    {
        [SerializeField] private float time;
        [SerializeField] private GuardState from;
        [SerializeField] private GuardState to;
        [SerializeField] private string reason;

        public float Time => time;
        public GuardState From => from;
        public GuardState To => to;
        public string Reason => reason;

        public GuardTransitionRecord(float time, GuardState from, GuardState to, string reason)
        {
            this.time = time;
            this.from = from;
            this.to = to;
            this.reason = reason;
        }
    }
}
