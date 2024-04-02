using PersistentEmpiresLib.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;


namespace PersistentEmpiresLib.SceneScripts
{
    public class PEFactionBanner : MissionObject
    {
        public int FactionIndex = 2;
		public String FactionName = "Simple Faction";
        public string BannerKey = "11.45.126.4345.4345.764.764.1.0.0.462.0.13.512.512.764.764.1.0.0";

		public String GetBannerKey() {
			return this.BannerKey;
		}

		public void SetBannerKey(string BannerKey) {
			this.BannerKey = BannerKey;
			BannerRenderer.RequestRenderBanner(new Banner(this.BannerKey), base.GameEntity);
		}
	}
}
