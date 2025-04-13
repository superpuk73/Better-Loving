using System.Collections.Generic;

namespace Better_Loving
{
    public class CommunicationTopics
    {
        public static void Init()
        {
            // small bug where the sprites clip.. i dunno how to fix that tho :<
            Add(new CommunicationAsset
            {
                id = "orientation",
                rate = 0.5f,
                check = pActor => QueerTraits.GetPreferenceFromActor(pActor, false) != Preference.Inapplicable 
                                  && QueerTraits.GetPreferenceFromActor(pActor, true) != Preference.Inapplicable
                                  && QueerTraits.GetQueerTraits(pActor, true).Count >= 2
                                  && !pActor.hasCultureTrait("orientationless"),
                pot_fill = (actor, sprites) =>
                {
                    if (QueerTraits.GetQueerTraits(actor, true).Count < 2) return;
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

                    sprites.Add(queerTraits[0].getSprite());
                    sprites.Add(queerTraits[1].getSprite());

                    if (unfitPreferences.Contains(sexualPreference) || unfitPreferences.Contains(romanticPreference))
                        actor.changeHappiness("orientation_does_not_fit");
                    else
                        actor.changeHappiness("orientation_fits");
                } 
            });
        }

        private static void Add(CommunicationAsset asset)
        {
            AssetManager.communication_topic_library.add(asset);
        }
    }
}