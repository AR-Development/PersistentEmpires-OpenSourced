using PersistentEmpires.Views.Views;
using PersistentEmpiresLib;
using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System.Collections.Generic;
using System;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using PersistentEmpiresLib.Factions;

namespace PersistentEmpiresClient.ViewsVM.AdminPanel.Buttons
{
    internal class MakeLord : PEAdminButtonVM
    {
        public override string GetCaption()
        {
            return GameTexts.FindText("PEAdminButtonMakeLord", null).ToString();
        }

        public override void Execute()
        {
            var factionBehavior = Mission.Current.GetMissionBehavior<FactionsBehavior>();
             
            MBInformationManager.ShowMultiSelectionInquiry(
                new MultiSelectionInquiryData(GameTexts.FindText("PEAdminButtonMakeLordInqCaption", null).ToString()
                    , GameTexts.FindText("PEAdminButtonMakeLordInqText", null).ToString()
                    , factionBehavior.Factions.Select(x => new InquiryElement(x.Key, $"{x.Value.name}", null)).ToList()
                    , true
                    , 1
                    , 1
                    , GameTexts.FindText("PE_InquiryData_Select", null).ToString()
                    , GameTexts.FindText("PE_InquiryData_Cancel", null).ToString()
                    , DoSelectFaction
                    , DoCancelAction));

            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestPermBan(SelectedPlayer.GetPeer()));
            GameNetwork.EndModuleEventAsClient();
        }

        private void DoCancelAction(List<InquiryElement> list)
        {
        }

        private void DoSelectFaction(List<InquiryElement> obj)
        {
            try
            {
                var factionId = (int)obj.FirstOrDefault().Identifier;

                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new FactionAdminAssignLord(SelectedPlayer.GetPeer(), factionId));
                GameNetwork.EndModuleEventAsClient();
            }
            catch (Exception ex)
            {
            }
        }
    }
}