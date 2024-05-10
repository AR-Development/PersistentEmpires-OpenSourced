using PersistentEmpiresHarmony.Patches;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace PersistentEmpires.Views.Views
{
    public class PEPatreonView : MissionView
    {
        public bool IsActive = false;
        public PatreonRegistryBehavior patreonRegistryBehavior;
        public PEPatreonView()
        {

        }

        public override void OnMissionScreenInitialize()
        {
            patreonRegistryBehavior = base.Mission.GetMissionBehavior<PatreonRegistryBehavior>();
            this.IsActive = patreonRegistryBehavior != null;

            if (this.IsActive)
            {
                PatchGlobalChat.OnGlobalChatReceived += this.OnGlobalChatReceived;
            }
        }

        private bool OnGlobalChatReceived(NetworkCommunicator peer, string message, bool teamOnly)
        {
            if (teamOnly) return true;

            if (this.patreonRegistryBehavior.PatreonRegistry.ContainsKey(peer))
            {
                PatreonData data = this.patreonRegistryBehavior.PatreonRegistry[peer];
                InformationManager.DisplayMessage(new InformationMessage("(" + data.Title.Split(' ')[0] + ") " + peer.GetComponent<MissionPeer>().DisplayedName + ": " + message, data.Color));
                return false;
            }
            return true;
        }
    }
}
