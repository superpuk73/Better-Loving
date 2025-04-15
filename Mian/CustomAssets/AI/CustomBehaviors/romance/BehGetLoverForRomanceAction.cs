using ai.behaviours;

namespace Topic_of_Love.Mian.CustomAssets.AI.CustomBehaviors.romance;
public class BehGetLoverForRomanceAction : BehaviourActionActor
{
    private float _distance;
    public BehGetLoverForRomanceAction(float distance=10f)
    {
        _distance = distance;
    }
    public override BehResult execute(Actor pActor)
    {
        Util.Debug(pActor.getName() + " is attempting to locate lover for romance!");
            
        if (!pActor.hasLover() || !pActor.isOnSameIsland(pActor.lover) || pActor.lover.isLying())
            return BehResult.Stop;
            
        if(pActor.distanceToActorTile(pActor.lover) > _distance)
            return BehResult.Stop;
            
        pActor.beh_actor_target = pActor.lover;
        pActor.lover.makeWait(_distance / 2);
        Util.Debug("Lover found!");

        return BehResult.Continue;
    }
}