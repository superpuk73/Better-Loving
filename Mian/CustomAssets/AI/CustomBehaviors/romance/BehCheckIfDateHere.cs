using ai.behaviours;

namespace Better_Loving.Mian.CustomAssets.AI.CustomBehaviors.romance;

public class BehCheckIfDateHere : BehaviourActionActor
{
    public override BehResult execute(Actor pActor)
    {
        Util.Debug("Checking if date arrived: "+ pActor.getName());
        if (pActor.beh_actor_target == null)
        {
            Util.Debug(pActor.getName()+": Cancelled from checking because actor was null");
            return BehResult.Stop;
        }
        var follower = pActor.beh_actor_target.a;
        if (!follower.isTask("follow_action_date"))
        {
            Util.Debug(pActor.getName()+"'s date has ended!");
            return BehResult.Stop;
        }
        
        if (follower.is_moving)
        {
            return BehResult.StepBack;
        }

        return BehResult.Continue;
    }
}