using HarmonyLib;

namespace Better_Loving
{
    public class Happiness
    {
        public static void Init()
        {
            // breakup
            Add(new HappinessAsset
            {
                id = "breakup",
                value = -15,
                pot_task_id = "crying",
                path_icon = "ui/Icons/iconCrying",
                ignored_by_psychopaths = true,
                pot_amount = 5,
            });
            
            // cheated on
            Add(new HappinessAsset
            {
                id = "cheated_on",
                value = -20,
                pot_task_id = "crying",
                path_icon = "ui/Icons/iconCrying",
                ignored_by_psychopaths = true,
                pot_amount = 5,
                show_change_happiness_effect = true
            });
            
            // true self
            Add(new HappinessAsset
            {
                id = "true_self",
                value = 20,
                pot_task_id = "singing",
                path_icon = "ui/Icons/iconAge",
                ignored_by_psychopaths = false,
                pot_amount = 5,
                show_change_happiness_effect = true
            });
            
            Add(new HappinessAsset
            {
                id = "orientation_fits",
                value = 5,
                pot_task_id = "singing",
                path_icon = "ui/Icons/iconAge",
                ignored_by_psychopaths = false,
                pot_amount = 5,
                show_change_happiness_effect = true,
                dialogs_amount = 2
            });
            
            Add(new HappinessAsset
            {
                id = "orientation_does_not_fit",
                value = -5,
                pot_task_id = "crying",
                path_icon = "ui/Icons/iconCrying",
                ignored_by_psychopaths = false,
                pot_amount = 5,
                show_change_happiness_effect = true,
                dialogs_amount = 2
            });
            
            Add(new HappinessAsset
            {
                id = "insulted_for_orientation",
                value = -15,
                pot_task_id = "crying",
                path_icon = "ui/Icons/iconCrying",
                ignored_by_psychopaths = false,
                pot_amount = 5,
                show_change_happiness_effect = true,
                dialogs_amount = 2
            });
            
            Add(new HappinessAsset
            {
                id = "enjoyed_sex",
                value = 15,
                path_icon = "ui/Icons/status/enjoyed_sex",
                ignored_by_psychopaths = false,
                show_change_happiness_effect = true,
                dialogs_amount = 2
            });
            
            Add(new HappinessAsset
            {
                id = "disliked_sex",
                value = -15,
                path_icon = "ui/Icons/status/disliked_sex",
                ignored_by_psychopaths = false,
                show_change_happiness_effect = true,
                dialogs_amount = 2
            });
        }

        private static void Add(HappinessAsset asset)
        {
            AssetManager.happiness_library.add(asset);
            asset.index = AssetManager.happiness_library.list.Count-1;
        }
    }
}