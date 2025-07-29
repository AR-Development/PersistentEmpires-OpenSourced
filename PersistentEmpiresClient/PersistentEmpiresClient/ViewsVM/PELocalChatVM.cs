using PersistentEmpiresLib.NetworkMessages.Client;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpires.Views.ViewsVM
{
    public class PELocalChatVM : ViewModel
    {
        private string _textInput;
        private bool _isFocused;

        public PELocalChatVM() { }

        [DataSourceProperty]
        public string TextInput
        {
            get => _textInput;
            set
            {
                if (value != _textInput)
                {
                    if (value.Length > 500)
                    {
                        value = $"{value.Substring(0, 500)}...";
                        OnPropertyChangedWithValue(value, "TextInput");
                    }
                    else
                    {
                        _textInput = value;
                        OnPropertyChangedWithValue(value, "TextInput");
                    }

                    if(!string.IsNullOrEmpty(value))
                    {
                        GameNetwork.BeginModuleEventAsClient();
                        GameNetwork.WriteMessage(new PlayerIsTypingMessage());
                        GameNetwork.EndModuleEventAsClient();
                    }
                }
            }
        }
        [DataSourceProperty]
        public bool IsFocused
        {
            get => this._isFocused;
            set
            {                
                if (value != this._isFocused)
                {
                    this._isFocused = value;
                    base.OnPropertyChangedWithValue(value, "IsFocused");
                }
            }
        }
    }
}
