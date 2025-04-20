using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Topic_of_Love.Mian.CustomAssets.TraitGroups
{
    public class CultureTraitGroups
    {
        public void Init()
        {
            Add(new CultureTraitGroupAsset()
            {
                id = "parenthood",
                name = "trait_group_parenthood",
                color = "#33A9AB",
            });
        }

        private void Add(CultureTraitGroupAsset group)
        {
            AssetManager.culture_trait_groups.add(group);
        }
    }
}
