using Newtonsoft.Json.Linq;
using PersistentEmpires.Views.Views.AdminPanel;
using PersistentEmpiresLib.SceneScripts;
using System;
using System.Collections.Generic;
using TaleWorlds.Library;

namespace PersistentEmpires.Views.ViewsVM.AdminPanel
{
    public class PEAdminTpItemVM : ViewModel
    {
        private string _description;
        private Action<PEAdminTpItemVM> _executeSelect;
        private AdminTp x;
        private Action executeSelect;
        public AdminTp TeleportLocation;

        public PEAdminTpItemVM(AdminTp teleportLocation, Action<PEAdminTpItemVM> executeSelect)
        {
            TeleportLocation = teleportLocation;
            _executeSelect = executeSelect;
            Description = $"{teleportLocation.Description} (Id: {teleportLocation.Id}, Position: {teleportLocation.SpawnPosition.ToString()})";
        }

        public PEAdminTpItemVM(AdminTp x, Action executeSelect)
        {
            this.x = x;
            this.executeSelect = executeSelect;
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
        }

        [DataSourceProperty]
        public string Description
        {
            get => _description;
            set
            {
                if (value != _description)
                {
                    _description = value;
                    OnPropertyChangedWithValue(value, "Description");
                }
            }
        }

        public void ExecuteSelect()
        {
            _executeSelect(this);
        }
    }
}