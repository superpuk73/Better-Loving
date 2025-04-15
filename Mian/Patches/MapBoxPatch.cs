using Topic_of_Love.Mian.CustomManagers;
using Topic_of_Love.Mian.CustomManagers.Dateable;
using HarmonyLib;
using NeoModLoader.services;

namespace Topic_of_Love.Mian.Patches;

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
    
    // [HarmonyPatch(typeof(MapBox), nameof(MapBox.addUnloadResources))]
    // class SaveDataPatch
    // {
    //     // save our data before addUnloadResources runs
    //     static void Prefix()
    //     {
    //         LogService.LogInfo("Loading TOL Data");
    //         if (CustomSavedData.Dateables != null)
    //             SmoothLoader.add(() => DateableManager.Manager.loadFromSave(CustomSavedData.Dateables), "Loading Dateables");
    //     }
    // }
}