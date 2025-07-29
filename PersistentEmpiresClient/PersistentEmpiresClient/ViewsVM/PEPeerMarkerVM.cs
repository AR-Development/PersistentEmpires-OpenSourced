using PersistentEmpires.Views.Views.Markers;
using System;
using System.Linq;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.FlagMarker.Targets;

namespace PersistentEmpires.Views.ViewsVM
{
    public class PEPeerMarkerVM : MissionMarkerTargetVM
    {
        private MBBindingList<PEChatBubbleVM> _chatMessages;

        public MissionPeer TargetPeer { get; private set; }
        public bool _isTextVisible { get; set; } = false;
        public bool _isIconVisible { get; set; } = true;
        public long _iconVisibleAt { get; set; } = 0;

        public PEPeerMarkerVM(MissionPeer peer) : base(MissionMarkerType.Peer)
        {
            this.ChatMessages = new MBBindingList<PEChatBubbleVM>();
            this.TargetPeer = peer;
            base.Name = peer.DisplayedName;
            this.SetVisual();
        }

        public void AddMessage(string message, string color)
        {
            IsTextVisible = true;
            IsIconVisible = false;
            if (this.ChatMessages.Count > 5)
            {
                this.ChatMessages.RemoveAt(0);
            }
            this.ChatMessages.Add(new PEChatBubbleVM(message, color));
        }

        public void NotifyTyping()
        {
            _iconVisibleAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if (ChatMessages.Count > 0)
            {
                IsTextVisible = true;
                IsIconVisible = false;
            }
            else
            {
                IsTextVisible = false;
                IsIconVisible = true;
            }
        }
        

        public void FadeOldMessages()
        {
            foreach (PEChatBubbleVM bubble in this.ChatMessages.ToList())
            {
                if (bubble.CreatedAt + 3 < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                {
                    ChatMessages.Remove(bubble);
                }
            }

            if (ChatMessages.Count == 0)
            {
                IsTextVisible = false;

                if(_iconVisibleAt + 3 >= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                {
                    IsIconVisible = true;
                }
            }
        }

        public void FadeOldIcon()
        {
            if (_iconVisibleAt + 3 < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                IsTextVisible = true;
                IsIconVisible = false;
            }
        }

        private void SetVisual()
        {
            string color = "#00FFFFFF";
            uint color2 = TaleWorlds.Library.Color.ConvertStringToColor("#FFFFFFFF").ToUnsignedInteger();
            uint color3 = TaleWorlds.Library.Color.ConvertStringToColor(color).ToUnsignedInteger();
            base.RefreshColor(color2, color3);
        }

        public override void UpdateScreenPosition(Camera missionCamera)
        {
            MissionPeer targetPeer = this.TargetPeer;
            if (((targetPeer != null) ? targetPeer.ControlledAgent : null) == null)
            {
                return;
            }
            base.UpdateScreenPosition(missionCamera);
            // base.OnPropertyChanged("FontSize");
            foreach (PEChatBubbleVM vm in this.ChatMessages)
            {
                int value = (-24 * this.Distance) / 30;
                vm.SetFontSize(value + 24);
            }
        }

        public override Vec3 WorldPosition
        {
            get
            {
                MissionPeer targetPeer = this.TargetPeer;
                if (((targetPeer != null) ? targetPeer.ControlledAgent : null) != null)
                {
                    return this.TargetPeer.ControlledAgent.Position + new Vec3(0f, 0f, this.TargetPeer.ControlledAgent.GetEyeGlobalHeight(), -1f);
                }
                Debug.FailedAssert("No target found!", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.ViewModelCollection\\Multiplayer\\FlagMarker\\Targets\\MissionPeerMarkerTargetVM.cs", "WorldPosition", 27);
                return Vec3.One;
            }
        }

        protected override float HeightOffset
        {
            get
            {
                return 0.3f;
            }
        }

        [DataSourceProperty]
        public MBBindingList<PEChatBubbleVM> ChatMessages
        {
            get => this._chatMessages;
            set
            {
                if (value != this._chatMessages)
                {
                    this._chatMessages = value;
                    base.OnPropertyChangedWithValue(value, "ChatMessages");
                }
            }
        }

        [DataSourceProperty]
        public bool IsTextVisible
        {
            get => _isTextVisible;
            set
            {
                if (value != _isTextVisible)
                {
                    _isTextVisible = value;
                    OnPropertyChangedWithValue(value, "IsTextVisible");
                }
            }
        }

        [DataSourceProperty]
        public bool IsIconVisible
        {
            get => _isIconVisible;
            set
            {
                if (value != _isIconVisible)
                {
                    _isIconVisible = value;
                    OnPropertyChangedWithValue(value, "IsIconVisible");
                }
            }
        }
    }
}
