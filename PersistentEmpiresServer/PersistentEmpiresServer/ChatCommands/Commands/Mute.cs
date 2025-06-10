using NetworkMessages.FromServer;
using PersistentEmpiresLib;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresServer.ServerMissions;
using System;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresServer.ChatCommands.Commands
{
    public class Mute : Command
    {
        private uint? _color = null;

        public uint Color
        {
            get
            {
                if (_color == null)
                {
                    _color = TaleWorlds.Library.Color.ConvertStringToColor(ConfigManager.GetStrConfig("MuteColor", ChatCommandSystem.Instance.DefaultMessageColor)).ToUnsignedInteger();
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
            return $"{ChatCommandSystem.Instance.CommandPrefix}mute";
        }

        public bool Execute(NetworkCommunicator networkPeer, string[] args)
        {
            if (args == null || args.Length == 0)
            {
                InformationComponent.Instance.SendMessage("Please provide a username. Player that contains provided input will be muted", Color, networkPeer);

                return true;
            }

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
                InformationComponent.Instance.SendMessage("Target player not found", Color, networkPeer);
                return true;
            }

            if (ChatCommandSystem.Instance.Muted.ContainsKey(targetPeer))
            {
                InformationComponent.Instance.SendMessage("Unmuted.", Color, networkPeer);
                ChatCommandSystem.Instance.Muted.Remove(targetPeer);
            }
            else
            {
                InformationComponent.Instance.SendMessage("Muted.", Color, networkPeer);
                ChatCommandSystem.Instance.Muted[targetPeer] = true;
            }

            return true;
        }

        public string Description()
        {
            return $"Mutes a player from global chat. Caution ! First user that contains the provided input will be muted. Usage {Command()} Player Name";
        }

        public string DetailedDescription()
        {
            return $"Usage: {Command()} [PlayerName]{Environment.NewLine}" +
                    $"Parameter: [PlayerName] name of player to be muted{Environment.NewLine}" +
                    $"Color: Same as this message{Environment.NewLine}" +
                    $"Description: Mutes a player from global chat. Caution ! First user that contains the provided input will be muted.{Environment.NewLine}" +
                    $"Example: {Command()} Player1{Environment.NewLine}";
        }

        public bool IsEnabled()
        {
            return ConfigManager.GetBoolConfig("MuteEnabled", true);
        }
    }
}