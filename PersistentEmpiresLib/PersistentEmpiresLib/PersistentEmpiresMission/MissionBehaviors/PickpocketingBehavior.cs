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
    public class PickpocketingBehavior : MissionNetwork
    {
        public static PickpocketingBehavior Instance;
        public string ItemId = "pe_stealing_dagger";
        private int ThousandPercentage = 50;
        private int RequiredPickpocketing = 10;
        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            if (GameNetwork.IsServer)
            {
                this.ItemId = ConfigManager.GetStrConfig("PickpocketingItem", "pe_stealing_dagger");
                this.ThousandPercentage = ConfigManager.GetIntConfig("PickpocketingPercentageThousands", 50);
                this.RequiredPickpocketing = ConfigManager.GetIntConfig("RequiredPickpocketing", 10);
            }
            Instance = this;
        }

        public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, in MissionWeapon affectorWeapon, in Blow blow, in AttackCollisionData attackCollisionData)
        {
            base.OnAgentHit(affectedAgent, affectorAgent, affectorWeapon, blow, attackCollisionData);
            if (GameNetwork.IsClient) return;
            SkillObject pickpocketingSkill = MBObjectManager.Instance.GetObject<SkillObject>("Pickpocketing");


            if (affectedAgent.IsPlayerControlled && affectorAgent != null && affectorAgent.IsPlayerControlled && affectorWeapon.Item != null && affectorWeapon.Item.StringId == this.ItemId && affectorAgent.Character.GetSkillValue(pickpocketingSkill) >= this.RequiredPickpocketing && blow.VictimBodyPart != BoneBodyPartType.None)
            {
                PersistentEmpireRepresentative representativeAffected = affectedAgent.MissionPeer.GetNetworkPeer().GetComponent<PersistentEmpireRepresentative>();
                PersistentEmpireRepresentative representativeAffector = affectorAgent.MissionPeer.GetNetworkPeer().GetComponent<PersistentEmpireRepresentative>();

                if (representativeAffected == null || representativeAffector == null) return;

                int gold = representativeAffected.Gold;
                int removeGold = (gold * this.ThousandPercentage) / 1000;
                representativeAffected.GoldLost(removeGold);
                representativeAffector.GoldGain(removeGold);

                affectedAgent.Health += 1;

            }
        }
    }
}
