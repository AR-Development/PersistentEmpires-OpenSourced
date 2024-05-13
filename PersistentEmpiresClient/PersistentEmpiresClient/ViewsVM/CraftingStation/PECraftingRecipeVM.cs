using TaleWorlds.Core;
using TaleWorlds.Library;

namespace PersistentEmpires.Views.ViewsVM.CraftingStation
{
    public class PECraftingRecipeVM : ViewModel
    {
        public ItemObject Item;
        private int _count;
        private ImageIdentifierVM _imageIdentifier;
        public PECraftingRecipeVM(ItemObject item, int count)
        {
            this.Item = item;
            this.ImageIdentifier = new ImageIdentifierVM(item);
            this.Count = count;
        }
        public void ExecuteHoverStart()
        {
            if (this.Item != null)
            {
                InformationManager.ShowTooltip(typeof(ItemObject), new object[] {
                    new EquipmentElement(this.Item)
                });
            }
        }
        public void ExecuteHoverEnd()
        {
            InformationManager.HideTooltip();
        }
        [DataSourceProperty]
        public int Count
        {
            get => this._count;
            set
            {
                if (value != this._count)
                {
                    this._count = value;
                    base.OnPropertyChangedWithValue(value, "Count");
                }
            }
        }
        [DataSourceProperty]
        public ImageIdentifierVM ImageIdentifier
        {
            get => this._imageIdentifier;
            set
            {
                if (value != this._imageIdentifier)
                {
                    this._imageIdentifier = value;
                    base.OnPropertyChangedWithValue(value, "ImageIdentifier");
                }
            }
        }
    }
}
