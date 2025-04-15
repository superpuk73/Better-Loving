using System.Collections.Generic;
using Topic_of_Love.Mian.CustomManagers;
using Topic_of_Love.Mian.CustomManagers.Dateable;
using HarmonyLib;
using NeoModLoader.services;

namespace Topic_of_Love.Mian.Patches;

public class SavedMapPatch
{
    [HarmonyPatch(typeof(SavedMap), nameof(SavedMap.check))]
    class CheckPatch
    {
        static void Prefix()
        {
            if (CustomSavedData.Dateables == null)
            {
                CustomSavedData.Dateables = new List<DateableData>();
            }
        }
    }

    // [HarmonyPatch(typeof(SavedMap), nameof(SavedMap.init))]
    // class InitPatch
    // {
    //     static void Postfix()
    //     {
    //         Util.LogWithId("Saving TOL data..");
    //
    //         CustomSavedData.Dateables = DateableManager.Manager.save();
    //     }
    // }
}