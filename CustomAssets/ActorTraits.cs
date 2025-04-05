namespace Better_Loving
{
    public class ActorTraits
    {
        public static void Init()
        {
            Add(new ActorTrait
            {
                id="faithful",
                group_id = "mind",
                rate_birth = 2,
                rate_acquire_grow_up = 4,
                type = TraitType.Positive,
                unlocked_with_achievement = false,
                rarity = Rarity.R1_Rare,
                needs_to_be_explored = true,
                affects_mind = true,
            });
            
            Add(new ActorTrait
            {
                id="unfaithful",
                group_id = "mind",
                rate_birth = 2,
                rate_acquire_grow_up = 4,
                type = TraitType.Negative,
                unlocked_with_achievement = false,
                rarity = Rarity.R1_Rare,
                needs_to_be_explored = true,
                affects_mind = true,
            });
        }

        private static void Add(ActorTrait trait)
        {
            trait.path_icon = "ui/Icons/actor_traits/"+trait.id;
            Util.AddActorTrait(trait);
        }
    }
}