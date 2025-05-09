﻿using ai.behaviours;
using Topic_of_Love.Mian.CustomAssets.Traits;

namespace Topic_of_Love.Mian.CustomAssets.AI.CustomBehaviors.romance;

public class BehCheckIfDateFinish : BehaviourActionActor
{
    public override BehResult execute(Actor pActor)
    {
        Util.Debug("Checking for date before finalizing: "+ pActor.getName());
        if (pActor.beh_actor_target == null)
        {
            Util.Debug(pActor.getName()+": Cancelled from finalizing because actor was null");
            return BehResult.Stop;
        }
        var target = pActor.beh_actor_target.a;

        pActor.data.get("date_happiness", out var happiness, 0f);
        if (QueerTraits.BothActorsPreferencesMatch(pActor, target, false))
        {
            happiness += Randy.randomFloat(5, 10f);
        }
        else
        {
            happiness += Randy.randomFloat(4, 7f);
        }
        pActor.data.set("date_happiness", happiness);

        if (happiness <= 100f)
        {
            if (happiness <= 15f || Randy.randomChance(1f - happiness / 100))
            {
                pActor.makeWait(3f);
                EffectsLibrary.spawnAt("fx_hearts", pActor.current_position, pActor.current_scale.y);
                EffectsLibrary.spawnAt("fx_hearts", pActor.beh_actor_target.current_position, pActor.beh_actor_target.current_scale.y);
                return BehResult.RestartTask;
            }
        }

        target.cancelAllBeh();

        pActor.addStatusEffect("went_on_date");
        target.addStatusEffect("went_on_date");
        
        Util.ChangeRelationshipHappinessBy(pActor, happiness);
        Util.ChangeRelationshipHappinessBy(target, happiness);
        
        pActor.data.removeFloat("date_happiness");
        
        Util.Debug("The date for "+pActor.getName()+" and "+target.getName() + " has finalized! Total happiness: "+happiness);
        return BehResult.Continue;
    }
}