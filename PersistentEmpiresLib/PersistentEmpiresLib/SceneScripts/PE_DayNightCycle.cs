/*
 *  Persistent Empires Open Sourced - A Mount and Blade: Bannerlord Mod
 *  Copyright (C) 2024  Free Software Foundation, Inc.
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.

 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.SceneScripts
{
    public class PE_DayNightCycle : ScriptComponentBehavior
    {
        public float TimeOfDay;
        public float SeasonTimeFactor;
        public float SpeedMultiplier = 1f;
        private int tickCount = 0;
        private void Init()
        {
            MBMapScene.LoadAtmosphereData(base.Scene);
            this.SeasonTimeFactor = 0f;
        }
        protected override void OnInit()
        {
            base.OnInit();
            this.Init();
        }
        public override ScriptComponentBehavior.TickRequirement GetTickRequirement()
        {
            return ScriptComponentBehavior.TickRequirement.Tick;
        }
        public void SetTimeOfDay(float tod)
        {
            this.TimeOfDay = tod;
        }

        protected override void OnTick(float dt)
        {
            this.TimeOfDay += (0.000106f * this.SpeedMultiplier);
            if (this.TimeOfDay >= 24f) this.TimeOfDay -= 24f;
            if (GameNetwork.IsClient && tickCount == 0)
            {
                this.ApplyAtmosphere(false);
            }
            tickCount = (tickCount + 1) % 7400;

        }

        public void ApplyAtmosphere(bool forceLoadTextures)
        {
            this.TimeOfDay = MBMath.ClampFloat(this.TimeOfDay, 0f, 23.99f);
            this.SeasonTimeFactor = MBMath.ClampFloat(this.SeasonTimeFactor, 0f, 1f);
            MBMapScene.SetFrameForAtmosphere(base.Scene, this.TimeOfDay * 10f, base.Scene.LastFinalRenderCameraFrame.origin.z, forceLoadTextures);
            /*float valueFrom = 0.55f;
			float valueTo = -0.1f;
			float seasonTimeFactor = this.SeasonTimeFactor;
			Vec3 dynamic_params = new Vec3(0f, 0.65f, 0f, -1f);
			dynamic_params.x = MBMath.Lerp(valueFrom, valueTo, seasonTimeFactor, 1E-05f);
			MBMapScene.SetTerrainDynamicParams(base.Scene, dynamic_params);*/
        }
    }
}
