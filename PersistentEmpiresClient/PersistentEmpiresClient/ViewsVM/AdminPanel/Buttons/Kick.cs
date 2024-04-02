using PersistentEmpiresLib.NetworkMessages.Client;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresClient.ViewsVM.AdminPanel.Buttons
{
    internal class Kick : PEAdminButtonVM
    {
        public override string GetCaption()
        {
            return "Kick";
        }

        public override void Execute()
        {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestKick(SelectedPlayer.GetPeer()));
            GameNetwork.EndModuleEventAsClient();
        }
    }
}