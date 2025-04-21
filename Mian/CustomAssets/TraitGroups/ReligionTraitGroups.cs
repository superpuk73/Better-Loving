using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Topic_of_Love.Mian.CustomAssets.TraitGroups
{
    public class ReligionTraitGroups
    {
        public void Init()
        {
            Add(new ReligionTraitGroupAsset()
            {
                id = "civic_position",
                name = "trait_group_civic_position",
                color = "#24ED63",
            });
        }

        private void Add(ReligionTraitGroupAsset group)
        {
            AssetManager.religion_trait_groups.add(group);
        }
    }
}
