using ai.behaviours;

namespace Better_Loving.Mian.CustomAssets.AI.CustomBehaviors.romance;

public class BehRandomizeDateTile : BehaviourActionActor
{
    public override BehResult execute(Actor pActor)
    {
        Util.Debug("Checking for date before continuing to randomize tile: "+ pActor.getName());
        if (pActor.beh_actor_target == null)
        {
            Util.Debug(pActor.getName()+": Cancelled from continuing date because actor was null");
            return BehResult.Stop;
        }
        var follower = pActor.beh_actor_target.a;

        if (!follower.isTask("follow_action_date"))
        {
            Util.Debug(pActor.getName()+"'s date has ended!");
            return BehResult.Stop;
        }

        if (Randy.randomBool()) // find random tile
        {
            return new BehFindRandomTile().execute(pActor);
        }
        
        // find random building
        var region = pActor.current_tile.region;
        if (Randy.randomChance(0.65f) && region.tiles.Count > 0)
        {
            pActor.beh_tile_target = region.tiles.GetRandom();
            return BehResult.Continue;
        }
        Building building1 = null;
        foreach (var building2 in Finder.getBuildingsFromChunk(pActor.current_tile, 1, pRandom: true))
        {
            if (building2.asset.city_building && building2.current_tile.isSameIsland(pActor.current_tile) && building2.isCiv())
            {
                building1 = building2;
                break;
            }
        }
        if (building1 == null)
        {
            if (region.tiles.Count <= 0)
                return BehResult.Stop;
            pActor.beh_tile_target = region.tiles.GetRandom();
            return BehResult.Continue;
        }
        pActor.beh_tile_target = building1.current_tile.region.tiles.GetRandom().getTileAroundThisOnSameIsland(pActor.current_tile, true);
        if (pActor.beh_tile_target == null)
            pActor.beh_tile_target = building1.current_tile.region.tiles.GetRandom();
        return BehResult.Continue;        
    }
}