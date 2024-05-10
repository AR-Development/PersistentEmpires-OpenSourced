using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Screens;

namespace PersistentEmpires.Views.Views
{
    public class PERangedSiegeWeaponView : UsableMissionObjectComponent
    {
        // Token: 0x17000070 RID: 112
        // (get) Token: 0x0600048E RID: 1166 RVA: 0x00022F6A File Offset: 0x0002116A
        // (set) Token: 0x0600048F RID: 1167 RVA: 0x00022F72 File Offset: 0x00021172
        public RangedSiegeWeapon RangedSiegeWeapon { get; private set; }

        // Token: 0x17000071 RID: 113
        // (get) Token: 0x06000490 RID: 1168 RVA: 0x00022F7B File Offset: 0x0002117B
        // (set) Token: 0x06000491 RID: 1169 RVA: 0x00022F83 File Offset: 0x00021183
        public MissionScreen MissionScreen { get; private set; }

        // Token: 0x17000072 RID: 114
        // (get) Token: 0x06000492 RID: 1170 RVA: 0x00022F8C File Offset: 0x0002118C
        // (set) Token: 0x06000493 RID: 1171 RVA: 0x00022F94 File Offset: 0x00021194
        public Camera Camera { get; private set; }

        // Token: 0x17000073 RID: 115
        // (get) Token: 0x06000494 RID: 1172 RVA: 0x00022F9D File Offset: 0x0002119D
        public GameEntity CameraHolder
        {
            get
            {
                return this.RangedSiegeWeapon.cameraHolder;
            }
        }

        // Token: 0x17000074 RID: 116
        // (get) Token: 0x06000495 RID: 1173 RVA: 0x00022FAA File Offset: 0x000211AA
        public Agent PilotAgent
        {
            get
            {
                return this.RangedSiegeWeapon.PilotAgent;
            }
        }

        // Token: 0x06000496 RID: 1174 RVA: 0x00022FB7 File Offset: 0x000211B7
        internal void Initialize(RangedSiegeWeapon rangedSiegeWeapon, MissionScreen missionScreen)
        {
            this.RangedSiegeWeapon = rangedSiegeWeapon;
            this.MissionScreen = missionScreen;
        }

        // Token: 0x06000497 RID: 1175 RVA: 0x00022FC7 File Offset: 0x000211C7
        protected override void OnAdded(Scene scene)
        {
            base.OnAdded(scene);
            if (this.CameraHolder != null)
            {
                this.CreateCamera();
            }
        }

        // Token: 0x06000498 RID: 1176 RVA: 0x00022FE4 File Offset: 0x000211E4
        protected override void OnMissionReset()
        {
            base.OnMissionReset();
            if (this.CameraHolder != null)
            {
                this._cameraYaw = this._cameraInitialYaw;
                this._cameraPitch = this._cameraInitialPitch;
                this.ApplyCameraRotation();
                this._isInWeaponCameraMode = false;
                this.ResetCamera();
            }
        }

        // Token: 0x06000499 RID: 1177 RVA: 0x00023030 File Offset: 0x00021230
        public override bool IsOnTickRequired()
        {
            return true;
        }

        // Token: 0x0600049A RID: 1178 RVA: 0x00023033 File Offset: 0x00021233
        protected override void OnTick(float dt)
        {
            base.OnTick(dt);
            if (!GameNetwork.IsReplay)
            {
                this.HandleUserInput(dt);
            }
        }

        // Token: 0x0600049B RID: 1179 RVA: 0x0002304C File Offset: 0x0002124C
        protected virtual void HandleUserInput(float dt)
        {
            if (this.PilotAgent != null && this.PilotAgent.IsMainAgent && this.CameraHolder != null)
            {
                if (!this._isInWeaponCameraMode)
                {
                    this._isInWeaponCameraMode = true;
                    this.StartUsingWeaponCamera();
                }
                this.HandleUserCameraRotation(dt);
            }
            if (this._isInWeaponCameraMode && (this.PilotAgent == null || !this.PilotAgent.IsMainAgent))
            {
                this._isInWeaponCameraMode = false;
                this.ResetCamera();
            }
            this.HandleUserAiming(dt);
        }

        // Token: 0x0600049C RID: 1180 RVA: 0x000230CC File Offset: 0x000212CC
        private void CreateCamera()
        {
            this.Camera = Camera.CreateCamera();
            float aspectRatio = Screen.AspectRatio;
            this.Camera.SetFovVertical(1.0471976f, aspectRatio, 0.1f, 1000f);
            this.Camera.Entity = this.CameraHolder;
            MatrixFrame frame = this.CameraHolder.GetFrame();
            Vec3 eulerAngles = frame.rotation.GetEulerAngles();
            this._cameraYaw = eulerAngles.z;
            this._cameraPitch = eulerAngles.x;
            this._cameraRoll = eulerAngles.y;
            this._cameraPositionOffset = frame.origin;
            this._cameraPositionOffset.RotateAboutZ(-this._cameraYaw);
            this._cameraPositionOffset.RotateAboutX(-this._cameraPitch);
            this._cameraPositionOffset.RotateAboutY(-this._cameraRoll);
            this._cameraInitialYaw = this._cameraYaw;
            this._cameraInitialPitch = this._cameraPitch;
        }

        // Token: 0x0600049D RID: 1181 RVA: 0x000231B0 File Offset: 0x000213B0
        protected virtual void StartUsingWeaponCamera()
        {
            if (this.CameraHolder != null && this.Camera.Entity != null)
            {
                this.MissionScreen.CustomCamera = this.Camera;
                Agent.Main.IsLookDirectionLocked = true;
                return;
            }
            Debug.FailedAssert("Camera entities are null.", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.View\\MissionViews\\SiegeWeapon\\RangedSiegeWeaponView.cs", "StartUsingWeaponCamera", 140);
        }

        // Token: 0x0600049E RID: 1182 RVA: 0x00023214 File Offset: 0x00021414
        private void ResetCamera()
        {
            if (this.MissionScreen.CustomCamera == this.Camera)
            {
                this.MissionScreen.CustomCamera = null;
                if (Agent.Main != null)
                {
                    Agent.Main.IsLookDirectionLocked = false;
                    this.MissionScreen.SetExtraCameraParameters(false, 0f);
                }
            }
        }

        // Token: 0x0600049F RID: 1183 RVA: 0x00023268 File Offset: 0x00021468
        protected virtual void HandleUserCameraRotation(float dt)
        {
            float cameraYaw = this._cameraYaw;
            float cameraPitch = this._cameraPitch;
            if (this.MissionScreen.SceneLayer.Input.IsGameKeyDown(10))
            {
                this._cameraYaw = this._cameraInitialYaw;
                this._cameraPitch = this._cameraInitialPitch;
            }
            this._cameraYaw += this.MissionScreen.SceneLayer.Input.GetMouseMoveX() * dt * 0.2f;
            this._cameraPitch += this.MissionScreen.SceneLayer.Input.GetMouseMoveY() * dt * 0.2f;
            this._cameraYaw = MBMath.ClampFloat(this._cameraYaw, 1.5707964f, 4.712389f);
            this._cameraPitch = MBMath.ClampFloat(this._cameraPitch, 1.0471976f, 1.7453294f);
            if (cameraPitch != this._cameraPitch || cameraYaw != this._cameraYaw)
            {
                this.ApplyCameraRotation();
            }
        }

        // Token: 0x060004A0 RID: 1184 RVA: 0x00023354 File Offset: 0x00021554
        private void ApplyCameraRotation()
        {
            MatrixFrame identity = MatrixFrame.Identity;
            identity.rotation.RotateAboutUp(this._cameraYaw);
            identity.rotation.RotateAboutSide(this._cameraPitch);
            identity.rotation.RotateAboutForward(this._cameraRoll);
            identity.Strafe(this._cameraPositionOffset.x);
            identity.Advance(this._cameraPositionOffset.y);
            identity.Elevate(this._cameraPositionOffset.z);
            this.CameraHolder.SetFrame(ref identity);
        }

        // Token: 0x060004A1 RID: 1185 RVA: 0x000233E4 File Offset: 0x000215E4
        private void HandleUserAiming(float dt)
        {
            bool flag = false;
            float num = 0f;
            float num2 = 0f;
            if (this.PilotAgent != null && this.PilotAgent.IsMainAgent)
            {
                if (this.UsesMouseForAiming)
                {
                    InputContext input = this.MissionScreen.SceneLayer.Input;
                    float num3 = dt / 0.0006f;
                    float num4 = input.GetMouseMoveX() + num3 * input.GetGameKeyAxis("CameraAxisX");
                    float num5 = input.GetMouseMoveY() + -num3 * input.GetGameKeyAxis("CameraAxisY");
                    if (NativeConfig.InvertMouse)
                    {
                        num5 *= -1f;
                    }
                    Vec2 vec = new Vec2(-num4, -num5);
                    if (vec.IsNonZero())
                    {
                        float num6 = vec.Normalize();
                        num6 = MathF.Min(5f, MathF.Pow(num6, 1.5f) * 0.025f);
                        vec *= num6;
                        num = vec.x;
                        num2 = vec.y;
                    }
                }
                else
                {
                    if (this.MissionScreen.SceneLayer.Input.IsGameKeyDown(2))
                    {
                        num = 1f;
                    }
                    else if (this.MissionScreen.SceneLayer.Input.IsGameKeyDown(3))
                    {
                        num = -1f;
                    }
                    if (this.MissionScreen.SceneLayer.Input.IsGameKeyDown(0))
                    {
                        num2 = 1f;
                    }
                    else if (this.MissionScreen.SceneLayer.Input.IsGameKeyDown(1))
                    {
                        num2 = -1f;
                    }
                }
                if (num != 0f)
                {
                    flag = true;
                }
                if (num2 != 0f)
                {
                    flag = true;
                }
            }
            if (flag)
            {
                this.RangedSiegeWeapon.GiveInput(num, num2);
            }
        }

        // Token: 0x040002C2 RID: 706
        private float _cameraYaw;

        // Token: 0x040002C3 RID: 707
        private float _cameraPitch;

        // Token: 0x040002C4 RID: 708
        private float _cameraRoll;

        // Token: 0x040002C5 RID: 709
        private float _cameraInitialYaw;

        // Token: 0x040002C6 RID: 710
        private float _cameraInitialPitch;

        // Token: 0x040002C7 RID: 711
        private Vec3 _cameraPositionOffset;

        // Token: 0x040002C8 RID: 712
        private bool _isInWeaponCameraMode;

        // Token: 0x040002C9 RID: 713
        protected bool UsesMouseForAiming;
    }
}