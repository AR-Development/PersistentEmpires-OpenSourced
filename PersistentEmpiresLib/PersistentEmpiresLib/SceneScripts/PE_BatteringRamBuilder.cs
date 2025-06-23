using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.SceneScripts
{
    public class PE_BatteringRamBuilder : PE_DestructableComponent
    {
        public int RequiredEngineeringSkillForRepair = 10;
        public string RepairItemRecipies = "pe_hardwood*2,pe_wooden_stick*1";
        public string RepairItem = "pe_buildhammer";
        public string ParticleEffectOnDestroy = "";
        public string SoundEffectOnDestroy = "";
        public string ParticleEffectOnRepair = "";
        public string SoundEffectOnRepair = "";
        public int RepairDamage = 20;

        private List<RepairReceipt> receipt = new List<RepairReceipt>();
        private bool batteringRamBuilt = false;
        private bool initialized = false;
        private bool batteringRamDestroyed = false;
        private long resetAt = 0;
        private PE_BatteringRam batteringRam;

        protected override void OnInit()
        {
            this.HitPoint = 0;
            this.ParseRepairReceipts();
        }
        private void ParseRepairReceipts()
        {
            string[] repairReceipt = this.RepairItemRecipies.Split(',');
            foreach (string receipt in repairReceipt)
            {
                string[] inflictedReceipt = receipt.Split('*');
                string receiptId = inflictedReceipt[0];
                int count = int.Parse(inflictedReceipt[1]);
                this.receipt.Add(new RepairReceipt(receiptId, count));
            }
        }

        protected override void OnTick(float dt)
        {
            try
            {
                if (initialized == false)
                {
                    this.batteringRam = base.GameEntity.Parent.GetFirstScriptInFamilyDescending<PE_BatteringRam>();
                    if (this.batteringRam == null) return;
                    this.batteringRam.DestructionComponent.OnDestroyed += this.BatteringRamOnDestroyed;
                    this.batteringRam.GameEntity.SetVisibilityExcludeParents(false);
                    initialized = true;
                }
            }
            catch (Exception e)
            {
            }
            if (this.batteringRamDestroyed == true && DateTimeOffset.UtcNow.ToUnixTimeSeconds() >= this.resetAt)
            {
                // Reset the shit man.
                this.Reset();
                if (GameNetwork.IsServer)
                {
                    GameNetwork.BeginBroadcastModuleEvent();
                    GameNetwork.WriteMessage(new ResetBatteringRam(this));
                    GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
                }
            }
        }
        public override ScriptComponentBehavior.TickRequirement GetTickRequirement() => initialized ? ScriptComponentBehavior.TickRequirement.None : ScriptComponentBehavior.TickRequirement.Tick;
        private void BatteringRamOnDestroyed(DestructableComponent target, Agent attackerAgent, in MissionWeapon weapon, ScriptComponentBehavior attackerScriptComponentBehavior, int inflictedDamage)
        {
            this.batteringRamDestroyed = true;
            this.resetAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 5;
        }

        public void Reset()
        {
            MethodInfo resetMethod = this.batteringRam.GetType().GetMethod("OnMissionReset", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            resetMethod.Invoke(this.batteringRam, new object[] { });
            this.batteringRam.DestructionComponent.Reset();
            this.batteringRam.GameEntity.SetVisibilityExcludeParents(false);
            this.batteringRamBuilt = false;
            this.SetHitPoint(0, new Vec3(0, 0, 0), null);
            this.batteringRamDestroyed = false;
        }


        public override void SetHitPoint(float hitPoint, Vec3 impactDirection, ScriptComponentBehavior attackerScriptComponentBehavior)
        {
            if (hitPoint <= 0) hitPoint = 0;
            if (hitPoint >= this.MaxHitPoint) hitPoint = this.MaxHitPoint;

            this.HitPoint = hitPoint;
            if (this.batteringRamBuilt == false && this.HitPoint >= this.MaxHitPoint)
            {
                if (this.ParticleEffectOnRepair != "")
                {
                    Mission.Current.Scene.CreateBurstParticle(ParticleSystemManager.GetRuntimeIdByName(this.ParticleEffectOnRepair), base.GameEntity.GetGlobalFrame());
                }
                if (this.SoundEffectOnRepair != "")
                {
                    Mission.Current.MakeSound(SoundEvent.GetEventIdFromString(this.SoundEffectOnRepair), base.GameEntity.GetGlobalFrame().origin, false, true, -1, -1);
                }
                this.batteringRam.GameEntity.SetVisibilityExcludeParents(true);
                batteringRamBuilt = true;
            }
            if (GameNetwork.IsServer)
            {
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new SyncObjectHitpointsPE(this, impactDirection, this.HitPoint));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord, null);
            }
        }
        protected override bool OnHit(Agent attackerAgent, int damage, Vec3 impactPosition, Vec3 impactDirection, in MissionWeapon weapon, ScriptComponentBehavior attackerScriptComponentBehavior, out bool reportDamage)
        {
            reportDamage = true;
            MissionWeapon missionWeapon = weapon;
            WeaponComponentData currentUsageItem = missionWeapon.CurrentUsageItem;
            if (
                attackerAgent != null &&
                attackerAgent.Character.GetSkillValue(DefaultSkills.Engineering) >= this.RequiredEngineeringSkillForRepair &&
                missionWeapon.Item != null &&
                missionWeapon.Item.StringId == this.RepairItem &&
                attackerAgent.IsHuman &&
                attackerAgent.IsPlayerControlled &&
                this.HitPoint != this.MaxHitPoint &&
                this.batteringRamBuilt == false
                )
            {
                reportDamage = false;
                NetworkCommunicator player = attackerAgent.MissionPeer.GetNetworkPeer();
                PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
                if (persistentEmpireRepresentative == null) return false;
                bool playerHasAllItems = this.receipt.All((r) => persistentEmpireRepresentative.GetInventory().IsInventoryIncludes(r.RepairItem, r.NeededCount));
                if (!playerHasAllItems)
                {
                    //TODO: Inform player
                    InformationComponent.Instance.SendMessage(GameTexts.FindText("PE_Required_Items", null).ToString(), 0x02ab89d9, player);
                    foreach (RepairReceipt r in this.receipt)
                    {
                        InformationComponent.Instance.SendMessage(r.NeededCount + " * " + r.RepairItem.Name.ToString(), 0x02ab89d9, player);
                    }
                    return false;
                }
                foreach (RepairReceipt r in this.receipt)
                {
                    persistentEmpireRepresentative.GetInventory().RemoveCountedItem(r.RepairItem, r.NeededCount);
                }
                InformationComponent.Instance.SendMessage((this.HitPoint + this.RepairDamage).ToString() + "/" + this.MaxHitPoint + ", repaired", 0x02ab89d9, player);
                this.SetHitPoint(this.HitPoint + this.RepairDamage, impactDirection, attackerScriptComponentBehavior);
                if (GameNetwork.IsServer)
                {
                    // LoggerHelper.LogAnAction(attackerAgent.MissionPeer.GetNetworkPeer(), LogAction.PlayerRepairesTheDestructable, null, new object[] { this.GetType().Name });
                }
            }

            return true;
        }
    }
}
