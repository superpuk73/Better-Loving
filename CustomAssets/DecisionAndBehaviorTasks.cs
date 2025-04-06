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
                weight = 0.5f,
                list_civ = true
            });
            AssetManager.subspecies_traits.get("wernicke_area").addDecision("insult_orientation_try");
            
            Finish();
        }

        public static void Finish()
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
        public Actor GetClosestActorMismatchOrientation(Actor pActor)
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
                
                bool pRandom = Randy.randomBool();
                int pChunkRadius = Randy.randomInt(1, 4);
                int num = Randy.randomInt(1, 4);
                foreach (Actor pTarget in Finder.getUnitsFromChunk(pActor.current_tile, pChunkRadius, pRandom: pRandom))
                {
                    if (pTarget != pActor && pActor.isSameIslandAs(pTarget) && pTarget.hasAnyCash())
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

    // Let's go one step further and allow the aggressor to attack ppl for their orientation or for the aggressed to fight back or cry
    public class BehInsultOrientation : BehaviourActionActor
    {
        public override BehResult execute(Actor pActor)
        {
            var target = pActor.beh_actor_target?.a;
            // LogService.LogInfo("LETS GO INSULT SOMEONE");
            if (target == null || Toolbox.DistTile(target.current_tile, pActor.current_tile) > 4.0)
                return BehResult.Stop;
            
            target.changeHappiness("insulted_for_orientation");
            LogService.LogInfo("INSULTED!");

            return BehResult.Stop;
        }
    }
}