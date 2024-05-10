using System;
using TaleWorlds.Library;

namespace PersistentEmpires.Views.Views.Markers
{
    public class PEChatBubbleVM : ViewModel
    {
        public long CreatedAt;
        private string _message;
        private string _color;
        private int _fontSize;

        public PEChatBubbleVM(String message, String color)
        {
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            this.Message = message;
            this.Color = color;
            this.FontSize = 24;
        }

        [DataSourceProperty]
        public string Color
        {
            get => this._color;
            set
            {
                if (value != this._color)
                {
                    this._color = value;
                    this.OnPropertyChangedWithValue(value, "Color");
                }
            }
        }

        [DataSourceProperty]
        public string Message
        {
            get => this._message;
            set
            {
                if (value != this._message)
                {
                    this._message = value;
                    this.OnPropertyChangedWithValue(value, "Message");
                }
            }
        }

        [DataSourceProperty]
        public int FontSize
        {
            get => this._fontSize;
            set
            {
                if (value != this._fontSize)
                {
                    this._fontSize = value;
                    base.OnPropertyChangedWithValue(value, "FontSize");
                }
            }
        }

        public void SetFontSize(int v)
        {
            this.FontSize = v;
        }
    }
}
