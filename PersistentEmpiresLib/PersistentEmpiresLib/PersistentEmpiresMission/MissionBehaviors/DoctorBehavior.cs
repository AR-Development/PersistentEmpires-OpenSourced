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
#if SERVER
            this.RequiredMedicineSkillForHealing = ConfigManager.GetIntConfig("RequiredMedicineSkillForHealing", 50);
            this.MedicineHealingAmount = ConfigManager.GetIntConfig("MedicineHealingAmount", 15);
            this.ItemId = ConfigManager.GetStrConfig("MedicineItemId", "pe_doctorscalpel");
#endif
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
