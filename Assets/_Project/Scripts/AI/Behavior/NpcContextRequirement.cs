using System;

namespace GiscardPunk77.AI.Behavior
{
    [Flags]
    public enum NpcContextRequirement
    {
        None = 0,
        Identity = 1 << 0,
        Health = 1 << 1,
        Motor = 1 << 2,
        Vision = 1 << 3,
        Awareness = 1 << 4,
        AlertService = 1 << 5,
        All = Identity | Health | Motor | Vision | Awareness | AlertService
    }
}
