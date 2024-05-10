using System.Reflection;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.SceneScripts
{
    public class PE_AToBNavigator : UsableMachine
    {
        public enum AToBNavigatorState
        {
            Idle,
            GoingToA,
            GoingToB
        }

        public string PortcullisTag = "";
        public string ElevationAnimation = "";
        public string LowerAnimation = "";

        public string ToAUseTag = "toa_standpoint";
        public string ToBUseTag = "tob_standpoint";

        public string PointATag = "point_a";
        public string PointBTag = "point_b";

        public float ToASpeed = 1f;
        public float ToBSpeed = 1f;

        private Vec3 _pointA { get; set; }
        private Vec3 _pointB { get; set; }
        private GameEntity _portcullis;
        private AToBNavigatorState portcullisState = AToBNavigatorState.Idle;
        public override ScriptComponentBehavior.TickRequirement GetTickRequirement() => !this.GameEntity.IsVisibleIncludeParents() ? base.GetTickRequirement() : ScriptComponentBehavior.TickRequirement.Tick | ScriptComponentBehavior.TickRequirement.TickParallel;
        protected override void OnInit()
        {
            base.OnInit();
            this._portcullis = base.GameEntity.GetFirstChildEntityWithTag(this.PortcullisTag);
            this._pointA = base.GameEntity.GetFirstChildEntityWithTag(this.PointATag).GetFrame().origin;
            this._pointB = base.GameEntity.GetFirstChildEntityWithTag(this.PointBTag).GetFrame().origin;

            SynchedMissionObject synchObject = this._portcullis.GetFirstScriptOfType<SynchedMissionObject>();

            var prop = typeof(SynchedMissionObject).GetField("_initialSynchFlags", BindingFlags.NonPublic | BindingFlags.Instance);
            SynchedMissionObject.SynchFlags syncFlags = (SynchedMissionObject.SynchFlags)prop.GetValue(synchObject);
            syncFlags |= SynchFlags.SynchTransform;
            prop.SetValue(synchObject, syncFlags);

            MatrixFrame frame = this._portcullis.GetFrame();

            frame.origin = this._pointA;
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
            GameEntity pointAEntity = base.GameEntity.GetFirstChildEntityWithTag(this.PointATag);
            GameEntity pointBEntity = base.GameEntity.GetFirstChildEntityWithTag(this.PointBTag);
            if (portcullis == null)
            {
                MBEditor.AddEntityWarning(base.GameEntity, "Portcullis body not found");
                return false;
            }
            if (pointAEntity == null)
            {
                MBEditor.AddEntityWarning(base.GameEntity, "Point a not found");
                return false;
            }
            if (pointBEntity == null)
            {
                MBEditor.AddEntityWarning(base.GameEntity, "Point b not found");
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

                    if (
                        frame.origin.Distance(this._pointA) <= 0.1f
                        && standingPoint.GameEntity.HasTag(this.ToAUseTag))
                    {
                        portcullisState = AToBNavigatorState.Idle;
                        standingPoint.UserAgent.StopUsingGameObjectMT(true);
                    }
                    else if (frame.origin.Distance(this._pointB) <= 0.1f
                        && standingPoint.GameEntity.HasTag(this.ToBUseTag))
                    {
                        portcullisState = AToBNavigatorState.Idle;
                        standingPoint.UserAgent.StopUsingGameObjectMT(true);
                    }
                    else if (standingPoint.GameEntity.HasTag(this.ToAUseTag))
                    {
                        if (standingPoint.UserAgent.GetCurrentAction(0).Name == "act_none" && this.ElevationAnimation != "")
                        {
                            standingPoint.UserAgent.SetActionChannel(0, ActionIndexCache.Create(this.ElevationAnimation), true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
                        }
                        portcullisState = AToBNavigatorState.GoingToA;
                    }
                    else if (standingPoint.GameEntity.HasTag(this.ToBUseTag))
                    {
                        if (standingPoint.UserAgent.GetCurrentAction(0).Name == "act_none" && this.LowerAnimation != "")
                        {
                            standingPoint.UserAgent.SetActionChannel(0, ActionIndexCache.Create(this.LowerAnimation), true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
                        }
                        portcullisState = AToBNavigatorState.GoingToB;
                    }

                }
            }
            if (!hasUser)
            {
                this.portcullisState = AToBNavigatorState.Idle;
            }
        }

        protected void TickAux(float dt)
        {
            if (this.portcullisState == AToBNavigatorState.GoingToA)
            {

                MatrixFrame frame = this._portcullis.GetFrame();
                Vec3 unitVector = (this._pointA - frame.origin).NormalizedCopy();
                frame.origin += unitVector * dt * this.ToASpeed;
                // frame.Elevate(this.ElevationSpeed * dt);
                if (frame.origin.Distance(this._pointA) <= 0.1f)
                {
                    frame.TransformToLocal(this._pointA);
                    this.portcullisState = AToBNavigatorState.Idle;
                }
                this._portcullis.SetFrame(ref frame);
            }
            else if (this.portcullisState == AToBNavigatorState.GoingToB)
            {
                MatrixFrame frame = this._portcullis.GetFrame();
                Vec3 unitVector = (this._pointB - frame.origin).NormalizedCopy();
                frame.origin += unitVector * dt * this.ToASpeed;
                if (frame.origin.Distance(this._pointB) <= 0.1f)
                {
                    frame.TransformToLocal(this._pointB);
                    this.portcullisState = AToBNavigatorState.Idle;
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
            if (usableGameObject.GameEntity.HasTag(this.ToAUseTag))
            {
                TextObject forStandingPoint = new TextObject("Use");
                forStandingPoint.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
                return forStandingPoint;
            }
            else if (usableGameObject.GameEntity.HasTag(this.ToBUseTag))
            {
                TextObject forStandingPoint = new TextObject("Use");
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
