using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;

namespace PersistentEmpires.Views.ViewsVM.FactionManagement
{
    public class PEFactionChangeBannerVM : ViewModel
    {
        private string _bannerCode;
        private string _bannerCodeNotApplicable;
        private bool _canApply;
        private Action _onCancel;
        private Action<string> _onApply;
        private Action _close;
        public PEFactionChangeBannerVM(Action OnCancel, Action<string> OnApply, Action Close)
        {
            this.BannerCode = "";
            this.BannerCodeNotApplicable = "Cannot be empty";
            this._onCancel = OnCancel;
            this._onApply = OnApply;
            this._close = Close;
        }

        private string CheckIconList(int id)
        {
            foreach (BannerIconGroup bannerIconGroup in BannerManager.Instance.BannerIconGroups)
            {
                if (bannerIconGroup.AllBackgrounds.ContainsKey(id))
                {
                    return bannerIconGroup.AllBackgrounds[id];
                }
                else if (bannerIconGroup.AllIcons.ContainsKey(id))
                {
                    return bannerIconGroup.AllIcons[id].MaterialName;
                }
            }

            return null;
        }

        private bool TryParseBanner(string bannerKey)
        {
            string[] array = bannerKey.Split('.');
            // The maximum size of the banner is Banner.BannerFullSize. But apparently negative values do not cause crashes. Anyway added some checks with tolerance to parse the banner.
            int maxX = 2 * (Banner.BannerFullSize / 2);
            int minX = 2 * -1 * (Banner.BannerFullSize / 2);
            int maxY = maxX;
            int minY = minX;

            int iconCounter = 0;
            int num = 0;
            while (num + 10 <= array.Length)
            {
                if (int.TryParse(array[num], out int iconId))
                {
                    if (CheckIconList(iconId) == null)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
                if (!int.TryParse(array[num + 1], out int colorId1) || !BannerManager.Instance.ReadOnlyColorPalette.ContainsKey(colorId1)
                            || !int.TryParse(array[num + 2], out int colorId2) || !BannerManager.Instance.ReadOnlyColorPalette.ContainsKey(colorId2)
                            || !int.TryParse(array[num + 3], out int sizeX)
                            || !int.TryParse(array[num + 4], out int sizeY)
                            || !int.TryParse(array[num + 5], out int posX) || posX > maxX || posX < minX
                            || !int.TryParse(array[num + 6], out int posY) || posY > maxY || posY < minY
                            || !int.TryParse(array[num + 7], out int drawStroke) || drawStroke > 1 || drawStroke < 0
                            || !int.TryParse(array[num + 8], out int mirrior) || mirrior > 1 || mirrior < 0)
                {
                    return false;
                }

                if (!int.TryParse(array[num + 9], out int rotation))
                {
                    return false;
                }
                else
                {
                    rotation %= 360;
                    while (rotation < 0)
                    {
                        rotation += 360;
                    }
                }
                num += 10;
                iconCounter++;
            }

            if (iconCounter == 0)
            {
                return false;
            }
            return true;
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            BannerCode = "";
        }

        public bool CanApplyValue()
        {
            if (this._bannerCode == "")
            {
                this.BannerCodeNotApplicable = "Cannot be empty";
                return false;
            }
            if ((int)(this._bannerCode.Split('.').Count() / 10) <= 0)
            {
                this.BannerCodeNotApplicable = "Invalid Banner Code";
                return false;
            }
            /*if(this.TryParseBanner(this.BannerCode) == false)
            {
                this.BannerCodeNotApplicable = "Invalid Banner Code";
                return false;
            }*/
            this.BannerCodeNotApplicable = "";
            return true;
        }

        public void OnCancel()
        {
            this._onCancel();
        }

        public void OnPaste()
        {
            var _tmp = Input.GetClipboardText();
            if (!string.IsNullOrEmpty(_tmp))
            {
                BannerCode = _tmp;
            }
        }

        public void OnApply()
        {
            if (this.CanApplyValue())
            {
                this._close();
                this._onApply(this.BannerCode);
            }
        }


        [DataSourceProperty]
        public string BannerCode
        {
            get => this._bannerCode;
            set
            {
                if (value != this._bannerCode)
                {
                    this._bannerCode = value;
                    CanApply = this.CanApplyValue();
                    base.OnPropertyChangedWithValue(value, "BannerCode");
                }
            }
        }
        [DataSourceProperty]
        public bool CanApply
        {
            get
            {
                return _canApply;
            }
            set
            {
                if (value != _canApply)
                {
                    this._canApply = value;
                    base.OnPropertyChangedWithValue(value, "CanApply");
                }
            }
        }

        [DataSourceProperty]
        public string BannerCodeNotApplicable
        {
            get => this._bannerCodeNotApplicable;
            set
            {
                if (value != this._bannerCodeNotApplicable)
                {
                    this._bannerCodeNotApplicable = value;
                    base.OnPropertyChangedWithValue(value, "BannerCodeNotApplicable");
                }
            }
        }
    }
}
