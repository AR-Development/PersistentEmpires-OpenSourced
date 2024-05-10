using System.Linq;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects.Siege;

namespace PersistentEmpiresLib.SceneScripts
{
    public class PE_BatteringRamSpawner : SpawnerBase
    {
        // Token: 0x0600319B RID: 12699 RVA: 0x000CC946 File Offset: 0x000CAB46
        protected override void OnEditorInit()
        {
            base.OnEditorInit();
            this._spawnerEditorHelper = new SpawnerEntityEditorHelper(this);
            this._spawnerEditorHelper.LockGhostParent = false;
            if (this._spawnerEditorHelper.IsValid)
            {
                this._spawnerEditorHelper.SetupGhostMovement(this.PathEntityName);
            }
        }

        // Token: 0x0600319C RID: 12700 RVA: 0x000CC984 File Offset: 0x000CAB84
        protected override void OnEditorTick(float dt)
        {
            base.OnEditorTick(dt);
            this._spawnerEditorHelper.Tick(dt);
        }

        // Token: 0x0600319D RID: 12701 RVA: 0x000CC99C File Offset: 0x000CAB9C
        protected override void OnEditorVariableChanged(string variableName)
        {
            base.OnEditorVariableChanged(variableName);
            if (variableName == "PathEntityName")
            {
                this._spawnerEditorHelper.SetupGhostMovement(this.PathEntityName);
                return;
            }
            if (variableName == "EnableAutoGhostMovement")
            {
                this._spawnerEditorHelper.SetEnableAutoGhostMovement(this.EnableAutoGhostMovement);
                return;
            }
            if (variableName == "SpeedModifierFactor")
            {
                this.SpeedModifierFactor = MathF.Clamp(this.SpeedModifierFactor, 0.8f, 1.2f);
            }
        }

        // Token: 0x0600319E RID: 12702 RVA: 0x000CCA18 File Offset: 0x000CAC18
        protected override bool OnCheckForProblems()
        {
            bool flag = base.OnCheckForProblems();
            if (!base.Scene.IsMultiplayerScene() && base.Scene.FindEntitiesWithTag("ditch_filler").FirstOrDefault((GameEntity df) => df.HasTag(this.SideTag)) != null)
            {
                if (this.DitchNavMeshID_1 >= 0 && !base.Scene.IsAnyFaceWithId(this.DitchNavMeshID_1))
                {
                    MBEditor.AddEntityWarning(base.GameEntity, "Couldn't find any face with 'DitchNavMeshID_1' id.");
                    flag = true;
                }
                if (this.DitchNavMeshID_2 >= 0 && !base.Scene.IsAnyFaceWithId(this.DitchNavMeshID_2))
                {
                    MBEditor.AddEntityWarning(base.GameEntity, "Couldn't find any face with 'DitchNavMeshID_2' id.");
                    flag = true;
                }
                if (this.GroundToBridgeNavMeshID_1 >= 0 && !base.Scene.IsAnyFaceWithId(this.GroundToBridgeNavMeshID_1))
                {
                    MBEditor.AddEntityWarning(base.GameEntity, "Couldn't find any face with 'GroundToBridgeNavMeshID_1' id.");
                    flag = true;
                }
                if (this.GroundToBridgeNavMeshID_2 >= 0 && !base.Scene.IsAnyFaceWithId(this.GroundToBridgeNavMeshID_2))
                {
                    MBEditor.AddEntityWarning(base.GameEntity, "Couldn't find any face with 'GroundToBridgeNavMeshID_1' id.");
                    flag = true;
                }
                if (this.BridgeNavMeshID_1 >= 0 && !base.Scene.IsAnyFaceWithId(this.BridgeNavMeshID_1))
                {
                    MBEditor.AddEntityWarning(base.GameEntity, "Couldn't find any face with 'BridgeNavMeshID_1' id.");
                    flag = true;
                }
                if (this.BridgeNavMeshID_2 >= 0 && !base.Scene.IsAnyFaceWithId(this.BridgeNavMeshID_2))
                {
                    MBEditor.AddEntityWarning(base.GameEntity, "Couldn't find any face with 'BridgeNavMeshID_2' id.");
                    flag = true;
                }
            }
            return flag;
        }

        // Token: 0x0600319F RID: 12703 RVA: 0x000CCB7D File Offset: 0x000CAD7D
        protected override void OnPreInit()
        {
            base.OnPreInit();
            this._spawnerMissionHelper = new SpawnerEntityMissionHelper(this, false);
        }

        // Token: 0x060031A0 RID: 12704 RVA: 0x000CCB94 File Offset: 0x000CAD94
        public override void AssignParameters(SpawnerEntityMissionHelper _spawnerMissionHelper)
        {
            PE_BatteringRam firstScriptOfType = _spawnerMissionHelper.SpawnedEntity.GetFirstScriptOfType<PE_BatteringRam>();
            // firstScriptOfType.AddOnDeployTag = this.AddOnDeployTag;
            // firstScriptOfType.RemoveOnDeployTag = this.RemoveOnDeployTag;
            firstScriptOfType.MaxSpeed *= this.SpeedModifierFactor;
            firstScriptOfType.MinSpeed *= this.SpeedModifierFactor;
            firstScriptOfType.AssignParametersFromSpawner(this.GateTag, this.SideTag, this.BridgeNavMeshID_1, this.BridgeNavMeshID_2, this.DitchNavMeshID_1, this.DitchNavMeshID_2, this.GroundToBridgeNavMeshID_1, this.GroundToBridgeNavMeshID_2, this.PathEntityName);
        }

        // Token: 0x0400153B RID: 5435
        private const float _modifierFactorUpperLimit = 1.2f;

        // Token: 0x0400153C RID: 5436
        private const float _modifierFactorLowerLimit = 0.8f;

        // Token: 0x0400153D RID: 5437
        [SpawnerBase.SpawnerPermissionField]
        public MatrixFrame wait_pos_ground = MatrixFrame.Zero;

        // Token: 0x0400153E RID: 5438
        [EditorVisibleScriptComponentVariable(true)]
        public string SideTag;

        // Token: 0x0400153F RID: 5439
        [EditorVisibleScriptComponentVariable(true)]
        public string GateTag = "";

        // Token: 0x04001540 RID: 5440
        [EditorVisibleScriptComponentVariable(true)]
        public string PathEntityName = "Path";

        // Token: 0x04001541 RID: 5441
        [EditorVisibleScriptComponentVariable(true)]
        public int BridgeNavMeshID_1 = 8;

        // Token: 0x04001542 RID: 5442
        [EditorVisibleScriptComponentVariable(true)]
        public int BridgeNavMeshID_2 = 8;

        // Token: 0x04001543 RID: 5443
        [EditorVisibleScriptComponentVariable(true)]
        public int DitchNavMeshID_1 = 9;

        // Token: 0x04001544 RID: 5444
        [EditorVisibleScriptComponentVariable(true)]
        public int DitchNavMeshID_2 = 10;

        // Token: 0x04001545 RID: 5445
        [EditorVisibleScriptComponentVariable(true)]
        public int GroundToBridgeNavMeshID_1 = 12;

        // Token: 0x04001546 RID: 5446
        [EditorVisibleScriptComponentVariable(true)]
        public int GroundToBridgeNavMeshID_2 = 13;

        // Token: 0x04001547 RID: 5447
        [EditorVisibleScriptComponentVariable(true)]
        public string AddOnDeployTag = "";

        // Token: 0x04001548 RID: 5448
        [EditorVisibleScriptComponentVariable(true)]
        public string RemoveOnDeployTag = "";

        // Token: 0x04001549 RID: 5449
        [EditorVisibleScriptComponentVariable(true)]
        public float SpeedModifierFactor = 1f;

        // Token: 0x0400154A RID: 5450
        public bool EnableAutoGhostMovement;
    }
}
