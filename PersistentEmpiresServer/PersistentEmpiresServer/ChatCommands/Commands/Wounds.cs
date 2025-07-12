using PersistentEmpiresLib;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresServer.ServerMissions;
using System;
using System.Text.RegularExpressions;
using TaleWorlds.Diamond;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresServer.ChatCommands.Commands
{
    public class Wounds : Command
    {
        private static int _distance = ConfigManager.GetIntConfig("WoundsDistance", 30);
        private uint? _color = null;
        private static bool _bubble = ConfigManager.GetBoolConfig("WoundsBubble", false);

        public uint Color
        {
            get
            {
                if (_color == null)
                {
                    _color = TaleWorlds.Library.Color.ConvertStringToColor(ConfigManager.GetStrConfig("WoundsColor", ChatCommandSystem.Instance.DefaultMessageColor)).ToUnsignedInteger();
                }

                return _color.Value;
            }
        }

        public bool CanUse(NetworkCommunicator networkPeer)
        {
            return true;
        }

        public string Command()
        {
            return $"{ChatCommandSystem.Instance.CommandPrefix}wounds";
        }

        public bool Execute(NetworkCommunicator player, string[] args)
        {
            if (WoundingBehavior.Instance.WoundedUntil.ContainsKey(player.VirtualPlayer?.ToPlayerId()) == false)
            {
                this.SendMessageToPlayer(player, "You are not wounded", Color, _bubble, LogAction.Wounds);
            }
            else
            {
                var tmp = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                tmp = tmp.AddSeconds(WoundingBehavior.Instance.WoundedUntil[player.VirtualPlayer?.ToPlayerId()].Value);
                var diff = DateTime.UtcNow - tmp;
                
                this.SendMessageToPlayer(player, $"You will be unwounded in {diff.Hours} houers and {diff.Minutes} minutes.", Color, _bubble, LogAction.Wounds);
            }

            return true;
        }

        public string Description()
        {
            return "Shows players wounded state.";
        }

        public string DetailedDescription()
        {
            return $"Usage: {Command()} {Environment.NewLine}" +
                    $"Color: Same as this message{Environment.NewLine}" +
                    $"Description: Shows players wounded state.{Environment.NewLine}" +
                    $"Example: {Command()}{Environment.NewLine}";
        }

        public bool IsEnabled()
        {
            return ConfigManager.GetBoolConfig("WoundsEnabled", false);
        }
    }
}