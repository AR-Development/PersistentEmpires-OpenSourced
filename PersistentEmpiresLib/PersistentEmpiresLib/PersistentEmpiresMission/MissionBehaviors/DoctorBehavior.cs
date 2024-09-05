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

using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class DoctorBehavior : MissionLogic
    {
        public static DoctorBehavior Instance;
        public string ItemId = "pe_doctorscalpel";
        public override void OnBehaviorInitialize()
        {
            if (GameNetwork.IsServer)
            {
                this.RequiredMedicineSkillForHealing = ConfigManager.GetIntConfig("RequiredMedicineSkillForHealing", 50);
                this.MedicineHealingAmount = ConfigManager.GetIntConfig("MedicineHealingAmount", 15);
                this.ItemId = ConfigManager.GetStrConfig("MedicineItemId", "pe_doctorscalpel");
            }
            Instance = this;
        }

        public int RequiredMedicineSkillForHealing = 50;
        public int MedicineHealingAmount = 15;
        public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, in MissionWeapon affectorWeapon, in Blow blow, in AttackCollisionData attackCollisionData)
        {
            if (!affectorAgent.IsHuman) return;
            if (affectorWeapon.Item == null) return;
            if (affectorWeapon.Item != null && affectorWeapon.Item.StringId != this.ItemId) return;
            SkillObject medicineSkill = MBObjectManager.Instance.GetObject<SkillObject>("Medicine");
            if (affectorAgent.Character.GetSkillValue(medicineSkill) < RequiredMedicineSkillForHealing) return;
            if (affectedAgent.MissionPeer == null)
            {
                affectedAgent.Health += MedicineHealingAmount;
                if (affectedAgent.Health > affectedAgent.HealthLimit) affectedAgent.Health = affectedAgent.HealthLimit;
                return;
            }
            NetworkCommunicator peer = affectedAgent.MissionPeer.GetNetworkPeer();
            if (peer == null) return;

            if (CombatlogBehavior.Instance != null && CombatlogBehavior.Instance.IsPlayerInCombatState(peer))
            {
                return;
            }


            affectedAgent.Health += MedicineHealingAmount;
            if (affectedAgent.Health > affectedAgent.HealthLimit) affectedAgent.Health = affectedAgent.HealthLimit;
        }

    }
}
