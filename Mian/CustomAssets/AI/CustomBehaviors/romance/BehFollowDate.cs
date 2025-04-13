using ai;
using ai.behaviours;

namespace Better_Loving.Mian.CustomAssets.AI.CustomBehaviors.romance;

public class BehFollowDate : BehaviourActionActor
{
    public override BehResult execute(Actor pActor)
    {
        Util.Debug("Checking to see if we can follow date: "+ pActor.getName());
        if (pActor.beh_actor_target == null)
        {
            Util.Debug(pActor.getName()+": Cancelled from following date because actor was null");
            return BehResult.Stop;
        }

        var toFollow = pActor.beh_actor_target.a;

        if (!toFollow.isTask("action_date") || toFollow.beh_tile_target == null)
        {
            Util.Debug(pActor.getName()+"'s date has ended!");
            return BehResult.Stop;
        }
        
        if (pActor.beh_tile_target != toFollow.beh_tile_target)
        {
            pActor.beh_tile_target = toFollow.beh_tile_target;
            Util.Debug(pActor.getName()+"'s date has moved! Attempting to follow!");
            
            var closestTile = pActor.beh_tile_target.getTileAroundThisOnSameIsland(pActor.current_tile, true);
            if (pActor.goTo(closestTile, false, true) == ExecuteEvent.False)
            {
                Util.Debug("Can't follow!");
                return BehResult.Stop;
            }
            Util.Debug("Following!");
            return BehResult.StepBack;
        }

        Util.Debug(pActor.getName()+"'s date hasn't moved yet, moving back a step!");
        return BehResult.StepBack;
    }
}