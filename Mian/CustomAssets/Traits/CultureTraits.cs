
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

            AddParenthoodRelatedTraits();

            Finish();
        }

        private void AddParenthoodRelatedTraits()
        {
            Add(new CultureTrait
            {
                id = "comfort_demand",
                group_id = "parenthood",
                needs_to_be_explored = true,
                rarity = Rarity.R2_Epic,
                can_be_in_book = true,
                can_be_removed = true,
                can_be_given = true
            }, List.Of("elf", "civ_wolf", "human", "dwarf", "civ_goat", "goat", "civ_cat", "civ_rhino", "ostrich", "seal", "druid", "plague_doctor", "white_mage"), List.Of("biome_maple", "biome_grass"));
            Finish();

            Add(new CultureTrait
            {
                id = "wealth_demand",
                group_id = "parenthood",
                needs_to_be_explored = true,
                rarity = Rarity.R2_Epic,
                can_be_in_book = true,
                can_be_removed = true,
                can_be_given = true
            }, List.Of("orc", "dwarf", "human", "civ_sheep", "bandit", "civ_scorpion", "civ_dog"), List.Of("biome_arcane_desert"));

            Add(new CultureTrait
            {
                id = "kills_demand",
                group_id = "parenthood",
                needs_to_be_explored = true,
                rarity = Rarity.R3_Legendary,
                can_be_in_book = true,
                can_be_removed = true,
                can_be_given = true
            }, List.Of("orc", "bandit", "demon", "civ_scorpion", "candy_man", "civ_wolf"), List.Of("biome_arcane_desert", "biome_candy"));

            Add(new CultureTrait
            {
                id = "deed_demand",
                group_id = "parenthood",
                needs_to_be_explored = true,
                rarity = Rarity.R2_Epic,
                can_be_in_book = true,
                can_be_removed = true,
                can_be_given = true
            }, List.Of("lemon_man", "garlic_man"), List.Of("biome_lemon", "biome_garlic"))
                .addOpposites(List.Of("kills_demand", "wealth_demand", "comfort_demand"));

            Add(new CultureTrait
            {
                id = "chaos_age_encouragement",
                group_id = "parenthood",
                needs_to_be_explored = true,
                rarity = Rarity.R3_Legendary,
                can_be_in_book = true,
                can_be_removed = true,
                can_be_given = true
            }, List.Of("demon", "evil_mage", "fire_elemental", "fire_skull"), List.Of("biome_infernal"))
                .addOpposites(List.Of("moon_age_encouragement", "despair_age_encouragement", "wonder_age_encouragement"));

            Add(new CultureTrait
            {
                id = "despair_age_encouragement",
                group_id = "parenthood",
                needs_to_be_explored = true,
                rarity = Rarity.R3_Legendary,
                can_be_in_book = true,
                can_be_removed = true,
                can_be_given = true
            }, List.Of("greg"), List.Of("biome_singularity"))
                .addOpposites(List.Of("moon_age_encouragement", "chaos_age_encouragement", "wonder_age_encouragement"));

            Add(new CultureTrait
            {
                id = "moon_age_encouragement",
                group_id = "parenthood",
                needs_to_be_explored = true,
                rarity = Rarity.R3_Legendary,
                can_be_in_book = true,
                can_be_removed = true,
                can_be_given = true
            }, List.Of("civ_wolf", "wolf"), List.Of("biome_clover", "biome_grass"))
                .addOpposites(List.Of("despair_age_encouragement", "chaos_age_encouragement", "wonder_age_encouragement"));

            Add(new CultureTrait
            {
                id = "wonder_age_encouragement",
                group_id = "parenthood",
                needs_to_be_explored = true,
                rarity = Rarity.R3_Legendary,
                can_be_in_book = true,
                can_be_removed = true,
                can_be_given = true
            }, List.Of("flower_bud", "civ_buffalo", "buffalo", "sheep", "civ_sheep", "cow", "civ_cow", "dog", "civ_dog"), List.Of("biome_flower", "biome_grass", "biome_celestial"))
                .addOpposites(List.Of("despair_age_encouragement", "chaos_age_encouragement", "moon_age_encouragement"));
        }
    }
}