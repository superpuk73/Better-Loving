using System.Collections.Generic;
using NeoModLoader.services;

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
            int limit = (int)pActor.subspecies.base_stats_meta["limit_population"];
            return pActor.subspecies.countCurrentFamilies() <= 3
                   || (limit != 0 ? pActor.subspecies.countUnits() <= limit / 3 : pActor.subspecies.countUnits() <= 50);
        }

        public static bool CanReproduce(Actor pActor, Actor pTarget)
        {
            return pActor.subspecies.isPartnerSuitableForReproduction(pActor, pTarget)
                   || pTarget.subspecies.hasTraitReproductionSexualHermaphroditic()
                   || pTarget.subspecies.hasTraitReproductionSexualHermaphroditic();
        }

        // handles the actors' happiness and mood towards their sex depending on their preferences
        // handle cheating here too
        public static void JustHadSex(Actor actor1, Actor actor2)
        {
            LogService.LogInfo("just had sex");
            if (actor1.hasSubspeciesTrait("amygdala"))
            {
                if (QueerTraits.PreferenceMatches(actor1, actor2, true))
                    actor1.addStatusEffect("enjoyed_sex");
                else
                    actor1.addStatusEffect("disliked_sex");   
            }

            if (actor2.hasSubspeciesTrait("amygdala"))
            {
                if (QueerTraits.PreferenceMatches(actor2, actor1, true))
                    actor2.addStatusEffect("enjoyed_sex");
                else
                    actor2.addStatusEffect("disliked_sex");   
            }

            if (!CanHaveSexWithoutRepercussionsWithSomeoneElse(actor1))
            {
                PotentiallyCheatedWith(actor1, actor2);
            }

            if (!CanHaveSexWithoutRepercussionsWithSomeoneElse(actor2))
            {
                PotentiallyCheatedWith(actor2, actor1);
            }
        }

        public static bool CanHaveSexWithoutRepercussionsWithSomeoneElse(Actor actor)
        {
            return !actor.hasLover() || (actor.hasLover()
                                         && !QueerTraits.PreferenceMatches(actor, actor.lover, true)
                                         && ((actor.hasCultureTrait("sexual_expectations") &&
                                              actor.lover.hasCultureTrait("sexual_expectations"))
                                             || (actor.hasSubspeciesTrait("preservation") && IsDyingOut(actor) && !CanMakeBabies(actor.lover))));
        }

        public static void PotentiallyCheatedWith(Actor actor, Actor actor2)
        {
            if (actor.hasLover() && actor.lover != actor2)
            {
                var cheatedActor = actor.lover;
                cheatedActor.setLover(null);
                cheatedActor.changeHappiness("cheated_on");
            }
        }

        public static void BreakUp(Actor actor)
        {
            actor.lover.setLover(null);
            actor.lover.changeHappiness("breakup");
            actor.setLover(null);
            actor.changeHappiness("breakup");
        }

        public static bool CanMakeBabies(Actor pActor)
        {
            // make it configurable so they have to be adults or not
            return pActor.isBreedingAge()
                   && !pActor.hasReachedOffspringLimit()
                   && (!pActor.hasCity() || !pActor.city.hasReachedWorldLawLimit() &&
                       (pActor.current_children_count == 0 || pActor.city.hasFreeHouseSlots()))
                   && pActor.haveNutritionForNewBaby() && !pActor.hasStatus("pregnant");
        }
    }
}