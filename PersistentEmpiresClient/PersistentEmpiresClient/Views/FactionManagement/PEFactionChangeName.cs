using PersistentEmpires.Views.ViewsVM.FactionManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;

namespace PersistentEmpires.Views.Views.FactionManagement
{
    public class PEFactionChangeName : PEMenuItem
    {
        public PEFactionChangeName() : base("PEFactionNameSelect")
        {
        }

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this._factionManagementComponent.OnNameChangeClick += this.OnOpen;
            this._dataSource = new PEFactionChangeNameVM(() => {
                this.CloseManagementMenu();
                this._factionManagementComponent.OnFactionManagementClickHandler();
            },
            (string FactionName) => {
                this.CloseManagementMenu();
                this._factionsBehavior.RequestUpdateFactionName(FactionName);
                
            },
            () => {
                this.CloseManagementMenu();
            });
        }
    }
}
