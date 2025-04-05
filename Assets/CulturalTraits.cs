namespace Better_Loving
{
    public class CulturalTraits
    {
        public static void Init()
        {
            Add(new CultureTrait
            {
                id = "homophobic",
                path_icon = "ui/Icons/actor_traits/homosexual", // temp
                group_id = "worldview",
                needs_to_be_explored = true,
                rarity = Rarity.R3_Legendary,
                can_be_in_book = true,
                can_be_removed = true,
                can_be_given = true
            });
            AssetManager.actor_library.get("demon").addCultureTrait("homophobic");
        }

        private static void Add(CultureTrait trait)
        {
            AssetManager.culture_traits.add(trait);
        }
    }
}