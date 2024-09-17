using System;
using System.Linq;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;

namespace PersistentEmpires.Views.Views
{
    public class PEChatBox : MissionView
    {
        private static int _maxHeight = 1000;
        private static int _minHeight = 300;
        private static int _maxWidth = 1000;
        private static int _minWidth = 460;

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
                                        if (h > _maxHeight)
                                            h = _maxHeight;
                                        chatLogWidget.SuggestedHeight = h;
                                    }
                                    else if (focusedLayer.Input.IsKeyReleased(InputKey.Down))
                                    {
                                        var h = chatLogWidget.SuggestedHeight;
                                        h -= 100;
                                        if (h < _minHeight)
                                            h = _minHeight;
                                        chatLogWidget.SuggestedHeight = h;
                                    }
                                    else if (focusedLayer.Input.IsKeyPressed(InputKey.Right))
                                    {
                                        var chatTextInputParent = tmp.RootWidget.AllChildren.ToList().Where(x => x.Id == "ChatTextInputParent").FirstOrDefault();
                                        if (chatTextInputParent != null)
                                        {
                                            chatTextInputParent.HorizontalAlignment = TaleWorlds.GauntletUI.HorizontalAlignment.Left;
                                            chatTextInputParent.MarginLeft = 8;
                                        }
                                        var w = chatLogWidget.SuggestedWidth;
                                        w += 100;
                                        if (w > _maxWidth)
                                            w = _maxWidth;
                                        chatLogWidget.SuggestedWidth= w;
                                    }
                                    else if (focusedLayer.Input.IsKeyReleased(InputKey.Left))
                                    {
                                        var chatTextInputParent = tmp.RootWidget.AllChildren.ToList().Where(x => x.Id == "ChatTextInputParent").FirstOrDefault();
                                        if (chatTextInputParent != null)
                                        {
                                            chatTextInputParent.HorizontalAlignment = TaleWorlds.GauntletUI.HorizontalAlignment.Left;
                                            chatTextInputParent.MarginLeft = 8;
                                        }
                                        var w = chatLogWidget.SuggestedWidth;
                                        w -= 100;
                                        if (w < _minWidth)
                                            w = _minWidth;
                                        chatLogWidget.SuggestedWidth = w;
                                    }
                                }
                            }
                        }
                        catch (ArgumentNullException ex)
                        {

                        }
                    }
                }
            }
        }
    }
}