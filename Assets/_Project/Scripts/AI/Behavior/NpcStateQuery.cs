using Unity.Behavior;

namespace GiscardPunk77.AI.Behavior
{
    /// <summary>Small set of component states that a Behavior graph may branch or abort on.</summary>
    [BlackboardEnum]
    public enum NpcStateQuery
    {
        Alive,
        Dead,
        AwarenessUnaware,
        AwarenessSuspicious,
        AwarenessAlerted,
        GlobalCalm,
        GlobalAlerted,
        MotorIdle,
        MotorMoving,
        MotorWaiting,
        MotorArrived,
        MotorFailed,
        DeadOrGloballyAlerted
    }
}
