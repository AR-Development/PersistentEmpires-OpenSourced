using System.Reflection;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.SceneScripts
{
    public class PE_LiftingDoor : UsableMachine
    {
        public enum PortcullisState
        {
            Idle,
            Elevating,
            Lowering
        }

        public string PortcullisTag = "";
        public string ElevationAnimation = "";
        public string LowerAnimation = "";
        public string ElevatorTag = "elevate_standpoint";
        public string LowerTag = "lower_standpoint";
        public string ElevationPointTag = "elevate_point";
        public float ElevationSpeed = 1f;
        public float LowerSpeed = 3f;

        private float _maxElevationZ { get; set; }
        private float _minElevationZ { get; set; }
        private GameEntity _portcullis;
        private PortcullisState portcullisState = PortcullisState.Idle;
        public override ScriptComponentBehavior.TickRequirement GetTickRequirement() => !this.GameEntity.IsVisibleIncludeParents() ? base.GetTickRequirement() : ScriptComponentBehavior.TickRequirement.Tick | ScriptComponentBehavior.TickRequirement.TickParallel;
        protected override void OnInit()
        {
            base.OnInit();
            this._portcullis = base.GameEntity.GetFirstChildEntityWithTag(this.PortcullisTag);
            this._maxElevationZ = base.GameEntity.GetFirstChildEntityWithTag(this.ElevationPointTag).GetFrame().origin.Z;
            this._minElevationZ = this._portcullis.GetFrame().origin.Z;

            SynchedMissionObject synchObject = this._portcullis.GetFirstScriptOfType<SynchedMissionObject>();

            var prop = typeof(SynchedMissionObject).GetField("_initialSynchFlags", BindingFlags.NonPublic | BindingFlags.Instance);
            SynchedMissionObject.SynchFlags syncFlags = (SynchedMissionObject.SynchFlags)prop.GetValue(synchObject);
            syncFlags |= SynchFlags.SynchTransform;
            prop.SetValue(synchObject, syncFlags);

            MatrixFrame frame = this._portcullis.GetFrame();
            frame.origin.z = this._maxElevationZ;
            this._portcullis.SetFrame(ref frame);


            foreach (StandingPoint standingPoint in this.StandingPoints)
            {
                standingPoint.AddComponent(new ResetAnimationOnStopUsageComponent(ActionIndexCache.act_none));
                standingPoint.AutoSheathWeapons = true;
            }
        }
        protected bool ValidateValues()
        {
            GameEntity portcullis = base.GameEntity.GetFirstChildEntityWithTag(this.PortcullisTag);
            GameEntity elevation = base.GameEntity.GetFirstChildEntityWithTag(this.ElevationPointTag);
            if (portcullis == null)
            {
                MBEditor.AddEntityWarning(base.GameEntity, "Portcullis body not found");
                return false;
            }
            if (elevation == null)
            {
                MBEditor.AddEntityWarning(base.GameEntity, "Elevation point not found");
                return false;
            }
            if (portcullis.GetFirstScriptOfType<SynchedMissionObject>() == null)
            {
                MBEditor.AddEntityWarning(base.GameEntity, "Portcullis body should have a SyncehdMissionObject script");
                return false;
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

        protected void UseTick(float dt)
        {
            bool hasUser = false;
            foreach (StandingPoint standingPoint in base.StandingPoints)
            {
                if (standingPoint.HasUser)
                {
                    hasUser = true;
                    MatrixFrame frame = this._portcullis.GetFrame();

                    if (frame.origin.Z >= this._maxElevationZ && standingPoint.GameEntity.HasTag(this.ElevatorTag))
                    {
                        portcullisState = PortcullisState.Idle;
                        standingPoint.UserAgent.StopUsingGameObjectMT(true);
                    }
                    else if (frame.origin.Z <= this._minElevationZ && standingPoint.GameEntity.HasTag(this.LowerTag))
                    {
                        portcullisState = PortcullisState.Idle;
                        standingPoint.UserAgent.StopUsingGameObjectMT(true);
                    }
                    else if (standingPoint.GameEntity.HasTag(this.ElevatorTag))
                    {
                        if (standingPoint.UserAgent.GetCurrentAction(0).Name == "act_none" && this.ElevationAnimation != "")
                        {
                            standingPoint.UserAgent.SetActionChannel(0, ActionIndexCache.Create(this.ElevationAnimation), true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
                        }
                        portcullisState = PortcullisState.Elevating;
                    }
                    else if (standingPoint.GameEntity.HasTag(this.LowerTag))
                    {
                        if (standingPoint.UserAgent.GetCurrentAction(0).Name == "act_none" && this.LowerAnimation != "")
                        {
                            standingPoint.UserAgent.SetActionChannel(0, ActionIndexCache.Create(this.LowerAnimation), true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
                        }
                        portcullisState = PortcullisState.Lowering;
                    }

                }
            }
            if (!hasUser)
            {
                this.portcullisState = PortcullisState.Idle;
            }
        }

        protected void TickAux(float dt)
        {
            if (this.portcullisState == PortcullisState.Elevating)
            {
                MatrixFrame frame = this._portcullis.GetFrame();
                frame.Elevate(this.ElevationSpeed * dt);
                if (frame.origin.Z >= this._maxElevationZ)
                {
                    Vec3 vec = frame.origin;
                    vec.z = this._maxElevationZ;
                    frame.TransformToLocal(vec);
                    this.portcullisState = PortcullisState.Idle;
                }
                this._portcullis.SetFrame(ref frame);
            }
            else if (this.portcullisState == PortcullisState.Lowering)
            {
                MatrixFrame frame = this._portcullis.GetFrame();
                frame.Elevate(-1 * this.LowerSpeed * dt);
                if (frame.origin.Z <= this._minElevationZ)
                {
                    Vec3 vec = frame.origin;
                    vec.z = this._minElevationZ;
                    frame.TransformToLocal(vec);
                    this.portcullisState = PortcullisState.Idle;
                }
                this._portcullis.SetFrame(ref frame);
            }
        }

        protected override void OnTick(float dt)
        {
            base.OnTick(dt);
            this.UseTick(dt);
            this.TickAux(dt);
        }

        public override TextObject GetActionTextForStandingPoint(UsableMissionObject usableGameObject)
        {
            if (usableGameObject.GameEntity.HasTag(this.ElevatorTag))
            {
                TextObject forStandingPoint = new TextObject("Elevate Portcullis");
                forStandingPoint.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
                return forStandingPoint;
            }
            else if (usableGameObject.GameEntity.HasTag(this.LowerTag))
            {
                TextObject forStandingPoint = new TextObject("Lower Portcullis");
                forStandingPoint.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
                return forStandingPoint;
            }

            return new TextObject("");
        }

        public override string GetDescriptionText(GameEntity gameEntity = null)
        {
            return "";
        }
    }
}
