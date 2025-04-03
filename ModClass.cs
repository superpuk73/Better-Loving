using System.Collections;
using System.Collections.Generic;
using ai.behaviours;
using UnityEngine;
using NeoModLoader.api;
using NeoModLoader.services;
using HarmonyLib;
using NeoModLoader.General;

// TODO: we also prob want to make sure they arent foes like xenophobic or anything lol (DONE)
// TODO: Figure out how reproduction methods like mitosis work (DONE)
// TODO: Figure out how to handle Harmony traits (DONE)
// TODO: figure out why 2+ creatures with mitosis wont fall in love with each other
// TODO: add options to configure chances (DONE)
namespace Better_Loving
{
    public class Util
    {
        // Returns the parent that has a population limit not REACHED yet
        public static Actor EnsurePopulationFromParent(List<Actor> parents)
        {
            var canMake = new List<Actor>();

            foreach (var parent in parents)
            {
                if (!parent.subspecies.hasReachedPopulationLimit())
                    canMake.Add(parent);
            }

            if (canMake.Count <= 0) return null;

            return canMake.GetRandom();
        }

        public static bool IsSmart(Actor actor)
        {
            return actor.hasSubspeciesTrait("prefrontal_cortex") 
                   && actor.hasSubspeciesTrait("advanced_hippocampus") 
                   && actor.hasSubspeciesTrait("amygdala") 
                   && actor.hasSubspeciesTrait("wernicke_area");
        }
    }
    public class ModClass : BasicMod<ModClass>
    {
        public static BasicMod<ModClass> Mod;
        
        protected override void OnModLoad()
        {
            Mod = this;
            // Initialize your mod.
            // Methods are called in the order: OnLoad -> Awake -> OnEnable -> Start -> Update
            LogService.LogInfo($"[{GetDeclaration().Name}]: Making people more loveable!");
            
            LM.AddToCurrentLocale("Incest", "Incest");
            LM.AddToCurrentLocale("Incest Description", "Whether family members can become lovers of one another. Aka, the good ol' medieval times.");
            LM.AddToCurrentLocale("ForbiddenLove", "Forbidden Love Chance");
            LM.AddToCurrentLocale("ForbiddenLove Description", "When the game rolls for two lovers that cannot reproduce together (this roll happens every time they socialize), this determines how likely it is that they will end up together. Do not set this too high or you will have population collapse! \n\nExample: Gay couples with the sexual reproduction method.");
            LM.AddToCurrentLocale("Misc", "Misc");
            
            LM.AddToCurrentLocale("CrossSpecies", "Cross Species");
            LM.AddToCurrentLocale("AllowCrossSpeciesLove", "Allow Cross Species Love");
            LM.AddToCurrentLocale("AllowCrossSpeciesLove Description", "Whether different species should be able to love each other.");
            LM.AddToCurrentLocale("MustBeSmart", "Must Be Smart");
            LM.AddToCurrentLocale("MustNotBeSmart Description", "Whether the two subspecies involved have to have all their brain functions in order to love each other.");
            LM.AddToCurrentLocale("MustBeXenophile", "Must Be Xenophile");
            LM.AddToCurrentLocale("MustBeXenophile Description", "Whether the initiating lover has to be xenophile in order to love another subspecies unlike them. The receiving lover does NOT need to be xenophile, but they cannot be xenophobic.");
            LM.ApplyLocale("en");
        }
        private void Awake()
        {
            var harmony = new Harmony("netdot.mian.patch");
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(BehTryToSocialize), nameof(BehTryToSocialize.getRandomActorAround))]
    // this patch makes it so ppl that DON'T need opposite sex type for reproduction can still become lovers
    // this makes it so ppl with like mitosis reproduction can still become lovers even if they dont need each other
    class GetRandomActorAroundPatch
    {
        static bool Prefix(Actor pActor, BehTryToSocialize __instance, ref Actor __result)
        {
            using (ListPool<Actor> listPool1 = new ListPool<Actor>(4))
            {
                using (ListPool<Actor> listPool2 = new ListPool<Actor>(4))
                {
                    bool flag1 = true; //pActor.subspecies.needOppositeSexTypeForReproduction();
                    bool flag2 = pActor.hasCulture() && pActor.culture.hasTrait("animal_whisperers");
                    int num = pActor.hasTelepathicLink() ? 1 : 0;
                    if (num != 0)
                        __instance.fillUnitsViaTelepathicLink(pActor, listPool1, listPool2);
                    int pChunkRadius = 1;
                    if (num != 0)
                        pChunkRadius = 2;
                    bool pRandom = Randy.randomBool();
                    foreach (Actor actor in Finder.getUnitsFromChunk(pActor.current_tile, pChunkRadius, pRandom: pRandom))
                    {
                        if (pActor.canTalkWith(actor))
                        {
                            if (pActor.isKingdomCiv())
                            {
                                if (actor.isKingdomMob())
                                {
                                    if (!flag2)
                                        continue;
                                }
                                else if (!actor.isKingdomCiv())
                                    ;
                            }
                            else if (!pActor.isSameSpecies(actor))
                                continue;
                            if (flag1 && pActor.canFallInLoveWith(actor))
                            {
                                listPool1.Add(actor);
                                if (pRandom)
                                {
                                    if (Randy.randomBool())
                                        break;
                                }
                            }
                            listPool2.Add(actor);
                            if (pRandom && ((ICollection)listPool1).Count > 0)
                            {
                                if (Randy.randomBool())
                                    break;
                            }
                        }
                    }
                    if (((ICollection)listPool1).Count > 0)
                        __result = listPool1.GetRandom<Actor>();
                    __result= ((ICollection) listPool2).Count > 0 ? listPool2.GetRandom<Actor>() : (Actor) null;
                }
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(Actor), nameof(Actor.becomeLoversWith))]
    class BecomeLoversWithPatch
    {
        // for the sake of not having weird family issues down the road, we'll just create a new family immediately when they become lovers
        static void Postfix(Actor pTarget, Actor __instance)
        {
            if(!__instance.hasFamily() && !pTarget.hasFamily())
                BehaviourActionBase<Actor>.world.families.newFamily(__instance, __instance.current_tile, pTarget);
        }
    }
    
    [HarmonyPatch(typeof(Actor), nameof(Actor.canFallInLoveWith))]
    class CheckLovePatch
    {
        static bool Prefix(Actor pTarget, ref bool __result, Actor __instance)
        {
            // LogService.LogInfo($"Tryna see if we can fall in love with {pTarget.getName()}");
            var config = ModClass.Mod.GetConfig();
            var forbiddenLove = (float)config["Misc"]["ForbiddenLove"].GetValue();
            var allowCrossSpeciesLove = (bool)config["CrossSpecies"]["AllowCrossSpeciesLove"].GetValue();
            var mustBeSmart = (bool)config["CrossSpecies"]["MustBeSmart"].GetValue();
            var mustBeXenophile = (bool)config["CrossSpecies"]["MustBeXenophile"].GetValue();
            var incest = (bool)config["Misc"]["Incest"].GetValue();
            
            if (__instance.hasLover()
                || !__instance.isAdult()
                || !__instance.isBreedingAge()
                || __instance.areFoes(pTarget)
                || !__instance.subspecies.needs_mate

                || (!__instance.isSameSpecies(pTarget) && !__instance.isSameSubspecies(pTarget.subspecies)
                     && (!(( __instance.hasCulture() && __instance.culture.hasTrait("xenophiles") || !mustBeXenophile)
                                                  && (Util.IsSmart(__instance) && Util.IsSmart(pTarget) || !mustBeSmart)
                          && (!pTarget.hasCulture() || (pTarget.hasCulture() && !pTarget.culture.hasTrait("xenophobic")))) || !allowCrossSpeciesLove)) // subspecies stuff!
                
                || (!__instance.subspecies.isPartnerSuitableForReproduction(__instance, pTarget) 
                      && !__instance.subspecies.hasTraitReproductionSexualHermaphroditic() 
                      && !pTarget.subspecies.hasTraitReproductionSexualHermaphroditic() && 
                    Random.Range(1, 101) / 100.0 > forbiddenLove) // gay chance
                
                || pTarget.hasLover()
                || !pTarget.isAdult()
                || !pTarget.isBreedingAge())
            {
                __result = false;
                return false;
            }

            if (!__instance.isSapient() || !__instance.hasFamily())
            {
                __result = true;
                return false;
            }

            if (__instance.isRelatedTo(pTarget) && !incest)
            {
                __result = false;
                return false;
            }
            
            __result = true;

            // LogService.LogInfo($"Success! They in love :D");
            return false; // It's harsh code I know, but this does basically overwrite how loving works so it's necessary.
            // Users probably won't have another mod that modifies how loving works anyways. Chances are this mod will cover that.
            // if an issue, we can solve this later
        }
    }

    [HarmonyPatch(typeof(BabyHelper), nameof(BabyHelper.checkMetaLimitsResult))]
    class checkMetaLimitsPatch
    {
        static bool Prefix(Actor pActor, ref bool __result)
        {
            // we removed the population check for subspecies cause we do our own in the other patches
            __result = !pActor.hasCity() || pActor.city.canProduceUnits();
            return false;
        }
    }

    [HarmonyPatch(typeof(BabyMaker), nameof(BabyMaker.makeBaby))]
    class MakeBabyPatch
    {
        static bool Prefix(
            Actor pParent1,
            Actor pParent2,
            ActorSex pForcedSexType,
            bool pCloneTraits,
            int pMutationRate,
            WorldTile pTile,
            bool pAddToFamily,
            ref Actor __result
            )
        {
            List<Actor> parents = new List<Actor>{pParent1};
            if(pParent2 != null)
                parents.Add(pParent2);
            
            City pCity = pParent1.city ?? pParent2?.city;
            
            if (pCity != null)
                --pCity.status.housing_free;

            Actor dominantParent = Util.EnsurePopulationFromParent(parents);
            if (dominantParent == null) 
                // there seems to be a bug in the game that allows reproduction strategies aren't sexual to PRODUCE beyond the harmony traits cap. probably because they dont check for populations there lol
                dominantParent = pParent1;
            
            Actor nonDominantParent = dominantParent != pParent1 ? pParent1 : pParent2;
            // Subspecies randomSpecies = randomDecidingParent.subspecies;
            ActorAsset asset = dominantParent.asset;
            
            ActorData pData = new ActorData();
            pData.created_time = World.world.getCurWorldTime();
            pData.id = World.world.map_stats.getNextId("unit");
            pData.asset_id = asset.id;
            int generation = dominantParent.data.generation;
            if (nonDominantParent != null && nonDominantParent.data.generation > generation)
                generation = nonDominantParent.data.generation;
            pData.generation = generation + 1;
            using (ListPool<WorldTile> list = new ListPool<WorldTile>(4))
            {
                foreach (WorldTile worldTile in dominantParent.current_tile.neighboursAll)
                {
                    if (worldTile != dominantParent.current_tile &&
                        (nonDominantParent == null || worldTile != nonDominantParent.current_tile) && worldTile.Type.ground)
                        list.Add(worldTile);
                }

                WorldTile pTile1 = pTile == null
                    ? ((ICollection) list).Count != 0 ? list.GetRandom<WorldTile>() : dominantParent.current_tile
                    : pTile;
                Actor actorFromData = World.world.units.createActorFromData(pData, pTile1, pCity);
                actorFromData.setParent1(dominantParent);
                if (nonDominantParent != null)
                    actorFromData.setParent2(nonDominantParent);
                if (pAddToFamily && !dominantParent.hasFamily())
                    World.world.families.newFamily(dominantParent, dominantParent.current_tile, nonDominantParent);
                BabyHelper.applyParentsMeta(dominantParent, nonDominantParent, actorFromData);
                // the game seems to have some sort of code that chooses a baby's subspecies based on generation? not really sure how it works tbh
                actorFromData.setSubspecies(dominantParent.subspecies);

                if (pCloneTraits || dominantParent.hasSubspeciesTrait("genetic_mirror"))
                {
                    if (pCloneTraits)
                    {
                        BabyHelper.traitsClone(actorFromData, dominantParent);
                        if (nonDominantParent != null)
                        {
                            BabyHelper.traitsClone(actorFromData, nonDominantParent);
                        }
                    }
                    else
                    {
                        BabyHelper.traitsClone(actorFromData, dominantParent);

                        if (nonDominantParent != null)
                        {
                            BabyHelper.traitsInherit(actorFromData, nonDominantParent, null);   
                        }                        
                    }
                }
                else
                {
                    foreach (ActorTrait trait in (IEnumerable<ActorTrait>)actorFromData.subspecies.getActorBirthTraits()
                                 .getTraits())
                        actorFromData.addTrait(trait);
                    BabyHelper.traitsInherit(actorFromData, pParent1, nonDominantParent);
                }

                actorFromData.checkTraitMutationBirth();
                actorFromData.setNutrition(SimGlobals.m.nutrition_start_level_baby);
                if (pForcedSexType != ActorSex.None)
                {
                    actorFromData.data.sex = pForcedSexType;
                }
                else
                {
                    ActorSex actorSex = ActorSex.None;
                    if (Randy.randomBool())
                        actorSex = !dominantParent.hasCity()
                            ? (dominantParent.subspecies.cached_females <= dominantParent.subspecies.cached_males
                                ? ActorSex.Female
                                : ActorSex.Male)
                            : (dominantParent.city.status.females <= dominantParent.city.status.males
                                ? ActorSex.Female
                                : ActorSex.Male);
                    if (actorSex != ActorSex.None)
                        actorFromData.data.sex = actorSex;
                    else
                        actorFromData.generateSex();
                }
                actorFromData.checkShouldBeEgg();
                actorFromData.makeStunned(10f);
                actorFromData.applyRandomForce();
                BabyHelper.countBirth(actorFromData);
                actorFromData.setStatsDirty();
                actorFromData.event_full_stats = true;
                __result = actorFromData;
                
                // LogService.LogInfo($"[{__result.getName()}]: Baby is of {__result.subspecies.name} and comes from {__result.asset.getTranslatedName()}");
                return false;
            }
        }
    }

    // this patch is to edit sexual reproduction before it starts making a baby!
    [HarmonyPatch(typeof(BehCheckForBabiesFromSexualReproduction),
        nameof(BehCheckForBabiesFromSexualReproduction.checkForBabies))]
    class CheckForBabiesPatch
    {
        static bool Prefix(Actor pParentA, Actor pParentB, BehCheckForBabiesFromSexualReproduction __instance)
        {
            if (!BabyHelper.canMakeBabies(pParentA) || !BabyHelper.canMakeBabies(pParentB))
                return false;

            // ensures that both subspecies HAVE not reached population limit
            if (pParentA.subspecies.hasReachedPopulationLimit() || pParentB.subspecies.hasReachedPopulationLimit())
                return false;
            
            Subspecies subspeciesA = pParentA.subspecies;
            Subspecies subspeciesB = pParentB.subspecies;

            Actor mother = null;
            
            if (subspeciesA.hasTraitReproductionSexual() && subspeciesB.hasTraitReproductionSexual())
            {
                // for gay ppl
                if (pParentA.data.sex == pParentB.data.sex) return false;
                
                if (pParentA.isSexFemale())
                    mother = pParentA;
                else if (pParentB.isSexFemale())
                    mother = pParentB;
            }
            else if ((
                         subspeciesA.hasTraitReproductionSexualHermaphroditic() ||
                     subspeciesB.hasTraitReproductionSexualHermaphroditic()) 
                     && ((subspeciesA.hasTraitReproductionSexual() || subspeciesB.hasTraitReproductionSexual()) 
                         || (subspeciesA.hasTraitReproductionSexualHermaphroditic() && subspeciesB.hasTraitReproductionSexualHermaphroditic())))
            {
                
                if (subspeciesA.hasTraitReproductionSexualHermaphroditic() &&
                    subspeciesB.hasTraitReproductionSexualHermaphroditic())
                {
                    mother = !Randy.randomBool() ? pParentB : pParentA;
                } 
                else if (subspeciesA.hasTraitReproductionSexualHermaphroditic())
                {
                    mother = pParentA;
                }
                else
                {
                    mother = pParentB;
                }
            }
            else
            {
                return false; // this means that they don't have the same reproduction methods WHICH we WILL refuse! Example: a sheep with mitosis should not be able to be bred from a rabbit with sexual reproduction!
            }
            
            if (mother == null)
                return false;
            
            // this creates a new family to assign with each other. This should be CALLED after checking to see if they can make babies together
            __instance.checkFamily(pParentA, pParentB);
            
            float maturationTimeSeconds = pParentA.getMaturationTimeSeconds();
            
            ReproductiveStrategy reproductionStrategy = mother.subspecies.getReproductionStrategy();
            switch (reproductionStrategy)
            {
                case ReproductiveStrategy.Egg:
                case ReproductiveStrategy.SpawnUnitImmediate:
                    BabyMaker.makeBabiesViaSexual(mother, pParentA, pParentB);
                    mother.subspecies.counterReproduction();
                    break;
                case ReproductiveStrategy.Pregnancy:
                    BabyHelper.babyMakingStart(mother);
                    mother.addStatusEffect("pregnant", maturationTimeSeconds);
                    mother.subspecies.counterReproduction();
                    break;
            }
            
            return false;
        }
    }
}