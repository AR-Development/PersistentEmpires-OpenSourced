using NetworkMessages.FromServer;
using PersistentEmpiresLib;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresServer.ServerMissions;
using System;
using System.Linq;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresServer.ChatCommands.Commands
{
    public class Commands : Command
    {
        private uint? _color = null;

        public uint Color
        {
            get
            {
                if (_color == null)
                {
                    _color = TaleWorlds.Library.Color.ConvertStringToColor(ConfigManager.GetStrConfig("CommandsColor", ChatCommandSystem.Instance.DefaultMessageColor)).ToUnsignedInteger();
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
            return $"{ChatCommandSystem.Instance.CommandPrefix}commands";
        }

        public bool Execute(NetworkCommunicator networkPeer, string[] args)
        {
            string[] commands = ChatCommandSystem.Instance.commands.Keys.ToArray();

            InformationComponent.Instance.SendMessage($"-==== Command List ===-", Color, networkPeer);

            foreach (string command in commands)
            {
                Command commandExecutable = ChatCommandSystem.Instance.commands[command];
                if (commandExecutable.CanUse(networkPeer))
                {
                    InformationComponent.Instance.SendMessage($"{command}: {commandExecutable.Description()}", Color, networkPeer);
                }
            }
            return true;
        }

        public string Description()
        {
            return "Displays all avaiable commands.";
        }

        public string DetailedDescription()
        {
            return $"Usage: {Command()}{Environment.NewLine}" +
                    $"Color: Same as this message{Environment.NewLine}" +
                    $"Description: Displays all enabled commands{Environment.NewLine}" +
                    $"Example: {Command()}{Environment.NewLine}";
        }

        public bool IsEnabled()
        {
            return ConfigManager.GetBoolConfig("CommandsEnabled", true);
        }
    }
}