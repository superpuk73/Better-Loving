using System.Collections.Generic;

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

        public static void AddActorTrait(ActorTrait trait)
        {
            for (int index = 0; index < trait.rate_birth; ++index)
                AssetManager.traits.pot_traits_birth.Add(trait);
            for (int index = 0; index < trait.rate_acquire_grow_up; ++index)
                AssetManager.traits.pot_traits_growup.Add(trait);
            if (trait.combat)
                AssetManager.traits.pot_traits_combat.Add(trait);
            
            AssetManager.traits.add(trait);
        }
        
        public static bool IsDyingOut(Actor pActor)
        {
            if (!pActor.hasSubspecies()) return false;
            int limit = (int) pActor.subspecies.base_stats_meta["limit_population"];
            return limit != 0 ? pActor.subspecies.countUnits() <= limit / 3 : pActor.subspecies.countUnits() <= 50;
        }

        public static bool CanReproduce(Actor pActor, Actor pTarget)
        {
            return pActor.subspecies.isPartnerSuitableForReproduction(pActor, pTarget)
                   || pTarget.subspecies.hasTraitReproductionSexualHermaphroditic()
                   || pTarget.subspecies.hasTraitReproductionSexualHermaphroditic();
        }
    }
}