using HarmonyLib;

namespace Topic_of_Love.Mian.Patches;

public class BaseSimObjectPatch
{
    [HarmonyPatch(typeof(BaseSimObject), nameof(BaseSimObject.canAttackTarget))]
    class FindEnemiesPatch
    {
        static void Postfix(BaseSimObject pTarget, ref bool __result, BaseSimObject __instance)
        {
            if (__instance.isActor() && __instance.a.lover == pTarget)
            {
                __result = false;
            }
        }
    }
}