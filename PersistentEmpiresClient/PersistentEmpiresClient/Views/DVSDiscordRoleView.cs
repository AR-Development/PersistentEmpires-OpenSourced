using PersistentEmpiresHarmony.Patches;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade;
using PersistentEmpiresMission.MissionBehaviors;

namespace PersistentEmpiresClient.Views.DragonV
{
    internal class DVSDiscordRoleView : MissionView
    {
        public bool IsActive = false;
        public DiscordRoleRegistryBehavior discordRegistryBehavior;
        public DVSDiscordRoleView()
        {

        }

        public override void OnMissionScreenInitialize()
        {
            discordRegistryBehavior = base.Mission.GetMissionBehavior<DiscordRoleRegistryBehavior>();
            this.IsActive = discordRegistryBehavior != null;

            if (this.IsActive)
            {
                PatchGlobalChat.OnGlobalChatReceived += this.OnGlobalChatReceived;
            }
        }

        private bool OnGlobalChatReceived(NetworkCommunicator peer, string message, bool teamOnly)
        {
            if (teamOnly) return true;

            if (this.discordRegistryBehavior.DiscordRegistry.ContainsKey(peer))
            {
                DiscordData data = this.discordRegistryBehavior.DiscordRegistry[peer];
                InformationManager.DisplayMessage(new InformationMessage("(" + data.Title.Split(' ')[0] + ") " + peer.GetComponent<MissionPeer>().DisplayedName + ": " + message, data.Color));
                return false;
            }
            return true;
        }
    }
}