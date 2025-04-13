using HarmonyLib;

namespace Better_Loving.Mian.Patches;

public class CultureManagerPatch
{
    [HarmonyPatch(typeof(CultureManager), nameof(CultureManager.newCulture))]
    class OnCultureCreatePatch
    {
        static void Postfix(Actor pActor, ref Culture __result)
        {
            // scar_of_incest prevents us from modifying the incest trait
            if (Util.IsDyingOut(pActor) && !__result.hasTrait("scar_of_incest"))
            {
                __result.addTrait("incest");
            }

            if (Randy.randomChance(0.2f))
            {
                if (Util.NeedDifferentSexTypeForReproduction(pActor))
                    __result.addTrait("homophobic");
                if (Util.NeedSameSexTypeForReproduction(pActor))
                    __result.addTrait("heterophobic");
                if (!Util.NeedDifferentSexTypeForReproduction(pActor) && !Util.NeedSameSexTypeForReproduction(pActor) && !Util.CanDoAnySexType(pActor))
                    __result.addTrait("orientationless");
            } else if (Randy.randomChance(0.2f))
            {
                var preference = QueerTraits.GetPreferenceFromActor(pActor, false);
                if(preference == Preference.DifferentSex)
                    __result.addTrait("homophobic");
                if(preference == Preference.SameSex)
                    __result.addTrait("heterophobic");
            }
        }
    }
}