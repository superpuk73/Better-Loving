using ai.behaviours;
using HarmonyLib;
using NeoModLoader.services;

namespace Better_Loving.Mian.Patches;

public class BehCFBFSRPatch
{
    [HarmonyPatch(typeof(BehCheckForBabiesFromSexualReproduction),
        nameof(BehCheckForBabiesFromSexualReproduction.execute))]
    // code is running twice??
    class SexPatch
    {
        static bool Prefix(Actor pActor, ref BehResult __result, BehCheckForBabiesFromSexualReproduction __instance)
        {
            var target = pActor.beh_actor_target != null ? pActor.beh_actor_target.a : pActor.lover;
            if (target == null)
            { 
                Util.Debug(pActor.getName()+": Cant do sex because target is null");
                __result = BehResult.Stop;
                return false;
            }
            
            pActor.subspecies.counter_reproduction_acts?.registerEvent();
            if(target.subspecies != pActor.subspecies)
                target.subspecies.counter_reproduction_acts?.registerEvent();
            __instance.checkForBabies(pActor, target);
            Util.JustHadSex(pActor, target);
            __result = BehResult.Continue;
            return false;
        }
    }
    
    // this patch handles who the mother is when it comes to sexual reproduction
    [HarmonyPatch(typeof(BehCheckForBabiesFromSexualReproduction),
        nameof(BehCheckForBabiesFromSexualReproduction.checkForBabies))]
    class CheckForBabiesPatch
    {
        static bool Prefix(Actor pParentA, Actor pParentB)
        {
            Util.Debug($"\nAble to make a baby?\n{pParentA.getName()}: "+(Util.CanMakeBabies(pParentA)+$"\n${pParentB.getName()}: "+(Util.CanMakeBabies(pParentB))));

            if (!Util.CanMakeBabies(pParentA) || !Util.CanMakeBabies(pParentB))
                return false;

            // ensures that both subspecies HAVE not reached population limit
            if (pParentA.subspecies.hasReachedPopulationLimit() || pParentB.subspecies.hasReachedPopulationLimit())
                return false;

            // var subspeciesA = pParentA.subspecies;
            // var subspeciesB = pParentB.subspecies;

            Actor pregnantActor = null;
            Actor nonPregnantActor;
            
            if (Util.NeedDifferentSexTypeForReproduction(pParentA) && Util.NeedDifferentSexTypeForReproduction(pParentB))
            {
                if (pParentA.data.sex == pParentB.data.sex) return false;
                
                if (pParentA.isSexFemale())
                    pregnantActor = pParentA;
                else if (pParentB.isSexFemale())
                    pregnantActor = pParentB;
            }
            else if(Util.NeedSameSexTypeForReproduction(pParentA) && Util.NeedSameSexTypeForReproduction(pParentB))
            {
                if (pParentA.data.sex != pParentB.data.sex) return false;
                pregnantActor = !Randy.randomBool() ? pParentB : pParentA;
            } else if (Util.CanDoAnySexType(pParentA) || Util.CanDoAnySexType(pParentB))
            {
                if(Util.CanDoAnySexType(pParentA) && Util.CanDoAnySexType(pParentB))
                    pregnantActor = !Randy.randomBool() ? pParentB : pParentA;
                else if (Util.CanDoAnySexType(pParentA))
                {
                    pregnantActor = pParentA;
                }
                else
                {
                    pregnantActor = pParentB;
                }
            }

            if (pregnantActor == null)
                return false;

            nonPregnantActor = pregnantActor == pParentA ? pParentB : pParentA;
            // this creates a new family to assign with each other. This should be CALLED after checking to see if they can make babies together
            // __instance.checkFamily(pParentA, pParentB);

            float maturationTimeSeconds = pParentA.getMaturationTimeSeconds();
            
            pParentA.data.get("sex_reason", out var sexReason, "");
            pParentB.data.get("sex_reason", out var sexReason1, "");

            bool success = sexReason1.Equals("casual") || sexReason.Equals("casual") ? Util.WantsBaby(pParentA) && Util.WantsBaby(pParentB) : true; // sexReason1.Equals("casual") || sexReason.Equals("casual") ? Randy.randomChance(0.1F) : true;
            if (success)
            {
                ReproductiveStrategy reproductionStrategy = pregnantActor.subspecies.getReproductionStrategy();
                switch (reproductionStrategy)
                {
                    case ReproductiveStrategy.Egg:
                    case ReproductiveStrategy.SpawnUnitImmediate:
                        BabyMaker.makeBabiesViaSexual(pregnantActor, pParentA, pParentB);
                        pregnantActor.subspecies.counterReproduction();
                        break;
                    case ReproductiveStrategy.Pregnancy:
                        pregnantActor.data.set("otherParent", nonPregnantActor.getID());

                        BabyHelper.babyMakingStart(pregnantActor);
                        pregnantActor.addStatusEffect("pregnant", maturationTimeSeconds);
                        pregnantActor.subspecies.counterReproduction();
                        break;
                }   
            }
            return false;
        }
    }
}