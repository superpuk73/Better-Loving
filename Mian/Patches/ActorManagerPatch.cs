using Topic_of_Love.Mian.CustomAssets;
using Topic_of_Love.Mian.CustomAssets.Traits;
using HarmonyLib;

namespace Topic_of_Love.Mian.Patches;

public class ActorManagerPatch
{
    [HarmonyPatch(typeof(ActorManager), nameof(ActorManager.finalizeActor))]
    class ActorFinalizePatch
    {
        static void Postfix(string pStats, Actor pActor)
        {
            if (pActor.isAdult())
            {
                if (!QueerTraits.HasQueerTraits(pActor)){
                    QueerTraits.GiveQueerTraits(pActor, false, true);
                    pActor.changeHappiness("true_self");
                }
            }
        }
    }
}