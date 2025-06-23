using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpiresLib.SceneScripts
{
    public struct RepairReceipt
    {
        public RepairReceipt(string repairReceiptId, int count)
        {
            this.RepairItem = MBObjectManager.Instance.GetObject<ItemObject>(repairReceiptId);
            this.NeededCount = count;
        }

        public ItemObject RepairItem { get; private set; }
        public int NeededCount { get; private set; }
    }
    public class PE_RepairableDestructableComponent : PE_DestructableComponent
    {


        public string DestructionState = "";
        public string ReferenceEntityTag = "destructable_entity";
        public string RepairItem = "pe_buildhammer";
        public int RequiredEngineeringSkillForRepair = 10;
        public string ParticleEffectOnDestroy = "";
        public string SoundEffectOnDestroy = "";
        public string ParticleEffectOnRepair = "";
        public string SoundEffectOnRepair = "";
        public bool DestroyedByStoneOnly = false;
        public bool AlwaysEffectOnDestroy = false;

        public delegate void DestroyedHandler(ScriptComponentBehavior attackerScript);
        public event DestroyedHandler OnDestroyed;

        public delegate void RepairedHandler();
        public event RepairedHandler OnRepaired;

        public delegate void HitHandler(Agent hitterAgent, in MissionWeapon weapon, ScriptComponentBehavior attackerScriptComponentBehavior, int inflictedDamage);
        public event HitHandler OnHitTaken;

        public string RepairItemRecipies = "pe_hardwood*2,pe_wooden_stick*1";
        public int RepairDamage = 20;

        private GameEntity _healthyState;
        private GameEntity _brokenState;
        private List<RepairReceipt> receipt = new List<RepairReceipt>();
        private long lastHittedAt;

        private string _originalStatePrefab;

        public bool IsBroken = false;
        private PersistentEmpireSceneSyncBehaviors _persistentEmpireSceneSyncBehaviors;

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

        // protected void 
        protected bool ValidateValues()
        {
            if (!string.IsNullOrEmpty(this.ReferenceEntityTag))
            {
                GameEntity entity = base.GameEntity.GetChildren().FirstOrDefault((GameEntity x) => x.HasTag(this.ReferenceEntityTag));
                if (entity == null)
                {
                    MBEditor.AddEntityWarning(base.GameEntity, base.GameEntity.GetPrefabName() + " RepairableDestructableComponent game entity tagged " + this.ReferenceEntityTag + " not found in childrens");
                    return false;
                }
            }
            return true;
        }
        protected override void OnSceneSave(string saveFolder)
        {
            this.ValidateValues();
        }
        protected override bool OnCheckForProblems()
        {
            return this.ValidateValues();
        }

        public GameEntity BrokenState()
        {
            return this._brokenState;
        }

        public GameEntity HealthyState()
        {
            return this._healthyState;
        }

        protected override void OnInit()
        {
            base.OnInit();
            this.HitPoint = this.MaxHitPoint;
            this._healthyState = string.IsNullOrEmpty(this.ReferenceEntityTag) ? base.GameEntity : base.GameEntity.GetChildren().FirstOrDefault((GameEntity x) => x.HasTag(this.ReferenceEntityTag));
            this._originalStatePrefab = this._healthyState.GetPrefabName();
            if (this.DestructionState != "")
            {
                this._brokenState = GameEntity.Instantiate(Mission.Current.Scene, this.DestructionState, this._healthyState.GetGlobalFrame());
                base.GameEntity.AddChild(this._brokenState, true);
                this._brokenState.SetVisibilityExcludeParents(false);
            }
            this.ParseRepairReceipts();
            if (GameNetwork.IsServer)
            {
                this._persistentEmpireSceneSyncBehaviors = Mission.Current.GetMissionBehavior<PersistentEmpireSceneSyncBehaviors>();
            }
        }
        public override void SetHitPoint(float hitPoint, Vec3 impactDirection, ScriptComponentBehavior attackerScriptComponentBehavior)
        {
            this.HitPoint = hitPoint;
            MatrixFrame globalFrame = base.GameEntity.GetGlobalFrame();
            if (this.HitPoint > this.MaxHitPoint) this.HitPoint = this.MaxHitPoint;
            if (this.HitPoint < 0) this.HitPoint = 0;



            if (this.HitPoint == 0 && (this.AlwaysEffectOnDestroy || this.IsBroken == false))
            {
                if (this.ParticleEffectOnDestroy != "")
                {
                    Mission.Current.Scene.CreateBurstParticle(ParticleSystemManager.GetRuntimeIdByName(this.ParticleEffectOnDestroy), globalFrame);
                }
                if (this.SoundEffectOnDestroy != "")
                {
                    Mission.Current.MakeSound(SoundEvent.GetEventIdFromString(this.SoundEffectOnDestroy), globalFrame.origin, false, true, -1, -1);
                }
                if (this._brokenState != null)
                {
                    this._brokenState.SetVisibilityExcludeParents(true);
                    this._healthyState.SetVisibilityExcludeParents(false);
                }
                this.IsBroken = true;
                if (this.OnDestroyed != null) this.OnDestroyed(attackerScriptComponentBehavior);
            }
            if (this.HitPoint == this.MaxHitPoint)
            {
                if (this.ParticleEffectOnRepair != "")
                {
                    Mission.Current.Scene.CreateBurstParticle(ParticleSystemManager.GetRuntimeIdByName(this.ParticleEffectOnRepair), globalFrame);
                }
                if (this.SoundEffectOnRepair != "")
                {
                    Mission.Current.MakeSound(SoundEvent.GetEventIdFromString(this.SoundEffectOnRepair), globalFrame.origin, false, true, -1, -1);
                }
                if (this._brokenState != null)
                {
                    this._brokenState.SetVisibilityExcludeParents(false);
                    this._healthyState.SetVisibilityExcludeParents(true);
                }
                if (this.OnRepaired != null) this.OnRepaired();
                this.IsBroken = false;
            }
            if (GameNetwork.IsServer)
            {
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new SyncObjectHitpointsPE(this, impactDirection, this.HitPoint));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord, null);
            }
        }

        public void Reset()
        {
            this.SetHitPoint(this.MaxHitPoint, Vec3.Zero, null);
        }


        public void TriggerOnHit(Agent attackerAgent, int inflictedDamage, Vec3 impactPosition, Vec3 impactDirection, in MissionWeapon weapon, ScriptComponentBehavior attackerScriptComponentBehavior)
        {
            bool flag;
            this.OnHit(attackerAgent, inflictedDamage, impactPosition, impactDirection, weapon, attackerScriptComponentBehavior, out flag);
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
                (missionWeapon.Item.StringId == this.RepairItem || missionWeapon.Item.StringId == "pe_adminhammer") &&
                attackerAgent.IsHuman &&
                attackerAgent.IsPlayerControlled &&
                this.HitPoint != this.MaxHitPoint
                )
            {
                reportDamage = false;
                NetworkCommunicator player = attackerAgent.MissionPeer.GetNetworkPeer();
                PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
                if (persistentEmpireRepresentative == null) return false;
                bool isAdmin = Main.IsPlayerAdmin(player);

                if (missionWeapon.Item.StringId == "pe_adminhammer")
                {
                    this.SetHitPoint(this.MaxHitPoint, impactDirection, attackerScriptComponentBehavior);
                    InformationComponent.Instance.SendMessage((this.HitPoint + this.RepairDamage).ToString() + "/" + this.MaxHitPoint + ", repaired", 0x02ab89d9, player);
                    LoggerHelper.LogAnAction(attackerAgent.MissionPeer.GetNetworkPeer(), LogAction.PlayerRepairesTheDestructable, null, new object[] { this.GetType().Name });
                    return false;
                }

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
                if (this.lastHittedAt + this._persistentEmpireSceneSyncBehaviors.RepairTimeoutAfterHit > DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                {
                    InformationComponent.Instance.SendMessage("You can't repair this object right now. You need to wait", 0x02ab89d9, player);
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
                    LoggerHelper.LogAnAction(attackerAgent.MissionPeer.GetNetworkPeer(), LogAction.PlayerRepairesTheDestructable, null, new object[] { this.GetType().Name });
                }
            }
            else
            {
                try
                {
                    if (this.DestroyedByStoneOnly)
                    {

                        if (currentUsageItem == null || (currentUsageItem.WeaponClass != WeaponClass.Stone && currentUsageItem.WeaponClass != WeaponClass.Boulder) || !currentUsageItem.WeaponFlags.HasAnyFlag(WeaponFlags.NotUsableWithOneHand))
                        {
                            damage = 0;
                        }

                    }
                }
                catch (NullReferenceException e)
                {
                    damage = 0;
                }
                if (impactDirection == null) impactDirection = Vec3.Zero;
                if (this.OnHitTaken != null)
                {
                    this.OnHitTaken(attackerAgent, weapon, attackerScriptComponentBehavior, damage);
                }
                this.SetHitPoint(this.HitPoint - damage, impactDirection, attackerScriptComponentBehavior);
                this.lastHittedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if (GameNetwork.IsServer)
                {
                    LoggerHelper.LogAnAction(attackerAgent.MissionPeer.GetNetworkPeer(), LogAction.PlayerHitToDestructable, null, new object[] { this.GetType().Name });
                }
            }
            return false;
        }
    }
}
