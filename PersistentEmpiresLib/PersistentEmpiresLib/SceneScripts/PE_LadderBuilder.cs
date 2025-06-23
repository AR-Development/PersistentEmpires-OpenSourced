using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.SceneScripts
{
    public class PE_LadderBuilder : PE_DestructableComponent
    {
        public string DestructionState = "";
        public string LadderEntityTag = "ladder_entity";
        public string RepairItem = "pe_buildhammer";
        public int RequiredEngineeringSkillForRepair = 10;
        public string ParticleEffectOnDestroy = "";
        public string SoundEffectOnDestroy = "";
        public string ParticleEffectOnRepair = "";
        public string SoundEffectOnRepair = "";
        public bool DestroyedByStoneOnly = false;
        public bool AlwaysEffectOnDestroy = false;

        public string RepairItemRecipies = "pe_hardwood*2,pe_wooden_stick*1";
        public int RepairDamage = 20;

        // private SiegeLadder siegeLadder;
        private List<RepairReceipt> receipt = new List<RepairReceipt>();
        private bool ladderBuilt = false;
        private bool initialized = false;
        private SiegeLadder siegeLadder;
        protected override void OnInit()
        {
            this.HitPoint = 0;
            this.ParseRepairReceipts();
        }

        public override ScriptComponentBehavior.TickRequirement GetTickRequirement() => initialized ? ScriptComponentBehavior.TickRequirement.None : ScriptComponentBehavior.TickRequirement.Tick;

        protected override void OnTick(float dt)
        {
            try
            {
                if (initialized == false)
                {
                    this.siegeLadder = base.GameEntity.Parent.GetFirstScriptInFamilyDescending<SiegeLadder>();
                    if (this.siegeLadder == null) return;
                    this.siegeLadder.GameEntity.SetVisibilityExcludeParents(ladderBuilt);
                    initialized = true;
                }
            }
            catch (Exception e)
            {

            }
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

        public bool GetLadderBuilt() => this.ladderBuilt;

        public void SetLadderBuilt(bool built)
        {
            this.ladderBuilt = built;
            if (this.siegeLadder != null)
            {
                this.siegeLadder.GameEntity.SetVisibilityExcludeParents(built);
            }
        }

        public override void SetHitPoint(float hitPoint, Vec3 impactDirection, ScriptComponentBehavior attackerScriptComponentBehavior)
        {

            if (hitPoint <= 0) hitPoint = 0;
            if (hitPoint >= this.MaxHitPoint) hitPoint = this.MaxHitPoint;

            this.HitPoint = hitPoint;

            if (this.ladderBuilt && this.HitPoint <= 0)
            {
                if (this.ParticleEffectOnDestroy != "")
                {
                    Mission.Current.Scene.CreateBurstParticle(ParticleSystemManager.GetRuntimeIdByName(this.ParticleEffectOnDestroy), base.GameEntity.GetGlobalFrame());
                }
                if (this.SoundEffectOnDestroy != "")
                {
                    Mission.Current.MakeSound(SoundEvent.GetEventIdFromString(this.SoundEffectOnDestroy), base.GameEntity.GetGlobalFrame().origin, false, true, -1, -1);
                }
                this.siegeLadder.GameEntity.SetVisibilityExcludeParents(false);
                ladderBuilt = false;
            }
            else if (this.ladderBuilt == false && this.HitPoint >= this.MaxHitPoint)
            {
                if (this.ParticleEffectOnRepair != "")
                {
                    Mission.Current.Scene.CreateBurstParticle(ParticleSystemManager.GetRuntimeIdByName(this.ParticleEffectOnRepair), base.GameEntity.GetGlobalFrame());
                }
                if (this.SoundEffectOnRepair != "")
                {
                    Mission.Current.MakeSound(SoundEvent.GetEventIdFromString(this.SoundEffectOnRepair), base.GameEntity.GetGlobalFrame().origin, false, true, -1, -1);
                }
                this.siegeLadder.GameEntity.SetVisibilityExcludeParents(true);
                ladderBuilt = true;
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
                this.HitPoint != this.MaxHitPoint
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
            else if (ladderBuilt == false || (ladderBuilt == true && this.siegeLadder.State == SiegeLadder.LadderState.OnLand))
            {
                if (this.DestroyedByStoneOnly)
                {
                    if (currentUsageItem == null || (currentUsageItem.WeaponClass != WeaponClass.Stone && currentUsageItem.WeaponClass != WeaponClass.Boulder) || !currentUsageItem.WeaponFlags.HasAnyFlag(WeaponFlags.NotUsableWithOneHand))
                    {
                        damage = 0;
                    }
                }
                if (impactDirection == null) impactDirection = Vec3.Zero;

                this.SetHitPoint(this.HitPoint - damage, impactDirection, attackerScriptComponentBehavior);
                if (GameNetwork.IsServer)
                {
                    // LoggerHelper.LogAnAction(attackerAgent.MissionPeer.GetNetworkPeer(), LogAction.PlayerHitToDestructable, null, new object[] { this.GetType().Name });
                }
            }
            return false;
        }
    }
}
