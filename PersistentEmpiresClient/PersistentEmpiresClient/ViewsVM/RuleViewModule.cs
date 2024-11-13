using NAudio.Wave;
using System;
using System.Data;
using System.Xml;
using TaleWorlds.Library;

namespace PersistentEmpires.Views.ViewsVM
{
    public class RuleViewModule : ViewModel
    {
        private string _ruleText;

        public RuleViewModule(string rule)
        {
            RuleText = rule;
        }

        
        [DataSourceProperty]
        public string RuleText
        {
            get
            {
                return _ruleText;
            }
            set
            {
                if (value == _ruleText)
                    return;

                _ruleText = value;
                OnPropertyChanged(nameof(RuleText));
            }
        }
    }
}