using PersistentEmpires.Views.ViewsVM.AdminPanel;
using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;
using TaleWorlds.Engine.GauntletUI;

namespace PersistentEmpires.Views.Views
{
    public class PEChatBox : MissionView
    {
        public override void OnMissionScreenTick(float dt)
        {
            base.OnMissionScreenTick(dt);

            var focusedLayer = ScreenManager.FocusedLayer;
            if (focusedLayer != null)
            {
                if (focusedLayer is TaleWorlds.Engine.GauntletUI.GauntletLayer layer)
                {
                    if (layer.MoviesAndDataSources[0].Item2 is TaleWorlds.MountAndBlade.ViewModelCollection.Multiplayer.MPChatVM)
                    {
                        var tmp = layer.MoviesAndDataSources[0].Item1 as TaleWorlds.GauntletUI.Data.GeneratedGauntletMovie;
                        if (tmp == null) return;
                        try
                        {
                            var chatLogWidget = tmp.RootWidget.AllChildren.ToList().Where(x => x.Id == "ChatLogWidget").FirstOrDefault();
                            if (chatLogWidget != null)
                            {
                                if (focusedLayer.Input.IsKeyDown(InputKey.LeftControl))
                                {
                                    if (focusedLayer.Input.IsKeyPressed(InputKey.Up))
                                    {
                                        var h = chatLogWidget.SuggestedHeight;
                                        h += 100;
                                        if (h > 1000)
                                            h = 1000;
                                        chatLogWidget.SuggestedHeight = h;
                                    }
                                    else if (focusedLayer.Input.IsKeyReleased(InputKey.Down))
                                    {
                                        var h = chatLogWidget.SuggestedHeight;
                                        h -= 100;
                                        if (h < 300)
                                            h = 300;
                                        chatLogWidget.SuggestedHeight = h;
                                    }
                                }
                            }
                        }catch(ArgumentNullException ex)
                        {

                        }
                    }
                }
            }
        }
    }
}