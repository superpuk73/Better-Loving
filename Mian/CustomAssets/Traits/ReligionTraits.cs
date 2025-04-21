
using System.Collections.Generic;

namespace Topic_of_Love.Mian.CustomAssets.Traits
{
    public class ReligionTraits : BaseTraits<ReligionTrait, ReligionTraitLibrary>
    {
        public void Init()
        {
            Init("religion");
            
            Add(new ReligionTrait
            {
                id = "homomisos",
                group_id = "civic_position",
                needs_to_be_explored = true,
                rarity = Rarity.R2_Epic,
                can_be_in_book = true,
                can_be_removed = true,
                can_be_given = true
            }, List.Of("human", "orc", "demon", "evil_mage", "bandit", "unicorn", "scorpion", "civ_scorpion"), List.Of("biome_infernal", "biome_arcane_desert"))
                .addOpposites(List.Of("heteromisos", "unneutrumomisos"));

            Add(new ReligionTrait
            {
                id = "heteromisos",
                group_id = "civic_position",
                needs_to_be_explored = true,
                rarity = Rarity.R2_Epic,
                can_be_in_book = true,
                can_be_removed = true,
                can_be_given = true
            }, List.Of("garl", "garlic_man", "flower_bud"), List.Of("biome_flower", "biome_garlic"))
                .addOpposites(List.Of("homomisos", "unneutrumomisos"));

            Add(new ReligionTrait
            {
                id = "unneutrumomisos",
                group_id = "civic_position",
                needs_to_be_explored = true,
                rarity = Rarity.R2_Epic,
                can_be_in_book = true,
                can_be_removed = true,
                can_be_given = true
            }, List.Of("lemon_snail", "lemon_man"), List.Of("biome_lemon"))
                .addOpposites(List.Of("heteromisos", "homomisos"));

            Add(new ReligionTrait
            {
                id = "lust_punishment",
                group_id = "civic_position",
                needs_to_be_explored = true,
                rarity = Rarity.R2_Epic,
                can_be_in_book = true,
                can_be_removed = true,
                can_be_given = true
            }, List.Of("human", "elf", "druid", "unicorn", "flower_bud", "civ_dog", "civ_sheep", "white_mage", "plague_doctor", "civ_wolf", "civ_scorpion"), List.Of("biome_celestial", "biome_flower")).addOpposite("lust_recognition");

            Add(new ReligionTrait
            {
                id = "lust_recognition",
                group_id = "civic_position",
                needs_to_be_explored = true,
                rarity = Rarity.R2_Epic,
                can_be_in_book = true,
                can_be_removed = true,
                can_be_given = true
            }, List.Of("orc", "demon", "necromancer", "evil_mage", "civ_rabbit", "rabbit", "rat", "civ_rat", "civ_hyena"), List.Of("biome_infernal", "biome_corrupted")).addOpposite("lust_punishment");

            Finish();
        }
    }
}