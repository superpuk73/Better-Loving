using System;
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
            duration = 60f,
            action_on_receive = (actor, _) =>
            {
                actor.a.data.get("sexual_happiness", out float happiness);
                actor.a.data.get("last_had_sex_with", out long partnerID);
                var changeBy = 0f;
                
                var sexPartner = MapBox.instance.units.get(partnerID);
                if (sexPartner != null && sexPartner == actor.a.lover)
                    changeBy += 30f;
                
                if (happiness <= 0)
                {
                    changeBy += Math.Abs(happiness / 2);
                }
                
                Util.ChangeSexualHappinessBy(actor.a, changeBy);
                
                actor.a.changeHappiness("enjoyed_sex");
                actor.a.finishStatusEffect("disliked_sex");
                actor.a.finishStatusEffect("okay_sex");
                return true;
            }
        });
        Add(new StatusAsset
        {
            id="disliked_sex",
            duration = 60f,
            action_on_receive = (actor, _) => 
            {
                actor.a.data.get("sexual_happiness", out float happiness);
                var changeBy = -20f;
                
                if (happiness <= 0)
                {
                    changeBy += happiness / 3; // become more deprived if the sex was bad
                }

                Util.ChangeSexualHappinessBy(actor.a, changeBy);
                actor.a.changeHappiness("disliked_sex");
                actor.a.finishStatusEffect("enjoyed_sex");
                actor.a.finishStatusEffect("okay_sex");
                return true;
            }
        });
        Add(new StatusAsset
        {
            id="okay_sex",
            duration = 60f,
            action_on_receive = (actor, _) =>
            {
                actor.a.data.get("last_had_sex_with", out long partnerID);
                var changeBy = 10f;
                
                var sexPartner = MapBox.instance.units.get(partnerID);
                if (sexPartner != null && sexPartner == actor.a.lover)
                    changeBy += 10f; // okay sex but add extra if with lover at least
                
                Util.ChangeSexualHappinessBy(actor.a, changeBy);
                actor.a.changeHappiness("okay_sex");
                actor.a.finishStatusEffect("enjoyed_sex");
                actor.a.finishStatusEffect("disliked_sex");
                return true;
            }
        });
        Add(new StatusAsset
        {
            id="cheated_on",
            duration = 60f,
            action_on_receive = (cheatedActor, _) =>
            {
                if (Randy.randomChance(0.5f))
                {
                    cheatedActor.a.addStatusEffect("crying");
                } else
                {
                    cheatedActor.a.addAggro(cheatedActor.a.lover);
                    cheatedActor.a.lover.data.get("last_had_sex_with", out long id);
                    var otherActorInvolved = MapBox.instance.units.get(id);
                    if (otherActorInvolved != null)
                    {
                        cheatedActor.a.addAggro(otherActorInvolved);
                        if(otherActorInvolved.isOnSameIsland(cheatedActor.a))
                            cheatedActor.a.startFightingWith(otherActorInvolved);
                    }
                    else
                    {
                        if(cheatedActor.a.lover.isOnSameIsland(cheatedActor.a))
                            cheatedActor.a.startFightingWith(cheatedActor.a.lover);
                    }
                }
                cheatedActor.a.changeHappiness("cheated_on");
                cheatedActor.a.data.set("cheated_" +cheatedActor.a.lover.getID(),true);
                return true;
            }
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
        asset.locale_id = "status_title_" + asset.id;
        asset.locale_description = "status_description_" + asset.id;
        asset.path_icon = "ui/Icons/status/" + asset.id;
        _assets.Add(asset);
    }
}