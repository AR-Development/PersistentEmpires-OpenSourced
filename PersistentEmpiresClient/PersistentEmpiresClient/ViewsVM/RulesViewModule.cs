using NAudio.Wave;
using System;
using System.Data;
using System.Xml;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;

namespace PersistentEmpires.Views.ViewsVM
{
    public class RulesViewModule : ViewModel
    {
        private string _forumLink;
        private string _message;
        private MBBindingList<RuleViewModule> _rules;

        public RulesViewModule()
        {
        }

        internal void Init(XmlDocument rules)
        {
            var element = rules.SelectSingleNode("/Rules/ForumLink");
            ForumLink = element.InnerText;
            if(!string.IsNullOrEmpty(ForumLink))
            {
                Input.SetClipboardText(ForumLink);
            }
            element = rules.SelectSingleNode("/Rules/Message");
            Message = element.InnerText;
            var elements = rules.SelectNodes("/Rules/Rule");
            Rules = new MBBindingList<RuleViewModule>();
            for(int i = 0; i < elements.Count; i++)
            {
                Rules.Add(new RuleViewModule(elements[i].InnerText));
            }
        }

        [DataSourceProperty]
        public string ForumLink
        {
            get
            {
                return _forumLink;
            }
            set
            {
                if (value == _forumLink)
                    return;

                _forumLink = value;
                OnPropertyChanged(nameof(ForumLink));
            }
        }

        [DataSourceProperty]
        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                if (value == _message)
                    return;

                _message = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        [DataSourceProperty]
        public MBBindingList<RuleViewModule> Rules
        {
            get
            {
                return _rules;
            }
            set
            {
                if (value == _rules)
                    return;
                _rules = value;
                OnPropertyChanged(nameof(Rules));
            }
        }
    }
}
