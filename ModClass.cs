using System.Collections;
using System.Collections.Generic;
using System.IO;
using ai.behaviours;
using UnityEngine;
using NeoModLoader.api;
using NeoModLoader.services;
using HarmonyLib;
using NeoModLoader.General;

namespace Better_Loving
{
    public class ModClass : BasicMod<ModClass>
    {
        public static BasicMod<ModClass> Mod;
        public Actor SelectedActorA;
        public Actor SelectedActorB;
        
        protected override void OnModLoad()
        {
            Mod = this;
            // Initialize your mod.
            // Methods are called in the order: OnLoad -> Awake -> OnEnable -> Start -> Update
            LogService.LogInfo($"[{GetDeclaration().Name}]: Making people more loveable!");
            
            var locale_dir = GetLocaleFilesDirectory(GetDeclaration());
            foreach (var file in Directory.GetFiles(locale_dir))
            {
                if (file.EndsWith(".json"))
                {
                    LM.LoadLocale(Path.GetFileNameWithoutExtension(file), file);
                }
                else if (file.EndsWith(".csv"))
                {
                    LM.LoadLocales(file);
                }
            }

            LM.ApplyLocale();
            
            // adds in Aromantic trait
            var aromanticTrait = new ActorTrait
            {
                id = "aromantic",
                path_icon = "ui/Icons/actor_traits/iconAromantic", // temporary icon!
                group_id = "mind",
                rate_birth=1,
                rate_acquire_grow_up = 1,
                type=TraitType.Other,
                unlocked_with_achievement = false,
                rarity = Rarity.R3_Legendary,
                needs_to_be_explored = true
            };
            
            for (int index = 0; index < aromanticTrait.rate_birth; ++index)
                AssetManager.traits.pot_traits_birth.Add(aromanticTrait);
            for (int index = 0; index < aromanticTrait.rate_acquire_grow_up; ++index)
                AssetManager.traits.pot_traits_growup.Add(aromanticTrait);
            
            AssetManager.traits.add(aromanticTrait);
            
            // adds in ForceLover tool
            var forceLover = new GodPower
            {
                id = "forceLover",
                name = "ForceLover",
                force_map_mode = MetaType.Unit,
                path_icon = "god_powers/iconForceLover", // temporary icon!
                can_drag_map = true,
                type = PowerActionType.PowerSpecial,
                select_button_action = powerId => 
                {
                    WorldTip.showNow("love_selected", pPosition: "top");
                    SelectedActorA = null;
                    SelectedActorB = null;
                    return false;
                },
                click_special_action = (pTile, powerId) =>
                {
                    var pActor = pTile != null ? ActionLibrary.getActorFromTile(pTile) : World.world.getActorNearCursor();
                    if (pActor == null)
                        return false;

                    if (SelectedActorA == null)
                    {
                        SelectedActorA = pActor;
                        ActionLibrary.showWhisperTip("love_selected_first");
                        return false;
                    } 
                    
                    if (SelectedActorB == null && pActor == SelectedActorA)
                    {
                        ActionLibrary.showWhisperTip("love_cancelled");
                        SelectedActorA = null;
                        SelectedActorB = null;
                        return false;
                    }

                    if (SelectedActorA.lover == pActor)
                    {
                        ActionLibrary.showWhisperTip("love_cancelled");
                        SelectedActorA = null;
                        SelectedActorB = null;
                        return false;
                    }
                
                    ActionLibrary.showWhisperTip("love_successful");
                    SelectedActorB = pActor;
                    SelectedActorA.becomeLoversWith(SelectedActorB);
                    SelectedActorA = null;
                    SelectedActorB = null;
                
                    return true;
                },
                tester_enabled = false
            };
            AssetManager.powers.add(forceLover);
            
            // TODO: finish this when NML unscuffs their UI
            // TabManager.TabOther.AddPowerButton("Default", PowerButtonCreator.CreateGodPowerButton("forceLover", 
                // SpriteTextureLoader.getSprite("ui/Icons/god_powers/iconForceLover")));
        }
        private void Awake()
        {
            var harmony = new Harmony("netdot.mian.patch");
            harmony.PatchAll();
        }
    }

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
    
    // gives asexual units the ability to find love!
    [HarmonyPatch(typeof(Actor), nameof(Actor.create))]
    class ActorCreatePatch
    {
        static void Postfix(Actor __instance)
        {
            __instance.asset.addDecision("find_lover");
        }
    }

    // for the sake of not having weird family issues down the road, we'll just create a new family immediately when they become lovers
    [HarmonyPatch(typeof(Actor), nameof(Actor.becomeLoversWith))]
    class BecomeLoversWithPatch
    {
        static void Postfix(Actor pTarget, Actor __instance)
        {
            if(!__instance.hasFamily() && !pTarget.hasFamily())
                BehaviourActionBase<Actor>.world.families.newFamily(__instance, __instance.current_tile, pTarget);
        }
    }
    
    // This is where we handle the beef of our code for having cross species and non-same reproduction method ppl fall in love
    [HarmonyPatch(typeof(Actor), nameof(Actor.canFallInLoveWith))]
    class CanFallInLoveWithPatch
    {
        static bool Prefix(Actor pTarget, ref bool __result, Actor __instance)
        {
            // LogService.LogInfo($"Can {__instance.getName()} fall in love with {pTarget.getName()}?");
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
                // || !__instance.subspecies.needs_mate this makes it so asexual ppl can reproduce
                || (!__instance.isSameSpecies(pTarget) && !__instance.isSameSubspecies(pTarget.subspecies)
                     && (!(( __instance.hasCulture() && __instance.culture.hasTrait("xenophiles") || !mustBeXenophile)
                                                  && (Util.IsSmart(__instance) && Util.IsSmart(pTarget) || !mustBeSmart)
                          && (!pTarget.hasCulture() || (pTarget.hasCulture() && !pTarget.culture.hasTrait("xenophobic")))) || !allowCrossSpeciesLove)) // subspecies stuff!
                
                || (!__instance.subspecies.isPartnerSuitableForReproduction(__instance, pTarget) 
                      && !__instance.subspecies.hasTraitReproductionSexualHermaphroditic() 
                      && !pTarget.subspecies.hasTraitReproductionSexualHermaphroditic() && 
                    Random.Range(1, 101) / 100.0 > forbiddenLove) // chance of getting together even if they cant reproduce
                
                || pTarget.hasLover()
                || !pTarget.isAdult()
                || !pTarget.isBreedingAge()
                || __instance.hasTrait("aromantic")
                || pTarget.hasTrait("aromantic"))
            {
                __result = false;
                return false;
            }

            if ((__instance.isChildOf(pTarget) || __instance.isParentOf(pTarget)) && !incest)
            {
                __result = false;
                return false;
            }
            
            __result = true;

            // LogService.LogInfo($"Success! They in love :D");
            return false;
        }
    }
    
    // We just randomize the parent chosen for the subspecies here
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
                // there seems to be a bug in the game that allows reproduction strategies that aren't sexual to PRODUCE beyond the harmony traits cap. probably because they dont check for populations there lol
                dominantParent = pParent1;
            
            Actor nonDominantParent = dominantParent != pParent1 ? pParent1 : pParent2;
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

    // this patch handles who the mother is when it comes to sexual reproduction
    [HarmonyPatch(typeof(BehCheckForBabiesFromSexualReproduction),
        nameof(BehCheckForBabiesFromSexualReproduction.checkForBabies))]
    class CheckForBabiesPatch
    {
        // custom method specifically to avoid population limit checks because we do that later
        static bool CanMakeBabies(Actor pActor)
        {
            return pActor.isAdult() && !pActor.hasReachedOffspringLimit() && (!pActor.hasCity() || pActor.city.canProduceUnits()) && pActor.haveNutritionForNewBaby();
        }
        static bool Prefix(Actor pParentA, Actor pParentB, BehCheckForBabiesFromSexualReproduction __instance)
        {
            if (!CanMakeBabies(pParentA) || !CanMakeBabies(pParentB))
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