using PersistentEmpiresLib.Database.DBEntities;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresLib.SceneScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class MoneyPouchBehavior : MissionNetwork
    {
        public delegate void RevealedMoneyPouch(NetworkCommunicator player, int Gold);
        public event RevealedMoneyPouch OnRevealedMoneyPouch;

        Dictionary<NetworkCommunicator, long> LastDroppedMoney = new Dictionary<NetworkCommunicator, long>();
        public Dictionary<PE_MoneyBag, long> MoneyBagCreatedAt = new Dictionary<PE_MoneyBag, long>();

        Dictionary<NetworkCommunicator, long> LastReveal = new Dictionary<NetworkCommunicator, long>();
        public Dictionary<Agent, bool> IgnoreAgentDropLoot = new Dictionary<Agent, bool>();

        public int DropPercentage { get; private set; }

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
#if SERVER

            this.DropPercentage = ConfigManager.GetIntConfig("DeathMoneyDropPercentage", 25);
#endif
        }
        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
        }
        public void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
        {
            GameNetwork.NetworkMessageHandlerRegisterer networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(mode);
            if (GameNetwork.IsServer)
            {
                networkMessageHandlerRegisterer.Register<RequestDropMoney>(this.HandleRequestDropMoney);
                networkMessageHandlerRegisterer.Register<RequestRevealMoneyPouch>(this.HandleRequestRevealMoneyPouch);
            }
            if (GameNetwork.IsClient)
            {
                networkMessageHandlerRegisterer.Register<RevealMoneyPouchServer>(this.HandleRevealMoneyPouchServer);
            }
        }

        private void HandleRevealMoneyPouchServer(RevealMoneyPouchServer message)
        {
            InformationManager.DisplayMessage(new InformationMessage(message.Player.UserName + GameTexts.FindText("MoneyPouchBehavior1", null).ToString() + message.Gold + GameTexts.FindText("MoneyPouchBehavior2", null).ToString(), Color.ConvertStringToColor("#FFEB3BFF")));

            if (this.OnRevealedMoneyPouch != null)
            {
                this.OnRevealedMoneyPouch(message.Player, message.Gold);
            }
        }

        private bool HandleRequestRevealMoneyPouch(NetworkCommunicator player, RequestRevealMoneyPouch message)
        {
            if (player.ControlledAgent == null) return false;
            PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null) return false;
            Vec3 position = player.ControlledAgent.Position;
            List<AffectedPlayer> affectedPlayers = new List<AffectedPlayer>();
            if (LastReveal.ContainsKey(player) && LastReveal[player] + 3 > DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                return false;
            }
            LastReveal[player] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            foreach (NetworkCommunicator otherPlayer in GameNetwork.NetworkPeers)
            {
                if (otherPlayer.ControlledAgent == null) continue;
                Vec3 otherPlayerPosition = otherPlayer.ControlledAgent.Position;
                float d = position.Distance(otherPlayerPosition);
                if (d < 30)
                {
                    GameNetwork.BeginModuleEventAsServer(otherPlayer);
                    GameNetwork.WriteMessage(new RevealMoneyPouchServer(player, persistentEmpireRepresentative.Gold));
                    GameNetwork.EndModuleEventAsServer();
                    if (otherPlayer != player)
                    {
                        affectedPlayers.Add(new AffectedPlayer(otherPlayer));
                    }
                }
            }
            LoggerHelper.LogAnAction(player, LogAction.PlayerRevealedMoneyPouch, affectedPlayers.ToArray(), new object[] {
                persistentEmpireRepresentative.Gold
            });
            return true;
        }

        private static int _counter = 0;
        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

            if (++_counter < 5)
                return;
            // Reset counter
            _counter = 0;

            if (GameNetwork.IsClientOrReplay) return;
            
            foreach (PE_MoneyBag moneyBag in this.MoneyBagCreatedAt.Keys.ToList())
            {
                if (this.MoneyBagCreatedAt[moneyBag] + 600 < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                {
                    this.MoneyBagCreatedAt.Remove(moneyBag);
                    if (moneyBag.GameEntity != null)
                    {
                        moneyBag.GameEntity.Remove(80);
                    }
                }
            }
        }

        private void DropMoney(MatrixFrame frame, int amount)
        {
            PE_MoneyBag moneyBag = (PE_MoneyBag)base.Mission.CreateMissionObjectFromPrefab("pe_moneybag", frame);
            this.MoneyBagCreatedAt[moneyBag] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            moneyBag.SetAmount(amount);

        }

        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);
            if (IgnoreAgentDropLoot.ContainsKey(affectedAgent))
            {
                IgnoreAgentDropLoot.Remove(affectedAgent);
                return;
            }
            if (
                GameNetwork.IsServer &&
                affectedAgent.IsHuman &&
                affectedAgent.IsPlayerControlled &&
                agentState == AgentState.Killed &&
                affectedAgent.MissionPeer != null &&
                (
                    (affectedAgent.MissionPeer.GetNetworkPeer().QuitFromMission == false && affectedAgent.MissionPeer.GetNetworkPeer().IsConnectionActive) //||
                                                                                                                                                           // (affectedAgent.MissionPeer.GetNetworkPeer().QuitFromMission == true && CombatlogBehavior.Instance != null && CombatlogBehavior.Instance.IsPlayerInCombatState(affectedAgent.MissionPeer.GetNetworkPeer()))
                )
            )
            {
                NetworkCommunicator player = affectedAgent.MissionPeer.GetNetworkPeer();
                PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
                if (persistentEmpireRepresentative == null) return;
                int amount = (persistentEmpireRepresentative.Gold * this.DropPercentage) / 100;
                if (amount == 0) return;
                MatrixFrame frame = affectedAgent.Frame;
                frame = frame.Advance(1);
                persistentEmpireRepresentative.GoldLost(amount);
                this.DropMoney(frame, amount);
            }
        }
        public bool HandleRequestDropMoney(NetworkCommunicator peer, RequestDropMoney message)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = peer.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null) return false;
            if (message.Amount <= 0) return false;
            if (this.LastDroppedMoney.ContainsKey(peer) && this.LastDroppedMoney[peer] + 1 > DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                return false;
            }
            if (!persistentEmpireRepresentative.ReduceIfHaveEnoughGold(message.Amount))
            {
                return false;
            }
            if (peer.ControlledAgent == null) return false;

            MatrixFrame frame = peer.ControlledAgent.Frame;
            this.DropMoney(frame, message.Amount);
            this.LastDroppedMoney[peer] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            LoggerHelper.LogAnAction(peer, LogAction.PlayerDroppedGold, null, new object[] { message.Amount });
            return true;
        }
    }
}
