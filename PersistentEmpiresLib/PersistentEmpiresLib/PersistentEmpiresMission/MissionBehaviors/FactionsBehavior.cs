/*
 *  Persistent Empires Open Sourced - A Mount and Blade: Bannerlord Mod
 *  Copyright (C) 2024  Free Software Foundation, Inc.
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.

 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using NetworkMessages.FromClient;
using PersistentEmpiresLib.Database.DBEntities;
using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresLib.SceneScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class FactionsBehavior : MissionNetwork
    {
        public InformationComponent _informationComponent;

        public delegate void FactionAddedHandler(Faction faction, int factionIndex);
        public delegate void FactionUpdatedHandler(int factionIndex, Faction faction);
        public delegate void PlayerJoinedFactionHandler(int factionIndex, Faction faction, int joinedFromIndex, NetworkCommunicator player);
        public delegate void FactionLordChangedHandler(Faction faction, int factionIndex, NetworkCommunicator newLord);
        public delegate void FactionMarshallChangedHandler(Faction faction, int factionIndex, NetworkCommunicator newMarshall);
        public delegate void FactionDeclaredWarHandler(int declarer, int declaredTo);
        public delegate void FactionMakePeaceHandler(int maker, int taker);
        public delegate void FactionKeyFetchedHandler(int factionIndex, string playerId, int keyType);


        public event FactionAddedHandler OnFactionAdded;
        public event FactionUpdatedHandler OnFactionUpdated;
        public event PlayerJoinedFactionHandler OnPlayerJoinedFaction;
        public event FactionLordChangedHandler OnFactionLordChanged;
        public event FactionMarshallChangedHandler OnFactionMarshallChanged;
        public event FactionDeclaredWarHandler OnFactionDeclaredWar;
        public event FactionMakePeaceHandler OnFactionMakePeace;
        public event FactionKeyFetchedHandler OnFactionKeyFetched;
        public Dictionary<int, Faction> Factions { get; set; }
        public Dictionary<int, PEFactionBanner> FactionBanners { get; set; }

        public Dictionary<int, long> FactionDeclaredWarLast { get; set; }

        /* Configs */

        public int WarDeclareTimeOut = 30; // ConfigManager.GetIntConfig("WarDeclareTimeOut", 30);
        public int PeaceDeclareTimeOut = 30; // ConfigManager.GetIntConfig("PeaceDeclareTimeOut", 30);
        public int MaxBannerLength = 100; // ConfigManager.GetIntConfig("MaxBannerLength", 100);

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            this.Factions = new Dictionary<int, Faction>();
            this.FactionBanners = new Dictionary<int, PEFactionBanner>();
            this.FactionDeclaredWarLast = new Dictionary<int, long>();
            this._informationComponent = base.Mission.GetMissionBehavior<InformationComponent>();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);

            if (GameNetwork.IsServer)
            {
                this.WarDeclareTimeOut = ConfigManager.GetIntConfig("WarDeclareTimeOut", 30);
                this.PeaceDeclareTimeOut = ConfigManager.GetIntConfig("PeaceDeclareTimeOut", 30);
                this.MaxBannerLength = ConfigManager.GetIntConfig("MaxBannerLength", 100);


            }

        }

        public static bool PatchGlobalChat_OnClientEventPlayerMessageTeam(NetworkCommunicator networkPeer, PlayerMessageTeam message)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = networkPeer.GetComponent<PersistentEmpireRepresentative>();
            Faction f = persistentEmpireRepresentative.GetFaction();
            if (message.Message.StartsWith("!") && f != null && (f.lordId == networkPeer.VirtualPlayer.Id.ToString() || f.marshalls.Contains(networkPeer.VirtualPlayer.Id.ToString())))
            {
                string updated = message.Message.Substring(1);

                foreach (NetworkCommunicator n in f.members)
                {
                    if (n.IsConnectionActive && n.IsNetworkActive)
                    {
                        InformationComponent.Instance.SendQuickInformationToPlayer("[" + networkPeer.UserName + "] " + updated, n);
                    }
                }

                return false;
            }
            return true;
        }

        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
        }

        public override void AfterStart()
        {
            List<GameEntity> gameEntities = new List<GameEntity>();
            base.Mission.Scene.GetAllEntitiesWithScriptComponent<PEFactionBanner>(ref gameEntities);
            foreach (GameEntity gameEntity in gameEntities)
            {
                PEFactionBanner factionBanner = gameEntity.GetFirstScriptOfType<PEFactionBanner>();
                this.FactionBanners[factionBanner.FactionIndex] = factionBanner;
            }

            if (GameNetwork.IsServer)
            {
                BasicCultureObject commonerCulture = MBObjectManager.Instance.GetObject<BasicCultureObject>("empire");
                BasicCultureObject outlawCulture = MBObjectManager.Instance.GetObject<BasicCultureObject>("empire");

                Banner banner = new Banner(commonerCulture.BannerKey, commonerCulture.BackgroundColor1, commonerCulture.ForegroundColor1);
                Banner banner2 = new Banner("24.193.116.1536.1536.768.768.1.0.0");

                Faction commoners = new Faction(commonerCulture, banner, "Commoners");
                Faction outlaws = new Faction(outlawCulture, banner2, "Outlaws");

                //Factions Setup

                commoners.team = Mission.Current.Teams.Add(BattleSideEnum.Attacker, commonerCulture.BackgroundColor1, commonerCulture.ForegroundColor1, banner);
                outlaws.team = Mission.Current.Teams.Add(BattleSideEnum.Attacker, outlawCulture.BackgroundColor1, outlawCulture.ForegroundColor1, banner2);
                outlaws.team.SetIsEnemyOf(outlaws.team, true);
                outlaws.team.SetIsEnemyOf(commoners.team, true);
                commoners.team.SetIsEnemyOf(commoners.team, true);

                this.AddFaction(0, commoners);
                this.AddFaction(1, outlaws);

                List<GameEntity> _gameEntites = new List<GameEntity>();
                base.Mission.Scene.GetAllEntitiesWithScriptComponent<PEFactionBanner>(ref _gameEntites);
                List<DBFactions> dbFactions = SaveSystemBehavior.HandleGetFactions().ToList();
                Dictionary<int, DBFactions> savedFactions = new Dictionary<int, DBFactions>();
                foreach (DBFactions dbFaction in dbFactions)
                {
                    savedFactions[dbFaction.FactionIndex] = dbFaction;
                }
                if (_gameEntites.Count > 0)
                {
                    foreach (GameEntity _factionBanner in _gameEntites)
                    {
                        PEFactionBanner peFactionBanner = _factionBanner.GetFirstScriptOfType<PEFactionBanner>();
                        if (this.Factions.ContainsKey(peFactionBanner.FactionIndex))
                        {
                            continue;
                        }
                        BasicCultureObject factionCulture = MBObjectManager.Instance.GetObject<BasicCultureObject>("empire");

                        Banner factBanner;
                        Faction fact;

                        if (savedFactions.ContainsKey(peFactionBanner.FactionIndex))
                        {
                            factBanner = new Banner(savedFactions[peFactionBanner.FactionIndex].BannerKey);
                            fact = new Faction(factionCulture, factBanner, savedFactions[peFactionBanner.FactionIndex].Name);
                            fact.lordId = savedFactions[peFactionBanner.FactionIndex].LordId;
                            fact.LoadMarshallsFromSerialized(savedFactions[peFactionBanner.FactionIndex].Marshalls);
                        }
                        else
                        {
                            factBanner = new Banner(peFactionBanner.GetBannerKey());
                            fact = new Faction(factionCulture, factBanner, peFactionBanner.FactionName);
                        }


                        fact.team = Mission.Current.Teams.Add(BattleSideEnum.Attacker, factBanner.GetPrimaryColor(), factBanner.GetSecondaryColor(), factBanner);
                        this.AddFaction(peFactionBanner.FactionIndex, fact);
                    }
                }
                List<Faction> factions = this.Factions.Values.ToList();

                for (int i = 0; i < factions.Count; i++)
                {
                    Faction fact1 = factions[i];
                    for (int j = 0; j < factions.Count; j++)
                    {
                        Faction fact2 = factions[j];

                        if (!fact1.team.IsEnemyOf(fact2.team))
                        {
                            fact1.team.SetIsEnemyOf(fact2.team, true);
                        }
                    }
                }
            }

        }

        public void AddFaction(int factionIndex, Faction f)
        {
            this.Factions[factionIndex] = f;
            if (GameNetwork.IsClient)
            {
                if (this.FactionBanners.ContainsKey(factionIndex))
                {
                    this.FactionBanners[factionIndex].SetBannerKey(f.banner.Serialize());
                }
            }

            if (GameNetwork.IsServer)
            {
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new AddFaction(f, factionIndex));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            }

            if (this.OnFactionAdded != null)
            {
                this.OnFactionAdded(f, factionIndex);
            }
        }

        public void RequestDeclareWar(Faction faction, int factionIndex)
        {
            if (GameNetwork.IsClient)
            {
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new DeclareWarRequest(factionIndex));
                GameNetwork.EndModuleEventAsClient();
            }
        }
        public void RequestMakePeace(Faction faction, int factionIndex)
        {
            if (GameNetwork.IsClient)
            {
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new MakePeaceRequest(factionIndex));
                GameNetwork.EndModuleEventAsClient();
            }
        }
        public void RequestUpdateFactionBanner(String bannerCode)
        {
            if (GameNetwork.IsClient)
            {
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new UpdateFactionBanner(bannerCode));
                GameNetwork.EndModuleEventAsClient();
            }
        }

        public void RequestChestKeyForUser(NetworkCommunicator member)
        {
            if (GameNetwork.IsClient)
            {
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new RequestChestKey(member));
                GameNetwork.EndModuleEventAsClient();
            }
        }
        public void RequestKickFromFaction(NetworkCommunicator member)
        {
            if (GameNetwork.IsClient)
            {
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new KickFromFaction(member));
                GameNetwork.EndModuleEventAsClient();
            }
        }
        public void RequestDoorKeyForUser(NetworkCommunicator member)
        {
            if (GameNetwork.IsClient)
            {
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new RequestDoorKey(member));
                GameNetwork.EndModuleEventAsClient();
            }
        }

        private bool HandleRequestKickFromFaction(NetworkCommunicator sender, KickFromFaction message)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = sender.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null)
            {
                return false;
            }
            Faction f = persistentEmpireRepresentative.GetFaction();
            if (f.lordId != sender.VirtualPlayer.Id.ToString() && f.marshalls.Contains(sender.VirtualPlayer.Id.ToString()) == false)
            {
                this._informationComponent.SendAnnouncementToPlayer("You don't have permission to do this.", sender);
                return false;
            }
            NetworkCommunicator targetPlayer = message.Target;
            PersistentEmpireRepresentative targetRepresentative = targetPlayer.GetComponent<PersistentEmpireRepresentative>();
            if (targetRepresentative == null) return false;
            if (targetPlayer.Equals(sender) && f.lordId != targetPlayer.VirtualPlayer.Id.ToString())
            {
                this._informationComponent.SendAnnouncementToPlayer("You can't kick yourself or the lord.", sender);
                return false;
            }
            if (f.lordId == targetPlayer.VirtualPlayer.Id.ToString())
            {
                this._informationComponent.SendAnnouncementToPlayer("You can't kick the lord", sender);
                return false;
            }
            if (f.marshalls.Contains(targetPlayer.VirtualPlayer.Id.ToString()))
            {
                this._informationComponent.SendAnnouncementToPlayer("You can't kick another marshall", sender);
                return false;
            }
            if (targetRepresentative.GetFactionIndex() != persistentEmpireRepresentative.GetFactionIndex())
            {
                this._informationComponent.SendAnnouncementToPlayer("This player is not in same faction with you", sender);
                return false;
            }
            this.SetPlayerFaction(targetPlayer, 0, targetRepresentative.GetFactionIndex());
            targetPlayer.ControlledAgent.SetClothingColor1(this.Factions[0].banner.GetPrimaryColor());
            targetPlayer.ControlledAgent.SetClothingColor2(this.Factions[0].banner.GetFirstIconColor());
            targetRepresentative.KickedFromFaction = true;
            return true;

        }

        public void UpdateFactionBanner(int factionIndex, string banner)
        {
            if (banner.Length > MaxBannerLength)
            {
                return;
            }
            this.Factions[factionIndex].banner = new Banner(banner);
            if (this.FactionBanners.ContainsKey(factionIndex))
            {
                this.FactionBanners[factionIndex].SetBannerKey(banner);
            }
            foreach (NetworkCommunicator member in this.Factions[factionIndex].members)
            {
                if (member.ControlledAgent == null) continue;
                member.ControlledAgent.SetClothingColor1(this.Factions[factionIndex].banner.GetPrimaryColor());
                member.ControlledAgent.SetClothingColor2(this.Factions[factionIndex].banner.GetFirstIconColor());
            }
            GameNetwork.BeginBroadcastModuleEvent();
            GameNetwork.WriteMessage(new UpdateFactionFromServer(this.Factions[factionIndex], factionIndex));
            GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
        }

        public void UpdateFactionName(int factionIndex, string name)
        {
            this.Factions[factionIndex].name = name;
            SaveSystemBehavior.HandleCreateOrSaveFaction(this.Factions[factionIndex], factionIndex);
            GameNetwork.BeginBroadcastModuleEvent();
            GameNetwork.WriteMessage(new UpdateFactionFromServer(this.Factions[factionIndex], factionIndex));
            GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
        }

        public bool HandleRequestUpdateFactionBanner(NetworkCommunicator player, UpdateFactionBanner updateFactionBanner)
        {

            PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null) return false;
            int factionIndex = persistentEmpireRepresentative.GetFactionIndex();
            if (this.Factions[factionIndex].lordId != player.VirtualPlayer.Id.ToString())
            {
                this._informationComponent.SendAnnouncementToPlayer("You don't have permission to do that", player);
                return false;
            }
            //if(updateFactionBanner.BannerCode.Split('.').Length % 10 != 0)
            //{
            //    InformationComponent.Instance.SendMessage("Banner data is invalid.", //Colors.Red.ToUnsignedInteger(), player);
            //    return false;
            //}
            if (updateFactionBanner.BannerCode.Length > this.MaxBannerLength)
            {
                InformationComponent.Instance.SendMessage("Banner length is too much.", Colors.Red.ToUnsignedInteger(), player);
                return false;
            }
            try
            {
                List<BannerData> bannerDatas = Banner.GetBannerDataFromBannerCode(updateFactionBanner.BannerCode);
                for (int i = 0; i < bannerDatas.Count; i++)
                {
                    BannerData bannerData = bannerDatas[i];
                    if (bannerData.MeshId > CompressionBasic.BannerDataMeshIdCompressionInfo.GetMaximumValue())
                    {
                        InformationComponent.Instance.SendMessage("This banner code is invalid. One of the icons is not valid " + i, Colors.Red.ToUnsignedInteger(), player);
                        return false;
                    }
                    if (bannerData.MeshId < 0)
                    {
                        InformationComponent.Instance.SendMessage("This banner code is invalid. One of the icons is not valid " + i, Colors.Red.ToUnsignedInteger(), player);
                        return false;
                    }
                    if (bannerData.ColorId > CompressionBasic.BannerDataColorIndexCompressionInfo.GetMaximumValue())
                    {
                        InformationComponent.Instance.SendMessage("This banner code is invalid. One of the icon's or banner's color is invalid " + i, Colors.Red.ToUnsignedInteger(), player);
                        return false;
                    }
                    if (bannerData.ColorId < 0)
                    {
                        InformationComponent.Instance.SendMessage("This banner code is invalid. One of the icon's or banner's color is invalid " + i, Colors.Red.ToUnsignedInteger(), player);
                        return false;
                    }

                    if (bannerData.ColorId2 > CompressionBasic.BannerDataColorIndexCompressionInfo.GetMaximumValue())
                    {
                        InformationComponent.Instance.SendMessage("This banner code is invalid. One of the icon's or banner's color is invalid " + i, Colors.Red.ToUnsignedInteger(), player);
                        return false;
                    }
                    if (bannerData.ColorId2 < 0)
                    {
                        InformationComponent.Instance.SendMessage("This banner code is invalid. One of the icon's or banner's color is invalid " + i, Colors.Red.ToUnsignedInteger(), player);
                        return false;
                    }

                    if ((int)bannerData.Size.X > CompressionBasic.BannerDataSizeCompressionInfo.GetMaximumValue())
                    {
                        InformationComponent.Instance.SendMessage("This banner code is invalid. One of the icon size is too big " + i, Colors.Red.ToUnsignedInteger(), player);
                        return false;
                    }
                    if ((int)bannerData.Size.X < -8000)
                    {
                        InformationComponent.Instance.SendMessage("This banner code is invalid. One of the icon size is too small " + i, Colors.Red.ToUnsignedInteger(), player);
                        return false;
                    }
                    if ((int)bannerData.Size.Y > CompressionBasic.BannerDataSizeCompressionInfo.GetMaximumValue())
                    {
                        InformationComponent.Instance.SendMessage("This banner code is invalid. One of the icon size is too big " + i, Colors.Red.ToUnsignedInteger(), player);
                        return false;
                    }
                    if ((int)bannerData.Size.Y < -8000)
                    {
                        InformationComponent.Instance.SendMessage("This banner code is invalid. One of the icon size is too small " + i, Colors.Red.ToUnsignedInteger(), player);
                        return false;
                    }

                    if ((int)bannerData.Position.X > CompressionBasic.BannerDataSizeCompressionInfo.GetMaximumValue())
                    {
                        InformationComponent.Instance.SendMessage("This banner code is invalid. One of the icon position is invalid " + i, Colors.Red.ToUnsignedInteger(), player);
                        return false;
                    }
                    if ((int)bannerData.Position.X < -8000)
                    {
                        InformationComponent.Instance.SendMessage("This banner code is invalid. One of the icon position is invalid " + i, Colors.Red.ToUnsignedInteger(), player);
                        return false;
                    }
                    if ((int)bannerData.Position.Y > CompressionBasic.BannerDataSizeCompressionInfo.GetMaximumValue())
                    {
                        InformationComponent.Instance.SendMessage("This banner code is invalid. One of the icon position is invalid " + i, Colors.Red.ToUnsignedInteger(), player);
                        return false;
                    }
                    if ((int)bannerData.Position.Y < -8000)
                    {
                        InformationComponent.Instance.SendMessage("This banner code is invalid. One of the icon position is invalid " + i, Colors.Red.ToUnsignedInteger(), player);
                        return false;
                    }
                    if ((int)bannerData.Rotation > 360)
                    {
                        InformationComponent.Instance.SendMessage("This banner code is invalid. One of the icon's rotation is invalid " + i, Colors.Red.ToUnsignedInteger(), player);
                        return false;
                    }
                    if ((int)bannerData.Rotation < -360)
                    {
                        InformationComponent.Instance.SendMessage("This banner code is invalid. One of the icon's rotation is invalid " + i, Colors.Red.ToUnsignedInteger(), player);
                        return false;
                    }
                }

                this.UpdateFactionBanner(factionIndex, updateFactionBanner.BannerCode);
            }
            catch (Exception e)
            {
                InformationComponent.Instance.SendMessage("An error hapened while parsing banner code.", Colors.Red.ToUnsignedInteger(), player);
                return false;
            }


            return true;
        }

        private bool HandleRequestChestKey(NetworkCommunicator player, RequestChestKey message)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null) return false;
            int factionIndex = persistentEmpireRepresentative.GetFactionIndex();
            if (this.Factions[factionIndex].lordId != player.VirtualPlayer.Id.ToString() && this.Factions[factionIndex].marshalls.Contains(player.VirtualPlayer.Id.ToString()) == false)
            {
                this._informationComponent.SendAnnouncementToPlayer("You don't have permission to do that", player);
                return false;
            }
            Faction f = this.Factions[factionIndex];
            if (f.chestManagers.Contains(message.Player.VirtualPlayer.Id.ToString()))
            {
                f.chestManagers.Remove(message.Player.VirtualPlayer.Id.ToString());
                this._informationComponent.SendAnnouncementToPlayer("Chest keys taken", message.Player);
                this._informationComponent.SendAnnouncementToPlayer("Chest keys taken from " + message.Player.UserName, player);
            }
            else
            {
                f.chestManagers.Add(message.Player.VirtualPlayer.Id.ToString());
                this._informationComponent.SendAnnouncementToPlayer("Chest keys given", message.Player);
                this._informationComponent.SendAnnouncementToPlayer("Chest keys given to " + message.Player.UserName, player);
            }
            return true;
        }
        private bool HandleRequestDoorKey(NetworkCommunicator player, RequestDoorKey message)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null) return false;
            int factionIndex = persistentEmpireRepresentative.GetFactionIndex();
            if (this.Factions[factionIndex].lordId != player.VirtualPlayer.Id.ToString() && this.Factions[factionIndex].marshalls.Contains(player.VirtualPlayer.Id.ToString()) == false)
            {
                this._informationComponent.SendAnnouncementToPlayer("Door is locked", player);
                return false;
            }
            Faction f = this.Factions[factionIndex];
            if (f.doorManagers.Contains(message.Player.VirtualPlayer.Id.ToString()))
            {
                f.doorManagers.Remove(message.Player.VirtualPlayer.Id.ToString());
                if (message.Player.IsConnectionActive)
                    this._informationComponent.SendAnnouncementToPlayer("Door keys taken", message.Player);
                this._informationComponent.SendAnnouncementToPlayer("Door keys taken from " + message.Player.UserName, player);
            }
            else
            {
                f.doorManagers.Add(message.Player.VirtualPlayer.Id.ToString());
                if (message.Player.IsConnectionActive)
                    this._informationComponent.SendAnnouncementToPlayer("Door keys given", message.Player);
                this._informationComponent.SendAnnouncementToPlayer("Door keys given to " + message.Player.UserName, player);
            }
            return true;
        }
        public bool HandleRequestUpdateFactionName(NetworkCommunicator player, UpdateFactionName updateFactionName)
        {
            // Check If user Is Elligeble to change name;
            PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null) return false;
            int factionIndex = persistentEmpireRepresentative.GetFactionIndex();
            if (this.Factions[factionIndex].lordId != player.VirtualPlayer.Id.ToString())
            {
                this._informationComponent.SendAnnouncementToPlayer("You don't have permission to do that", player);
                return false;
            }
            this.UpdateFactionName(factionIndex, updateFactionName.NewName);
            return true;
        }
        private bool HandleRequestDeclareWar(NetworkCommunicator player, DeclareWarRequest message)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null) return false;
            int playerFactionIndex = persistentEmpireRepresentative.GetFactionIndex();
            Faction f = this.Factions[playerFactionIndex];
            string playerId = player.VirtualPlayer.Id.ToString();
            if (f.lordId != playerId && f.marshalls.Contains(playerId) == false)
            {
                this._informationComponent.SendAnnouncementToPlayer("You are not the lord", player);
                return false;
            }
            if (message.FactionIndex == playerFactionIndex)
            {
                this._informationComponent.SendAnnouncementToPlayer("You can't declare war to yourself", player);
                return false;
            }
            if (message.FactionIndex <= 1)
            {
                this._informationComponent.SendAnnouncementToPlayer("You can't do that", player);
                return false;
            }
            if (f.warDeclaredTo.Contains(message.FactionIndex))
            {
                this._informationComponent.SendAnnouncementToPlayer("You already declared a war to this faction", player);
                return false;
            }
            if (FactionDeclaredWarLast.ContainsKey(playerFactionIndex) && DateTimeOffset.UtcNow.ToUnixTimeSeconds() < FactionDeclaredWarLast[playerFactionIndex] + WarDeclareTimeOut)
            {
                this._informationComponent.SendMessage("Please wait 30 sec to declare a new war.", new Color(1f, 0f, 0f).ToUnsignedInteger(), player);
                return false;
            }
            FactionDeclaredWarLast[playerFactionIndex] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            f.warDeclaredTo.Add(message.FactionIndex);
            GameNetwork.BeginBroadcastModuleEvent();
            GameNetwork.WriteMessage(new WarDecleration(playerFactionIndex, message.FactionIndex));
            GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            LoggerHelper.LogAnAction(player, LogAction.FactionDeclaredWar, null, new object[] { this.Factions[message.FactionIndex] });
            return true;
        }

        private bool HandleRequestMakePeace(NetworkCommunicator player, MakePeaceRequest message)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null) return false;
            int playerFactionIndex = persistentEmpireRepresentative.GetFactionIndex();
            Faction f = this.Factions[playerFactionIndex];
            string playerId = player.VirtualPlayer.Id.ToString();
            if (f.lordId != playerId && f.marshalls.Contains(playerId) == false)
            {
                this._informationComponent.SendAnnouncementToPlayer("You are not the lord", player);
                return false;
            }
            if (message.FactionIndex == playerFactionIndex)
            {
                this._informationComponent.SendAnnouncementToPlayer("You can't make peace with yourself", player);
                return false;
            }
            if (message.FactionIndex <= 1)
            {
                this._informationComponent.SendAnnouncementToPlayer("You can't do that", player);
                return false;
            }
            if (!f.warDeclaredTo.Contains(message.FactionIndex))
            {
                this._informationComponent.SendAnnouncementToPlayer("You didn't declared a war.", player);
                return false;
            }
            if (FactionDeclaredWarLast.ContainsKey(playerFactionIndex) && DateTimeOffset.UtcNow.ToUnixTimeSeconds() < FactionDeclaredWarLast[playerFactionIndex] + PeaceDeclareTimeOut)
            {
                this._informationComponent.SendMessage("Please wait 30 secs to declare a peace.", new Color(1f, 0f, 0f).ToUnsignedInteger(), player);
                return false;
            }
            FactionDeclaredWarLast[playerFactionIndex] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            f.warDeclaredTo.Remove(message.FactionIndex);
            GameNetwork.BeginBroadcastModuleEvent();
            GameNetwork.WriteMessage(new PeaceDecleration(playerFactionIndex, message.FactionIndex));
            GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            LoggerHelper.LogAnAction(player, LogAction.FactionMadePeace, null, new object[] { this.Factions[message.FactionIndex] });
            return true;
        }

        public void RequestUpdateFactionName(String NewName)
        {
            if (GameNetwork.IsClient)
            {
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new UpdateFactionName(NewName));
                GameNetwork.EndModuleEventAsClient();
            }
        }

        public void SetPlayerFaction(NetworkCommunicator player, int factionIndex, int joinedFrom)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();

            if (persistentEmpireRepresentative == null) return;


            if (persistentEmpireRepresentative.GetFactionIndex() != -1)
            {
                if (factionIndex != persistentEmpireRepresentative.GetFactionIndex())
                {
                    Faction joinedFromF = this.Factions[persistentEmpireRepresentative.GetFactionIndex()];
                    if (factionIndex != -1)
                    {
                        if (joinedFromF.chestManagers.Contains(player.VirtualPlayer.Id.ToString()))
                        {
                            joinedFromF.chestManagers.Remove(player.VirtualPlayer.Id.ToString());
                        }
                        if (joinedFromF.doorManagers.Contains(player.VirtualPlayer.Id.ToString()))
                        {
                            joinedFromF.doorManagers.Remove(player.VirtualPlayer.Id.ToString());
                        }
                        if (joinedFromF.lordId == player.VirtualPlayer.Id.ToString())
                        {
                            joinedFromF.lordId = "";
                            joinedFromF.pollUnlockedAt = 0;
                            joinedFromF.marshalls.Clear();
                            joinedFromF.chestManagers.Clear();
                            joinedFromF.doorManagers.Clear();
                            SaveSystemBehavior.HandleCreateOrSaveFaction(joinedFromF, persistentEmpireRepresentative.GetFactionIndex());
                        }
                        if (joinedFromF.marshalls.Contains(player.VirtualPlayer.Id.ToString()))
                        {
                            joinedFromF.marshalls.Remove(player.VirtualPlayer.Id.ToString());
                        }
                    }
                }
                this.Factions[persistentEmpireRepresentative.GetFactionIndex()].members.Remove(player);
            }
            if (factionIndex != -1)
            {
                LoggerHelper.LogAnAction(player, LogAction.PlayerFactionChange, null, new object[] { persistentEmpireRepresentative.GetFaction(), this.Factions[factionIndex] });
                persistentEmpireRepresentative.SetFaction(this.Factions[factionIndex], factionIndex);
                player.VirtualPlayer.BannerCode = this.Factions[factionIndex].banner.Serialize();
                this.Factions[factionIndex].members.Add(player);
            }
            else
            {
                persistentEmpireRepresentative.SetFaction(null, factionIndex);
            }

            if (GameNetwork.IsServer)
            {
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new PlayerJoinedFaction(factionIndex, joinedFrom, player));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            }
            if (this.OnPlayerJoinedFaction != null)
            {
                this.OnPlayerJoinedFaction(factionIndex, factionIndex == -1 ? null : this.Factions[factionIndex], joinedFrom, player);
            }
        }

        public void AssignMarshall(NetworkCommunicator player, int factionIndex)
        {
            bool updateStatus = true;
            Faction f = this.Factions[factionIndex];
            updateStatus = f.marshalls.Contains(player.VirtualPlayer.Id.ToString()) == false;
            if (updateStatus)
                f.marshalls.Add(player.VirtualPlayer.Id.ToString());
            else
                f.marshalls.Remove(player.VirtualPlayer.Id.ToString());
            if (GameNetwork.IsServer)
            {
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new FactionUpdateMarshall(factionIndex, player, updateStatus));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
                SaveSystemBehavior.HandleCreateOrSaveFaction(f, factionIndex);
            }
        }
        public void SetFactionLord(NetworkCommunicator player, int factionIndex)
        {

            Faction f = this.Factions[factionIndex];
            if (player.VirtualPlayer.Id.ToString() != f.lordId)
            {
                f.chestManagers.Clear();
                f.doorManagers.Clear();
                f.marshalls.Clear();
            }

            f.lordId = player.VirtualPlayer.Id.ToString();
            if (GameNetwork.IsServer)
            {
                SaveSystemBehavior.HandleCreateOrSaveFaction(f, factionIndex);

                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new FactionUpdateLord(factionIndex, player));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);

                LoggerHelper.LogAnAction(player, LogAction.FactionLordChanged);
            }
            else if (GameNetwork.IsClient)
            {
                MBInformationManager.AddQuickInformation(new TextObject("{PLAYER} is now lord of {FACTION}").SetTextVariable("PLAYER", player.UserName).SetTextVariable("FACTION", f.name));
            }
            if (this.OnFactionLordChanged != null)
            {
                this.OnFactionLordChanged(f, factionIndex, player);
            }
        }

        public override void OnPlayerDisconnectedFromServer(NetworkCommunicator networkPeer)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = networkPeer.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null) return;

            this.SetPlayerFaction(networkPeer, -1, persistentEmpireRepresentative.GetFactionIndex());

            base.OnPlayerDisconnectedFromServer(networkPeer);
            // Clean up
        }

        public void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
        {
            GameNetwork.NetworkMessageHandlerRegisterer networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(mode);
            if (GameNetwork.IsClient)
            {
                networkMessageHandlerRegisterer.Register<AddFaction>(this.HandleAddFaction);
                networkMessageHandlerRegisterer.Register<PlayerJoinedFaction>(this.HandlePlayerJoinedFaction);
                networkMessageHandlerRegisterer.Register<UpdateFactionFromServer>(this.HandleUpdateFactionFromServer);
                networkMessageHandlerRegisterer.Register<FactionUpdateLord>(this.HandleFactionUpdateLordFromServer);
                networkMessageHandlerRegisterer.Register<PeaceDecleration>(this.HandlePeaceDeclerationFromServer);
                networkMessageHandlerRegisterer.Register<WarDecleration>(this.HandleWarDeclerationFromServer);
                networkMessageHandlerRegisterer.Register<FactionUpdateMarshall>(this.HandleFactionUpdateMarshallFromServer);
                networkMessageHandlerRegisterer.Register<SyncFactionKey>(this.HandleSyncFactionKeyFromServer);

            }
            if (GameNetwork.IsServer)
            {
                networkMessageHandlerRegisterer.Register<UpdateFactionBanner>(this.HandleRequestUpdateFactionBanner);
                networkMessageHandlerRegisterer.Register<UpdateFactionName>(this.HandleRequestUpdateFactionName);
                networkMessageHandlerRegisterer.Register<KickFromFaction>(this.HandleRequestKickFromFaction);
                networkMessageHandlerRegisterer.Register<RequestDoorKey>(this.HandleRequestDoorKey);
                networkMessageHandlerRegisterer.Register<RequestChestKey>(this.HandleRequestChestKey);
                networkMessageHandlerRegisterer.Register<MakePeaceRequest>(this.HandleRequestMakePeace);
                networkMessageHandlerRegisterer.Register<DeclareWarRequest>(this.HandleRequestDeclareWar);
                networkMessageHandlerRegisterer.Register<FactionAssignMarshall>(this.HandleClientFactionAssignMarshall);
                networkMessageHandlerRegisterer.Register<RequestLordshipTransfer>(this.HandleClientRequestLordshipTransfer);
                networkMessageHandlerRegisterer.Register<RequestFactionKeys>(this.HandleRequestFactionKeys);

            }
        }

        private void HandleSyncFactionKeyFromServer(SyncFactionKey message)
        {
            if (this.OnFactionKeyFetched != null)
            {
                this.OnFactionKeyFetched(message.FactionIndex, message.PlayerId, message.KeyType);
            }
        }

        private bool HandleRequestFactionKeys(NetworkCommunicator peer, RequestFactionKeys message)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = peer.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null || persistentEmpireRepresentative.GetFaction() == null) return true;

            if (message.KeyType == 0)
            {
                foreach (string playerId in persistentEmpireRepresentative.GetFaction().doorManagers)
                {
                    GameNetwork.BeginModuleEventAsServer(peer);
                    GameNetwork.WriteMessage(new SyncFactionKey(persistentEmpireRepresentative.GetFactionIndex(), playerId, 0));
                    GameNetwork.EndModuleEventAsServer();
                }
            }
            if (message.KeyType == 1)
            {
                foreach (string playerId in persistentEmpireRepresentative.GetFaction().chestManagers)
                {
                    GameNetwork.BeginModuleEventAsServer(peer);
                    GameNetwork.WriteMessage(new SyncFactionKey(persistentEmpireRepresentative.GetFactionIndex(), playerId, 1));
                    GameNetwork.EndModuleEventAsServer();
                }
            }

            return true;
        }

        private void HandleFactionUpdateMarshallFromServer(FactionUpdateMarshall message)
        {
            //this.AssignMarshall(message.TargetPlayer, message.FactionIndex);
            NetworkCommunicator player = message.TargetPlayer;
            int factionIndex = message.FactionIndex;
            Faction f = this.Factions[message.FactionIndex];

            if (message.IsMarshall) f.marshalls.Add(message.TargetPlayer.VirtualPlayer.Id.ToString());
            else f.marshalls.Remove(message.TargetPlayer.VirtualPlayer.Id.ToString());

            if (GameNetwork.IsClient)
            {
                PersistentEmpireRepresentative myRepr = GameNetwork.MyPeer.GetComponent<PersistentEmpireRepresentative>();
                if (myRepr == null) return;
                if (myRepr.GetFactionIndex() != factionIndex) return;
                if (message.IsMarshall)
                {
                    MBInformationManager.AddQuickInformation(new TaleWorlds.Localization.TextObject(player.UserName + " is your new marshall !"));
                }
            }

            if (this.OnFactionMarshallChanged != null)
            {
                this.OnFactionMarshallChanged(f, factionIndex, player);
            }
        }

        private bool HandleClientRequestLordshipTransfer(NetworkCommunicator peer, RequestLordshipTransfer message)
        {
            PersistentEmpireRepresentative repr = peer.GetComponent<PersistentEmpireRepresentative>();
            if (repr == null) return true;
            if (repr.GetFaction() == null) return true;
            if (repr.GetFaction().lordId != peer.VirtualPlayer.Id.ToString()) return true;
            PersistentEmpireRepresentative repr2 = message.TargetPlayer.GetComponent<PersistentEmpireRepresentative>();
            if (repr.GetFactionIndex() != repr2.GetFactionIndex()) return true;
            this.SetFactionLord(message.TargetPlayer, repr.GetFactionIndex());
            return true;
        }

        private bool HandleClientFactionAssignMarshall(NetworkCommunicator peer, FactionAssignMarshall message)
        {
            PersistentEmpireRepresentative repr = peer.GetComponent<PersistentEmpireRepresentative>();
            if (repr == null) return true;
            if (repr.GetFaction() == null) return true;
            if (repr.GetFaction().lordId != peer.VirtualPlayer.Id.ToString()) return true;
            PersistentEmpireRepresentative repr2 = message.TargetPlayer.GetComponent<PersistentEmpireRepresentative>();
            if (repr.GetFactionIndex() != repr2.GetFactionIndex()) return true;
            this.AssignMarshall(message.TargetPlayer, repr.GetFactionIndex());
            return true;
        }

        private void HandleWarDeclerationFromServer(WarDecleration message)
        {
            int declarerIndex = message.WarDeclarerIndex;
            int declaredToIndex = message.WarDeclaredTo;
            Faction declarerFaction = this.Factions[declarerIndex];
            Faction declaredFaction = this.Factions[declaredToIndex];
            declarerFaction.warDeclaredTo.Add(declaredToIndex);
            TextObject t = new TextObject("{Declarer} has declared War on {Declared}");
            t.SetTextVariable("Declarer", declarerFaction.name);
            t.SetTextVariable("Declared", declaredFaction.name);
            MBInformationManager.AddQuickInformation(t);
            if (this.OnFactionDeclaredWar != null)
            {
                this.OnFactionDeclaredWar(declarerIndex, declaredToIndex);
            }
        }

        private void HandlePeaceDeclerationFromServer(PeaceDecleration message)
        {
            int declarerIndex = message.PeaceDeclarerIndex;
            int declaredToIndex = message.PeaceDeclaredTo;
            Faction declarerFaction = this.Factions[declarerIndex];
            Faction declaredFaction = this.Factions[declaredToIndex];
            declarerFaction.warDeclaredTo.Remove(declaredToIndex);
            TextObject t = new TextObject("{Declarer} has made peace with {Declared}");
            t.SetTextVariable("Declarer", declarerFaction.name);
            t.SetTextVariable("Declared", declaredFaction.name);
            MBInformationManager.AddQuickInformation(t);
            if (this.OnFactionMakePeace != null)
            {
                this.OnFactionMakePeace(declarerIndex, declaredToIndex);
            }
        }

        private void HandleFactionUpdateLordFromServer(FactionUpdateLord message)
        {
            this.SetFactionLord(message.Player, message.FactionIndex);
        }

        protected void HandleAddFaction(AddFaction packet)
        {
            InformationManager.DisplayMessage(new InformationMessage("Faction added " + packet.factionIndex + " " + packet.faction.name));
            this.AddFaction(packet.factionIndex, packet.faction);
        }
        protected void HandlePlayerJoinedFaction(PlayerJoinedFaction packet)
        {
            this.SetPlayerFaction(packet.player, packet.factionIndex, packet.joinedFrom);
            if (packet.player.IsMine)
            {
                Faction f = this.Factions[packet.factionIndex];
                InformationManager.DisplayMessage(new InformationMessage("You have joined " + f.name, Color.FromUint(f.banner.GetPrimaryColor())));
            }
        }

        protected void HandleUpdateFactionFromServer(UpdateFactionFromServer updateFactionFromServer)
        {
            string newBanner = updateFactionFromServer.BannerCode;
            string newName = updateFactionFromServer.Name;
            int factionIndex = updateFactionFromServer.FactionIndex;

            if (!this.Factions[factionIndex].name.Equals(newName))
            {
                TextObject t = new TextObject("{OldName} Now Known As {NewName}");
                t.SetTextVariable("OldName", this.Factions[factionIndex].name);
                t.SetTextVariable("NewName", newName);
                MBInformationManager.AddQuickInformation(t);
                this.Factions[factionIndex].name = newName;
            }
            if (!this.Factions[factionIndex].banner.Serialize().Equals(newBanner))
            {
                this.Factions[factionIndex].banner = new Banner(newBanner);
                if (this.FactionBanners.ContainsKey(factionIndex))
                {
                    this.FactionBanners[factionIndex].SetBannerKey(newBanner);
                }

                CastlesBehavior cb = base.Mission.GetMissionBehavior<CastlesBehavior>();
                cb.ReloadCastleBanner(factionIndex);
                foreach (NetworkCommunicator member in this.Factions[factionIndex].members)
                {
                    if (member.ControlledAgent == null) continue;
                    member.ControlledAgent.SetClothingColor1(this.Factions[factionIndex].banner.GetPrimaryColor());
                    member.ControlledAgent.SetClothingColor2(this.Factions[factionIndex].banner.GetFirstIconColor());
                    AgentHelpers.ResetAgentMesh(member.ControlledAgent);
                }
            }
            if (this.OnFactionUpdated != null)
            {
                this.OnFactionUpdated(factionIndex, this.Factions[factionIndex]);
            }

        }
    }
}
