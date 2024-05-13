using PersistentEmpires.Views.ViewsVM.FactionManagement;

namespace PersistentEmpires.Views.Views.FactionManagement
{
    public class PEFactionChangeBanner : PEMenuItem
    {
        public PEFactionChangeBanner() : base("PEFactionBannerCodeSelect")
        {

        }
        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this._factionManagementComponent.OnBannerChangeClick += this.OnOpen;
            this._dataSource = new PEFactionChangeBannerVM(() =>
            {
                this.CloseManagementMenu();
                this._factionManagementComponent.OnFactionManagementClickHandler();
            },
            (string BannerCode) =>
            {
                this.CloseManagementMenu();
                this._factionsBehavior.RequestUpdateFactionBanner(BannerCode);
            },
            () =>
            {
                this.CloseManagementMenu();
            });
        }

    }
}
