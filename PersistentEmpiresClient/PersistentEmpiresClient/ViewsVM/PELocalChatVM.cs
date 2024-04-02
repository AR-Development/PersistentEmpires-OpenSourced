using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;

namespace PersistentEmpires.Views.ViewsVM
{
    public class PELocalChatVM : ViewModel
    {
        private string _textInput;
        private bool _isFocused;

        public PELocalChatVM() { }

        [DataSourceProperty]
        public string TextInput {
            get => this._textInput;
            set
            {
                if(value != this._textInput)
                {
                    this._textInput = value;
                    base.OnPropertyChangedWithValue(value, "TextInput");
                }
            }
        }
        [DataSourceProperty]
        public bool IsFocused
        {
            get => this._isFocused;
            set
            {
                if(value != this._isFocused)
                {
                    this._isFocused = value;
                    base.OnPropertyChangedWithValue(value, "IsFocused");
                }
            }
        }
    }
}
