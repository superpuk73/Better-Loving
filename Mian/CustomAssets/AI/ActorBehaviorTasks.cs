using ai.behaviours;
using Topic_of_Love.Mian.CustomAssets.AI.CustomBehaviors;
using Topic_of_Love.Mian.CustomAssets.AI.CustomBehaviors.ivf;
using Topic_of_Love.Mian.CustomAssets.AI.CustomBehaviors.other;
using Topic_of_Love.Mian.CustomAssets.AI.CustomBehaviors.romance;
using Topic_of_Love.Mian.CustomAssets.AI.CustomBehaviors.sex;

namespace Topic_of_Love.Mian.CustomAssets.AI
{
    public class ActorBehaviorTasks
    {
        public static void Init()
        {
            BehaviourTaskActor insultOrientation = new BehaviourTaskActor
            {
                id = "insult_orientation",
                locale_key = "task_insult_orientation",
                path_icon = "ui/Icons/culture_traits/orientationless",
            };
            insultOrientation.addBeh(new BehFindMismatchedOrientation());
            insultOrientation.addBeh(new BehGoToActorTarget(GoToActorTargetType.NearbyTileClosest, pCalibrateTargetPosition: true));
            insultOrientation.addBeh(new BehCheckNearActorTarget());
            insultOrientation.addBeh(new BehInsultOrientation());
            Add(insultOrientation);

            BehaviourTaskActor inquisitionHuntdownOrientation = new BehaviourTaskActor
            {
                id = "inquisition_huntdown_orientation",
                locale_key = "task_inquisition_huntdown_orientation",
                path_icon = "ui/Icons/religion_traits/unneutrumomisos",
            };
            inquisitionHuntdownOrientation.addBeh(new BehFindSinfulOrientation());
            inquisitionHuntdownOrientation.addBeh(new BehGoToActorTarget(GoToActorTargetType.NearbyTileClosest, pCalibrateTargetPosition: true));
            inquisitionHuntdownOrientation.addBeh(new BehCheckNearActorTarget());
            inquisitionHuntdownOrientation.addBeh(new BehHuntdownOrientation());
            Add(inquisitionHuntdownOrientation);

            InitRomance();
            InitSex();
            InitSexualIvf();
        }

        private static void InitRomance()
        {
            
            var forceKiss = new BehaviourTaskActor
            {
                id = "force_kiss",
                locale_key = "task_kiss_lover",
                path_icon = "ui/Icons/status/went_on_date"
            };
            forceKiss.addBeh(new BehMakeTargetWait(30f));
            forceKiss.addBeh(new BehGoToActorTarget(pCalibrateTargetPosition:true));
            forceKiss.addBeh(new BehKissTarget());
            Add(forceKiss);

            var doKissWithLover = new BehaviourTaskActor
            {
                id = "kiss_lover",
                locale_key = "task_kiss_lover",
                path_icon = "ui/Icons/status/went_on_date"
            };
            doKissWithLover.addBeh(new BehGetLoverForRomanceAction(20f));
            doKissWithLover.addBeh(new BehGoToActorTarget(pCalibrateTargetPosition:true));
            doKissWithLover.addBeh(new BehKissTarget());
            
            Add(doKissWithLover);
            
            var forceDate = new BehaviourTaskActor
            {
                id = "force_date",
                locale_key = "task_try_date",
                path_icon = "ui/Icons/status/went_on_date"
            };
            forceDate.addBeh(new BehMakeTargetWait(30f));
            forceDate.addBeh(new BehGoToActorTarget(GoToActorTargetType.NearbyTileClosest, pCalibrateTargetPosition:true));
            forceDate.addBeh(new BehCheckForDate());
            Add(forceDate);

            var tryDate = new BehaviourTaskActor
            {
                id = "try_date",
                locale_key = "task_try_date",
                path_icon = "ui/Icons/status/went_on_date"
            };
            tryDate.addBeh(new BehGetLoverForRomanceAction(20f));
            tryDate.addBeh(new BehGoToActorTarget(GoToActorTargetType.NearbyTileClosest, pCalibrateTargetPosition:true));
            tryDate.addBeh(new BehCheckForDate());
            Add(tryDate);
            
            var actionDate = new BehaviourTaskActor
            {
                id = "action_date",
                locale_key = "task_action_date",
                path_icon = "ui/Icons/status/went_on_date"
            };
            actionDate.addBeh(new BehRandomizeDateTile());
            actionDate.addBeh(new BehGoToTileTarget());
            actionDate.addBeh(new BehWait(4f));
            actionDate.addBeh(new BehCheckIfDateHere());
            actionDate.addBeh(new BehCheckIfDateFinish());
            Add(actionDate);

            var followActionDate = new BehaviourTaskActor
            {
                id = "follow_action_date",
                locale_key = "task_follow_action_date",
                path_icon = "ui/Icons/status/went_on_date"
            };
            followActionDate.addBeh(new BehWait());
            followActionDate.addBeh(new BehFollowDate());

            Add(followActionDate);
        }

        private static void InitSex()
        {
            var reproduceForPreservation = new BehaviourTaskActor
            {
                id = "reproduce_preservation",
                locale_key = "task_reproduce_preservation",
                path_icon = "ui/Icons/status/disliked_sex",
            };
            reproduceForPreservation.addBeh(new BehFindReproduceableSex());
            reproduceForPreservation.addBeh(new BehGetPossibleTileForSex());
            Add(reproduceForPreservation);
            
            var inviteForSex = new BehaviourTaskActor
            {
                id = "invite_for_sex",
                locale_key = "task_invite_for_sex",
                path_icon = "ui/Icons/status/enjoyed_sex",
            };
            inviteForSex.addBeh(new BehFindMatchingPreference());
            inviteForSex.addBeh(new BehGetPossibleTileForSex());
            Add(inviteForSex);

            var haveSexGo = new BehaviourTaskActor // our alternative of the sexual_reproduction_civ_go because it only works on lovers
            {
                id = "have_sex_go",
                locale_key = "task_have_sex_go",
                path_icon = "ui/Icons/status/enjoyed_sex",
            };
            haveSexGo.addBeh(new BehGoToTileTarget());
            for (int index = 0; index < 6; ++index)
            {
                haveSexGo.addBeh(new BehRandomWait(1f, 2f));
                haveSexGo.addBeh(new BehCheckForSexTarget());
                haveSexGo.addBeh(new BehRandomWait(1f, 2f));
            }
            Add(haveSexGo);
        }

        private static void InitSexualIvf()
        {
            var trySexualIvf = new BehaviourTaskActor
            {
                id = "try_sexual_ivf",
                locale_key = "task_try_sexual_ivf",
                path_icon = "ui/Icons/status/adopted_baby"
            };
            trySexualIvf.addBeh(new BehTrySexualIvf());
            Add(trySexualIvf);

            var goSexualIvf = new BehaviourTaskActor
            {
                id = "go_sexual_ivf",
                locale_key = "task_go_sexual_ivf",
                path_icon = "ui/Icons/status/adopted_baby"
            };
            goSexualIvf.addBeh(new BehGoToBuildingTarget());
            for (int index = 0; index < 6; ++index)
            {
                goSexualIvf.addBeh(new BehRandomWait(1f, 2f));
                goSexualIvf.addBeh(new BehCheckForSexualIvf());
                goSexualIvf.addBeh(new BehRandomWait(1f, 2f));
            }
            Add(goSexualIvf);
            
            var goAndWaitSexualIvf = new BehaviourTaskActor
            {
                id = "go_and_wait_sexual_ivf",
                locale_key = "task_go_and_wait_sexual_ivf",
                path_icon = "ui/Icons/status/adopted_baby"
            };
            goAndWaitSexualIvf.addBeh(new BehGoToBuildingTarget());
            for (int index = 0; index < 6; ++index)
            {
                goAndWaitSexualIvf.addBeh(new BehRandomWait(1f, 2f));
            }
            Add(goAndWaitSexualIvf);
            
            var actionSexualIvf = new BehaviourTaskActor
            {
                id = "action_sexual_ivf",
                locale_key = "task_action_sexual_ivf",
                path_icon = "ui/Icons/status/adopted_baby"
            };
            actionSexualIvf.addBeh(new BehStayInBuildingTarget(2f, 3f));
            actionSexualIvf.addBeh(new BehStartSexualIvf());
            actionSexualIvf.addBeh(new BehExitBuilding());
            Add(actionSexualIvf);
           
            var waitSexualIvf = new BehaviourTaskActor
            {
                id = "wait_sexual_ivf",
                locale_key = "task_wait_sexual_ivf",
                path_icon = "ui/Icons/status/adopted_baby"
            };
            waitSexualIvf.addBeh(new BehStayInBuildingTarget(2f, 3f));
            waitSexualIvf.addBeh(new BehExitBuilding());
            Add(waitSexualIvf);
        }
        private static void Add(BehaviourTaskActor behaviour)
        {
            AssetManager.tasks_actor.add(behaviour);
        }
    }
}