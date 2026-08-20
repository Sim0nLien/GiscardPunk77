using Unity.Behavior;

namespace GiscardPunk77.AI.Behavior.Guard
{
    [BlackboardEnum]
    public enum GuardState
    {
        Idle,
        Patrol,
        Suspicious,
        InvestigateLastKnownPosition,
        GlobalAlerted
    }
}
