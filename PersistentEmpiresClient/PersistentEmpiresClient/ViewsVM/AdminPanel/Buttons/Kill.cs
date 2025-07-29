using PersistentEmpiresLib.NetworkMessages.Client;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresClient.ViewsVM.AdminPanel.Buttons
{
    internal class Kill : PEAdminButtonVM
    {
        public override string GetCaption()
        {
            return GameTexts.FindText("PEAdminButtonKill", null).ToString();
        }

        public override void Execute()
        {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestKill(SelectedPlayer.GetPeer()));
            GameNetwork.EndModuleEventAsClient();
        }
    }
}