using PersistentEmpiresLib.Database.DBEntities;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ModuleManager;
using TaleWorlds.ObjectSystem;
using TaleWorlds.MountAndBlade.DedicatedCustomServer;

namespace PersistentEmpiresServer.ServerMissions
{
    public class AdminServerBehavior : MissionNetwork
    {
        private PatreonRegistryBehavior patreonRegistry;
        public string BanFile = "BannedPlayers.txt";
        public string AdminFile = "AdminPlayers.txt";
        public bool DisableGlobalChat = false;

        public Dictionary<NetworkCommunicator, bool> Freezed = new Dictionary<NetworkCommunicator, bool>();
        public Dictionary<NetworkCommunicator, long> LastChangedName = new Dictionary<NetworkCommunicator, long>();

        public delegate bool IsPlayerBannedDelegate(string PlayerId);
        public static event IsPlayerBannedDelegate OnIsPlayerBanned;

        public delegate void BanPlayerDelegate(string PlayerId, string PlayerName, long BanEndsAt);
        public static event BanPlayerDelegate OnBanPlayer;

        public static AdminServerBehavior Instance { get; private set; }

        public string BanFilePath()
        {
            return ModuleHelper.GetModuleFullPath("PersistentEmpires") + this.BanFile;
        }
        public string AdminPlayerFilePath()
        {
            return ModuleHelper.GetModuleFullPath("PersistentEmpires") + this.AdminFile;
        }
        public void BanPlayer(NetworkCommunicator player, int seconds)
        {
            long bannedUntil = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + seconds;
            if (!File.Exists(BanFilePath()))
            {
                File.Create(BanFilePath()).Close();
            }
            using (StreamWriter sw = File.AppendText(BanFilePath()))
            {
                sw.WriteLine(player.UserName + "|" + player.VirtualPlayer.Id.ToString() + "|" + bannedUntil.ToString());
            }

            if (player.IsConnectionActive)
            {
                DedicatedCustomServerSubModule.Instance.DedicatedCustomGameServer.KickPlayer(player.VirtualPlayer.Id, false);
            }
        }



        protected override void HandleLateNewClientAfterSynchronized(NetworkCommunicator networkPeer)
        {
            base.HandleLateNewClientAfterSynchronized(networkPeer);
            if (networkPeer.IsConnectionActive == false || networkPeer.IsNetworkActive == false) return;
            if (GameNetwork.IsClientOrReplay) return;
            if (IsPlayerBanned(networkPeer))
            {
                InformationComponent.Instance.SendAnnouncementToPlayer("You are banned from the server. Please refer to discord server for information", networkPeer);
                InformationComponent.Instance.SendMessage("You are banned from the server. Please refer to discord server for information", Color.ConvertStringToColor("#d32f2fff").ToUnsignedInteger(), networkPeer);
                Task.Delay(3000).ContinueWith(_ =>
                {
                    if (networkPeer != null && networkPeer.IsConnectionActive)
                    {
                        DedicatedCustomServerSubModule.Instance.DedicatedCustomGameServer.KickPlayer(networkPeer.VirtualPlayer.Id, false);
                    }
                });
            }
            if (IsPlayerAdmin(networkPeer))
            {
                networkPeer.GetComponent<PersistentEmpireRepresentative>().IsAdmin = true;
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new AuthorizeAsAdmin());
                GameNetwork.EndModuleEventAsServer();
            }
        }
        public bool IsPlayerAdmin(NetworkCommunicator player)
        {
            if (!File.Exists(AdminPlayerFilePath())) return false;

            string[] lines = File.ReadAllLines(AdminPlayerFilePath());

            foreach (string line in lines)
            {
                if (line.Trim().Equals("")) continue;
                string adminId = line.Trim();

                if (player.VirtualPlayer.Id.ToString().Equals(adminId))
                {
                    return true;
                }
            }

            return false;
        }
        public bool IsPlayerBanned(NetworkCommunicator player)
        {
            if (OnIsPlayerBanned != null)
            {
                return OnIsPlayerBanned(player.VirtualPlayer.Id.ToString());
            }

            if (!File.Exists(BanFilePath())) return false;

            string[] lines = File.ReadAllLines(BanFilePath());

            foreach (string line in lines)
            {

                if (line.Trim().Equals("")) continue;
                string[] splitted = line.Trim().Split('|');
                string bannedId = splitted[1];
                long bannedUntil = 0;
                bool parsedSuccess = long.TryParse(splitted[2], out bannedUntil);
                if (!parsedSuccess) continue;
                if (player.VirtualPlayer.Id.ToString().Equals(bannedId) && DateTimeOffset.UtcNow.ToUnixTimeSeconds() < bannedUntil)
                {
                    return true;
                }
            }

            return false;
        }
        public int nameChangeGold = 5000;
        public int cooldown = 3600;
        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            AdminServerBehavior.Instance = this;
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
            this.patreonRegistry = base.Mission.GetMissionBehavior<PatreonRegistryBehavior>();
            nameChangeGold = ConfigManager.GetIntConfig("NameChangeGold", 5000);
            cooldown = ConfigManager.GetIntConfig("NameChangeCooldownInSeconds", 3600);

            Main.IsAdminFunc = IsPlayerAdmin;
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
                networkMessageHandlerRegisterer.Register<RequestTempBan>(this.HandleRequestTempBanFromClient);
                networkMessageHandlerRegisterer.Register<RequestPermBan>(this.HandleRequestPermBanFromClient);
                networkMessageHandlerRegisterer.Register<RequestKick>(this.HandleRequestKickFromClient);
                networkMessageHandlerRegisterer.Register<RequestKill>(this.HandleRequestKillFromClient);
                networkMessageHandlerRegisterer.Register<RequestFade>(this.HandleRequestFadeFromClient);
                networkMessageHandlerRegisterer.Register<RequestFreeze>(this.HandleRequestFreezeFromClient);
                networkMessageHandlerRegisterer.Register<RequestTpToMe>(this.HandleRequestTpToMeFromClient);
                networkMessageHandlerRegisterer.Register<RequestTpTo>(this.HandleRequestTpToFromClient);
                networkMessageHandlerRegisterer.Register<RequestHeal>(this.HandleRequestHealFromClient);
                networkMessageHandlerRegisterer.Register<RequestUnWound>(this.HandleRequestunWoundFromClient);
                networkMessageHandlerRegisterer.Register<RequestItemSpawn>(this.HandleRequestItemSpawn);
                networkMessageHandlerRegisterer.Register<RequestAdminJoinFaction>(this.HandleRequestAdminJoinFaction);
                networkMessageHandlerRegisterer.Register<RequestAdminResetFactionBanner>(this.HandleRequestAdminResetFactionBanner);
                networkMessageHandlerRegisterer.Register<RequestAdminSetFactionName>(this.HandleRequestAdminSetFactionName);
                networkMessageHandlerRegisterer.Register<RequestAdminGold>(this.HandleRequestAdminGold);
                networkMessageHandlerRegisterer.Register<RequestBecameGodlike>(this.HandleRequestBecameGodlike);
                networkMessageHandlerRegisterer.Register<AdminChat>(this.HandleAdminChatFromServer);
            }
        }

        private bool HandleAdminChatFromServer(NetworkCommunicator player, AdminChat message)
        {
            foreach (NetworkCommunicator oPlayer in GameNetwork.NetworkPeers)
            {
                if (oPlayer.IsConnectionActive)
                {
                    PersistentEmpireRepresentative persistentEmpireRepresentative = oPlayer.GetComponent<PersistentEmpireRepresentative>();
                    if (persistentEmpireRepresentative == null) continue;
                    if (persistentEmpireRepresentative.IsAdmin || player == oPlayer)
                    {
                        InformationComponent.Instance.SendMessage(String.Format("[{0}] {1}", player.UserName, message.Message), Color.ConvertStringToColor("#D81B60FF").ToUnsignedInteger(), oPlayer);
                    }
                }
            }
            return true;
        }

        private bool HandleRequestBecameGodlike(NetworkCommunicator player, RequestBecameGodlike message)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
            if (!persistentEmpireRepresentative.IsAdmin)
            {
                return false;
            }
            if (player.ControlledAgent == null || player.ControlledAgent.IsActive() == false)
            {
                return false;
            }
            BasicCharacterObject godlike = MBObjectManager.Instance.GetObject<BasicCharacterObject>("pe_combattest2");
            if (player.ControlledAgent.Character.StringId == godlike.StringId)
            {
                BasicCharacterObject bco = MBObjectManager.Instance.GetObject<BasicCharacterObject>(persistentEmpireRepresentative.GetClassId());
                AgentHelpers.RespawnAgentOnPlaceForFaction(player.ControlledAgent, persistentEmpireRepresentative.GetFaction(), null, bco);
                player.ControlledAgent.BaseHealthLimit = 200;
                player.ControlledAgent.Health = 200;
                LoggerHelper.LogAnAction(player, LogAction.PlayerClassChange, null, new object[] {
                    bco
                });
            }
            else
            {
                AgentHelpers.RespawnAgentOnPlaceForFaction(player.ControlledAgent, persistentEmpireRepresentative.GetFaction(), null, godlike);
                player.ControlledAgent.BaseHealthLimit = 2000;
                player.ControlledAgent.Health = 2000;
                LoggerHelper.LogAnAction(player, LogAction.PlayerBecomesGodlike);

            }


            return true;
        }

        private bool HandleRequestAdminGold(NetworkCommunicator player, RequestAdminGold message)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
            if (!persistentEmpireRepresentative.IsAdmin)
            {
                return false;
            }
            persistentEmpireRepresentative.GoldGain(message.Gold);
            LoggerHelper.LogAnAction(player, LogAction.PlayerSpawnedMoney, null, new object[] { message.Gold });
            return true;
        }

        private bool HandleRequestAdminSetFactionName(NetworkCommunicator player, RequestAdminSetFactionName message)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
            if (!persistentEmpireRepresentative.IsAdmin)
            {
                return false;
            }
            FactionsBehavior factionsBehavior = base.Mission.GetMissionBehavior<FactionsBehavior>();
            factionsBehavior.UpdateFactionName(message.FactionIndex, message.FactionName);
            return true;
        }

        private bool HandleRequestAdminResetFactionBanner(NetworkCommunicator player, RequestAdminResetFactionBanner message)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
            if (!persistentEmpireRepresentative.IsAdmin)
            {
                return false;
            }

            FactionsBehavior factionsBehavior = base.Mission.GetMissionBehavior<FactionsBehavior>();
            factionsBehavior.UpdateFactionBanner(message.FactionIndex, MBObjectManager.Instance.GetObject<BasicCultureObject>("empire").BannerKey);
            return true;
        }

        private bool HandleRequestAdminJoinFaction(NetworkCommunicator player, RequestAdminJoinFaction message)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
            if (!persistentEmpireRepresentative.IsAdmin)
            {
                return false;
            }
            FactionsBehavior factionsBehavior = base.Mission.GetMissionBehavior<FactionsBehavior>();
            factionsBehavior.SetPlayerFaction(player, message.FactionIndex, persistentEmpireRepresentative.GetFactionIndex());
            return true;
        }

        private bool HandleRequestItemSpawn(NetworkCommunicator player, RequestItemSpawn message)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
            if (!persistentEmpireRepresentative.IsAdmin)
            {
                return false;
            }
            string itemId = message.ItemId;
            int count = message.Count;
            if (count == 0) return false;
            ItemObject item = MBObjectManager.Instance.GetObject<ItemObject>(itemId);
            if (item == null)
            {
                InformationComponent.Instance.SendMessage("Item not found", new Color(1f, 0f, 0f).ToUnsignedInteger(), player);
                return false;
            }
            if (persistentEmpireRepresentative.GetInventory().HasEnoughRoomFor(item, count) == false)
            {
                InformationComponent.Instance.SendMessage("You don't have enough room", new Color(1f, 0f, 0f).ToUnsignedInteger(), player);
                return false;
            }

            if (item.IsMountable && player.ControlledAgent != null && player.ControlledAgent.IsActive())
            {
                ItemRosterElement rosterElement = new ItemRosterElement(item, 0, null);
                ItemRosterElement harnessRosterElement = default(ItemRosterElement);
                Agent agent = base.Mission.SpawnMonster(rosterElement, harnessRosterElement, player.ControlledAgent.Position + new Vec3(1, 0, 0), player.ControlledAgent.LookRotation.f.AsVec2, -1);
            }
            else
            {
                int ammo = ItemHelper.GetMaximumAmmo(item);
                persistentEmpireRepresentative.GetInventory().AddCountedItemSynced(item, count, ammo);
                InformationComponent.Instance.SendMessage(item.Name + " added to your inventory.", new Color(0f, 1f, 0f).ToUnsignedInteger(), player);
            }
            LoggerHelper.LogAnAction(player, LogAction.PlayerSpawnsItem, null, new object[] {
                item,
                count
            });
            return true;
        }

        public bool HandleRequestTempBanFromClient(NetworkCommunicator player, RequestTempBan requestTempBan)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
            if (!persistentEmpireRepresentative.IsAdmin)
            {
                return false;
            }
            if (requestTempBan.Player == null)
            {
                return false;
            }
            if (requestTempBan.Player.VirtualPlayer.Id == player.VirtualPlayer.Id)
            {
                InformationComponent.Instance.SendMessage("You can't ban yourself silly", new Color(0f, 0f, 1f).ToUnsignedInteger(), player);
                return false;
            }
            if (OnBanPlayer == null)
            {
                this.BanPlayer(requestTempBan.Player, 1 * 60 * 60); // 6 fuckin hours
            }
            else
            {
                OnBanPlayer(requestTempBan.Player.VirtualPlayer.Id.ToString(), requestTempBan.Player.UserName, DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 1 * 60 * 60);
            }
            LoggerHelper.LogAnAction(player, LogAction.PlayerTempBanPlayer, new AffectedPlayer[] { new AffectedPlayer(requestTempBan.Player) });
            InformationComponent.Instance.SendMessage("Suspect banned for 6 hours.", new Color(0f, 0f, 1f).ToUnsignedInteger(), player);
            return true;
        }
        public bool HandleRequestPermBanFromClient(NetworkCommunicator admin, RequestPermBan message)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = admin.GetComponent<PersistentEmpireRepresentative>();

            if (!persistentEmpireRepresentative.IsAdmin)
            {
                return false;
            }
            if (message.Player == null)
            {
                return false;
            }
            if (message.Player.VirtualPlayer.Id == admin.VirtualPlayer.Id)
            {
                InformationComponent.Instance.SendMessage("You can't ban yourself silly", new Color(0f, 0f, 1f).ToUnsignedInteger(), admin);
                return false;
            }

            if (OnBanPlayer == null)
            {
                this.BanPlayer(message.Player, 10000 * 24 * 60 * 60); // 6 fuckin hours
            }
            else
            {
                OnBanPlayer(message.Player.VirtualPlayer.Id.ToString(), message.Player.UserName, DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 10000 * 24 * 60 * 60);
            }
            InformationComponent.Instance.SendMessage("Suspect banned for 10000 days.", new Color(0f, 0f, 1f).ToUnsignedInteger(), admin);
            LoggerHelper.LogAnAction(admin, LogAction.PlayerBansPlayer, new AffectedPlayer[] { new AffectedPlayer(message.Player) });
            return true;
        }
        public bool HandleRequestKickFromClient(NetworkCommunicator admin, RequestKick message)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = admin.GetComponent<PersistentEmpireRepresentative>();
            if (!persistentEmpireRepresentative.IsAdmin)
            {
                return false;
            }
            if (message.Player == null)
            {
                return false;
            }
            if (message.Player.VirtualPlayer.Id == admin.VirtualPlayer.Id)
            {
                InformationComponent.Instance.SendMessage("You can't ban yourself silly", new Color(0f, 0f, 1f).ToUnsignedInteger(), admin);
                return false;
            }
            if (message.Player.IsConnectionActive)
            {
                DedicatedCustomServerSubModule.Instance.DedicatedCustomGameServer.KickPlayer(message.Player.VirtualPlayer.Id, false);
            }
            LoggerHelper.LogAnAction(admin, LogAction.PlayerKicksPlayer, new AffectedPlayer[] { new AffectedPlayer(message.Player) });
            return true;
        }
        public bool HandleRequestKillFromClient(NetworkCommunicator admin, RequestKill message)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = admin.GetComponent<PersistentEmpireRepresentative>();
            if (!persistentEmpireRepresentative.IsAdmin)
            {
                return false;
            }
            if (message.Player == null)
            {
                return false;
            }
            if (message.Player.ControlledAgent == null || !message.Player.ControlledAgent.IsActive())
            {
                InformationComponent.Instance.SendMessage("Target is not spawned yet", new Color(1f, 0f, 0f).ToUnsignedInteger(), message.Player);
                return false;
            }

            Agent agent = message.Player.ControlledAgent;
            Blow blow = new Blow(agent.Index);
            blow.DamageType = TaleWorlds.Core.DamageTypes.Pierce;
            blow.BoneIndex = agent.Monster.HeadLookDirectionBoneIndex;
            blow.GlobalPosition = agent.Position;
            blow.GlobalPosition.z = blow.GlobalPosition.z + agent.GetEyeGlobalHeight();
            blow.BaseMagnitude = 2000f;
            blow.WeaponRecord.FillAsMeleeBlow(null, null, -1, -1);
            blow.InflictedDamage = 2000;
            blow.SwingDirection = agent.LookDirection;
            MatrixFrame frame = agent.Frame;
            blow.SwingDirection = frame.rotation.TransformToParent(new Vec3(-1f, 0f, 0f, -1f));
            blow.SwingDirection.Normalize();
            blow.Direction = blow.SwingDirection;
            blow.DamageCalculated = true;
            sbyte mainHandItemBoneIndex = agent.Monster.MainHandItemBoneIndex;
            AttackCollisionData attackCollisionDataForDebugPurpose = AttackCollisionData.GetAttackCollisionDataForDebugPurpose(false, false, false, true, false, false, false, false, false, false, false, false, CombatCollisionResult.StrikeAgent, -1, 0, 2, blow.BoneIndex, BoneBodyPartType.Head, mainHandItemBoneIndex, Agent.UsageDirection.AttackLeft, -1, CombatHitResultFlags.NormalHit, 0.5f, 1f, 0f, 0f, 0f, 0f, 0f, 0f, Vec3.Up, blow.Direction, blow.GlobalPosition, Vec3.Zero, Vec3.Zero, agent.Velocity, Vec3.Up);
            agent.RegisterBlow(blow, attackCollisionDataForDebugPurpose);
            LoggerHelper.LogAnAction(admin, LogAction.PlayerSlayPlayer, new AffectedPlayer[] { new AffectedPlayer(message.Player) });
            return true;
        }
        public bool HandleRequestFadeFromClient(NetworkCommunicator admin, RequestFade message)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = admin.GetComponent<PersistentEmpireRepresentative>();
            if (!persistentEmpireRepresentative.IsAdmin)
            {
                return false;
            }
            if (message.Player == null)
            {
                return false;
            }
            if (message.Player.ControlledAgent == null || !message.Player.ControlledAgent.IsActive())
            {
                InformationComponent.Instance.SendMessage("Target is not spawned yet", new Color(1f, 0f, 0f).ToUnsignedInteger(), message.Player);
                return false;
            }
            LoggerHelper.LogAnAction(admin, LogAction.PlayerFadesPlayer, new AffectedPlayer[] { new AffectedPlayer(message.Player) });

            message.Player.ControlledAgent.FadeOut(true, true);
            return true;
        }
        public bool HandleRequestFreezeFromClient(NetworkCommunicator admin, RequestFreeze message)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = admin.GetComponent<PersistentEmpireRepresentative>();
            if (!persistentEmpireRepresentative.IsAdmin)
            {
                return false;
            }
            if (message.Player == null)
            {
                return false;
            }
            if (message.Player.ControlledAgent == null || !message.Player.ControlledAgent.IsActive())
            {
                InformationComponent.Instance.SendMessage("Target is not spawned yet", new Color(1f, 0f, 0f).ToUnsignedInteger(), message.Player);
                return false;
            }
            // InformationComponent.Instance.SendMessage("Not implemented yet", new Color(1f, 0, 0).ToUnsignedInteger(), admin);
            if (Freezed.ContainsKey(message.Player))
            {
                Freezed.Remove(message.Player);
                message.Player.ControlledAgent.ClearTargetFrame();
                LoggerHelper.LogAnAction(admin, LogAction.PlayerUnfreezePlayer, new AffectedPlayer[] { new AffectedPlayer(message.Player) });
            }
            else
            {
                Freezed[message.Player] = true;
                Vec2 vec = message.Player.ControlledAgent.Position.AsVec2;
                message.Player.ControlledAgent.SetTargetPositionSynched(ref vec);
                LoggerHelper.LogAnAction(admin, LogAction.PlayerFreezePlayer, new AffectedPlayer[] { new AffectedPlayer(message.Player) });

            }
            return true;
        }
        public bool HandleRequestTpToMeFromClient(NetworkCommunicator admin, RequestTpToMe message)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = admin.GetComponent<PersistentEmpireRepresentative>();
            if (!persistentEmpireRepresentative.IsAdmin)
            {
                return false;
            }
            if (admin.ControlledAgent == null || !admin.ControlledAgent.IsActive())
            {
                return false;
            }
            if (message.Player == null)
            {
                return false;
            }
            if (message.Player.ControlledAgent == null || !message.Player.ControlledAgent.IsActive())
            {
                InformationComponent.Instance.SendMessage("Target is not spawned yet", new Color(1f, 0f, 0f).ToUnsignedInteger(), message.Player);
                return false;
            }
            if (message.Player.ControlledAgent.MountAgent == null)
            {
                Vec3 targetPos = admin.ControlledAgent.Position;
                targetPos.x = targetPos.x + 1;
                message.Player.ControlledAgent.TeleportToPosition(targetPos);
            }
            else
            {
                Vec3 targetPos = admin.ControlledAgent.Position;
                targetPos.x = targetPos.x + 1;
                message.Player.ControlledAgent.MountAgent.TeleportToPosition(targetPos);
            }
            LoggerHelper.LogAnAction(admin, LogAction.PlayerTpToMePlayer, new AffectedPlayer[] { new AffectedPlayer(message.Player) });
            return true;
        }
        public bool HandleRequestTpToFromClient(NetworkCommunicator admin, RequestTpTo message)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = admin.GetComponent<PersistentEmpireRepresentative>();
            if (!persistentEmpireRepresentative.IsAdmin)
            {
                return false;
            }
            if (admin.ControlledAgent == null || !admin.ControlledAgent.IsActive())
            {
                return false;
            }
            if (message.Player == null)
            {
                return false;
            }
            if (message.Player.ControlledAgent == null || !message.Player.ControlledAgent.IsActive())
            {
                InformationComponent.Instance.SendMessage("Target is not spawned yet", new Color(1f, 0f, 0f).ToUnsignedInteger(), message.Player);
                return false;
            }
            if (admin.ControlledAgent.MountAgent == null)
            {
                Vec3 targetPos = message.Player.ControlledAgent.Position;
                targetPos.x = targetPos.x + 1;
                admin.ControlledAgent.TeleportToPosition(targetPos);
            }
            else
            {
                Vec3 targetPos = message.Player.ControlledAgent.Position;
                targetPos.x = targetPos.x + 1;
                admin.ControlledAgent.TeleportToPosition(targetPos);
            }
            LoggerHelper.LogAnAction(admin, LogAction.PlayerTpToPlayer, new AffectedPlayer[] { new AffectedPlayer(message.Player) });

            return true;
        }

        public bool HandleRequestHealFromClient(NetworkCommunicator admin, RequestHeal message)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = admin.GetComponent<PersistentEmpireRepresentative>();
            if (!persistentEmpireRepresentative.IsAdmin)
            {
                return false;
            }
            if (message.Player == null)
            {
                return false;
            }
            if (message.Player.ControlledAgent == null || !message.Player.ControlledAgent.IsActive())
            {
                InformationComponent.Instance.SendMessage("Target is not spawned yet", new Color(1f, 0f, 0f).ToUnsignedInteger(), message.Player);
                return false;
            }
            message.Player.ControlledAgent.Health = message.Player.ControlledAgent.HealthLimit;
            LoggerHelper.LogAnAction(admin, LogAction.PlayerHealedPlayer, new AffectedPlayer[] { new AffectedPlayer(message.Player) });

            return true;
        }

        public bool HandleRequestunWoundFromClient(NetworkCommunicator admin, RequestUnWound message)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = admin.GetComponent<PersistentEmpireRepresentative>();
            if (!persistentEmpireRepresentative.IsAdmin)
            {
                return false;
            }
            if (message.Player == null)
            {
                return false;
            }
            if (message.Player.ControlledAgent == null || !message.Player.ControlledAgent.IsActive())
            {
                InformationComponent.Instance.SendMessage("Target is not spawned yet", new Color(1f, 0f, 0f).ToUnsignedInteger(), message.Player);
                return false;
            }

            WoundingBehavior.Instance.HealPlayer(message.Player);
            LoggerHelper.LogAnAction(admin, LogAction.PlayerHealedPlayer, new AffectedPlayer[] { new AffectedPlayer(message.Player) });

            return true;
        }
    }
}
