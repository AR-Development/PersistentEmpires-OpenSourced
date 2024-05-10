using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.FlagMarker.Targets;

namespace PersistentEmpires.Views.ViewsVM.MapMarkers
{
    public class PEPlayerMapMarkerVM : MissionMarkerTargetVM
    {
        public PEPlayerMapMarkerVM() : base(MissionMarkerType.Peer)
        {
        }
        public override void UpdateScreenPosition(Camera missionCamera)
        {
            if (Agent.Main == null) return;
            base.UpdateScreenPosition(missionCamera);
        }
        public override Vec3 WorldPosition
        {
            get
            {
                if (Agent.Main != null)
                {
                    return Agent.Main.Position;
                }
                return Vec3.One;
            }
        }

        protected override float HeightOffset => 0;
    }
}
