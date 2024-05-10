using PersistentEmpiresLib.Helpers;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace PersistentEmpires.Views.Views
{
    public class PEBannerRenderConsumer : MissionView
    {
        public bool Locked = false;

        // [HandleProcessCorruptedStateExceptions]
        // [SecurityCritical]
        public override void OnMissionScreenTick(float dt)
        {
            base.OnMissionScreenTick(dt);
            if (Locked) return;
            if (ViewHandler.RenderQueue == null) return;

            if (ViewHandler.RenderQueue.Count > 0)
            {
                BannerRender render = ViewHandler.RenderQueue.Dequeue();
                if (render.RenderOnTo == null || render.RenderOnTo.GetVisibilityExcludeParents() == false || render.RenderOnTo.IsVisibleIncludeParents() == false) return;
                if (render.RenderBanner == null) return;

                Locked = true;
                render.RenderBanner.GetTableauTextureLarge((Texture t) =>
                {
                    BannerRenderer.OnBannerTableauRenderDone(render.RenderOnTo, t);
                    Locked = false;
                });
            }
        }
    }
}
