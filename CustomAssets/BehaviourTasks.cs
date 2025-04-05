using System.Collections.Generic;
using ai.behaviours;
using UnityEngine;

namespace Better_Loving
{
    public class BehaviourTasks
    {
        public static void Init()
        {
            BehaviourTaskActor insultOrientation = new BehaviourTaskActor
            {
                id = "insult_orientation",
                locale_key = "task_insult_orientation",
                path_icon = "ui/Icons/culture_traits/orientationless",
                
            };
            insultOrientation.addBeh(new BehGoToActorTarget(GoToActorTargetType.NearbyTileClosest, pCalibrateTargetPosition: true));
            insultOrientation.addBeh(new BehCheckNearActorTarget());
            insultOrientation.addBeh(new BehInsultOrientation());
            Add(insultOrientation);
            
            foreach (var actorAsset in AssetManager.actor_library.list)
            {
                actorAsset.addDecision("insult_orientation");
            }
        }

        private static void Add(BehaviourTaskActor behaviour)
        {
            AssetManager.tasks_actor.add(behaviour);
        }
    }

    public class BehInsultOrientation : BehaviourActionActor
    {
        public override BehResult execute(Actor pActor)
        {
            var a = pActor.beh_actor_target?.a;
            if (a == null || !StillCanTalk(a) || (!pActor.hasTelepathicLink() || !a.hasTelepathicLink()) && (double) Toolbox.DistTile(a.current_tile, pActor.current_tile) > 4.0)
                return BehResult.Stop;
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
            var queerTraits = QueerTraits.GetQueerTraits(a, true);
            var romanticPreference = queerTraits[1].preference;
            var sexualPreference = queerTraits[0].preference;

            if (unfitPreferences.Contains(romanticPreference) || unfitPreferences.Contains(sexualPreference))
            {
                a.changeHappiness("insulted_for_orientation");
            }
            
            return BehResult.Stop;
        }
        
        private static bool StillCanTalk(Actor pTarget) => pTarget.isAlive() && !pTarget.isLying();
    }
}