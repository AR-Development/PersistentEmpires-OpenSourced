using NetworkMessages.FromClient;
using PersistentEmpiresHarmony.Patches;
using PersistentEmpiresLib;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresServer.ChatCommands.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresServer.ServerMissions
{
    public class ChatCommandSystem : MissionNetwork
    {
        internal Dictionary<string, Command> commands;
        internal static ChatCommandSystem Instance;
        internal bool DisableGlobalChat;
        internal PatreonRegistryBehavior patreonRegistry;
        internal Dictionary<NetworkCommunicator, bool> Muted;
        internal string CommandPrefix = "!";
        internal string DefaultMessageColor = "#FFFDFDFD";

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            commands = new Dictionary<string, Command>();
            Muted = new Dictionary<NetworkCommunicator, bool>();
            Instance = this;
            PatchGlobalChat.OnClientEventPlayerMessageAll += PatchGlobalChat_OnClientEventPlayerMessageAll;
            LocalChatComponent localChat = base.Mission.GetMissionBehavior<LocalChatComponent>();
            localChat.OnPrefixHandleLocalChatFromClient += this.OnPrefixHandleLocalChatFromClient;
            patreonRegistry = base.Mission.GetMissionBehavior<PatreonRegistryBehavior>();
            CommandPrefix = ConfigManager.GetStrConfig("MessagePrefix", "!");
            DefaultMessageColor = ConfigManager.GetStrConfig("DefaultMessageColor", "#FFFDFDFD");

            Initialize();
        }

        private bool OnPrefixHandleLocalChatFromClient(NetworkCommunicator Sender, string Message, bool shout)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = Sender.GetComponent<PersistentEmpireRepresentative>();
            if (Message.StartsWith(CommandPrefix))
            {
                string[] argsWithCommand = Message.Split(' ');
                string command = argsWithCommand[0];
                string[] args = argsWithCommand.Skip(1).ToArray();
                this.Execute(Sender, command, args);
                return false;
            }
            return true;
        }

        private bool PatchGlobalChat_OnClientEventPlayerMessageAll(NetworkCommunicator networkPeer, PlayerMessageAll message)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = networkPeer.GetComponent<PersistentEmpireRepresentative>();
            if (message.Message.StartsWith(CommandPrefix))
            {
                string[] argsWithCommand = message.Message.Split(' ');
                string command = argsWithCommand[0];
                string[] args = argsWithCommand.Skip(1).ToArray();
                this.Execute(networkPeer, command, args);
                return false;
            }
            else if (persistentEmpireRepresentative != null && persistentEmpireRepresentative.IsAdmin)
            {
                InformationComponent.Instance.BroadcastMessage("(Admin) " + networkPeer.GetComponent<MissionPeer>().DisplayedName + ": " + message.Message, Color.ConvertStringToColor("#FDD835FF").ToUnsignedInteger());
                return false;
            }
            if (persistentEmpireRepresentative.IsAdmin || this.patreonRegistry.IsPlayerPatreon(networkPeer)) return true;
            if (this.DisableGlobalChat) return false;
            if (this.Muted.ContainsKey(networkPeer))
            {
                InformationComponent.Instance.SendMessage("You are muted.", Colors.Red.ToUnsignedInteger(), networkPeer);
                return false;
            }
            return true;
        }

        public bool Execute(NetworkCommunicator networkPeer, string command, string[] args)
        {
            Command executableCommand;
            bool exists = commands.TryGetValue(command, out executableCommand);
            if (!exists)
            {
                InformationComponent.Instance.SendMessage("This command is not exists", Colors.Red.ToUnsignedInteger(), networkPeer);
                return false;
            }
            if (!executableCommand.CanUse(networkPeer))
            {
                InformationComponent.Instance.SendMessage("You are not authorized to run this command", Colors.Red.ToUnsignedInteger(), networkPeer);
                return false;
            }
            return executableCommand.Execute(networkPeer, args);
        }

        private void Initialize()
        {
            this.commands = new Dictionary<string, Command>();
            foreach (Type mytype in System.Reflection.Assembly.GetExecutingAssembly().GetTypes()
                 .Where(mytype => mytype.GetInterfaces().Contains(typeof(Command))))
            {
                Command command = (Command)Activator.CreateInstance(mytype);
                if (!commands.ContainsKey(command.Command()) && command.IsEnabled())
                {
                    Debug.Print("** Chat Command " + command.Command() + " have been initiated !", 0, Debug.DebugColor.Green);
                    commands.Add(command.Command(), command);
                }
            }
        }
    }
}