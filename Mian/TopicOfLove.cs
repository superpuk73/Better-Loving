using System.IO;
using Topic_of_Love.Mian.CustomAssets;
using Topic_of_Love.Mian.CustomAssets.AI;
using Topic_of_Love.Mian.CustomAssets.Traits;
using Topic_of_Love.Mian.CustomAssets.TraitGroups;
using NeoModLoader.api;
using HarmonyLib;
using NeoModLoader.General;

/*

- lovers in wild shouldnt attack each other (done)
- lovers should create kingdoms together if in wild (even from different subspecies) (done)

- custom find lover decision (combination of romantic tasks that infleunce romantic opinion, lovers may randomly fall in love even when fighting)
- cupid who can force lovers together
- kings from two different kingdoms who are lovers will have good opinions on each other (done, also did homo/hetero sexuality)

- other specices no reproduce :(((

- queer traits arent 100% being added

- sprites are offseted upwards for our speech bubbles idk why :(

*/
namespace Topic_of_Love.Mian
{
    public class TopicOfLove : BasicMod<TopicOfLove>
    {
        public static BasicMod<TopicOfLove> Mod;
        
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
            new ActorTraits().Init();
            new CultureTraitGroups().Init();
            new CultureTraits().Init();
            new SubspeciesTraits().Init();
            new ClanTraits().Init();
            new ReligionTraits().Init();
            StatusEffects.Init();
            Happiness.Init();
            CommunicationTopics.Init();
            ActorBehaviorTasks.Init();
            Decisions.Init();
            Opinions.Init();
            GodPowers.Init();
            TabsAndButtons.Init();
            
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