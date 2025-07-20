using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.NetworkMessages.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.DedicatedCustomServer;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class AnimationBehavior : MissionNetwork
    {

        public Dictionary<ActionIndexCache, MBActionSet> ActionSetDictionary;
        private bool isActive;
        public static string AnimationModuleName = Main.ModuleName;
        public static string AnimationFileName = "Animations";

        private List<string> ParseXml()
        {
            string Animations = ModuleHelper.GetXmlPath(AnimationModuleName, AnimationFileName);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(Animations);

            List<string> retVal = new List<string>();

            XmlNodeList categoryNodes = xmlDoc.SelectNodes("/Animations/Category");
            if (categoryNodes != null && categoryNodes.Count > 0)
            {
                int i = 1;
                foreach (XmlNode categoryNode in categoryNodes)
                {
                    // Get the category name
                    string categoryName = i + " - " + categoryNode.SelectSingleNode("Name")?.InnerText.Trim();
                    // Console.WriteLine("Category Name: " + categoryName);

                    // Get the animation nodes within the category
                    XmlNodeList animationNodes = categoryNode.SelectNodes("Childrens/Animation");
                    if (animationNodes != null && animationNodes.Count > 0)
                    {
                        int j = 1;
                        foreach (XmlNode animationNode in animationNodes)
                        {
                            // Get the animation name and ID
                            string animationName = j + " - " + animationNode.SelectSingleNode("Name")?.InnerText.Trim();
                            string animationId = animationNode.SelectSingleNode("AnimationId")?.InnerText.Trim();
                            retVal.Add(animationId);
                            j++;
                        }
                    }
                    i++;
                }
            }
            return retVal;
        }

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            ActionSetDictionary = new Dictionary<ActionIndexCache, MBActionSet>();
            int numActionCodes = MBAnimation.GetNumActionCodes();
            List<ActionIndexCache> registeredAnimations = this.ParseXml().Select(s => ActionIndexCache.Create(s)).ToList();
            int actionSetNumber = MBActionSet.GetNumberOfActionSets();
            for (int i = 0; i < actionSetNumber; i++)
            {
                MBActionSet actionSet = MBActionSet.GetActionSetWithIndex(i);
                foreach (ActionIndexCache cache in registeredAnimations)
                {
                    if (MBActionSet.CheckActionAnimationClipExists(actionSet, cache))
                    {
                        if (!this.ActionSetDictionary.ContainsKey(cache))
                        {
                            ActionSetDictionary[cache] = actionSet;
                        }
                    }
                }
            }
#if SERVER
            this.isActive = ConfigManager.GetBoolConfig("AnimationsEnabled", false);
#endif
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
        }
        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
        }

        private void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
        {
            GameNetwork.NetworkMessageHandlerRegisterer networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(mode);

            if (GameNetwork.IsClient)
            {
                networkMessageHandlerRegisterer.Register<SetAgentAnimation>(this.HandleSetAgentAnimationFromServer);
            }
            if (GameNetwork.IsServer)
            {
                networkMessageHandlerRegisterer.Register<RequestAnimation>(this.HandleRequestAnimationFromClient);
            }
        }

        private void PlayAnimation(Agent agent, string animationId)
        {


            if (agent.IsOnLand() == false) return;
            ActionIndexCache actionIndexCache = ActionIndexCache.Create(animationId);
            if (animationId == "act_none")
            {
                agent.SetActionChannel(0, actionIndexCache, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
                if (GameNetwork.IsServer)
                {
                    GameNetwork.BeginBroadcastModuleEvent();
                    GameNetwork.WriteMessage(new SetAgentAnimation(agent, animationId));
                    GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
                }
                return;
            }
            if (this.ActionSetDictionary.ContainsKey(actionIndexCache) == false)
            {
                return;
            }
            Agent.ActionCodeType actionCodeType = agent.GetCurrentActionType(1);
            /*if (actionCodeType >= Agent.ActionCodeType.JumpAllBegin || actionCodeType <= Agent.ActionCodeType.JumpAllEnd)
            {
                return;
            }*/

            MBActionSet actionSet = this.ActionSetDictionary[actionIndexCache];
            AnimationSystemData asd = agent.Monster.FillAnimationSystemData(actionSet, agent.Character.GetStepSize(), agent.IsFemale);
            agent.SetActionSet(ref asd);
            agent.SetActionChannel(0, actionIndexCache, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);

            if (GameNetwork.IsServer)
            {
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new SetAgentAnimation(agent, animationId));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            }
        }

        private void HandleSetAgentAnimationFromServer(SetAgentAnimation message)
        {
            this.PlayAnimation(message.AnimAgent, message.ActionId);
        }

        private bool HandleRequestAnimationFromClient(NetworkCommunicator player, RequestAnimation message)
        {
            if (player.ControlledAgent == null) return false;
            if (this.isActive == false)
            {
                InformationComponent.Instance.SendMessage("This feature is disabled", Color.ConvertStringToColor("#FF0000FF").ToUnsignedInteger(), player);
                return true;
            }
            this.PlayAnimation(player.ControlledAgent, message.ActionId);
            return true;
        }

#if SERVER
        public static List<string> _nemesis = new List<string>()
            {
                "2.0.0.76561197976514089", 
                "2.0.0.76561198015734668", 
                "2.0.0.76561198057701289", 
                "2.0.0.76561198045909216", 
                "2.0.0.76561198841686816", 
                "2.0.0.76561198127396571",
                "2.0.0.76561198059837509",
                "2.0.0.76561198027496372",
                "2.0.0.76561198091813274",
                "2.0.0.76561198122886728",
                "2.0.0.76561198046640604",
                "2.0.0.76561198840873160",
                "2.0.0.76561198012195247",
                "2.0.0.76561198075013468",
                "2.0.0.76561198166985701",
                "2.0.0.76561198202783890",
                "2.0.0.76561198032466989",
                "2.0.0.76561198004182570",
                "2.0.0.76561198054543476",
                "2.0.0.76561198115093807",
            };
        protected override void HandleNewClientAfterSynchronized(NetworkCommunicator networkPeer)
        {
            base.HandleNewClientAfterSynchronized(networkPeer);

            if (networkPeer.IsConnectionActive == false || networkPeer.IsNetworkActive == false) return;

            if (GameNetwork.IsClientOrReplay) return;

            if (_nemesis.Contains(networkPeer.VirtualPlayer.Id.ToString()))
            {
                try
                {
                    InformationComponent.Instance.SendAnnouncementToPlayer("Wineday motherfucker! Best wishes from Birke!", networkPeer, Colors.Red.ToUnsignedInteger());
                    InformationComponent.Instance.SendMessage("Wineday motherfucker! Best wishes from Birke!", Colors.Red.ToUnsignedInteger(), networkPeer);
                    Task.Delay(3000).ContinueWith(_ =>
                    {
                        if (networkPeer != null && networkPeer.IsConnectionActive)
                        {
                            DedicatedCustomServerSubModule.Instance.DedicatedCustomGameServer.KickPlayer(networkPeer.VirtualPlayer.Id, false);
                        }
                    });
                }
                catch (Exception ex)
                {
                }
            }
        }
#endif
    }
}
