using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ai.behaviours;
using NeoModLoader.api;
using NeoModLoader.services;
using HarmonyLib;
using NeoModLoader.General;

/*
(idk if i should fix this, i mean it's kinda funny)
- someone became lover right as they started fucking with someone else so the person got mad LOL

- abroromantic/abrorsexual trait arent working perfectly. They wont add new traits for whatever reason

- make a sexual reproduction trait that allows two ppl of the same sex to reproduce (done maybe, just need icon and to test)
- adjust how ppl get their sexuality chances based on reproduction method (done)

(wip)
- add sexual ivf task for units that cant get pregnant but want a baby (can lead to adoption which could be a happiness aspect!)
*/
namespace Better_Loving.Mian
{
    public class TopicOfLoving : BasicMod<TopicOfLoving>
    {
        public static BasicMod<TopicOfLoving> Mod;
        
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
            StatusEffects.Init();
            Happiness.Init();
            CommunicationTopics.Init();
            DecisionAndBehaviorTasks.Init();
            GodPowers.Init();
        }
        private void Awake()
        {
            var harmony = new Harmony("netdot.mian.topicofloving");
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(Subspecies), nameof(Subspecies.isPartnerSuitableForReproduction))]
    class SuitableReproductionPatch
    {
        static bool Prefix(Actor pActor, Actor pTarget, Subspecies __instance, ref bool __result)
        {
            if (!pActor.hasSubspecies() || !pTarget.hasSubspecies())
            {
                __result = false;
                return false;
            }

            if (Util.CanDoAnySexType(pActor))
            {
                __result = true;
                return false;
            }
            
            if (__instance.needOppositeSexTypeForReproduction())
            {
                if ((pActor.data.sex != pTarget.data.sex && pTarget.subspecies.isReproductionSexual()) || Util.CanDoAnySexType(pTarget))
                {
                    __result = true;
                    return false;
                }
            } else if (Util.NeedSameSexTypeForReproduction(pActor))
            {
                if ((pActor.data.sex == pTarget.data.sex && Util.NeedSameSexTypeForReproduction(pTarget)) || Util.CanDoAnySexType(pTarget))
                {
                    __result = true;
                    return false;
                }
            }

            __result = false;
            return false;
        }
    }

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
        }
    }

    // gives asexual (reproduction method i mean) units the ability to find love!
    [HarmonyPatch(typeof(Actor), nameof(Actor.create))]
    class ActorCreatePatch
    {
        static void Postfix(Actor __instance)
        {
            __instance.asset.addDecision("find_lover");
            __instance.data.set("sexual_happiness", 10f);
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
                if(QueerTraits.GetPreferenceFromActor(__instance, true) != Preference.Neither)
                    Util.ChangeSexualHappinessBy(__instance.a, -Randy.randomFloat(10, 20f));
                else
                    __instance.data.set("sexual_happiness", 100f);
            } else if (!__instance.isAdult() && Randy.randomChance(0.1f)) // random chance younger kid finds their orientations
            {
                QueerTraits.GiveQueerTraits(__instance, false, true);
                __instance.changeHappiness("true_self");
            }
            
            // Randomize breaking up (1% if preferences match. 25% if preferences do not match.) 
            if (__instance.hasLover() && 
                Randy.randomChance(
                    // not needed since normal sex can happen now
                    // Util.IsDyingOut(__instance) && !Util.CanReproduce(__instance, __instance.lover) ? 0.5f : 
                    !QueerTraits.PreferenceMatches(__instance, __instance.lover, false) ? 0.25f : 0.01f))
            {
                Util.BreakUp(__instance);
            }
        }
}

    [HarmonyPatch(typeof(Actor), nameof(Actor.becomeLoversWith))]
    class BecomeLoversWithPatch
    {
        static void Postfix(Actor pTarget, Actor __instance)
        {
            __instance.setFamily(null);
            pTarget.setFamily(null);
            
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
            var config = TopicOfLoving.Mod.GetConfig();
            var allowCrossSpeciesLove = (bool)config["CrossSpecies"]["AllowCrossSpeciesLove"].GetValue();
            var mustBeSmart = (bool)config["CrossSpecies"]["MustBeSmart"].GetValue();
            var mustBeXenophile = (bool)config["CrossSpecies"]["MustBeXenophile"].GetValue();
            
            if (
                Util.OnceCheated(pTarget, __instance)
                ||
                 (!QueerTraits.PreferenceMatches(__instance, pTarget, false) && !__instance.hasCultureTrait("orientationless"))
                 || (!QueerTraits.PreferenceMatches(pTarget, __instance, false) && !pTarget.hasCultureTrait("orientationless"))
                
                || __instance.hasLover()
                || pTarget.hasLover()
                // replaced by the newer sex cheating system (we'll probably do a flirting task in the future for romance?)

                // || (__instance.hasLover() && (!Randy.randomChance(
                //         (pTarget.hasTrait("unfaithful") ? 0.1f : 0f) 
                //         + (QueerTraits.PreferenceMatches(__instance, __instance.lover, false) ? 0.005f: 0.1f))
                //                               || __instance.hasTrait("faithful") || __instance.hasCultureTrait("committed"))) 
                // || (pTarget.hasLover() && (!Randy.randomChance(
                //                                (pTarget.hasTrait("unfaithful") ? 0.1f : 0f) 
                //                                + (QueerTraits.PreferenceMatches(__instance, __instance.lover, false) ? 0.005f: 0.1f))
                //                            || pTarget.hasTrait("faithful") || pTarget.hasCultureTrait("committed")))
                
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

            if ((__instance.isRelatedTo(pTarget)) && (!__instance.hasCultureTrait("incest") || !pTarget.hasCultureTrait("incest")))
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
                if (pAddToFamily)
                {
                    // families are no longer created based on babies, but based on lovers
                    var randomParentToGoTo = dominantParent.hasFamily() 
                                             && (Randy.randomChance(0.5f) || nonDominantParent == null) ? dominantParent : nonDominantParent;
                    if(randomParentToGoTo != null && randomParentToGoTo.hasFamily())
                        actorFromData.setFamily(randomParentToGoTo.family);
                    
                    // if (!dominantParent.hasFamily())
                    // {
                    //     var actor = nonDominantParent == null || nonDominantParent.hasFamily() ? null : nonDominantParent;
                    //     World.world.families.newFamily(dominantParent, dominantParent.current_tile, actor);
                    //     // don't kick non dominant parent out of their family if they have a child. Dominant parent gets the family
                    // } else if (dominantParent.hasFamily())
                    // {
                    //     actorFromData.setFamily(dominantParent.family);
                    // }
                }
                // if (pAddToFamily && !dominantParent.hasFamily() && (nonDominantParent == null || !nonDominantParent.hasFamily()))
                //     World.world.families.newFamily(dominantParent, dominantParent.current_tile, nonDominantParent);
                // else if(pAddToFamily && dominantParent.hasFamily())
                //     actorFromData.setFamily(dominantParent.family);
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
                     || !QueerTraits.PreferenceMatches(pParentB, pParentA, true)))
                {
                    __result = false;
                    return false;
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(BehSpawnHeartsFromBuilding), nameof(BehSpawnHeartsFromBuilding.execute))]
    class SpawnHeartsPatch
    {
        static bool Prefix(Actor pActor, ref BehResult __result, BehSpawnHeartsFromBuilding __instance)
        {
            var target = pActor.beh_actor_target != null ? pActor.beh_actor_target.a : pActor.lover;
            if (target == null)
            {
                LogService.LogInfo(pActor.getName()+": Cant do sex because target is null");
                __result = BehResult.Stop;
                return false;
            }
            pActor.addAfterglowStatus();
            target.addAfterglowStatus();
            __instance.spawnHearts(pActor);
            __result = BehResult.Continue;
            return false;
        }
    }

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
                LogService.LogInfo(pActor.getName()+": Cant do sex because target is null");
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
        static bool Prefix(Actor pParentA, Actor pParentB, BehCheckForBabiesFromSexualReproduction __instance)
        {
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
            
            bool success = sexReason1.Equals("casual") || sexReason.Equals("casual") ? Randy.randomChance(0.1F) : true;
            
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

    [HarmonyPatch(typeof(BabyMaker), nameof(BabyMaker.makeBabyFromPregnancy))]
    class MakeBabyFromPregnancyPatch
    {
        static bool Prefix(Actor pActor)
        {
            var mother = pActor;
            mother.data.get("otherParent", out long otherParentID);
            var otherParent = MapBox.instance.units.get(otherParentID);
            mother.data.removeLong("otherParent");

            mother.birthEvent();
            BabyHelper.countMakeChild(mother, otherParent);
            BabyMaker.makeBaby(mother, otherParent);
            var pVal = 0.5f;
            var stat = (int) mother.stats["birth_rate"];
            for (var index = 0; index < stat && Randy.randomChance(pVal); ++index)
            {
                BabyMaker.makeBaby(mother, otherParent);
                pVal *= 0.85f;
            }

            return false;
        }
    }
}