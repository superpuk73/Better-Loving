using System.Collections.Generic;

namespace Better_Loving
{
    public class SubspeciesTraits
    {
        private static List<SubspeciesTrait> traits = new List<SubspeciesTrait>();
        public static void Init()
        {
            Add(new SubspeciesTrait
            {
                id="preservation",
                group_id = "mind",
                rarity = Rarity.R0_Normal,
                in_mutation_pool_add = true,
                in_mutation_pool_remove = true,
                remove_for_zombies = true
            });
            // preservation is a mustttttttt for preventing population collapse
            foreach (var actorAsset in AssetManager.actor_library.list)
            {
                actorAsset.addSubspeciesTrait("preservation");
            }
            
            Finish();
        }

        public static void Finish()
        {
            foreach (SubspeciesTrait pObject in traits)
            {
                if (pObject.in_mutation_pool_add)
                    AssetManager.subspecies_traits._pool_mutation_traits_add.AddTimes(pObject.rarity.GetRate(), pObject);
                if (pObject.in_mutation_pool_remove)
                    AssetManager.subspecies_traits._pool_mutation_traits_remove.AddTimes(pObject.rarity.GetRate(), pObject);
                if (pObject.phenotype_egg && pObject.after_hatch_from_egg_action != null)
                    pObject.has_after_hatch_from_egg_action = true;
            }
        }
        
        private static void Add(SubspeciesTrait trait, params string[] assets)
        {
            trait.path_icon = "ui/Icons/subspecies_traits/" + trait.id;
            AssetManager.subspecies_traits.add(trait);
            foreach (var asset in assets)
            {
                var actorAsset = AssetManager.actor_library.get(asset);
                if(actorAsset != null)
                    actorAsset.addSubspeciesTrait(trait.id);
            }
            traits.Add(trait);
        }
    }
}