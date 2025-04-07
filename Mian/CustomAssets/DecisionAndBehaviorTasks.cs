using System.Collections;
using System.Collections.Generic;
using ai.behaviours;
using HarmonyLib;
using NeoModLoader.services;

namespace Better_Loving
{
    public class DecisionAndBehaviorTasks
    {
        private static List<DecisionAsset> _decisionAssets = new List<DecisionAsset>();
        public static void Init()
        {
            BehaviourTaskActor insultOrientation = new BehaviourTaskActor
            {
                id = "insult_orientation",
                locale_key = "task_insult_orientation",
                path_icon = "ui/Icons/culture_traits/orientationless",
            };
            insultOrientation.addBeh(new BehFindMismatchedOrientation());
            insultOrientation.addBeh(new BehGoToActorTarget(GoToActorTargetType.NearbyTileClosest, pCalibrateTargetPosition: true));
            insultOrientation.addBeh(new BehCheckNearActorTarget());
            insultOrientation.addBeh(new BehInsultOrientation());
            AddBehavior(insultOrientation);

            AddDecision(new DecisionAsset
            {
                id = "insult_orientation_try",
                task_id = "insult_orientation",
                priority = NeuroLayer.Layer_2_Moderate,
                path_icon = "ui/Icons/culture_traits/orientationless",
                cooldown = 30,
                action_check_launch = actor => actor.hasCultureTrait("homophobic") || actor.hasCultureTrait("heterophobic"),
                weight = 0.75f,
                list_civ = true
            });
            AssetManager.subspecies_traits.get("wernicke_area").addDecision("insult_orientation_try");
            
            AddDecision(new DecisionAsset
            {
                id = "invite_for_sex",
                priority = NeuroLayer.Layer_2_Moderate,
                path_icon = "ui/Icons/status/enjoyed_sex",
                cooldown = 30,
                action_check_launch = actor => Util.IsSmart(actor) && (Util.CanHaveSexWithoutRepercussionsWithSomeoneElse(actor) || (actor.hasTrait("unfaithful") && Randy.randomChance(0.1f))),
                list_civ = true,
                weight = 0.75f
            });
            foreach (var actorAsset in AssetManager.actor_library.list)
            {
                actorAsset.addDecision("invite_for_sex");
            }
            
            AddDecision(new DecisionAsset
            {
                id = "prostitution",
                priority = NeuroLayer.Layer_4_Critical,
                path_icon = "ui/Icons/status/disliked_sex",
                cooldown = 15,
                action_check_launch = actor => Util.IsSmart(actor) && Util.CanHaveSexWithoutRepercussionsWithSomeoneElse(actor) && !actor.hasAnyCash(),
                list_civ = true,
                weight = 1f
            });
            foreach (var actorAsset in AssetManager.actor_library.list)
            {
                actorAsset.addDecision("prostitution");
            }

            var inviteForSex = new BehaviourTaskActor
            {
                id = "invite_for_sex",
                locale_key = "task_invite_for_sex",
                path_icon = "ui/Icons/status/enjoyed_sex",
            };
            inviteForSex.addBeh(new BehFindMatchingPreference(true));
            inviteForSex.addBeh(new BehGoToActorTarget(GoToActorTargetType.NearbyTileClosest, pCalibrateTargetPosition: true));
            inviteForSex.addBeh(new BehCheckNearActorTarget());
            inviteForSex.addBeh(new BehSetNextTask("have_sex_go"));
            AddBehavior(inviteForSex);

            var paymentForProstitution = 10;
            var prostitution = new BehaviourTaskActor
            {
                id = "prostitution",
                locale_key = "task_prostitution",
                path_icon = "ui/Icons/status/disliked_sex",
            };
            inviteForSex.addBeh(new BehFindMatchingPreference(false));
            inviteForSex.addBeh(new BehGoToActorTarget(GoToActorTargetType.NearbyTileClosest, pCalibrateTargetPosition: true));
            inviteForSex.addBeh(new BehCheckNearActorTarget());
            prostitution.addBeh(new BehSetNextTask("have_sex_go"));
            prostitution.addBeh(new BehRecievePaymentFromTarget(paymentForProstitution));
            AddBehavior(prostitution);
            
            var haveSexGo = new BehaviourTaskActor // our alternative of the sexual_reproduction_civ_go because it only works on lovers
            {
                id = "have_sex_go",
                locale_key = "task_have_sex_go",
                path_icon = "ui/Icons/status/enjoyed_sex",
            };
            haveSexGo.addBeh(new BehGetTargetBuildingMainTile());
            haveSexGo.addBeh(new BehGoToTileTarget());
            for (int index = 0; index < 6; ++index)
            {
                haveSexGo.addBeh(new BehRandomWait(1f, 2f));
                haveSexGo.addBeh(new BehCheckForSexTarget()); // replace with check with actor to have sex with
                haveSexGo.addBeh(new BehRandomWait(1f, 2f));
            }
            AddBehavior(haveSexGo);
            
            Finish();
        }

        private static void Finish()
        {
            foreach (var decisionAsset in _decisionAssets)
            {
                decisionAsset.priority_int_cached = (int) decisionAsset.priority;
                decisionAsset.has_weight_custom = decisionAsset.weight_calculate_custom != null;
                if (!decisionAsset.unique)
                {
                    if (decisionAsset.list_baby)
                        AssetManager.decisions_library.list_only_children = AssetManager.decisions_library.list_only_children.AddToArray(decisionAsset);
                    else if (decisionAsset.list_animal)
                        AssetManager.decisions_library.list_only_animal = AssetManager.decisions_library.list_only_animal.AddToArray(decisionAsset);
                    else if (decisionAsset.list_civ)
                        AssetManager.decisions_library.list_only_civ = AssetManager.decisions_library.list_only_civ.AddToArray(decisionAsset);
                    else
                        AssetManager.decisions_library.list_only_city = AssetManager.decisions_library.list_only_city.AddToArray(decisionAsset);
                }
            }
        }

        private static void AddBehavior(BehaviourTaskActor behaviour)
        {
            AssetManager.tasks_actor.add(behaviour);
        }

        private static void AddDecision(DecisionAsset asset)
        {
            AssetManager.decisions_library.add(asset);
            asset.decision_index = AssetManager.decisions_library.list.Count-1;
            _decisionAssets.Add(asset);
        }
    }

    public class BehRecievePaymentFromTarget : BehaviourActionActor
    {
        private int _payment;

        public BehRecievePaymentFromTarget(int payment)
        {
            _payment = payment;
        }

        public override BehResult execute(Actor pActor)
        {
            if (pActor.beh_actor_target == null) return BehResult.Stop;
            Actor actor = pActor.beh_actor_target.a;
            actor.spendMoney(_payment);
            pActor.addMoney(_payment);
            return BehResult.Continue;
        }
    }

    public class BehCheckForSexTarget : BehaviourActionActor
    {
        public override void setupErrorChecks()
        {
            base.setupErrorChecks();
            check_building_target_non_usable = true;
            null_check_building_target = true;
        }

        public override BehResult execute(Actor pActor)
        {
            if (pActor.beh_actor_target == null)
                return BehResult.Stop;
            LogService.LogInfo("Checking to see if we can have sex soon..");
            Actor sexActor = pActor.beh_actor_target.a;
            if (sexActor.isTask("have_sex_go") && sexActor.beh_building_target == pActor.beh_building_target && sexActor.ai.action_index > 3)
            {
                pActor.stayInBuilding(pActor.beh_building_target);
                sexActor.stayInBuilding(sexActor.beh_building_target);
                sexActor.setTask("sexual_reproduction_civ_wait", false, pForceAction: true);
                return forceTask(pActor, "sexual_reproduction_civ_action", false, true);
            }
            return !sexActor.isTask("have_sex_go") ? BehResult.Stop : BehResult.Continue;
        }
    }

    public class BehFindMatchingPreference : BehaviourActionActor
    {
        private bool _preferenceMustMatch;

        public BehFindMatchingPreference(bool preferenceMustMatch)
        {
            _preferenceMustMatch = preferenceMustMatch;
        }
        public override BehResult execute(Actor pActor)
        {
            Actor closestActor = GetClosestPossibleMatchingActor(pActor);
            if (closestActor == null)
                return BehResult.Stop;
            pActor.beh_actor_target = closestActor;
            return BehResult.Continue;
        }
        
        private Actor GetClosestPossibleMatchingActor(Actor pActor)
        {
            var bestFriendIsValid = pActor.hasBestFriend() && pActor.isSameIslandAs(pActor.getBestFriend()) 
                                                           && pActor.distanceToActorTile(pActor.getBestFriend()) < 50f; 
            
            using (ListPool<Actor> pCollection = new ListPool<Actor>(4))
            {
                if (bestFriendIsValid && QueerTraits.PreferenceMatches(pActor, pActor.getBestFriend(), true) &&
                    QueerTraits.PreferenceMatches(pActor.getBestFriend(), pActor, true))
                    return pActor.getBestFriend();
                
                var pRandom = Randy.randomBool();
                var pChunkRadius = Randy.randomInt(1, 4);
                var num = Randy.randomInt(5, 10);
                foreach (Actor pTarget in Finder.getUnitsFromChunk(pActor.current_tile, pChunkRadius, pRandom: pRandom))
                {
                    if (pTarget != pActor && pActor.isSameIslandAs(pTarget) && QueerTraits.PreferenceMatches(pActor, pTarget, true) && QueerTraits.PreferenceMatches(pTarget, pActor, true))
                    {
                        pCollection.Add(pTarget);
                        if (((ICollection) pCollection).Count >= num)
                            break;
                    }
                }

                if (((ICollection) pCollection).Count <= 0 && !_preferenceMustMatch)
                {
                    if (bestFriendIsValid && QueerTraits.PreferenceMatches(pActor.getBestFriend(), pActor, true))
                        return pActor.getBestFriend();
                    foreach (Actor pTarget in Finder.getUnitsFromChunk(pActor.current_tile, pChunkRadius, pRandom: pRandom))
                    {
                        if (pTarget != pActor && pActor.isSameIslandAs(pTarget) && QueerTraits.PreferenceMatches(pTarget, pActor, true))
                        {
                            pCollection.Add(pTarget);
                            if (((ICollection) pCollection).Count >= num)
                                break;
                        }
                    }

                }
                
                return Toolbox.getClosestActor(pCollection, pActor.current_tile);
            }
        }
    }

    public class BehFindMismatchedOrientation : BehaviourActionActor
    {
        public override BehResult execute(Actor pActor)
        {
            Actor closestActorWithMismatchedOrientation = GetClosestActorMismatchOrientation(pActor);
            if (closestActorWithMismatchedOrientation == null)
                return BehResult.Stop;
            pActor.beh_actor_target = closestActorWithMismatchedOrientation;
            return BehResult.Continue;
        }
        private static Actor GetClosestActorMismatchOrientation(Actor pActor)
        {
            using (ListPool<Actor> pCollection = new ListPool<Actor>(4))
            {
                var unfitPreferences = new List<Preference>();
                if (pActor.hasCultureTrait("homophobic"))
                {
                    unfitPreferences.Add(Preference.All);
                    unfitPreferences.Add(Preference.SameSex);
                    unfitPreferences.Add(Preference.SameOrDifferentSex);
                }
                if (pActor.hasCultureTrait("heterophobic"))
                {
                    unfitPreferences.Add(Preference.DifferentSex);
                }
                
                var pActorTraits = QueerTraits.GetQueerTraits(pActor, true);
                if (pActorTraits.Count < 2) return null;

                if (unfitPreferences.Contains(pActorTraits[0].preference) ||
                    unfitPreferences.Contains(pActorTraits[1].preference)) return null;
                // don't insult someone else
                
                var pRandom = Randy.randomBool();
                var pChunkRadius = Randy.randomInt(1, 4);
                var num = Randy.randomInt(1, 4);
                foreach (Actor pTarget in Finder.getUnitsFromChunk(pActor.current_tile, pChunkRadius, pRandom: pRandom))
                {
                    if (pTarget != pActor && pActor.isSameIslandAs(pTarget))
                    {
                        var queerTraits = QueerTraits.GetQueerTraits(pTarget, true);
                        if (queerTraits.Count < 2) continue;
                        var romanticPreference = queerTraits[1].preference;
                        var sexualPreference = queerTraits[0].preference;

                        if (unfitPreferences.Contains(romanticPreference) || unfitPreferences.Contains(sexualPreference))
                        {
                            pCollection.Add(pTarget);
                        }
                        if (((ICollection) pCollection).Count >= num)
                            break;
                    }
                }
                
                return Toolbox.getClosestActor(pCollection, pActor.current_tile);
            }
        }
    }
    public class BehInsultOrientation : BehaviourActionActor
    {
        public override BehResult execute(Actor pActor)
        {
            var target = pActor.beh_actor_target?.a;
            if (target == null || Toolbox.DistTile(target.current_tile, pActor.current_tile) > 4.0)
                return BehResult.Stop;
            target.changeHappiness("insulted_for_orientation");

            if (Randy.randomChance(0.25f))
            {
                pActor.addAggro(target);
                pActor.startFightingWith(target);
            }
            else if (Randy.randomChance(0.8f))
            {
                target.addStatusEffect("crying");
            } else if (Randy.randomChance(0.8f))
            {
                target.addAggro(pActor);
                target.startFightingWith(pActor);
            }

            return BehResult.Stop;
        }
    }
}