using System.IO;
using Better_Loving.Mian.CustomAssets;
using Better_Loving.Mian.CustomAssets.AI;
using NeoModLoader.api;
using HarmonyLib;
using NeoModLoader.General;

/*

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