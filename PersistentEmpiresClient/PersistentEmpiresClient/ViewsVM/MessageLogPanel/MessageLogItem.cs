using TaleWorlds.Library;

namespace PersistentEmpiresClient.ViewsVM.MessageLogPanel
{
    public class MessageLogItem : ViewModel
    {
        private string _date;
        private string _description;
        public MessageLogItem(string date, string description)
        {
            Date = date;
            Description = description;
            RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
        }

        private void ExecuteLink(string link)
        {
        }

        [DataSourceProperty]
        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                if (value == _description)
                    return;
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        [DataSourceProperty]
        public string Date
        {
            get
            {
                return _date;
            }
            set
            {
                if (value == _date)
                    return;
                _date = value;
                OnPropertyChanged(nameof(Date));
            }
        }
    }
}