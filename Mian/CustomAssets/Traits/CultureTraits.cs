
using System.Collections.Generic;

namespace Topic_of_Love.Mian.CustomAssets.Traits
{
    public class CultureTraits : BaseTraits<CultureTrait, CultureTraitLibrary>
    {
        public void Init()
        {
            Init("culture");
            
            Add(new CultureTrait
            {
                id = "orientationless",
                group_id = "miscellaneous",
                needs_to_be_explored = true,
                rarity = Rarity.R1_Rare,
                can_be_in_book = true,
                can_be_removed = true,
                can_be_given = true
            }, List.Of("angle", "snowman")).addOpposites(new List<string>{"heterophobic", "homophobic"});

            Add(new CultureTrait
            {
                id = "homophobic",
                group_id = "worldview",
                needs_to_be_explored = true,
                rarity = Rarity.R1_Rare,
                can_be_in_book = true,
                can_be_removed = true,
                can_be_given = true
            }, List.Of("orc", "demon")).addOpposite("heterophobic");

            Add(new CultureTrait
            {
                id = "heterophobic",
                group_id = "worldview",
                needs_to_be_explored = true,
                rarity = Rarity.R1_Rare,
                can_be_in_book = true,
                can_be_removed = true,
                can_be_given = true
            }, List.Of("flower_bud", "garl")).addOpposite("homophobic");
            
            Add(new CultureTrait
            {
                id = "incest_taboo",
                group_id = "miscellaneous",
                needs_to_be_explored = true,
                rarity = Rarity.R1_Rare,
                can_be_in_book = true,
                can_be_removed = true,
                can_be_given = true
            }, List.Of("human", "dwarf", "elf", "druid", "civ_dog", "white_mage", "plague_doctor"), List.Of("biome_celestial", "biome_maple")).addOpposite("scar_of_incest");

            Add(new CultureTrait
            {
                id = "scar_of_incest",
                group_id = "miscellaneous",
                rarity = Rarity.R1_Rare,
                can_be_given = true,
                can_be_in_book = false,
                can_be_removed = true,
            }, List.Of("orc", "demon", "rabbit", "hyena", "civ_hyena", "rat", "civ_rat", "evil_mage", "bandit"), List.Of("biome_infernal", "biome_corrupted")).addOpposite("incest_taboo");
            
            Add(new CultureTrait
            {
                id = "committed",
                group_id = "miscellaneous",
                needs_to_be_explored = true,
                rarity = Rarity.R1_Rare,
                can_be_in_book = true,
                can_be_removed = true,
                can_be_given = true
            }, List.Of("elf", "civ_penguin"), List.Of("biome_celestial", "biome_flower"));
            
            Add(new CultureTrait
            {
                id = "mature_dating",
                group_id = "miscellaneous",
                needs_to_be_explored = true,
                rarity = Rarity.R1_Rare,
                can_be_in_book = true,
                can_be_removed = true,
                can_be_given = true
            }, List.Of("human", "elf", "dwarf"), List.Of("biome_grass", "biome_maple"));

            Add(new CultureTrait
            {
                id = "sexual_expectations",
                group_id = "miscellaneous",
                needs_to_be_explored = true,
                rarity = Rarity.R1_Rare,
                can_be_in_book = true,
                can_be_removed = true,
                can_be_given = true
            }, List.Of("elf"), List.Of("biome_maple"));
            Finish();
        }
    }
}