using HarmonyLib;

namespace Better_Loving.Mian.Patches;

public class MapBoxPatch
{
    [HarmonyPatch(typeof(MapBox), nameof(MapBox.updateObjectAge))]
    class OnAgePatch
    {
        // a year has passed!
        static void Postfix(MapBox __instance)
        {
            foreach (var culture in __instance.cultures.list)
            {
                if (culture.hasTrait("incest") && !culture.hasTrait("scar_of_incest") 
                                               && culture.getAge() > 30
                                               && culture.countFamilies() >= 10 && culture.countUnits() >= 60
                                               && Randy.randomChance(0.1f))
                {
                    culture.removeTrait("incest");
                }
            }
        }
    }
}