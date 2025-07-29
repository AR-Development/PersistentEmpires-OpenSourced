using NetworkMessages.FromServer;
using PersistentEmpiresLib;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresServer.ServerMissions;
using System;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresServer.ChatCommands.Commands
{
    public class VMute : Command
    {
        private uint? _color = null;
        public uint Color
        {
            get
            {
                if (_color == null)
                {
                    _color = TaleWorlds.Library.Color.ConvertStringToColor(ConfigManager.GetStrConfig("VmuteColor", ChatCommandSystem.Instance.DefaultMessageColor)).ToUnsignedInteger();
                }

                return _color.Value;
            }
        }

        public bool CanUse(NetworkCommunicator networkPeer)
        {
            return AdminServerBehavior.Instance.IsPlayerAdmin(networkPeer);
        }

        public string Command()
        {
            return $"{ChatCommandSystem.Instance.CommandPrefix}vmute";
        }

        public bool Execute(NetworkCommunicator networkPeer, string[] args)
        {
            if (args == null || args.Length == 0)
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new ServerMessage("Please provide a username. Player that contains provided input will be muted"));
                GameNetwork.EndModuleEventAsServer();
                return true;
            }
            ProximityChatComponent pcc = Mission.Current.GetMissionBehavior<ProximityChatComponent>();
            NetworkCommunicator targetPeer = null;
            foreach (NetworkCommunicator peer in GameNetwork.NetworkPeers)
            {
                if (peer.UserName.Contains(string.Join(" ", args)))
                {
                    targetPeer = peer;
                    break;
                }
            }
            if (targetPeer == null)
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new ServerMessage("Target player not found"));
                GameNetwork.EndModuleEventAsServer();
                return true;
            }

            if (pcc.GlobalMuted.ContainsKey(targetPeer))
            {
                InformationComponent.Instance.SendMessage("Unmuted.", Colors.Green.ToUnsignedInteger(), networkPeer);
                pcc.GlobalMuted.Remove(targetPeer);
            }
            else
            {
                InformationComponent.Instance.SendMessage("Muted.", Colors.Green.ToUnsignedInteger(), networkPeer);
                pcc.GlobalMuted[targetPeer] = true;
            }
            return true;
        }

        public string Description()
        {
            return $"Mutes a player from voice chat. Caution ! First user that contains the provided input will be muted. Usage {Command()} Player Name";
        }

        public string DetailedDescription()
        {
            return $"Usage: {Command()} [PlayerName]{Environment.NewLine}" +
                    $"Parameter: [PlayerName] name of player to be muted{Environment.NewLine}" +
                    $"Color: Same as this message{Environment.NewLine}" +
                    $"Description: Mutes a player from voice chat. Caution ! First user that contains the provided input will be muted.{Environment.NewLine}" +
                    $"Example: {Command()} Player1{Environment.NewLine}";
        }

        public bool IsEnabled()
        {
            return ConfigManager.GetBoolConfig("VMuteEnabled", true);
        }
    }
}