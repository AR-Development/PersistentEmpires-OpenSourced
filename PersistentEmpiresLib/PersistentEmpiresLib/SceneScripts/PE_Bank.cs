using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.SceneScripts
{
    public class PE_Bank : UsableMissionObject
    {
        private BankingComponent bankingComponent;
        protected override void OnInit()
        {
            base.OnInit();

            bankingComponent = Mission.Current.GetMissionBehavior<BankingComponent>();
            TextObject actionMessage = new TextObject("Bank");
            base.ActionMessage = actionMessage;
            TextObject descriptionMessage = new TextObject("Press {KEY} To Access");
            descriptionMessage.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
            base.DescriptionMessage = descriptionMessage;
        }
        public override string GetDescriptionText(GameEntity gameEntity = null)
        {
            return "Bank";
        }

        public override void OnUse(Agent userAgent)
        {

            base.OnUse(userAgent);
            if (GameNetwork.IsServer)
            {
                this.bankingComponent.OpenBankForPeer(userAgent.MissionPeer.GetNetworkPeer(), this);
            }
            userAgent.StopUsingGameObjectMT(true);
        }
    }
}
