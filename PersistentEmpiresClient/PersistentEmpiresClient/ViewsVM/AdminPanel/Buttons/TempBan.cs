using PersistentEmpiresLib.NetworkMessages.Client;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresClient.ViewsVM.AdminPanel.Buttons
{
    internal class TempBan : PEAdminButtonVM
    {
        public override string GetCaption()
        {
            return "Temp Ban";
        }

        public override void Execute()
        {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestTempBan(SelectedPlayer.GetPeer()));
            GameNetwork.EndModuleEventAsClient();
        }
    }
}