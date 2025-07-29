using PersistentEmpiresLib.NetworkMessages.Client;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresClient.ViewsVM.AdminPanel.Buttons
{
    internal class PermBan : PEAdminButtonVM
    {
        public override string GetCaption()
        {
            return GameTexts.FindText("PEAdminButtonPermBan", null).ToString();
        }

        public override void Execute()
        {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestPermBan(SelectedPlayer.GetPeer()));
            GameNetwork.EndModuleEventAsClient();
        }
    }
}