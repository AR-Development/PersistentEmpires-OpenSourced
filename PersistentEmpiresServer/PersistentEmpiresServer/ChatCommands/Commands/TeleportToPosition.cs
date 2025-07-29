using PersistentEmpiresLib;
using PersistentEmpiresLib.Database.DBEntities;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresServer.ServerMissions;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Diamond;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresServer.ChatCommands.Commands
{
    public class TeleportToPosition : Command
    {
        private uint? _color = null;

        public uint Color
        {
            get
            {
                if (_color == null)
                {
                    _color = TaleWorlds.Library.Color.ConvertStringToColor(ConfigManager.GetStrConfig("TeleportToPositionColor", ChatCommandSystem.Instance.DefaultMessageColor)).ToUnsignedInteger();
                }

                return _color.Value;
            }
        }

        public string Command()
        {
            return $"{ChatCommandSystem.Instance.CommandPrefix}tp";
        }

        public bool CanUse(NetworkCommunicator networkPeer)
        {
            return AdminServerBehavior.Instance.IsPlayerAdmin(networkPeer);
        }

        public bool Execute(NetworkCommunicator networkPeer, string[] args)
        {
            if (args.Count() == 2)
            {
                if (string.IsNullOrEmpty(args[1]))
                {
                    return true;
                }

                var cooridnates = args[1].Split(',');

                if (cooridnates.Count() != 3)
                {
                    return true;
                }

                float tmpFloat;
                float x, y, z;
                var tmpString = cooridnates[0].Replace("(", string.Empty);

                if (!float.TryParse(tmpString.TrimStart().TrimEnd(), out tmpFloat))
                {
                    return true;
                }

                x = tmpFloat;
                tmpString = cooridnates[1];
                if (!float.TryParse(tmpString.TrimStart().TrimEnd(), out tmpFloat))
                {
                    return true;
                }

                y = tmpFloat;
                tmpString = cooridnates[2].Replace(")", string.Empty);
                if (!float.TryParse(tmpString.TrimStart().TrimEnd(), out tmpFloat))
                {
                    return true;
                }
                z = tmpFloat;

                LoggerHelper.LogAnAction(networkPeer, LogAction.TeleportToPosition, new AffectedPlayer[0], new object[] { args[1] });

                networkPeer.ControlledAgent.TeleportToPosition(new Vec3(x, y, z));
            }

            return true;
        }

        public string Description()
        {
            return "Teleport to position";
        }

        public string DetailedDescription()
        {
            return $"Usage: {Command()} [v3position]{Environment.NewLine}" +
                    $"Parameter: [v3position] Position on map to teleport to{Environment.NewLine}" +
                    $"Color: Same as this message{Environment.NewLine}" +
                    $"Description: Teleports player to choosen position{Environment.NewLine}" +
                    $"Example: {Command()} (0,0,0){Environment.NewLine}";
        }

        public bool IsEnabled()
        {
            return ConfigManager.GetBoolConfig("TeleportToPositionEnabled", true);
        }
    }
}