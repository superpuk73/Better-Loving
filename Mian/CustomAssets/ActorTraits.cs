using System.Collections.Generic;

namespace Better_Loving.Mian.CustomAssets
{
    public class ActorTraits
    {
        public static void Init()
        {
            var faithful = new ActorTrait
            {
                id = "faithful",
                group_id = "mind",
                rate_birth = 2,
                rate_acquire_grow_up = 4,
                type = TraitType.Positive,
                unlocked_with_achievement = false,
                rarity = Rarity.R1_Rare,
                needs_to_be_explored = true,
                affects_mind = true,
                opposite_traits = new HashSet<ActorTrait>()
            };

            var unfaithful = new ActorTrait
            {
                id = "unfaithful",
                group_id = "mind",
                rate_birth = 2,
                rate_acquire_grow_up = 4,
                type = TraitType.Negative,
                unlocked_with_achievement = false,
                rarity = Rarity.R1_Rare,
                needs_to_be_explored = true,
                affects_mind = true,
                opposite_traits = new HashSet<ActorTrait>()
            };
            faithful.opposite_traits.Add(unfaithful);
            unfaithful.opposite_traits.Add(faithful);
            
            Add(new ActorTrait
            {
                id = "sex_indifferent",
                group_id = "mind",
                rate_birth = 2,
                rate_acquire_grow_up = 4,
                type = TraitType.Other,
                unlocked_with_achievement = false,
                rarity = Rarity.R1_Rare,
                needs_to_be_explored = true,
                affects_mind = true
            });
            
            Add(faithful);
            Add(unfaithful);
        }

        private static void Add(ActorTrait trait)
        {
            trait.path_icon = "ui/Icons/actor_traits/"+trait.id;
            Util.AddActorTrait(trait);
        }
    }
}