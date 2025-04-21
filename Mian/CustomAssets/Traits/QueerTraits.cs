using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

namespace Topic_of_Love.Mian.CustomAssets.Traits
{
    public enum Preference
    {
        All,
        SameSex,
        DifferentSex,
        SameOrDifferentSex,
        Neither,
        Inapplicable
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
            AddQueerTrait("abrosexual", Preference.Inapplicable, true);
            
            AddQueerTrait("heteroromantic", Preference.DifferentSex, false);
            AddQueerTrait("homoromantic", Preference.SameSex, false);
            AddQueerTrait("biromantic", Preference.SameOrDifferentSex, false);
            AddQueerTrait("aromantic", Preference.Neither, false);
            AddQueerTrait("abroromantic", Preference.Inapplicable, false);
            
            Finish();
        }

        public static void Finish()
        {
            foreach(var trait in _sexualityTraits)
            {
                if (trait.preference == Preference.Inapplicable) continue;
                
                trait.opposite_traits = new HashSet<ActorTrait>();
                foreach (var traitToAdd in _sexualityTraits)
                {
                    if (trait == traitToAdd || traitToAdd.preference == Preference.Inapplicable) continue;
                    trait.opposite_traits.Add(traitToAdd);
                }

                // trait.traits_to_remove_ids = trait.opposite_list.ToArray();
            }
            
            foreach(var trait in _romanticTraits)
            {
                if (trait.preference == Preference.Inapplicable) continue;
                
                trait.opposite_traits = new HashSet<ActorTrait>();
                foreach (var traitToAdd in _romanticTraits)
                {
                    if (trait == traitToAdd || traitToAdd.preference == Preference.Inapplicable) continue;
                    trait.opposite_traits.Add(traitToAdd);
                }
                
                // trait.traits_to_remove_ids = trait.opposite_list.ToArray();
            }
            _allTraits = _sexualityTraits.ConvertAll(trait => trait);
            _allTraits.AddRange(_romanticTraits);
        }

        public static void CleanQueerTraits(Actor pActor, bool sexual, bool clearInapplicable=false)
        {
            var list = (sexual ? _sexualityTraits : _romanticTraits).Where(trait => !trait.preference.Equals(Preference.Inapplicable) || clearInapplicable).ToList();
            foreach (var trait in list)
            {
                pActor.removeTrait(trait);
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

        public static List<QueerTrait> GetQueerTraits(Actor pActor, bool excludeInapplicable=false)
        {
            List<QueerTrait> list = new List<QueerTrait>();
            foreach (var trait in _sexualityTraits)
            {
                if (pActor.hasTrait(trait) && (!trait.preference.Equals(Preference.Inapplicable) || !excludeInapplicable))
                {
                    list.Add(trait);
                }
            }
            foreach (var trait in _romanticTraits)
            {
                if (pActor.hasTrait(trait) && (!trait.preference.Equals(Preference.Inapplicable) || !excludeInapplicable))
                {
                    list.Add(trait);
                }
            }

            return list;
        }

        public static void GiveQueerTraits(Actor pActor, bool clearInapplicable = false)
        {
            if (!pActor.hasSubspecies()) return;
            var currentTraits = GetQueerTraits(pActor);
            foreach (var trait in currentTraits)
            {
                if (!clearInapplicable && trait.preference.Equals(Preference.Inapplicable)) continue;
                pActor.removeTrait(trait);
            }
            
            var queerTraits = RandomizeQueerTraits(pActor, currentTraits);
            for(int i = 0; i < queerTraits.Count; i++)
            {
                if (queerTraits[i] != null)
                {
                    pActor.addTrait(queerTraits[i]);
                }
            }
        }

        public static List<List<int>> GetSexualPrefBasedOnReproduction(Actor pActor)
        {
            if (!pActor.hasSubspecies()) return null;
            if (pActor.subspecies.hasTraitReproductionSexual() && !pActor.hasSubspeciesTrait("reproduction_parthenogenetic"))
                return repSexualChances;
            if (!pActor.subspecies.hasTraitReproductionSexual() && pActor.hasSubspeciesTrait("reproduction_parthenogenetic"))
                return repParthenogeneticChances;
            if (pActor.subspecies.hasTraitReproductionSexual() && pActor.hasSubspeciesTrait("reproduction_parthenogenetic"))
                return repSexualParthenogeneticChances;
            if (pActor.subspecies.hasTraitReproductionSexualHermaphroditic())
                return repHermaphroditismChances;
            if (pActor.hasSubspeciesTrait("reproduction_same_sex"))
                return repSameSexChances;
            return repEqualChances;
        }

        // randomizes chances based on what actor's reproduction methods
        public static List<QueerTrait> RandomizeQueerTraits(Actor pActor, List<QueerTrait> exclude)
        {
            if (!pActor.hasSubspecies()) return null;
            // randomize for sexual
            var matchingPreference = GetSexualPrefBasedOnReproduction(pActor);
            if (matchingPreference != null)
                return null;

            List<QueerTrait> randomPool = new List<QueerTrait>();

            int random = Randy.randomInt(1, 101);
            for (int i = 0; i < 4; i++)
            {
                if (random >= matchingPreference[i][0] && random <= matchingPreference[i][1])
                {
                    if (pActor.hasSubspeciesTrait("amygdala"))
                    {
                        randomPool.Add(_romanticTraits[i]);
                    }
                    if (pActor.isSapient())
                    {
                        randomPool.Add(_sexualityTraits[i]);
                    }
                }
            }

            random = Randy.randomInt(1, 101);
            if (random >= matchingPreference[4][0]  && random <= matchingPreference[4][1])
            {
                randomPool.Add(_romanticTraits[4]);
            }

            random = Randy.randomInt(1, 101);
            if (random >= matchingPreference[4][0] && random <= matchingPreference[4][1])
            {
                randomPool.Add(_sexualityTraits[4]);
            }

            return randomPool;
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
                if(pActor.hasTrait(trait) && !trait.preference.Equals(Preference.Inapplicable))
                    return trait.preference;
            }

            return Preference.Inapplicable; // if they have no preference, then they like neither
        }

        public static bool IncludesHomoPreference(Preference preference)
        {
            return preference.Equals(Preference.SameSex) || preference.Equals(Preference.SameOrDifferentSex) || preference.Equals(Preference.All);
        }
        
        // Important note that this checks FROM the first actor's point of view, you should also use PreferenceMatches on the other actor to confirm they both like each other!
        public static bool PreferenceMatches(Actor pActor, Actor pTarget, bool sexual)
        {
            if (pActor == null || pTarget == null) return false;
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

        public static bool BothPreferencesMatch(Actor actor1, Actor actor2)
        {
            return PreferenceMatches(actor1, actor2, false) && PreferenceMatches(actor1, actor2, true);
        }

        public static bool BothActorsPreferencesMatch(Actor actor1, Actor actor2, bool sexual)
        {
            return PreferenceMatches(actor1, actor2, sexual) && PreferenceMatches(actor2, actor1, sexual);
        }

        internal static void AddQueerTrait(string name, Preference preference, bool sexual)
        {
            var trait = new QueerTrait
            {
                id = name,
                path_icon = "ui/Icons/actor_traits/" + name,
                group_id = "mind",
                spawn_random_trait_allowed = false,
                rate_birth = 0,
                rate_acquire_grow_up = 0,
                is_mutation_box_allowed = false,
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

            AssetManager.traits.add(trait);
        }

        private static List<List<int>> repSexualChances = List.Of(List.Of(1, 90), List.Of(91, 95), List.Of(95, 98), List.Of(99, 100), List.Of(1, 1));
        private static List<List<int>> repSameSexChances = List.Of(List.Of(1, 3), List.Of(4, 77), List.Of(78, 95), List.Of(96, 100), List.Of(1, 10));
        private static List<List<int>> repHermaphroditismChances = List.Of(List.Of(1, 15), List.Of(16, 20), List.Of(21, 97), List.Of(98, 100), List.Of(0, 0));
        private static List<List<int>> repParthenogeneticChances = List.Of(List.Of(1, 4), List.Of(5, 6), List.Of(6, 17), List.Of(18, 100), List.Of(1, 2));
        private static List<List<int>> repSexualParthenogeneticChances = List.Of(List.Of(1, 50), List.Of(51, 52), List.Of(53, 60), List.Of(61, 100), List.Of(1, 2));
        private static List<List<int>> repEqualChances = List.Of(List.Of(1, 25), List.Of(26, 50), List.Of(51, 75), List.Of(76, 100), List.Of(1, 25));
    }
}