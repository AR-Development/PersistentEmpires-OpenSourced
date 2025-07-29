using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;

namespace PersistentEmpiresClient.ViewsVM.MessageLogPanel
{
    public class MessageLog : ViewModel
    {
        private MBBindingList<MessageLogItem> _items = new MBBindingList<MessageLogItem>();
        public MessageLog()
        {
            RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
        }

        internal void Init(List<KeyValuePair<string, string>> log)
        {
            var tmp = new MBBindingList<MessageLogItem>();
            log.ForEach(x => tmp.Add(new MessageLogItem(x.Key, x.Value)));
            Items = tmp;
        }

        internal void Add(KeyValuePair<string, string> item)
        {
            var tmp = Items.ToList();
            tmp.Insert(0, new MessageLogItem(item.Key, item.Value));
            var tmp2 = new MBBindingList<MessageLogItem>();
            tmp.ForEach(x => tmp2.Add(x));
            Items = tmp2;
        }

        private void ExecuteLink(string link)
        {
        }

        internal void Copy()
        {
            var s = string.Join(Environment.NewLine, _items.Select(x => $"{x.Date} {x.Description}"));
            Input.SetClipboardText(s);
        }

        [DataSourceProperty]
        public MBBindingList<MessageLogItem> Items
        {
            get
            {
                return _items;
            }
            set
            {
                if (value == _items)
                    return;
                _items = value;
                OnPropertyChangedWithValue(_items, nameof(Items));
            }
        }
    }
}