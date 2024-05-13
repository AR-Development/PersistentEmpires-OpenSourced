using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Library;

namespace PersistentEmpires.Views.ViewsVM.AnimationMenu
{
    public class PEAnimationSubMenuVM : ViewModel
    {
        private string _name;
        // private MBBindingList<PEAnimationVM> _animations = new MBBindingList<PEAnimationVM>();
        private List<List<PEAnimationVM>> _pages = new List<List<PEAnimationVM>>();
        private int _pageNumber = 0;
        private List<List<T>> ChunkBy<T>(List<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }

        public List<List<PEAnimationVM>> NativePages
        {
            get => _pages;
        }

        public PEAnimationSubMenuVM(string categoryName, List<PEAnimationVM> animations)
        {
            this.Name = categoryName;
            this._pages = ChunkBy<PEAnimationVM>(animations, 8);
            this.PageNumber = 0;
            base.OnPropertyChanged("Page");
        }

        [DataSourceProperty]
        public MBBindingList<PEAnimationVM> Page
        {
            get
            {
                List<PEAnimationVM> page = this._pages[PageNumber];
                MBBindingList<PEAnimationVM> retVal = new MBBindingList<PEAnimationVM>();
                foreach (PEAnimationVM p in page) retVal.Add(p);
                return retVal;
            }
        }

        [DataSourceProperty]
        public int PageNumber
        {
            get => this._pageNumber;
            set
            {
                if (value != this._pageNumber && value < this._pages.Count)
                {
                    this._pageNumber = value;
                    base.OnPropertyChangedWithValue(value, "PageNumber");
                    base.OnPropertyChanged("Page");
                }
            }
        }

        [DataSourceProperty]
        public string Name
        {
            get => this._name;
            set
            {
                if (value != this._name)
                {
                    this._name = value;
                    base.OnPropertyChangedWithValue(value, "Name");
                }
            }
        }
    }
}
