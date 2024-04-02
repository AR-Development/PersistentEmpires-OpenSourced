using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresServer.ChatCommands.Commands
{
    public interface Command
    {
        string Command();
        bool CanUse(NetworkCommunicator networkPeer);
        bool Execute(NetworkCommunicator networkPeer, string[] args);
        string Description();
    }
}