using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using PersistentEmpiresServer.ChatCommands.Commands;
using PersistentEmpiresHarmony.Patches;
using NetworkMessages.FromClient;
using TaleWorlds.Diamond;

namespace PersistentEmpiresServer.ServerMissions
{
    public class ChatCommandSystem : MissionNetwork
    {
        public Dictionary<string, Command> commands;
        public static ChatCommandSystem Instance;
        public bool DisableGlobalChat;
        private PatreonRegistryBehavior patreonRegistry;
        public Dictionary<NetworkCommunicator, bool> Muted;
        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            commands = new Dictionary<string, Command>();
            Muted = new Dictionary<NetworkCommunicator, bool>();
            Instance = this;
            PatchGlobalChat.OnClientEventPlayerMessageAll += PatchGlobalChat_OnClientEventPlayerMessageAll;
            LocalChatComponent localChat = base.Mission.GetMissionBehavior<LocalChatComponent>();
            localChat.OnPrefixHandleLocalChatFromClient += this.OnPrefixHandleLocalChatFromClient;

            this.patreonRegistry = base.Mission.GetMissionBehavior<PatreonRegistryBehavior>();
            this.Initialize();
        }

        private bool OnPrefixHandleLocalChatFromClient(NetworkCommunicator Sender, string Message, bool shout)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = Sender.GetComponent<PersistentEmpireRepresentative>();
            if (Message.StartsWith("!"))
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
            if (message.Message.StartsWith("!"))
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
                if (!commands.ContainsKey(command.Command()))
                {
                    Debug.Print("** Chat Command " + command.Command() + " have been initiated !", 0, Debug.DebugColor.Green);
                    commands.Add(command.Command(), command);
                }
            }
        }
    }
}
