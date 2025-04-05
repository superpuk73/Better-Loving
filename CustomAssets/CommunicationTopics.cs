using System.Collections.Generic;

namespace Better_Loving
{
    public class CommunicationTopics
    {
        public static void Init()
        {
            Add(new CommunicationAsset
            {
                id = "orientation",
                show_topic = true,
                rate = 0.3f,
                check = pActor => (pActor.hasCultureTrait("homophobic") || pActor.hasCultureTrait("heterophobic")) 
                    && QueerTraits.GetPreferenceFromActor(pActor, false) != Preference.Inapplicable 
                    && QueerTraits.GetPreferenceFromActor(pActor, true) != Preference.Inapplicable
                && QueerTraits.GetQueerTraits(pActor, true).Count >= 2,
                pot_fill = (actor, sprites) =>
                {
                    var unfitPreferences = new List<Preference>();
                    if (actor.hasCultureTrait("homophobic"))
                    {
                        unfitPreferences.Add(Preference.All);
                        unfitPreferences.Add(Preference.SameSex);
                        unfitPreferences.Add(Preference.SameOrDifferentSex);
                    }
                    if (actor.hasCultureTrait("heterophobic"))
                    {
                        unfitPreferences.Add(Preference.DifferentSex);
                    }

                    var queerTraits = QueerTraits.GetQueerTraits(actor, true);
                    var sexualPreference = queerTraits[0].preference;
                    var romanticPreference = queerTraits[1].preference;
                    var happy = true;

                    sprites.Add(queerTraits[0].getSprite());
                    sprites.Add(queerTraits[1].getSprite());

                    if (unfitPreferences.Contains(sexualPreference) || unfitPreferences.Contains(romanticPreference))
                        happy = false;

                    if (happy)
                    {
                        actor.changeHappiness("orientation_fits");
                        sprites.Add(HappinessHelper.getSpriteBasedOnHappinessValue(100));
                    }
                    else
                    {
                        actor.changeHappiness("orientation_does_not_fit");
                        sprites.Add(HappinessHelper.getSpriteBasedOnHappinessValue(-100));
                    }
                } 
            });
        }

        private static void Add(CommunicationAsset asset)
        {
            AssetManager.communication_topic_library.add(asset);
        }
    }
}