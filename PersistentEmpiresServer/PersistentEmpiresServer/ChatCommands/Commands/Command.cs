using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresServer.ChatCommands.Commands
{
    public interface Command
    {
        public string Command();

        public bool CanUse(NetworkCommunicator networkPeer);

        public bool Execute(NetworkCommunicator networkPeer, string[] args);

        public string Description();

        public string DetailedDescription();

        public bool IsEnabled();

        public uint Color { get; }
    }
}