using NetworkMessages.FromServer;
using PersistentEmpiresLib;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresServer.ServerMissions;
using System;
using System.Linq;
using TaleWorlds.Diamond;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresServer.ChatCommands.Commands
{
    public class Help : Command
    {
        private uint? _color = null;

        public uint Color
        {
            get
            {
                if (_color == null)
                {
                    _color = TaleWorlds.Library.Color.ConvertStringToColor(ConfigManager.GetStrConfig("HelpColor", ChatCommandSystem.Instance.DefaultMessageColor)).ToUnsignedInteger();
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
            return $"{ChatCommandSystem.Instance.CommandPrefix}help";
        }

        public bool Execute(NetworkCommunicator networkPeer, string[] args)
        {
            if (args != null && args.Any())
            {
                var command = ChatCommandSystem.Instance.commands[args.First()];

                if (command != null)
                {
                    InformationComponent.Instance.SendMessage(command.DetailedDescription(), Color, networkPeer);

                    return true;
                }
            }

            InformationComponent.Instance.SendMessage("Wrong format", Color, networkPeer);

            return false;
        }

        public string Description()
        {
            return "Displays detailed information about a command.";
        }

        public string DetailedDescription()
        {
            return $"Usage: {Command()} [CommandName]{Environment.NewLine}" +
                    $"Parameter: [CommandName] name of command{Environment.NewLine}" +
                    $"Color: Same as this message{Environment.NewLine}" +
                    $"Description: Displays detailed information about command use{Environment.NewLine}" +
                    $"Example: {Command()} dice{Environment.NewLine}";
        }

        public bool IsEnabled()
        {
            return ConfigManager.GetBoolConfig("HelpEnabled", true);
        }
    }
}