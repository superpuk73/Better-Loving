using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ai.behaviours;
using NeoModLoader.api;
using NeoModLoader.services;
using HarmonyLib;
using NeoModLoader.General;

// TODO: improve homophobic & heterophobic trait, they are lacking. (maybe add task to insult?)
namespace Better_Loving
{
    public class ModClass : BasicMod<ModClass>
    {
        public static BasicMod<ModClass> Mod;
        
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
            
            QueerTraits.Init();
            ActorTraits.Init();
            CultureTraits.Init();
            SubspeciesTraits.Init();
            Happiness.Init();
            CommunicationTopics.Init();
            BehaviourTasks.Init();
            GodPowers.Init();
        }
        private void Awake()
        {
            var harmony = new Harmony("netdot.mian.patch");
            harmony.PatchAll();
        }
    }

    // gives asexual (reproduction method i mean) units the ability to find love!
    [HarmonyPatch(typeof(Actor), nameof(Actor.create))]
    class ActorCreatePatch
    {
        static void Postfix(Actor __instance)
        {
            __instance.asset.addDecision("find_lover");
        }
    }

    [HarmonyPatch(typeof(Actor), nameof(Actor.updateAge))]
    class CalcAgeStatesPatch
    {
        static void Postfix(Actor __instance)
        {
            if (__instance.isAdult()) // fluid sexuality
            {
                if (!QueerTraits.HasQueerTraits(__instance)){
                    QueerTraits.GiveQueerTraits(__instance, false, true);
                    __instance.changeHappiness("true_self");
                }
                else
                {
                    bool changed = false;
                    var list = QueerTraits.GetQueerTraits(__instance);
                    list = QueerTraits.RandomizeQueerTraits(__instance, true, list);
                    if (__instance.hasTrait("abroromantic") && Randy.randomChance(0.1f))
                    {
                        QueerTraits.CleanQueerTraits(__instance, false);
                        __instance.addTrait(list[1]);
                        changed = true;
                    }
                    if (__instance.hasTrait("abrosexual") && Randy.randomChance(0.1f))
                    {
                        QueerTraits.CleanQueerTraits(__instance, true);
                        __instance.addTrait(list[0]);
                        changed = true;
                    }
                    if(changed)
                        __instance.changeHappiness("true_self");
                }
            } else if (!__instance.isAdult() && Randy.randomChance(0.1f)) // random chance younger kid finds their orientations
            {
                QueerTraits.GiveQueerTraits(__instance, false, true);
                __instance.changeHappiness("true_self");
            }
            
            // Randomize breaking up (0.1% if preferences match. 5% if preferences do not match. 50% if they are dying out and they cannot reproduce with their lover) 
            if (__instance.hasLover() && 
                Randy.randomChance(
                    // might change how homophobia works
                    // __instance.data.sex == __instance.lover.data.sex && (__instance.hasCultureTrait("homophobic") || __instance.lover.hasCultureTrait("homophobic")) ? 0.25f : 
                    Util.IsDyingOut(__instance) && !Util.CanReproduce(__instance, __instance.lover) ? 0.5f 
                    : !QueerTraits.PreferenceMatches(__instance, __instance.lover, false) ? 0.05f : 0.001f))
            {
                __instance.lover.setLover(null);
                __instance.lover.changeHappiness("breakup");
                __instance.setLover(null);
                __instance.changeHappiness("breakup");
            }
        }
}

    [HarmonyPatch(typeof(Actor), nameof(Actor.becomeLoversWith))]
    class BecomeLoversWithPatch
    {
        // removes past lovers for cheating purposes
        static void Prefix(Actor pTarget, Actor __instance)
        {
            if (__instance.hasLover() && __instance.lover != pTarget)
            {
                var cheatedActor = __instance.lover;
                cheatedActor.setLover(null);
                cheatedActor.changeHappiness("cheated_on");
            }
        }
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
        private static bool WithinOfAge(Actor pActor, Actor pTarget)
        { 
            int higherAge = Math.Max(pActor.age, pTarget.age);
            int lowerAge = Math.Min(pActor.age, pTarget.age);
            int minimumAge = higherAge / 2 + 7;
            return lowerAge >= minimumAge || (!pActor.hasCultureTrait("mature_dating") && !pTarget.hasCultureTrait("mature_dating"));
        }
        static bool Prefix(Actor pTarget, ref bool __result, Actor __instance)
        {
            // LogService.LogInfo($"Can {__instance.getName()} fall in love with {pTarget.getName()}?");
            var config = ModClass.Mod.GetConfig();
            var allowCrossSpeciesLove = (bool)config["CrossSpecies"]["AllowCrossSpeciesLove"].GetValue();
            var mustBeSmart = (bool)config["CrossSpecies"]["MustBeSmart"].GetValue();
            var mustBeXenophile = (bool)config["CrossSpecies"]["MustBeXenophile"].GetValue();
            
            if (
                (!QueerTraits.PreferenceMatches(__instance, pTarget, false) && !__instance.hasCultureTrait("orientationless") 
                                                                            && (!Util.IsDyingOut(__instance) || !__instance.hasSubspeciesTrait("preservation")))
                 || (!QueerTraits.PreferenceMatches(pTarget, __instance, false) && !pTarget.hasCultureTrait("orientationless") 
                    && (!Util.IsDyingOut(pTarget) || !pTarget.hasSubspeciesTrait("preservation")))
                
                // homophobic cultural trait
                // ||(__instance.data.sex == pTarget.data.sex && (__instance.hasCultureTrait("homophobic") || pTarget.hasCultureTrait("homophobic")))
                
                || (__instance.hasLover() && (!Randy.randomChance(
                        (pTarget.hasTrait("unfaithful") ? 0.1f : 0f) 
                        + (QueerTraits.PreferenceMatches(__instance, __instance.lover, false) ? 0.005f: 0.1f))
                                              || __instance.hasTrait("faithful") || __instance.hasCultureTrait("committed"))) 
                || (pTarget.hasLover() && (!Randy.randomChance(
                                               (pTarget.hasTrait("unfaithful") ? 0.1f : 0f) 
                                               + (QueerTraits.PreferenceMatches(__instance, __instance.lover, false) ? 0.005f: 0.1f))
                                           || pTarget.hasTrait("faithful") || pTarget.hasCultureTrait("committed")))
                
                // make this a cultural trait to configure
                || !WithinOfAge(__instance, pTarget)
                
                || __instance.areFoes(pTarget)
                
                || (!__instance.isSameSpecies(pTarget) && !__instance.isSameSubspecies(pTarget.subspecies)
                                                       && (!(( __instance.hasCulture() && __instance.culture.hasTrait("xenophiles") || !mustBeXenophile)
                                                             && (Util.IsSmart(__instance) && Util.IsSmart(pTarget) || !mustBeSmart)
                                                             && (!pTarget.hasCulture() || (pTarget.hasCulture() && !pTarget.culture.hasTrait("xenophobic")))) 
                                                           || !allowCrossSpeciesLove)) // subspecies stuff!
                
                // if queer but culture trait says they do not matter
                || ((pTarget.hasCultureTrait("orientationless") || __instance.hasCultureTrait("orientationless")) 
                    && !Util.CanReproduce(__instance, pTarget)))
            {
                __result = false;
                return false;
            }

            if ((__instance.isChildOf(pTarget) || __instance.isParentOf(pTarget)) && (!__instance.hasCultureTrait("incest") || !pTarget.hasCultureTrait("incest")))
            {
                __result = false;
                return false;
            }

            if (
                (__instance.hasSubspeciesTrait("preservation") && Util.IsDyingOut(__instance) && !Util.CanReproduce(__instance, pTarget))
                ||
                (pTarget.hasSubspeciesTrait("preservation") && Util.IsDyingOut(pTarget) && !Util.CanReproduce(pTarget, __instance))
                ) {
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

    // this patch handles who the mother is when it comes to sexual reproduction and if it should even happen
    [HarmonyPatch(typeof(BehCheckForBabiesFromSexualReproduction),
        nameof(BehCheckForBabiesFromSexualReproduction.checkForBabies))]
    class CheckForBabiesPatch
    {
        // custom method specifically to avoid population limit checks because we do that later
        static bool CanMakeBabies(Actor pActor)
        {
            // make it configurable so they have to be adults or not
            return pActor.isBreedingAge() 
                   && !pActor.hasReachedOffspringLimit() 
                   && (!pActor.hasCity() || pActor.city.canProduceUnits()) 
                   && pActor.haveNutritionForNewBaby();
        }
        static bool Prefix(Actor pParentA, Actor pParentB, BehCheckForBabiesFromSexualReproduction __instance)
        {
            if (!CanMakeBabies(pParentA) || !CanMakeBabies(pParentB))
                return false;

            // ensures that both subspecies HAVE not reached population limit
            if (pParentA.subspecies.hasReachedPopulationLimit() || pParentB.subspecies.hasReachedPopulationLimit())
                return false;

            if ((!QueerTraits.PreferenceMatches(pParentA, pParentB, true)
                || !QueerTraits.PreferenceMatches(pParentB, pParentA, true))
                && (!Util.IsDyingOut(pParentA) || !pParentA.hasSubspeciesTrait("preservation")) && (!Util.IsDyingOut(pParentB) || !pParentA.hasSubspeciesTrait("preservation")))
                return false;
            
            var subspeciesA = pParentA.subspecies;
            var subspeciesB = pParentB.subspecies;

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