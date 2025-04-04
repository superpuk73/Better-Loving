using System;
using System.Collections.Generic;
using NeoModLoader.services;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Better_Loving
{
    public enum Preference
    {
        All,
        SameSex,
        DifferentSex,
        SameOrDifferentSex,
        Neither
    }

    public class QueerTrait : ActorTrait
    {
        public Preference preference;
    }
    
    // this will be improved upon in the future cuz it's not perfect representation
    public class QueerTraits
    {
        private static List<QueerTrait> _sexualityTraits = new List<QueerTrait>();
        private static List<QueerTrait> _romanticTraits = new List<QueerTrait>();
        private static List<QueerTrait> _allTraits = new List<QueerTrait>();
        public static void Init()
        {
            // maybe add actions to break up when romantic traits change
            AddQueerTrait("heterosexual", Preference.DifferentSex, true);
            AddQueerTrait("homosexual", Preference.SameSex, true);
            AddQueerTrait("bisexual", Preference.SameOrDifferentSex, true);
            AddQueerTrait("asexual", Preference.Neither, true);
            
            AddQueerTrait("heteroromantic", Preference.DifferentSex, false);
            AddQueerTrait("homoromantic", Preference.SameSex, false);
            AddQueerTrait("biromantic", Preference.SameOrDifferentSex, false);
            AddQueerTrait("aromantic", Preference.Neither, false);
            Finish();
        }

        public static void Finish()
        {
            foreach(var trait in _sexualityTraits)
            {
                trait.opposite_traits = new HashSet<ActorTrait>();
                foreach (var traitToAdd in _sexualityTraits)
                {
                    if (trait == traitToAdd) continue;
                    trait.opposite_traits.Add(traitToAdd);
                }

                // trait.traits_to_remove_ids = trait.opposite_list.ToArray();
            }
            
            foreach(var trait in _romanticTraits)
            {
                trait.opposite_traits = new HashSet<ActorTrait>();
                foreach (var traitToAdd in _romanticTraits)
                {
                    if (trait == traitToAdd) continue;
                    trait.opposite_traits.Add(traitToAdd);
                }
                
                // trait.traits_to_remove_ids = trait.opposite_list.ToArray();
            }
            _allTraits = _sexualityTraits.ConvertAll(trait => trait);
            _allTraits.AddRange(_romanticTraits);
        }

        public static void CleanQueerTraits(Actor pActor)
        {
            foreach (var trait in _allTraits)
            {
                pActor.removeTrait(trait); // don't let them lose their lovers on cleaning, we want them to stay lovers
            }
        }

        public static bool HasQueerTraits(Actor pActor)
        {
            foreach (var trait in _allTraits)
            {
                if (pActor.hasTrait(trait))
                {
                    return true;
                }
            }

            return false;
        }

        public static void GiveQueerTraits(Actor pActor)
        {
            if (!pActor.hasSubspecies()) return;
            CleanQueerTraits(pActor);
            var queerTraits = QueerTraits.RandomizeQueerTraits(pActor);
            pActor.addTrait(queerTraits[0]);
            if(queerTraits[1] != null)
                pActor.addTrait(queerTraits[1]);   
        }

        // randomizes chances based on what actor's reproduction methods
        public static List<QueerTrait> RandomizeQueerTraits(Actor pActor)
        {
            if (!pActor.hasSubspecies()) return null;
            // randomize for sexual
            Preference matchingPreference;
            if (pActor.subspecies.hasTraitReproductionSexual())
            {
                matchingPreference = Preference.DifferentSex;
            } else if (pActor.subspecies.hasTraitReproductionSexualHermaphroditic())
            {
                matchingPreference = Preference.SameOrDifferentSex;
            }
            else
            {
                matchingPreference = Preference.All;
            }

            List<QueerTrait> randomPool = new List<QueerTrait>();
            foreach (var trait in _sexualityTraits)
            {
                if (trait.preference.Equals(matchingPreference) || matchingPreference.Equals(Preference.All))
                {
                    for (int i = 0; i < 20; i++)
                    {
                        randomPool.Add(trait);
                    }
                }
                else
                {
                    randomPool.Add(trait);
                }
            }

            var sexualTrait = randomPool[Random.Range(0, randomPool.Count)];
            var romanticTrait = GetOppositeVariant(sexualTrait);
            
            // random chance that romantic trait will not fit sexuality trait
            if (Random.Range(1, 101) <= 3)
            {
                romanticTrait = _romanticTraits[Random.Range(0, _romanticTraits.Count)];
            }

            return List.Of(sexualTrait, romanticTrait);
        }
        
        // gets the romantic/sexual version of the trait that matches preferences
        public static QueerTrait GetOppositeVariant(QueerTrait trait)
        {
            return (_sexualityTraits.Contains(trait) ? _romanticTraits : _sexualityTraits).Find(traitInList => traitInList.preference.Equals(trait.preference));
        }
        public static Preference GetPreferenceFromActor(Actor pActor, bool sexual)
        {
            List<QueerTrait> list = sexual ? _sexualityTraits : _romanticTraits;
            foreach (QueerTrait trait in list)
            {
                if(pActor.hasTrait(trait))
                    return trait.preference;
            }

            return Preference.Neither; // if they have no preference, then they like neither
        }

        public static Preference GetPreferenceFromActor(ActorAsset asset, bool sexual)
        {
            List<QueerTrait> list = sexual ? _sexualityTraits : _romanticTraits;
            foreach (QueerTrait trait in list)
            {
                if(asset.traits.Contains(trait.id))
                    return trait.preference;
            }

            return Preference.Neither; // if they have no preference, then they like neither
        }
        
        // Important note that this checks FROM the first actor's point of view, you should also use PreferenceMatches on the other actor to confirm they both like each other!
        public static bool PreferenceMatches(Actor pActor, Actor pTarget, bool sexual)
        {
            var preference = GetPreferenceFromActor(pActor, sexual);
            switch (preference)
            {
                case Preference.SameOrDifferentSex:
                    return true;
                case Preference.SameSex:
                    return pActor.data.sex == pTarget.data.sex;
                case Preference.DifferentSex:
                    return pActor.data.sex != pTarget.data.sex;
                default:
                    return false;
            }
        }

        internal static void AddQueerTrait(string name, Preference preference, bool sexual)
        {
            var trait = new QueerTrait
            {
                id = name,
                path_icon = "ui/Icons/actor_traits/" + name, // temporary icon!
                group_id = "mind",
                rate_birth = 0,
                rate_acquire_grow_up = 0,
                type = TraitType.Other,
                unlocked_with_achievement = false,
                rarity = Rarity.R3_Legendary,
                needs_to_be_explored = true,
                affects_mind = true,
                preference = preference
            };
            if(sexual)
                _sexualityTraits.Add(trait);
            else
            {
                _romanticTraits.Add(trait);
            }

            Util.AddActorTrait(trait);
        }
    }
}