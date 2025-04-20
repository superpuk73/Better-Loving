﻿using System.Collections;
using System.Collections.Generic;
using ai.behaviours;
using Topic_of_Love.Mian.CustomAssets.Traits;

namespace Topic_of_Love.Mian.CustomAssets.AI.CustomBehaviors;
public class BehFindSinfulOrientation : BehaviourActionActor
    {
        public override BehResult execute(Actor pActor)
        {
            Actor closestActorWithSinfulOrientation = GetClosestActorSinfulOrientation(pActor);
            if (closestActorWithSinfulOrientation == null)
                return BehResult.Stop;
            pActor.beh_actor_target = closestActorWithSinfulOrientation;
            return BehResult.Continue;
        }
        private static Actor GetClosestActorSinfulOrientation(Actor pActor)
        {
            using (ListPool<Actor> pCollection = new ListPool<Actor>(4))
            {
                var unfitPreferences = new List<Preference>();
                if (!pActor.hasReligion())
                    return null;
                if (pActor.religion.hasTrait("homomisos"))
                {
                    unfitPreferences.Add(Preference.All);
                    unfitPreferences.Add(Preference.SameSex);
                    unfitPreferences.Add(Preference.SameOrDifferentSex);
                    unfitPreferences.Add(Preference.Neither);
                }
                if (pActor.religion.hasTrait("heteromisos"))
                {
                    unfitPreferences.Add(Preference.DifferentSex);
                    unfitPreferences.Add(Preference.Neither);
                }
                if (pActor.religion.hasTrait("unneutrumomisos"))
                {
                    unfitPreferences.Add(Preference.All);
                    unfitPreferences.Add(Preference.SameSex);
                    unfitPreferences.Add(Preference.SameOrDifferentSex);
                    unfitPreferences.Add(Preference.DifferentSex);
                }

                var pActorTraits = QueerTraits.GetQueerTraits(pActor, true);
                if (pActorTraits.Count < 2) return null;

                if (unfitPreferences.Contains(pActorTraits[0].preference) ||
                    unfitPreferences.Contains(pActorTraits[1].preference)) return null;
                // don't insult someone else
                
                var pRandom = Randy.randomBool();
                var pChunkRadius = Randy.randomInt(1, 2);
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