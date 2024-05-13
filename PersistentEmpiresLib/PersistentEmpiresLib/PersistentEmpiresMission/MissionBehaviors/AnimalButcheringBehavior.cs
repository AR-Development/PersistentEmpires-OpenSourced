using PersistentEmpiresLib.SceneScripts;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class AnimalButcheringBehavior : MissionNetwork
    {
        public Dictionary<Agent, PE_AnimalSpawner> AgentToAnimalSpawner;
        public static AnimalButcheringBehavior Instance;
        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            this.AgentToAnimalSpawner = new Dictionary<Agent, PE_AnimalSpawner>();
            AnimalButcheringBehavior.Instance = this;
        }
        private void SpawnItem(MatrixFrame frame, ItemObject item)
        {
            MissionWeapon spawnWeapon = new MissionWeapon(item, null, null);
            frame.Rotate(MBMath.PI / 2, new Vec3(1, 0, 0));
            frame.origin.x += MBRandom.RandomFloatRanged(-1, 1);
            frame.origin.z += 1;
            frame.origin.y += MBRandom.RandomFloatRanged(-1, 1);
            frame.Scale(new Vec3(1, 1, 1));
            GameEntity entity = Mission.Current.SpawnWeaponWithNewEntity(ref spawnWeapon, Mission.WeaponSpawnFlags.WithPhysics | Mission.WeaponSpawnFlags.WithHolster, frame);
            SpawnedItemEntity itemEntity = entity.GetFirstScriptOfType<SpawnedItemEntity>();
            itemEntity.HasLifeTime = true;
        }
        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);

            if (!AgentToAnimalSpawner.ContainsKey(affectedAgent) || affectorAgent == null)
            {
                return;
            }
            PE_AnimalSpawner spawner = AgentToAnimalSpawner[affectedAgent];
            AgentToAnimalSpawner[affectedAgent].RemoveSpawnedAnimal(affectedAgent);
            AgentToAnimalSpawner.Remove(affectedAgent);

            if (spawner != null && affectorAgent.IsHuman && affectorAgent.IsPlayerControlled)
            {
                SkillObject relevantButcheringSkill = MBObjectManager.Instance.GetObject<SkillObject>(spawner.RelevantButcheringSkill);
                if (relevantButcheringSkill == null)
                {
                    return;
                }
                int requiredSkill = spawner.RelevantRequiredSkill;
                if (affectorAgent.Character == null || affectorAgent.Character.GetSkillValue(relevantButcheringSkill) < requiredSkill)
                {
                    return;
                }
                EquipmentIndex mainHandIndex = affectorAgent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
                if (mainHandIndex == EquipmentIndex.None) return;

                ItemObject item = affectorAgent.Equipment[mainHandIndex].Item;
                if (item == null || item.StringId != spawner.ButcheringItem)
                {
                    return;
                }
                foreach (Receipt r in spawner.GetDropReceipts())
                {
                    for (int i = 0; i < r.NeededCount; i++)
                    {
                        this.SpawnItem(affectedAgent.Frame, r.Item);
                    }
                }
            }

        }
    }
}
