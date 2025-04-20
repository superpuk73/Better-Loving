using ai.behaviours;

namespace Topic_of_Love.Mian.CustomAssets.AI.CustomBehaviors;
public class BehHuntdownOrientation : BehaviourActionActor
{
    public override BehResult execute(Actor pActor)
    {
        var target = pActor.beh_actor_target?.a;
        if (target == null || Toolbox.DistTile(target.current_tile, pActor.current_tile) > 200.0)
            return BehResult.Stop;

        pActor.addAggro(target);
        pActor.startFightingWith(target);
        Util.Debug($"{target.name} is a target of {pActor.name}! inquisition is coming!");

        return BehResult.Stop;
    }
}