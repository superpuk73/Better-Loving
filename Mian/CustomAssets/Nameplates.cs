using System.Numerics;

namespace Topic_of_Love.Mian.CustomAssets;

public class Nameplates
{
    public static void Init()
    {
        var unit = new NameplateAsset
        {
            id = "plate_unit",
            path_sprite = "ui/nameplates/nameplate_subspecies",
            padding_left = 11,
            padding_right = 13,
            map_mode = MetaType.Unit,
            action_main = (manager, asset) =>
            {
                foreach (var subspecies in World.world.subspecies)
                {
                    var oldestVisibleUnit = subspecies.getOldestVisibleUnit();
                    if (oldestVisibleUnit != null)
                    {
                        var nameplateText = manager.prepareNext(asset);
                        var pPosition = oldestVisibleUnit.current_position;
                        pPosition.y += oldestVisibleUnit.getHeight();
                        pPosition.y -= 2f;
                        nameplateText.showTextSubspecies(subspecies, pPosition);
                    }
                }
            }
        };
        Add(unit);
        // AssetManager.nameplates_library.map_modes_nameplates.Add(MetaType.Unit, unit);
    }

    private static void Add(NameplateAsset nameplateAsset)
    {
        AssetManager.nameplates_library.add(nameplateAsset); 
    }
}