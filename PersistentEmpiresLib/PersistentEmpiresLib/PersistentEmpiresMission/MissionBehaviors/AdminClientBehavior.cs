using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresLib.SceneScripts;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class AdminClientBehavior : MissionNetwork
    {
        public delegate void AdminPanelClick();
        public event AdminPanelClick OnAdminPanelClick;
        public static List<AdminTp> AdminTps = new List<AdminTp>();

        public void HandleAdminPanelClick()
        {
            if (this.OnAdminPanelClick != null)
            {
                this.OnAdminPanelClick();
            }
        }

        public void HandleUnbanPlayerClick()
        {
            InformationManager.ShowTextInquiry(
             new TextInquiryData(
             "Unban player"
             , "Write players id"
             , true
             , true
             , "Select"
             , "Cancel"
             , OnIdWritten
             , (Action)null
             , false
             , IsNameApplicable
             , ""
             , ""));
        }
        
        private void OnIdWritten(string name)
        {
            var message = new RequestUnBan(GameNetwork.MyPeer.VirtualPlayer?.ToPlayerId(), name);
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(message);
            GameNetwork.EndModuleEventAsClient();
        }

        private Tuple<bool, string> IsNameApplicable(string inputText)
        {
            var reg = new Regex(@"^[0-9.]+$");
            var result = reg.Match(inputText);

            if (!result.Success)
                return new Tuple<bool, string>(false, "Wrong format");

            if (string.IsNullOrWhiteSpace(inputText))
                return new Tuple<bool, string>(false, "Id must be a sting of valid characters");

            if (inputText.Length > 30)
                return new Tuple<bool, string>(false, "Id is not valid");

            return new Tuple<bool, string>(true, inputText);
        }

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
        }
        public override void OnRemoveBehavior()
        {
            AdminTps.Clear();
            base.OnRemoveBehavior();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
        }

        public void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
        {
            GameNetwork.NetworkMessageHandlerRegisterer networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(mode);
            if (GameNetwork.IsClient)
            {
                networkMessageHandlerRegisterer.Register<AuthorizeAsAdmin>(this.HandleAuthorizeAsAdminFromServer);
            }
        }

        private void HandleAuthorizeAsAdminFromServer(AuthorizeAsAdmin message)
        {
            GameNetwork.MyPeer.GetComponent<PersistentEmpireRepresentative>().IsAdmin = true;
        }


        internal static void Register(AdminTp adminTp)
        {
            AdminTps.Add(adminTp);
        }
    }
}