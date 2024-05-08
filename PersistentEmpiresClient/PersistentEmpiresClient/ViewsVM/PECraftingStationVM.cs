using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.SceneScripts;
using PersistentEmpires.Views.ViewsVM.CraftingStation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;
using PersistentEmpires.Views.ViewsVM.StockpileMarket;
using PersistentEmpiresClient.ViewsVM;

namespace PersistentEmpires.Views.ViewsVM
{
    public class PECraftingStationVM : PEBaseItemListVM<PECraftingStationItemVM, Craftable>
    {
        private PE_CraftingStation craftingStation;
        private Action<PECraftingStationItemVM> _craft;
        private bool _isCrafting;
        private int _craftingDuration;
        private int _pastDuration;

        public PECraftingStationVM(Action<PEItemVM> _handleClickItem) : base(_handleClickItem)
        {
            maxItemsRenderedPerTick = 30;
        }

        public override void AddItem(object obj, int i)
        {
            Craftable item = (Craftable) obj;
            MBBindingList <PECraftingRecipeVM> craftingRecipes = new MBBindingList<PECraftingRecipeVM>();
            foreach (CraftingRecipe recipe in item.Recipe)
            {
                craftingRecipes.Add(new PECraftingRecipeVM(recipe.Item, recipe.NeededCount));
            }
            this.FilteredItemList.Add(new PECraftingStationItemVM(item.CraftableItem, item.OutputCount, item.Tier, CraftingStation.upgradeableBuilding.CurrentTier, craftingRecipes, this.ExecuteCraft, i, item.CraftTime));
        }

        public void RefreshValues(PE_CraftingStation craftingStation, Inventory inventory, Action<PECraftingStationItemVM> craft)
        {
            this.CraftingStation = craftingStation;
            this._craft = craft;
            base.RefreshValues(craftingStation.Craftables, inventory);
        }

        private void ExecuteCraft(PECraftingStationItemVM obj)
        {
            this._craft(obj); 
        }

        [DataSourceProperty]
        public int PastDuration
        {
            get => this._pastDuration;
            set
            {
                if (value != this._pastDuration)
                {
                    if (value == 0)
                    {
                        InformationManager.DisplayMessage(new InformationMessage("Crafting completed!", new Color(0, 1f, 0)));
                    }
                    this._pastDuration = value;
                    base.OnPropertyChangedWithValue(value, "PastDuration");
                }
            }
        }

        [DataSourceProperty]
        public int CraftingDuration
        {
            get => this._craftingDuration;
            set
            {
                if (value != this._craftingDuration)
                {
                    this._craftingDuration = value;
                    base.OnPropertyChangedWithValue(value, "CraftingDuration");
                }
            }
        }

        [DataSourceProperty]
        public bool IsCrafting
        {
            get => this._isCrafting;
            set
            {
                if (value != this._isCrafting)
                {
                    this._isCrafting = value;
                    base.OnPropertyChangedWithValue(value, "IsCrafting");
                }
            }
        }

        protected PE_CraftingStation CraftingStation { get => craftingStation; set => craftingStation = value; }
    }
}
