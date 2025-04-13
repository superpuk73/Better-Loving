using Better_Loving.Mian.CustomAssets;
using HarmonyLib;

namespace Better_Loving.Mian.Patches;

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