using System.Collections;
using System.Collections.Generic;
using ai.behaviours;
using Topic_of_Love.Mian.CustomAssets.Traits;

namespace Topic_of_Love.Mian.CustomAssets.AI.CustomBehaviors;
public class BehFindLustCommitter : BehaviourActionActor
    {
        public override BehResult execute(Actor pActor)
        {
            Actor closestActorLustComitter = GetClosestActorLustCommitter(pActor);
            if (closestActorLustComitter == null)
                return BehResult.Stop;
            pActor.beh_actor_target = closestActorLustComitter;
            return BehResult.Continue;
        }
        private static Actor GetClosestActorLustCommitter(Actor pActor)
        {
            if (!pActor.hasReligion())
                return null;

            using (ListPool<Actor> pCollection = new ListPool<Actor>(4))
            {
                var pRandom = Randy.randomBool();
                var pChunkRadius = Randy.randomInt(1, 2);
                var num = Randy.randomInt(1, 4);
                foreach (Actor pTarget in Finder.getUnitsFromChunk(pActor.current_tile, pChunkRadius, pRandom: pRandom))
                {
                    if (pTarget != pActor && pActor.isSameIslandAs(pTarget))
                    {
                        if (pTarget.hasTrait("cheated") || pTarget.hasTrait("committed_incest"))
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