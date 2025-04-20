using System.Collections.Generic;

namespace Topic_of_Love.Mian.CustomAssets.Traits
{
    public class ClanTraits : BaseTraits<ClanTrait, ClanTraitLibrary>
    {
        public void Init()
        {
            Init("clan", false);
            
            Add(new ClanTrait
            {
                id = "clanbound_isolation",
                group_id = "special",
                unlocked_with_achievement = false,
                rarity = Rarity.R2_Epic,
                needs_to_be_explored = true,
                spawn_random_trait_allowed = true,
            }, List.Of("human", "civ_wolf", "necromancer", "lemon_snail", "lemon_man", "garl", "garlic_man", "plague_doctor"), List.Of("biome_lemon", "biome_garlic"));

            Finish();
        }

        protected override void Finish()
        {
            base.Finish();
        }
    }
}