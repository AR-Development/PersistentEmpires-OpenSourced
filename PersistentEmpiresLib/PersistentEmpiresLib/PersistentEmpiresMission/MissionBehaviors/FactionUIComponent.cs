/*
 *  Persistent Empires Open Sourced - A Mount and Blade: Bannerlord Mod
 *  Copyright (C) 2024  Free Software Foundation, Inc.
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.

 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

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
