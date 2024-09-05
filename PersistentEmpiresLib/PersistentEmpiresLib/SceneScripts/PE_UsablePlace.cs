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

using TaleWorlds.Engine;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.SceneScripts
{
    public class PE_UsablePlace : UsableMachine
    {
        // Token: 0x060001EA RID: 490 RVA: 0x0000D2D9 File Offset: 0x0000B4D9
        public override string GetDescriptionText(GameEntity gameEntity = null)
        {
            StandingPoint pilotStandingPoint = base.PilotStandingPoint;
            if (pilotStandingPoint == null)
            {
                return null;
            }
            return pilotStandingPoint.DescriptionMessage.ToString();
        }

        // Token: 0x060001EB RID: 491 RVA: 0x0000D2F1 File Offset: 0x0000B4F1
        public override TextObject GetActionTextForStandingPoint(UsableMissionObject usableGameObject)
        {
            StandingPoint pilotStandingPoint = base.PilotStandingPoint;
            if (pilotStandingPoint == null)
            {
                return null;
            }
            return pilotStandingPoint.ActionMessage;
        }
    }
}
