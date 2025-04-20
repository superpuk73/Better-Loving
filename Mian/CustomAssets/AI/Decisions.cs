﻿using System.Collections.Generic;
using Topic_of_Love.Mian.CustomAssets.Traits;
using HarmonyLib;

namespace Topic_of_Love.Mian.CustomAssets.AI;

public class Decisions
{
    private static List<DecisionAsset> _decisionAssets = new List<DecisionAsset>();

    public static void Init()
    {
        Add(new DecisionAsset
        {
            id = "insult_orientation_try",
            task_id = "insult_orientation",
            priority = NeuroLayer.Layer_2_Moderate,
            path_icon = "ui/Icons/culture_traits/orientationless",
            cooldown = 30,
            action_check_launch = actor => (actor.hasCultureTrait("homophobic") || actor.hasCultureTrait("heterophobic")) && Util.IsOrientationSystemEnabledFor(actor),
            weight = 0.5f,
            list_civ = true,
            only_safe = true
        });
        AssetManager.subspecies_traits.get("wernicke_area").addDecision("insult_orientation_try");

        Add(new DecisionAsset
        {
            id = "inquisition_huntdown_orientation_try",
            task_id = "inquisition_huntdown_orientation",
            priority = NeuroLayer.Layer_2_Moderate,
            path_icon = "ui/Icons/religion_traits/unneutrumomisos",
            cooldown = 30,
            action_check_launch = actor =>
            {
                if (!actor.hasReligion())
                    return false;
                return (actor.religion.hasTrait("homomisos") || actor.religion.hasTrait("heteromisos") || actor.religion.hasTrait("unneutrumomisos"))
                && Util.IsOrientationSystemEnabledFor(actor) && actor.isWarrior();
            },
            weight = 0.5f,
            list_civ = true,
            only_safe = true
        });
        AssetManager.subspecies_traits.get("prefrontal_cortex").addDecision("inquisition_huntdown_orientation_try");

        Add(new DecisionAsset
        {
            id = "inquisition_huntdown_lust_try",
            task_id = "inquisition_huntdown_lust",
            priority = NeuroLayer.Layer_2_Moderate,
            path_icon = "ui/Icons/religion_traits/lust_punishment",
            cooldown = 30,
            action_check_launch = actor =>
            {
                if (!actor.hasReligion())
                    return false;
                return actor.religion.hasTrait("lust_punishment") && actor.isWarrior();
            },
            weight = 0.5f,
            list_civ = true,
            only_safe = true
        });
        AssetManager.subspecies_traits.get("prefrontal_cortex").addDecision("inquisition_huntdown_lust_try");

        Add(new DecisionAsset
        {
            id = "kiss_lover",
            priority = NeuroLayer.Layer_2_Moderate,
            path_icon = "ui/Icons/status/just_kissed",
            cooldown = 15,
            action_check_launch = actor => actor.hasLover()
                                           && QueerTraits.GetQueerTraits(actor).Count >= 2 
                                           && !Util.IsRelationshipHappinessEnough(actor, 100f)
                                           && Util.IsOrientationSystemEnabledFor(actor)
                                           && !actor.hasStatus("just_kissed")
                                           && (QueerTraits.BothPreferencesMatch(actor, actor.lover) || Randy.randomChance(0.5f)),
            list_civ = true,
            weight_calculate_custom = actor => Util.IsRelationshipHappinessEnough(actor, 75f) ? 0.5f: 
                Util.IsRelationshipHappinessEnough(actor, 50f) ? 0.6f : Util.IsRelationshipHappinessEnough(actor, 0) ? .8f : 
                Util.IsRelationshipHappinessEnough(actor, -50) ? 1f : Util.IsRelationshipHappinessEnough(actor, -100f) ? 1.5f : 1.25f,
            only_safe = true,
            cooldown_on_launch_failure = true
        });
        AssetManager.subspecies_traits.get("amygdala").addDecision("kiss_lover");

        Add(new DecisionAsset
        {
            id = "try_date",
            priority = NeuroLayer.Layer_2_Moderate,
            path_icon = "ui/Icons/status/went_on_date",
            cooldown = 30,
            action_check_launch = actor => actor.isSapient()
                                           && actor.hasLover()
                                           && !Util.IsRelationshipHappinessEnough(actor, 100f)
                                           && Util.IsOrientationSystemEnabledFor(actor)
                                           && (QueerTraits.BothActorsPreferencesMatch(actor, actor.lover, false) || Randy.randomChance(0.5f))
                                           && !actor.hasStatus("went_on_date"),
            list_civ = true,
            weight_calculate_custom = actor => Util.IsRelationshipHappinessEnough(actor, 75f) ? 0.5f: 
                Util.IsRelationshipHappinessEnough(actor, 50f) ? 0.6f : Util.IsRelationshipHappinessEnough(actor, 0) ? .8f : 
                Util.IsRelationshipHappinessEnough(actor, -50) ? 1f : Util.IsRelationshipHappinessEnough(actor, -100f) ? 1.5f : 1.25f,
            only_safe = true,
            cooldown_on_launch_failure = true
        });

        Add(new DecisionAsset
        {
            id = "reproduce_preservation",
            priority = NeuroLayer.Layer_3_High,
            path_icon = "ui/Icons/status/disliked_sex",
            cooldown = 20,
            action_check_launch = actor =>
            {
                actor.subspecies.countReproductionNeuron();
                return Util.IsDyingOut(actor)
                       && QueerTraits.GetQueerTraits(actor).Count >= 2
                       && Util.CanMakeBabies(actor)
                       && actor.hasSubspeciesTrait("preservation")
                       && Util.IsOrientationSystemEnabledFor(actor);
            },
            weight_calculate_custom = actor => Util.CanMakeBabies(actor) ? 2f : 0.1f,
            only_adult = true,
            only_safe = true,
            cooldown_on_launch_failure = true
        });
        // will force all units to make babies regardless of orientation if they have preservation
        AssetManager.subspecies_traits.get("reproduction_sexual").addDecision("reproduce_preservation");
        AssetManager.subspecies_traits.get("reproduction_same_sex").addDecision("reproduce_preservation");
        AssetManager.subspecies_traits.get("reproduction_hermaphroditic").addDecision("reproduce_preservation");
        
        // should not always cause a pregnancy!
        Add(new DecisionAsset
        {
            id = "invite_for_sex",
            priority = NeuroLayer.Layer_2_Moderate,
            path_icon = "ui/Icons/status/enjoyed_sex",
            cooldown = 30,
            action_check_launch = actor => actor.isSapient()
                                           && QueerTraits.GetQueerTraits(actor).Count >= 2 
                                           && !QueerTraits.GetPreferenceFromActor(actor, true).Equals(Preference.Neither)
                                           && !Util.IsRelationshipHappinessEnough(actor, 100f)
                                           && Util.IsOrientationSystemEnabledFor(actor),
            list_civ = true,
            weight_calculate_custom = actor => Util.IsRelationshipHappinessEnough(actor, 75f) ? 0.25f: 
                Util.IsRelationshipHappinessEnough(actor, 50f) ? 0.5f : Util.IsRelationshipHappinessEnough(actor, 0) ? .75f : 
                Util.IsRelationshipHappinessEnough(actor, -50) ? 1f : Util.IsRelationshipHappinessEnough(actor, -100f) ? 1.5f : 1.25f,
            only_adult = true,
            only_safe = true,
            cooldown_on_launch_failure = true
        });
        AssetManager.subspecies_traits.get("reproduction_sexual").addDecision("invite_for_sex");
        AssetManager.subspecies_traits.get("reproduction_same_sex").addDecision("invite_for_sex");
        AssetManager.subspecies_traits.get("reproduction_hermaphroditic").addDecision("invite_for_sex");
        AssetManager.subspecies_traits.get("reproduction_parthenogenesis").addDecision("invite_for_sex");

        Add(new DecisionAsset
        {
            id = "try_sexual_ivf",
            priority = NeuroLayer.Layer_2_Moderate, // temporary
            path_icon = "ui/Icons/status/adopted_baby",
            cooldown = 15,
            action_check_launch = actor =>
            {
                if (!actor.isSapient() || !Util.WantsBaby(actor, false))
                    return false;
                    
                var bestFriend = actor.getBestFriend();

                if (actor.hasLover())
                {
                    if (!Util.WantsBaby(actor.lover, false))
                        return false;
                        
                    if (Util.CanReproduce(actor, actor.lover) && !QueerTraits.BothActorsPreferencesMatch(actor, actor.lover, true))
                        return true;

                    if (Util.CanReproduce(actor, actor.lover) &&
                        QueerTraits.BothActorsPreferencesMatch(actor, actor.lover, true))
                        return false;
                }
                    
                return bestFriend != null && Util.CanReproduce(actor, bestFriend) && !bestFriend.hasStatus("pregnant");
            },
            list_civ = true,
            weight = 0.5f,
            only_safe = true,
            only_adult = true,
            cooldown_on_launch_failure = true
        });
        AssetManager.subspecies_traits.get("reproduction_sexual").addDecision("try_sexual_ivf");
        AssetManager.subspecies_traits.get("reproduction_same_sex").addDecision("try_sexual_ivf");
        AssetManager.subspecies_traits.get("reproduction_hermaphroditic").addDecision("try_sexual_ivf");

        Finish();
    }
    
            private static void Finish()
        {
            // using (ListPool<DecisionAsset> list1 = new ListPool<DecisionAsset>(AssetManager.decisions_library.list_only_civ))
            // {
            //     using (ListPool<DecisionAsset> list2 = new ListPool<DecisionAsset>(AssetManager.decisions_library.list_only_children))
            //     {
            //         using (ListPool<DecisionAsset> list3 = new ListPool<DecisionAsset>(AssetManager.decisions_library.list_only_city))
            //         {
            //             using (ListPool<DecisionAsset> list4 = new ListPool<DecisionAsset>(AssetManager.decisions_library.list_only_animal))
            //             {
            //                 using (ListPool<DecisionAsset> list5 = new ListPool<DecisionAsset>(AssetManager.decisions_library.list_others))
            //                 {
            //                     int num = 0;
            //                     for(int i = 0; i < _decisionAssets.Count; i++)
            //                     {
            //                         DecisionAsset decisionAsset = _decisionAssets[i];
            //                         decisionAsset.decision_index = num++;
            //                         decisionAsset.priority_int_cached = (int) decisionAsset.priority;
            //                         decisionAsset.has_weight_custom = decisionAsset.weight_calculate_custom != null;
            //                         if (!decisionAsset.unique)
            //                         {
            //                             if (decisionAsset.list_baby)
            //                                 list2.Add(decisionAsset);
            //                             else if (decisionAsset.list_animal)
            //                                 list4.Add(decisionAsset);
            //                             else if (decisionAsset.list_civ)
            //                                 list1.Add(decisionAsset);
            //                             else
            //                                 list5.Add(decisionAsset);
            //                         }
            //                     }
            //                     this.list_only_civ = list1.ToArray<DecisionAsset>();
            //                     this.list_only_children = list2.ToArray<DecisionAsset>();
            //                     this.list_only_city = list3.ToArray<DecisionAsset>();
            //                     this.list_only_animal = list4.ToArray<DecisionAsset>();
            //                     this.list_others = list5.ToArray<DecisionAsset>();
            //                     base.linkAssets();
            //                 }
            //             }
            //         }
            //     }
            // }
            
            for(int i = 0; i < _decisionAssets.Count; i++)
            {
                var decisionAsset = _decisionAssets[i];
                decisionAsset.priority_int_cached = (int) decisionAsset.priority;
                decisionAsset.has_weight_custom = decisionAsset.weight_calculate_custom != null;
                if (!decisionAsset.unique)
                {
                    if (decisionAsset.list_baby)
                        AssetManager.decisions_library.list_only_children = AssetManager.decisions_library.list_only_children.AddToArray(decisionAsset);
                    else if (decisionAsset.list_animal)
                        AssetManager.decisions_library.list_only_animal = AssetManager.decisions_library.list_only_animal.AddToArray(decisionAsset);
                    else if (decisionAsset.list_civ)
                        AssetManager.decisions_library.list_only_civ = AssetManager.decisions_library.list_only_civ.AddToArray(decisionAsset);
                    else
                        AssetManager.decisions_library.list_only_city = AssetManager.decisions_library.list_only_city.AddToArray(decisionAsset);
                }
            }
        }

    
    private static void Add(DecisionAsset asset)
    {
        AssetManager.decisions_library.add(asset);
        asset.decision_index = AssetManager.decisions_library.list.Count-1;
        _decisionAssets.Add(asset);
    }
}