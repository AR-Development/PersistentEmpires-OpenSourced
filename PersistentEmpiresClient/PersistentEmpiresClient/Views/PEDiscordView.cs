using Discord;
using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.PersistentEmpiresMission;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpires.Views.Views
{
    public class PEDiscordView : MissionView
    {
        // my app id: 
        // PE app id: 1066379293204172971
        public long applicationId = 1220322074581798942;

        private Discord.Discord discord;

        public bool DiscordNotWorks = false;
        public PersistentEmpireClientBehavior persistentEmpireClientBehavior;


        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            try
            {
                discord = new Discord.Discord(applicationId, (System.UInt64)Discord.CreateFlags.NoRequireDiscord);
                persistentEmpireClientBehavior = base.Mission.GetMissionBehavior<PersistentEmpireClientBehavior>();
                persistentEmpireClientBehavior.OnSynchronized += this.OnSynchronized;
            }
            catch (Exception e)
            {
                // InformationManager.DisplayMessage(new InformationMessage("Unable to connect discord. Please load desktop application of discord for easy communication about complaints and ban reasons", Color.ConvertStringToColor("#d32f2fff")));
                this.DiscordNotWorks = true;
            }
        }

        public override void OnMissionScreenTick(float dt)
        {
            if(this.DiscordNotWorks == false)
            {
                try
                {
                    discord.RunCallbacks();
                }catch(Exception e)
                {
                    this.DiscordNotWorks = false;
                }
            }
        }

        private void OnSynchronized()
        {
            if(this.DiscordNotWorks)
            {
                InformationManager.DisplayMessage(new InformationMessage("Unable to connect discord.", Color.ConvertStringToColor("#d32f2fff")));
                return;
            }
            discord.GetUserManager().OnCurrentUserUpdate += this.OnCurrentUserUpdate;
        }

        private void OnCurrentUserUpdate()
        {
            try
            {
                Discord.User user = discord.GetUserManager().GetCurrentUser();
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new MyDiscordId(user.Id.ToString()));
                GameNetwork.EndModuleEventAsClient();
            }
            catch (Exception e)
            {
                // InformationManager.DisplayMessage(new InformationMessage("Unable to get discord user. Reason: " + e.ToString(), Color.ConvertStringToColor("#d32f2fff")));
            }
        }
    }
}
