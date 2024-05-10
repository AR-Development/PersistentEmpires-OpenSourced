using PersistentEmpires.Views.ViewsVM;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib.SceneScripts;
using System;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.GauntletUI.Mission;
using TaleWorlds.MountAndBlade.Missions.Handlers;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;

namespace PersistentEmpires.Views.Views
{
    public class PEAgentStatusView : MissionGauntletBattleUIBase
    {
        private GauntletLayer _gauntletLayer;
        private PEHungerVM _dataSource;

        public override void OnMissionStateActivated()
        {
            base.OnMissionStateActivated();
            PEHungerVM dataSource = this._dataSource;
            if (dataSource == null)
            {
                return;
            }
            dataSource.OnMainAgentWeaponChange();
        }

        // Token: 0x06000185 RID: 389 RVA: 0x00008AC8 File Offset: 0x00006CC8
        public override void EarlyStart()
        {
            base.EarlyStart();
            this._dataSource = new PEHungerVM(100, base.Mission, base.MissionScreen.CombatCamera, new Func<float>(base.MissionScreen.GetCameraToggleProgress));
            this._gauntletLayer = new GauntletLayer(this.ViewOrderPriority);
            this._gauntletLayer.LoadMovie("PEAgentStatusMain", this._dataSource);
            base.MissionScreen.AddLayer(this._gauntletLayer);
            this._dataSource.TakenDamageController.SetIsEnabled(BannerlordConfig.EnableDamageTakenVisuals);
            this.RegisterInteractionEvents();
            CombatLogManager.OnGenerateCombatLog += this.OnGenerateCombatLog;
            ManagedOptions.OnManagedOptionChanged = (ManagedOptions.OnManagedOptionChangedDelegate)Delegate.Combine(ManagedOptions.OnManagedOptionChanged, new ManagedOptions.OnManagedOptionChangedDelegate(this.OnManagedOptionChanged));
            this._agentHungerBehavior = base.Mission.GetMissionBehavior<AgentHungerBehavior>();
            this._agentHungerBehavior.OnAgentHungerChanged += this.SetHunger;
        }

        public void SetHunger(int hunger)
        {
            this._dataSource.Hunger = hunger;
        }

        protected override void OnCreateView()
        {
            this._dataSource.IsAgentStatusAvailable = true;
        }

        // Token: 0x06000187 RID: 391 RVA: 0x00008BA1 File Offset: 0x00006DA1
        protected override void OnDestroyView()
        {
            this._dataSource.IsAgentStatusAvailable = false;
        }

        // Token: 0x06000188 RID: 392 RVA: 0x00008BAF File Offset: 0x00006DAF
        private void OnManagedOptionChanged(ManagedOptions.ManagedOptionsType changedManagedOptionsType)
        {
            if (changedManagedOptionsType == ManagedOptions.ManagedOptionsType.EnableDamageTakenVisuals)
            {
                PEHungerVM dataSource = this._dataSource;
                if (dataSource == null)
                {
                    return;
                }
                dataSource.TakenDamageController.SetIsEnabled(BannerlordConfig.EnableDamageTakenVisuals);
            }
        }

        // Token: 0x06000189 RID: 393 RVA: 0x00008BD0 File Offset: 0x00006DD0
        public override void AfterStart()
        {
            base.AfterStart();
            PEHungerVM dataSource = this._dataSource;
            if (dataSource == null)
            {
                return;
            }
            dataSource.InitializeMainAgentPropterties();
        }

        // Token: 0x0600018A RID: 394 RVA: 0x00008BE8 File Offset: 0x00006DE8
        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this._isInDeployement = (base.Mission.GetMissionBehavior<BattleDeploymentHandler>() != null);
            if (this._isInDeployement)
            {
                this._deploymentMissionView = base.Mission.GetMissionBehavior<DeploymentMissionView>();
                if (this._deploymentMissionView != null)
                {
                    DeploymentMissionView deploymentMissionView = this._deploymentMissionView;
                    deploymentMissionView.OnDeploymentFinish = (OnPlayerDeploymentFinishDelegate)Delegate.Combine(deploymentMissionView.OnDeploymentFinish, new OnPlayerDeploymentFinishDelegate(this.OnDeploymentFinish));
                }
            }
        }

        // Token: 0x0600018B RID: 395 RVA: 0x00008C57 File Offset: 0x00006E57
        private void OnDeploymentFinish()
        {
            this._isInDeployement = false;
            DeploymentMissionView deploymentMissionView = this._deploymentMissionView;
            deploymentMissionView.OnDeploymentFinish = (OnPlayerDeploymentFinishDelegate)Delegate.Remove(deploymentMissionView.OnDeploymentFinish, new OnPlayerDeploymentFinishDelegate(this.OnDeploymentFinish));
        }

        // Token: 0x0600018C RID: 396 RVA: 0x00008C88 File Offset: 0x00006E88
        public override void OnMissionScreenFinalize()
        {
            base.OnMissionScreenFinalize();
            this.UnregisterInteractionEvents();
            ManagedOptions.OnManagedOptionChanged = (ManagedOptions.OnManagedOptionChangedDelegate)Delegate.Remove(ManagedOptions.OnManagedOptionChanged, new ManagedOptions.OnManagedOptionChangedDelegate(this.OnManagedOptionChanged));
            CombatLogManager.OnGenerateCombatLog -= this.OnGenerateCombatLog;
            base.MissionScreen.RemoveLayer(this._gauntletLayer);
            this._gauntletLayer = null;
            PEHungerVM dataSource = this._dataSource;
            if (dataSource != null)
            {
                dataSource.OnFinalize();
            }
            this._dataSource = null;
            this._missionMainAgentController = null;
        }

        // Token: 0x0600018D RID: 397 RVA: 0x00008D09 File Offset: 0x00006F09
        public override void OnMissionScreenTick(float dt)
        {
            base.OnMissionScreenTick(dt);
            this._dataSource.IsInDeployement = this._isInDeployement;
            this._dataSource.Tick(dt);

            if (this._missionMainAgentController != null && this._missionMainAgentController.InteractionComponent != null)
            {
                if (this._missionMainAgentController.InteractionComponent.CurrentFocusedObject != null)
                {
                    if (this._missionMainAgentController.InteractionComponent.CurrentFocusedObject is PE_Gate)
                    {
                        PE_Gate gate = (PE_Gate)this._missionMainAgentController.InteractionComponent.CurrentFocusedObject;
                        if (gate.GameEntity.HasScriptOfType<PE_RepairableDestructableComponent>())
                        {
                            PE_RepairableDestructableComponent comp = gate.GameEntity.GetFirstScriptOfType<PE_RepairableDestructableComponent>();
                            this._dataSource.InteractionInterface.OnFocusedHealthChanged(this._missionMainAgentController.InteractionComponent.CurrentFocusedObject, comp.HitPoint / comp.MaxHitPoint, true);
                        }
                    }

                    if (this._missionMainAgentController.InteractionComponent.CurrentFocusedObject is PE_NativeGate)
                    {
                        PE_NativeGate gate = (PE_NativeGate)this._missionMainAgentController.InteractionComponent.CurrentFocusedObject;
                        if (gate.GameEntity.HasScriptOfType<PE_RepairableDestructableComponent>())
                        {
                            PE_RepairableDestructableComponent comp = gate.GameEntity.GetFirstScriptOfType<PE_RepairableDestructableComponent>();
                            this._dataSource.InteractionInterface.OnFocusedHealthChanged(this._missionMainAgentController.InteractionComponent.CurrentFocusedObject, comp.HitPoint / comp.MaxHitPoint, true);
                        }
                    }
                    if (this._missionMainAgentController.InteractionComponent.CurrentFocusedObject is PE_InventoryEntity)
                    {
                        PE_InventoryEntity gate = (PE_InventoryEntity)this._missionMainAgentController.InteractionComponent.CurrentFocusedObject;
                        if (gate.GameEntity.HasScriptOfType<PE_RepairableDestructableComponent>())
                        {
                            PE_RepairableDestructableComponent comp = gate.GameEntity.GetFirstScriptOfType<PE_RepairableDestructableComponent>();
                            this._dataSource.InteractionInterface.OnFocusedHealthChanged(this._missionMainAgentController.InteractionComponent.CurrentFocusedObject, comp.HitPoint / comp.MaxHitPoint, true);
                        }
                    }
                    if (this._missionMainAgentController.InteractionComponent.CurrentFocusedObject is PE_RepairableDestructableComponent)
                    {
                        PE_RepairableDestructableComponent gate = (PE_RepairableDestructableComponent)this._missionMainAgentController.InteractionComponent.CurrentFocusedObject;
                        if (gate.GameEntity.HasScriptOfType<PE_RepairableDestructableComponent>())
                        {
                            PE_RepairableDestructableComponent comp = gate.GameEntity.GetFirstScriptOfType<PE_RepairableDestructableComponent>();
                            this._dataSource.InteractionInterface.OnFocusedHealthChanged(this._missionMainAgentController.InteractionComponent.CurrentFocusedObject, comp.HitPoint / comp.MaxHitPoint, true);
                        }
                    }
                    if (this._missionMainAgentController.InteractionComponent.CurrentFocusedObject is PE_DestructibleWithItem)
                    {
                        PE_DestructibleWithItem gate = (PE_DestructibleWithItem)this._missionMainAgentController.InteractionComponent.CurrentFocusedObject;
                        if (gate.GameEntity.HasScriptOfType<PE_DestructibleWithItem>())
                        {
                            PE_DestructibleWithItem comp = gate.GameEntity.GetFirstScriptOfType<PE_DestructibleWithItem>();
                            this._dataSource.InteractionInterface.OnFocusedHealthChanged(this._missionMainAgentController.InteractionComponent.CurrentFocusedObject, comp.HitPoint / comp.MaxHitPoint, true);
                        }
                    }
                }
            }
        }

        // Token: 0x0600018E RID: 398 RVA: 0x00008D2F File Offset: 0x00006F2F
        public override void OnFocusGained(Agent mainAgent, IFocusable focusableObject, bool isInteractable)
        {
            base.OnFocusGained(mainAgent, focusableObject, isInteractable);
            PEHungerVM dataSource = this._dataSource;
            if (dataSource == null)
            {
                return;
            }
            dataSource.OnFocusGained(mainAgent, focusableObject, isInteractable);
        }

        // Token: 0x0600018F RID: 399 RVA: 0x00008D4D File Offset: 0x00006F4D
        public override void OnAgentInteraction(Agent userAgent, Agent agent)
        {
            base.OnAgentInteraction(userAgent, agent);
            PEHungerVM dataSource = this._dataSource;
            if (dataSource == null)
            {
                return;
            }
            dataSource.OnAgentInteraction(userAgent, agent);
        }

        // Token: 0x06000190 RID: 400 RVA: 0x00008D69 File Offset: 0x00006F69
        public override void OnFocusLost(Agent agent, IFocusable focusableObject)
        {
            base.OnFocusLost(agent, focusableObject);
            PEHungerVM dataSource = this._dataSource;
            if (dataSource == null)
            {
                return;
            }
            dataSource.OnFocusLost(agent, focusableObject);
        }

        // Token: 0x06000191 RID: 401 RVA: 0x00008D85 File Offset: 0x00006F85
        public override void OnAgentDeleted(Agent affectedAgent)
        {
            PEHungerVM dataSource = this._dataSource;
            if (dataSource == null)
            {
                return;
            }
            dataSource.OnAgentDeleted(affectedAgent);
        }

        // Token: 0x06000192 RID: 402 RVA: 0x00008D98 File Offset: 0x00006F98
        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
        {
            PEHungerVM dataSource = this._dataSource;
            if (dataSource == null)
            {
                return;
            }
            dataSource.OnAgentRemoved(affectedAgent);
        }

        // Token: 0x06000193 RID: 403 RVA: 0x00008DAC File Offset: 0x00006FAC
        private void OnGenerateCombatLog(CombatLogData logData)
        {
            if (logData.IsVictimAgentMine && logData.TotalDamage > 0 && logData.BodyPartHit != BoneBodyPartType.None)
            {
                PEHungerVM dataSource = this._dataSource;
                if (dataSource == null)
                {
                    return;
                }
                dataSource.OnMainAgentHit(logData.TotalDamage, (float)(logData.IsRangedAttack ? 1 : 0));
            }
        }

        // Token: 0x06000194 RID: 404 RVA: 0x00008DF8 File Offset: 0x00006FF8
        private void RegisterInteractionEvents()
        {
            this._missionMainAgentController = base.Mission.GetMissionBehavior<MissionMainAgentController>();
            if (this._missionMainAgentController != null)
            {
                this._missionMainAgentController.InteractionComponent.OnFocusGained += this._dataSource.OnSecondaryFocusGained;
                this._missionMainAgentController.InteractionComponent.OnFocusLost += this._dataSource.OnSecondaryFocusLost;
                this._missionMainAgentController.InteractionComponent.OnFocusHealthChanged += this._dataSource.InteractionInterface.OnFocusedHealthChanged;
            }
            this._missionMainAgentEquipmentControllerView = base.Mission.GetMissionBehavior<MissionGauntletMainAgentEquipmentControllerView>();
            if (this._missionMainAgentEquipmentControllerView != null)
            {
                this._missionMainAgentEquipmentControllerView.OnEquipmentDropInteractionViewToggled += this._dataSource.OnEquipmentInteractionViewToggled;
                this._missionMainAgentEquipmentControllerView.OnEquipmentEquipInteractionViewToggled += this._dataSource.OnEquipmentInteractionViewToggled;
            }
        }



        private void UnregisterInteractionEvents()
        {
            if (this._missionMainAgentController != null)
            {
                this._missionMainAgentController.InteractionComponent.OnFocusGained -= this._dataSource.OnSecondaryFocusGained;
                this._missionMainAgentController.InteractionComponent.OnFocusLost -= this._dataSource.OnSecondaryFocusLost;
                this._missionMainAgentController.InteractionComponent.OnFocusHealthChanged -= this._dataSource.InteractionInterface.OnFocusedHealthChanged;
            }
            if (this._missionMainAgentEquipmentControllerView != null)
            {
                this._missionMainAgentEquipmentControllerView.OnEquipmentDropInteractionViewToggled -= this._dataSource.OnEquipmentInteractionViewToggled;
                this._missionMainAgentEquipmentControllerView.OnEquipmentEquipInteractionViewToggled -= this._dataSource.OnEquipmentInteractionViewToggled;
            }
        }

        // Token: 0x06000196 RID: 406 RVA: 0x00008F95 File Offset: 0x00007195
        public override void OnPhotoModeActivated()
        {
            base.OnPhotoModeActivated();
            this._gauntletLayer.UIContext.ContextAlpha = 0f;
        }

        // Token: 0x06000197 RID: 407 RVA: 0x00008FB2 File Offset: 0x000071B2
        public override void OnPhotoModeDeactivated()
        {
            base.OnPhotoModeDeactivated();
            this._gauntletLayer.UIContext.ContextAlpha = 1f;
        }
        // Token: 0x040000C3 RID: 195
        private MissionMainAgentController _missionMainAgentController;

        // Token: 0x040000C4 RID: 196
        private MissionGauntletMainAgentEquipmentControllerView _missionMainAgentEquipmentControllerView;

        // Token: 0x040000C5 RID: 197
        private DeploymentMissionView _deploymentMissionView;

        // Token: 0x040000C6 RID: 198
        private bool _isInDeployement;
        private AgentHungerBehavior _agentHungerBehavior;
    }
}
