using HarmonyLib;

namespace Better_Loving.Mian.Patches;

public class SubspeciesPatch
{
    [HarmonyPatch(typeof(Subspecies), nameof(Subspecies.isPartnerSuitableForReproduction))]
    class SuitableReproductionPatch
    {
        static bool Prefix(Actor pActor, Actor pTarget, Subspecies __instance, ref bool __result)
        {
            if (!pActor.hasSubspecies() || !pTarget.hasSubspecies())
            {
                __result = false;
                return false;
            }

            if (Util.CanDoAnySexType(pActor))
            {
                __result = true;
                return false;
            }
            
            if (__instance.needOppositeSexTypeForReproduction())
            {
                if ((pActor.data.sex != pTarget.data.sex && pTarget.subspecies.isReproductionSexual()) || Util.CanDoAnySexType(pTarget))
                {
                    __result = true;
                    return false;
                }
            } else if (Util.NeedSameSexTypeForReproduction(pActor))
            {
                if ((pActor.data.sex == pTarget.data.sex && Util.NeedSameSexTypeForReproduction(pTarget)) || Util.CanDoAnySexType(pTarget))
                {
                    __result = true;
                    return false;
                }
            }

            __result = false;
            return false;
        }
    }
}