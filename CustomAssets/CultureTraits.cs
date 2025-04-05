using System.Collections.Generic;

namespace Better_Loving
{
    public class CultureTraits
    {
        public static void Init()
        {
            Add(new CultureTrait
            {
                id = "homophobic",
                group_id = "worldview",
                needs_to_be_explored = true,
                rarity = Rarity.R1_Rare,
                can_be_in_book = true,
                can_be_removed = true,
                can_be_given = true
            }, "orc", "demon");
            
            Add(new CultureTrait
            {
                id = "heterophobic",
                group_id = "worldview",
                needs_to_be_explored = true,
                rarity = Rarity.R1_Rare,
                can_be_in_book = true,
                can_be_removed = true,
                can_be_given = true
            }, "flower_bud", "garl");
            
            Add(new CultureTrait
            {
                id = "incest",
                group_id = "miscellaneous",
                needs_to_be_explored = true,
                rarity = Rarity.R1_Rare,
                can_be_in_book = true,
                can_be_removed = true,
                can_be_given = true
            }, "orc", "demon");
            
            Add(new CultureTrait
            {
                id = "committed",
                group_id = "miscellaneous",
                needs_to_be_explored = true,
                rarity = Rarity.R1_Rare,
                can_be_in_book = true,
                can_be_removed = true,
                can_be_given = true
            }, "elf", "coolbeak");
            
            Add(new CultureTrait
            {
                id = "mature_dating",
                group_id = "miscellaneous",
                needs_to_be_explored = true,
                rarity = Rarity.R1_Rare,
                can_be_in_book = true,
                can_be_removed = true,
                can_be_given = true
            }, "human", "elf", "dwarf");
            
            Add(new CultureTrait
            {
                id = "orientationless",
                group_id = "miscellaneous",
                needs_to_be_explored = true,
                rarity = Rarity.R1_Rare,
                can_be_in_book = true,
                can_be_removed = true,
                can_be_given = true
            }, "angle", "snowman");
        }

        private static void Add(CultureTrait trait, params string[] assets)
        {
            trait.path_icon = "ui/Icons/culture_traits/" + trait.id;
            AssetManager.culture_traits.add(trait);
            foreach (var asset in assets)
            {
                var actorAsset = AssetManager.actor_library.get(asset);
                if(actorAsset != null)
                    actorAsset.addCultureTrait(trait.id);
            }
        }
    }
}