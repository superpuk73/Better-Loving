using ai.behaviours;

namespace Topic_of_Love.Mian.CustomAssets.AI.CustomBehaviors.other;

public class BehMakeTargetWait : BehaviourActionActor
{
    private float _wait;

    public BehMakeTargetWait(float wait=10f)
    {
        _wait = wait;
    }
    
    public override BehResult execute(Actor pActor)
    {
        if (pActor.beh_actor_target == null)
            return BehResult.Stop;
        pActor.makeWait(_wait);
        return BehResult.Continue;
    }
}