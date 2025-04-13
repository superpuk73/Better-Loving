using HarmonyLib;

namespace Better_Loving.Mian.Patches;

public class DecisionAssetPatch
{
    // stops people with mismatching sexual preferences from attempting sex in the vanilla game 
    [HarmonyPatch(typeof(DecisionAsset), nameof(DecisionAsset.isPossible))]
    class DecisionPatch
    {
        static bool Prefix(Actor pActor, DecisionAsset __instance, ref bool __result)
        {
            // this is for decision asset: sexual_reproduction_try, this basically cancels sex with their partner
            if (__instance.id.Equals("sexual_reproduction_try"))
            {
                var pParentA = pActor;
                var pParentB = pActor.lover;
                if (pActor.hasLover() && 
                    (!QueerTraits.PreferenceMatches(pParentA, pParentB, true)
                     || !QueerTraits.PreferenceMatches(pParentB, pParentA, true) 
                     || !Util.CanReproduce(pParentA, pParentB)
                    || !Util.WantsBaby(pParentA) || !Util.WantsBaby(pParentB)))
                {
                    __result = false;
                    return false;
                }
            }

            return true;
        }
    }
}