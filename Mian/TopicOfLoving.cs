using System.IO;
using Better_Loving.Mian.CustomAssets;
using Better_Loving.Mian.CustomAssets.AI;
using NeoModLoader.api;
using HarmonyLib;
using NeoModLoader.General;

/*

- rework orientationless trait with new changes
(needs testing)

- homosexuals/heterosexuals have a 50% chance of having cultures that are heterophobic/homophobic
(needs testing)

- make a sexual reproduction trait that allows two ppl of the same sex to reproduce
(needs testing)

- test insult orientation task to make sure it still works
(needs testing)

- make spouses leave family when they break up (children will be shared if same subspecies. If different subspecies, the child will go to the same subspecies parent)
(needs testing)

- make people unable to fall in love temporarily after breakups/cheating
(needs testing)

- improve breaking up and cheating post-systems. 
Rn the units just never date again but it should prob be altered so that they can date someone again after a period of time.
(needs testing)

- lovers will defend each other task
(needs testing)

- add kissing task
(needs testing)

- no sexual needs trait
(needs testing)

- add sexual ivf task for units that cant get pregnant but want a baby (can lead to adoption which could be a happiness aspect!)
(needs testing)

- dating romantic task
(needs testing)

- queer traits arent 100% being added

- sprites are offseted upwards for our speech bubbles idk why :(

*/
namespace Better_Loving.Mian
{
    public class TopicOfLoving : BasicMod<TopicOfLoving>
    {
        public static BasicMod<TopicOfLoving> Mod;
        
        protected override void OnModLoad()
        {
            Mod = this;
            // Initialize your mod.
            // Methods are called in the order: OnLoad -> Awake -> OnEnable -> Start -> Update
            Util.LogWithId("Making people more loveable!");
            
            var localeDir = GetLocaleFilesDirectory(GetDeclaration());
            foreach (var file in Directory.GetFiles(localeDir))
            {
                if (file.EndsWith(".json"))
                {
                    LM.LoadLocale(Path.GetFileNameWithoutExtension(file), file);
                }
                else if (file.EndsWith(".csv"))
                {
                    LM.LoadLocales(file);
                }
            }

            LM.ApplyLocale();
            
            QueerTraits.Init();
            ActorTraits.Init();
            CultureTraits.Init();
            SubspeciesTraits.Init();
            StatusEffects.Init();
            Happiness.Init();
            CommunicationTopics.Init();
            ActorBehaviorTasks.Init();
            Decisions.Init();
            GodPowers.Init();
            
            // Managers
            // DateableManager.Init();
        }
        private void Awake()
        {
            var harmony = new Harmony("netdot.mian.topicofloving");
            harmony.PatchAll();
        }
    }
}