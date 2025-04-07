using System.Collections.Generic;
using UnityEngine;

namespace Better_Loving;

public class StatusEffects
{
    private static List<StatusAsset> _assets = new List<StatusAsset>();
    
    public static void Init()
    {
        Add(new StatusAsset
        {
            id="enjoyed_sex",
            locale_id = "status_title_enjoyed_sex",
            locale_description = "status_description_enjoyed_sex",
            duration = 60f,
            path_icon = "ui/Icons/status/enjoyed_sex",
            action_on_receive = (actor, _) => actor.a.changeHappiness("enjoyed_sex")
        });
        Add(new StatusAsset
        {
            id="disliked_sex",
            locale_id = "status_title_disliked_sex",
            locale_description = "status_description_disliked_sex",
            duration = 60f,
            path_icon = "ui/Icons/status/disliked_sex",
            action_on_receive = (actor, _) => actor.a.changeHappiness("disliked_sex")
        });
        Finish();
    }

    private static void Finish()
    {
        foreach (var statusAsset in _assets)
        {
            if (statusAsset.get_override_sprite != null)
            {
                statusAsset.has_override_sprite = true;
                statusAsset.need_visual_render = true;
            }
            if (statusAsset.get_override_sprite_position != null)
                statusAsset.has_override_sprite_position = true;
            if (statusAsset.get_override_sprite_rotation_z != null)
                statusAsset.has_override_sprite_rotation_z = true;
            if (statusAsset.texture != null)
                statusAsset.need_visual_render = true;
            
            if (statusAsset.texture != null && statusAsset.sprite_list == null)
                statusAsset.sprite_list = Resources.LoadAll<Sprite>("effects/" + statusAsset.texture);
        }
    }
    
    private static void Add(StatusAsset asset)
    {
        AssetManager.status.add(asset);
        _assets.Add(asset);
    }
}