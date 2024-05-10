using PersistentEmpires.Views.Views.AdminPanel;
using TaleWorlds.Library;

namespace PersistentEmpiresClient.ViewsVM.AdminPanel.Buttons
{
    public abstract class PEAdminButtonVM : ViewModel
    {
        public PEAdminPlayerVM SelectedPlayer;

        public abstract void Execute();

        public abstract string GetCaption();

        public virtual bool IsButtonEnabled()
        {
            return SelectedPlayer != null;
        }

        public void SetSelectedPlayer(PEAdminPlayerVM selectedPlayer)
        {
            SelectedPlayer = selectedPlayer;
            OnPropertyChanged("IsEnabled");
        }

        public override bool Equals(object obj) => this.Equals(obj as PEAdminButtonVM);

        public bool Equals(PEAdminButtonVM button)
        {
            if (button is null)
            {
                return false;
            }

            if (ReferenceEquals(this, button))
            {
                return true;
            }

            if (GetCaption() == button.GetCaption())
            {
                return true;
                //return base.Equals((PEAdminButtonVM)button);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode() => GetCaption().GetHashCode();

        public static bool operator ==(PEAdminButtonVM lhs, PEAdminButtonVM rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                {
                    return true;
                }

                return false;
            }
            return lhs.Equals(rhs);
        }

        public static bool operator !=(PEAdminButtonVM lhs, PEAdminButtonVM rhs) => !(lhs == rhs);

        [DataSourceProperty]
        public bool IsEnabled
        {
            get => IsButtonEnabled();
        }

        [DataSourceProperty]
        public string Caption
        {
            get => GetCaption();
        }
    }
}