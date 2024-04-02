using PersistentEmpiresLib.NetworkMessages.Client;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresClient.ViewsVM.AdminPanel.Buttons
{
    internal class PermBan : PEAdminButtonVM
    {
        public override string GetCaption()
        {
            return "Perm Ban";
        }

        public override void Execute()
        {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestPermBan(SelectedPlayer.GetPeer()));
            GameNetwork.EndModuleEventAsClient();
        }
    }
}