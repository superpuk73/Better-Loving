﻿using System.Collections.Generic;

namespace Topic_of_Love.Mian.CustomAssets.Traits
{
    public class ActorTraits : BaseTraits<ActorTrait, ActorTraitLibrary>
    {
        public void Init()
        {
            Init("actor", false);
            
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
            
            Add(new ActorTrait
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
            }, List.Of("civ_dog")).addOpposite("unfaithful");

            Add(new ActorTrait
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
            }).addOpposite("faithful");

            Add(new ActorTrait
            {
                id = "cheated",
                group_id = "acquired",
                rate_birth = 2,
                rate_acquire_grow_up = 4,
                type = TraitType.Negative,
                unlocked_with_achievement = false,
                rarity = Rarity.R1_Rare,
                needs_to_be_explored = true,
                affects_mind = true,
            });

            Add(new ActorTrait
            {
                id = "committed_incest",
                group_id = "acquired",
                rate_birth = 2,
                rate_acquire_grow_up = 4,
                type = TraitType.Negative,
                unlocked_with_achievement = false,
                rarity = Rarity.R1_Rare,
                needs_to_be_explored = true,
                affects_mind = true,
            });

            Finish();
        }

        protected override void Finish()
        {
            foreach (var trait in _assets)
            {
                for (int index = 0; index < trait.rate_birth; ++index)
                    AssetManager.traits.pot_traits_birth.Add(trait);
                for (int index = 0; index < trait.rate_acquire_grow_up; ++index)
                    AssetManager.traits.pot_traits_growup.Add(trait);
                if (trait.combat)
                    AssetManager.traits.pot_traits_combat.Add(trait);   
            }
            base.Finish();
        }
    }
}