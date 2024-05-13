using System;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class FactionUIComponent : MissionLogic
    {
        public delegate void FactionManagementClick();
        public event FactionManagementClick OnFactionManagementClick;

        public delegate void BannerChangeClick();
        public event BannerChangeClick OnBannerChangeClick;

        public delegate void NameChangeClick();
        public event NameChangeClick OnNameChangeClick;

        public delegate void KickSomeoneFromFactionClick();
        public event KickSomeoneFromFactionClick OnKickSomeoneFromFactionClick;

        public delegate void ManageDoorKeysClick();
        public event ManageDoorKeysClick OnManageDoorKeysClick;

        public delegate void DiplomacyMenuClick();
        public event DiplomacyMenuClick OnDiplomacyMenuClick;

        public delegate void FactionLordPollClick();
        public event FactionLordPollClick OnFactionLordPollClick;

        public delegate void ManageChestKeysClick();
        public event ManageChestKeysClick OnManageChestKeyClick;

        public event Action OnAssignMarshallClick;
        public event Action OnAssignTransferLordshipClick;


        public void OnAssignMarshallClickHandler()
        {
            if (this.OnAssignMarshallClick != null) this.OnAssignMarshallClick();
        }

        public void OnAssignTransferLordshipClickHandler()
        {
            if (this.OnAssignTransferLordshipClick != null) this.OnAssignTransferLordshipClick();

        }

        public void OnManageChestKeyClickHandler()
        {
            if (this.OnManageChestKeyClick != null)
            {
                this.OnManageChestKeyClick();
            }
        }
        public void OnFactionManagementClickHandler()
        {
            if (this.OnFactionManagementClick != null)
            {
                this.OnFactionManagementClick();
            }
        }


        public void OnDiplomacyMenuClickHandler()
        {
            if (this.OnDiplomacyMenuClick != null)
            {
                this.OnDiplomacyMenuClick();
            }
        }
        public void OnManageDoorKeysClickHandler()
        {
            if (this.OnManageDoorKeysClick != null)
            {
                this.OnManageDoorKeysClick();
            }
        }
        public void OnKickSomeoneFromFactionClickHandler()
        {
            if (this.OnKickSomeoneFromFactionClick != null)
            {
                this.OnKickSomeoneFromFactionClick();
            }
        }
        public void OnNameChangeClickHandler()
        {
            if (this.OnNameChangeClick != null)
            {
                this.OnNameChangeClick();
            }
        }
        public void OnBannerChangeClickHandler()
        {
            if (this.OnBannerChangeClick != null)
            {
                this.OnBannerChangeClick();
            }
        }

        public void OnFactionLordPollClickHandler()
        {
            if (this.OnFactionLordPollClick != null)
            {
                this.OnFactionLordPollClick();
            }
        }
    }
}
