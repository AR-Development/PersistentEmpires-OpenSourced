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
#if SERVER
            this.ItemId = ConfigManager.GetStrConfig("PickpocketingItem", "pe_stealing_dagger");
            this.ThousandPercentage = ConfigManager.GetIntConfig("PickpocketingPercentageThousands", 50);
            this.RequiredPickpocketing = ConfigManager.GetIntConfig("RequiredPickpocketing", 10);
#endif
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
