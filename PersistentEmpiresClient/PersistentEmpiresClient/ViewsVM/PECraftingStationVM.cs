using PersistentEmpires.Views.ViewsVM.CraftingStation;
using PersistentEmpiresClient.ViewsVM;
using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.SceneScripts;
using System;
using TaleWorlds.Core;
using TaleWorlds.Library;

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
            maxItemsRenderedPerTick = 15;
        }

        public override void AddItem(object obj, int i)
        {
            Craftable item = (Craftable)obj;
            MBBindingList<PECraftingRecipeVM> craftingRecipes = new MBBindingList<PECraftingRecipeVM>();
            foreach (CraftingRecipe recipe in item.Recipe)
            {
                craftingRecipes.Add(new PECraftingRecipeVM(recipe.Item, recipe.NeededCount));
            }
            this.FilteredItemList.Add(new PECraftingStationItemVM(item.Item, item.OutputCount, item.Tier, CraftingStation.upgradeableBuilding.CurrentTier, craftingRecipes, this.ExecuteCraft, i, item.CraftTime));
        }

        public void RefreshValues(PE_CraftingStation craftingStation, Inventory inventory, Action<PECraftingStationItemVM> craft)
        {
            this.CraftingStation = craftingStation;
            this.Craft = craft;
            base.RefreshValues(craftingStation.Craftables, inventory);
        }

        private void ExecuteCraft(PECraftingStationItemVM obj)
        {
            this.Craft(obj);
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
                        InformationManager.DisplayMessage(new InformationMessage(GameTexts.FindText("PECraftingStationCompleted", null).ToString(), new Color(0, 1f, 0)));
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

        public PE_CraftingStation CraftingStation { get => craftingStation; set => craftingStation = value; }
        public Action<PECraftingStationItemVM> Craft { get => _craft; set => _craft = value; }
    }
}
