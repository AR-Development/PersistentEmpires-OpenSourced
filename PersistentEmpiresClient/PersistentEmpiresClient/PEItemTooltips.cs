using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.PersistentEmpiresGameModels;
using System;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpires.Views
{
    public static class PEItemTooltips
    {
        public static void FillTooltipTypes()
        {
            InformationManager.RegisterTooltip<ItemObject, PropertyBasedTooltipVM>(ItemTooltipAction, "PropertyBasedTooltip");
            InformationManager.RegisterTooltip<BasicCharacterObject, PropertyBasedTooltipVM>(BasicCharacterObjectTooltipAction, "PropertyBasedTooltip");

        }

        private static void ItemTooltipAction(PropertyBasedTooltipVM propertyBasedTooltipVM, object[] args)
        {
            // propertyBasedTooltipVM.UpdateTooltip(args[0] as EquipmentElement?);
            EquipmentElement equipmentElement = (EquipmentElement)args[0];
            ItemObject item = equipmentElement.Item;
            propertyBasedTooltipVM.Mode = 1;
            propertyBasedTooltipVM.AddProperty("", item.Name.ToString(), 0, TooltipProperty.TooltipPropertyFlags.Title);

            propertyBasedTooltipVM.AddProperty(new TextObject("{=zMMqgxb1}Type", null).ToString(), GameTexts.FindText("str_inventory_type_" + (int)item.Type, null).ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
            propertyBasedTooltipVM.AddProperty(" ", " ", 0, TooltipProperty.TooltipPropertyFlags.None);

            if (item.Tier >= ItemObject.ItemTiers.Tier1)
            {
                propertyBasedTooltipVM.AddProperty("Tier", ((int)item.Tier + 1).ToString(), 0);
                propertyBasedTooltipVM.AddProperty(" ", " ", 0, TooltipProperty.TooltipPropertyFlags.RundownSeperator);
                propertyBasedTooltipVM.AddProperty(" ", " ", 0, TooltipProperty.TooltipPropertyFlags.None);

            }

            if (item.RelevantSkill != null && item.Difficulty > 0)
            {
                propertyBasedTooltipVM.AddProperty(new TextObject("{=dWYm9GsC}Requires", null).ToString(), " ", 0, TooltipProperty.TooltipPropertyFlags.None);
                propertyBasedTooltipVM.AddProperty(" ", " ", 0, TooltipProperty.TooltipPropertyFlags.RundownSeperator);
                Color cantUseColor = new Color(1f, 0f, 0f);
                Color canUseColor = new Color(0f, 1f, 0f);
                Agent myAgent = Agent.Main;
                if (myAgent != null && myAgent.Character != null)
                {
                    propertyBasedTooltipVM.AddColoredProperty(item.RelevantSkill.Name.ToString(), item.Difficulty.ToString(), myAgent.Character.GetSkillValue(item.RelevantSkill) >= item.Difficulty ? canUseColor : cantUseColor, 0, TooltipProperty.TooltipPropertyFlags.None);
                }
                propertyBasedTooltipVM.AddProperty(" ", " ", 0, TooltipProperty.TooltipPropertyFlags.None);
            }
            else if (item.Tier >= ItemObject.ItemTiers.Tier1 && item.RelevantSkill != null)
            {
                int minTierSkill = 0;
                if (item.Tier > ItemObject.ItemTiers.Tier1)
                {
                    minTierSkill = 25 * (int)item.Tier;
                }
                if (minTierSkill > 0)
                {
                    propertyBasedTooltipVM.AddProperty(new TextObject("{=dWYm9GsC}Requires", null).ToString(), " ", 0, TooltipProperty.TooltipPropertyFlags.None);
                    propertyBasedTooltipVM.AddProperty(" ", " ", 0, TooltipProperty.TooltipPropertyFlags.RundownSeperator);
                    Color cantUseColor = new Color(1f, 0f, 0f);
                    Color canUseColor = new Color(0f, 1f, 0f);
                    Agent myAgent = Agent.Main;
                    if (myAgent != null && myAgent.Character != null)
                    {
                        propertyBasedTooltipVM.AddColoredProperty(item.RelevantSkill.Name.ToString(), minTierSkill.ToString(), myAgent.Character.GetSkillValue(item.RelevantSkill) >= minTierSkill ? canUseColor : cantUseColor, 0, TooltipProperty.TooltipPropertyFlags.None);
                    }
                    propertyBasedTooltipVM.AddProperty(" ", " ", 0, TooltipProperty.TooltipPropertyFlags.None);
                }
            }
            else if (item.HasArmorComponent && item.Tier >= ItemObject.ItemTiers.Tier1)
            {
                int minTierSkill = 0;
                if (item.Tier > ItemObject.ItemTiers.Tier1)
                {
                    minTierSkill = 10 * ((int)item.Tier + 1);
                }
                if (minTierSkill > 0)
                {
                    propertyBasedTooltipVM.AddProperty(new TextObject("{=dWYm9GsC}Requires", null).ToString(), " ", 0, TooltipProperty.TooltipPropertyFlags.None);
                    propertyBasedTooltipVM.AddProperty(" ", " ", 0, TooltipProperty.TooltipPropertyFlags.RundownSeperator);
                    Color cantUseColor = new Color(1f, 0f, 0f);
                    Color canUseColor = new Color(0f, 1f, 0f);
                    Agent myAgent = Agent.Main;
                    if (myAgent != null && myAgent.Character != null)
                    {
                        propertyBasedTooltipVM.AddColoredProperty("Endurance", minTierSkill.ToString(), myAgent.Character.GetSkillValue(PersistentEmpireSkills.Endurance) >= minTierSkill ? canUseColor : cantUseColor, 0, TooltipProperty.TooltipPropertyFlags.None);
                    }
                    propertyBasedTooltipVM.AddProperty(" ", " ", 0, TooltipProperty.TooltipPropertyFlags.None);
                }
            }
            propertyBasedTooltipVM.AddProperty(new TextObject("{=4Dd2xgPm}Weight", null).ToString(), item.Weight.ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
            string text = "";
            if (item.IsUniqueItem)
            {
                text = text + GameTexts.FindText("str_inventory_flag_unique", null).ToString() + " ";
            }
            if (item.ItemFlags.HasAnyFlag(ItemFlags.NotUsableByFemale))
            {
                text = text + GameTexts.FindText("str_inventory_flag_male_only", null).ToString() + " ";
            }
            if (item.ItemFlags.HasAnyFlag(ItemFlags.NotUsableByMale))
            {
                text = text + GameTexts.FindText("str_inventory_flag_female_only", null).ToString() + " ";
            }
            if (!string.IsNullOrEmpty(text))
            {
                propertyBasedTooltipVM.AddProperty(new TextObject("{=eHVq6yDa}Item Properties", null).ToString(), text, 0, TooltipProperty.TooltipPropertyFlags.None);
            }
            if (item.HasArmorComponent)
            {
                if (item.ArmorComponent.HeadArmor != 0)
                {
                    propertyBasedTooltipVM.AddProperty(new TextObject("{=O3dhjtOS}Head Armor", null).ToString(), equipmentElement.GetModifiedHeadArmor().ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
                }
                if (item.ArmorComponent.BodyArmor != 0)
                {
                    if (item.Type == ItemObject.ItemTypeEnum.HorseHarness)
                    {
                        propertyBasedTooltipVM.AddProperty(new TextObject("{=kftE5nvv}Horse Armor", null).ToString(), equipmentElement.GetModifiedMountBodyArmor().ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
                    }
                    else
                    {
                        propertyBasedTooltipVM.AddProperty(new TextObject("{=HkfY3Ds5}Body Armor", null).ToString(), equipmentElement.GetModifiedBodyArmor().ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
                    }
                }
                if (item.ArmorComponent.ArmArmor != 0)
                {
                    propertyBasedTooltipVM.AddProperty(new TextObject("{=kx7q8ybD}Arm Armor", null).ToString(), equipmentElement.GetModifiedArmArmor().ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
                }
                if (item.ArmorComponent.LegArmor != 0)
                {
                    propertyBasedTooltipVM.AddProperty(new TextObject("{=eIws123Z}Leg Armor", null).ToString(), equipmentElement.GetModifiedLegArmor().ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
                    return;
                }
            }
            else if (item.WeaponComponent != null && item.Weapons.Count > 0)
            {
                int num = (item.Weapons.Count > 1 && propertyBasedTooltipVM.IsExtended) ? 1 : 0;
                WeaponComponentData weaponComponentData = item.Weapons[num];
                propertyBasedTooltipVM.AddProperty(new TextObject("{=sqdzHOPe}Class", null).ToString(), GameTexts.FindText("str_inventory_weapon", ((int)weaponComponentData.WeaponClass).ToString()).ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
                ItemObject.ItemTypeEnum itemTypeFromWeaponClass = WeaponComponentData.GetItemTypeFromWeaponClass(weaponComponentData.WeaponClass);
                if (itemTypeFromWeaponClass == ItemObject.ItemTypeEnum.OneHandedWeapon || itemTypeFromWeaponClass == ItemObject.ItemTypeEnum.TwoHandedWeapon || itemTypeFromWeaponClass == ItemObject.ItemTypeEnum.Polearm)
                {
                    if (weaponComponentData.SwingDamageType != DamageTypes.Invalid)
                    {
                        propertyBasedTooltipVM.AddProperty(new TextObject("{=sVZaIPoQ}Swing Speed", null).ToString(), equipmentElement.GetModifiedSwingSpeedForUsage(num).ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
                        propertyBasedTooltipVM.AddProperty(new TextObject("{=QeToaiLt}Swing Damage", null).ToString(), equipmentElement.GetModifiedSwingDamageForUsage(num).ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
                    }
                    if (weaponComponentData.ThrustDamageType != DamageTypes.Invalid)
                    {
                        propertyBasedTooltipVM.AddProperty(new TextObject("{=4uMWNDoi}Thrust Speed", null).ToString(), equipmentElement.GetModifiedThrustSpeedForUsage(num).ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
                        propertyBasedTooltipVM.AddProperty(new TextObject("{=dO95yR9b}Thrust Damage", null).ToString(), equipmentElement.GetModifiedThrustDamageForUsage(num).ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
                    }
                    propertyBasedTooltipVM.AddProperty(new TextObject("{=ZcybPatO}Weapon Length", null).ToString(), weaponComponentData.WeaponLength.ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
                    propertyBasedTooltipVM.AddProperty(new TextObject("{=oibdTnXP}Handling", null).ToString(), weaponComponentData.Handling.ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
                }
                if (itemTypeFromWeaponClass == ItemObject.ItemTypeEnum.Thrown)
                {
                    propertyBasedTooltipVM.AddProperty(new TextObject("{=ZcybPatO}Weapon Length", null).ToString(), weaponComponentData.WeaponLength.ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
                    propertyBasedTooltipVM.AddProperty(new TextObject("{=s31DnnAf}Damage", null).ToString(), ItemHelper.GetMissileDamageText(weaponComponentData, equipmentElement.ItemModifier).ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
                    propertyBasedTooltipVM.AddProperty(new TextObject("{=bAqDnkaT}Missile Speed", null).ToString(), equipmentElement.GetModifiedMissileSpeedForUsage(num).ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
                    propertyBasedTooltipVM.AddProperty(new TextObject("{=TAnabTdy}Accuracy", null).ToString(), weaponComponentData.Accuracy.ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
                    propertyBasedTooltipVM.AddProperty(new TextObject("{=twtbH1zv}Stack Amount", null).ToString(), equipmentElement.GetModifiedStackCountForUsage(num).ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
                }
                if (itemTypeFromWeaponClass == ItemObject.ItemTypeEnum.Shield)
                {
                    propertyBasedTooltipVM.AddProperty(new TextObject("{=6GSXsdeX}Speed", null).ToString(), equipmentElement.GetModifiedSwingSpeedForUsage(num).ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
                    propertyBasedTooltipVM.AddProperty(new TextObject("{=oBbiVeKE}Hit Points", null).ToString(), equipmentElement.GetModifiedMaximumHitPointsForUsage(num).ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
                }
                if (itemTypeFromWeaponClass == ItemObject.ItemTypeEnum.Bow || itemTypeFromWeaponClass == ItemObject.ItemTypeEnum.Crossbow)
                {
                    propertyBasedTooltipVM.AddProperty(new TextObject("{=6GSXsdeX}Speed", null).ToString(), equipmentElement.GetModifiedSwingSpeedForUsage(num).ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
                    propertyBasedTooltipVM.AddProperty(new TextObject("{=s31DnnAf}Damage", null).ToString(), ItemHelper.GetThrustDamageText(weaponComponentData, equipmentElement.ItemModifier).ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
                    propertyBasedTooltipVM.AddProperty(new TextObject("{=TAnabTdy}Accuracy", null).ToString(), weaponComponentData.Accuracy.ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
                    propertyBasedTooltipVM.AddProperty(new TextObject("{=bAqDnkaT}Missile Speed", null).ToString(), equipmentElement.GetModifiedMissileSpeedForUsage(num).ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
                    if (itemTypeFromWeaponClass == ItemObject.ItemTypeEnum.Crossbow)
                    {
                        propertyBasedTooltipVM.AddProperty(new TextObject("{=cnmRwV4s}Ammo Limit", null).ToString(), weaponComponentData.MaxDataValue.ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
                    }
                }
                if (weaponComponentData.IsAmmo)
                {
                    propertyBasedTooltipVM.AddProperty(new TextObject("{=TAnabTdy}Accuracy", null).ToString(), weaponComponentData.Accuracy.ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
                    propertyBasedTooltipVM.AddProperty(new TextObject("{=s31DnnAf}Damage", null).ToString(), ItemHelper.GetThrustDamageText(weaponComponentData, equipmentElement.ItemModifier).ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
                    propertyBasedTooltipVM.AddProperty(new TextObject("{=twtbH1zv}Stack Amount", null).ToString(), equipmentElement.GetModifiedStackCountForUsage(num).ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
                }
                if (item.Weapons.Any(delegate (WeaponComponentData x)
                {
                    string weaponDescriptionId = x.WeaponDescriptionId;
                    return weaponDescriptionId != null && weaponDescriptionId.IndexOf("couch", StringComparison.OrdinalIgnoreCase) >= 0;
                }))
                {
                    propertyBasedTooltipVM.AddProperty("", GameTexts.FindText("str_inventory_flag_couchable", null).ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
                }
                if (item.Weapons.Any(delegate (WeaponComponentData x)
                {
                    string weaponDescriptionId = x.WeaponDescriptionId;
                    return weaponDescriptionId != null && weaponDescriptionId.IndexOf("bracing", StringComparison.OrdinalIgnoreCase) >= 0;
                }))
                {
                    propertyBasedTooltipVM.AddProperty("", GameTexts.FindText("str_inventory_flag_braceable", null).ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
                    return;
                }
            }
            else if (item.HasHorseComponent)
            {
                if (item.HorseComponent.IsMount)
                {
                    propertyBasedTooltipVM.AddProperty(new TextObject("{=8BlMRMiR}Horse Tier", null).ToString(), ((int)(item.Tier + 1)).ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
                    propertyBasedTooltipVM.AddProperty(new TextObject("{=Mfbc4rQR}Charge Damage", null).ToString(), equipmentElement.GetModifiedMountCharge(EquipmentElement.Invalid).ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
                    propertyBasedTooltipVM.AddProperty(new TextObject("{=6GSXsdeX}Speed", null).ToString(), equipmentElement.GetModifiedMountSpeed(EquipmentElement.Invalid).ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
                    propertyBasedTooltipVM.AddProperty(new TextObject("{=rg7OuWS2}Maneuver", null).ToString(), equipmentElement.GetModifiedMountManeuver(EquipmentElement.Invalid).ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
                    propertyBasedTooltipVM.AddProperty(new TextObject("{=oBbiVeKE}Hit Points", null).ToString(), equipmentElement.GetModifiedMountHitPoints().ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
                    propertyBasedTooltipVM.AddProperty(new TextObject("{=ZUgoQ1Ws}Horse Type", null).ToString(), item.ItemCategory.GetName().ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
                }
            }
            if (args.Length > 1)
            {
                propertyBasedTooltipVM.AddProperty("", "Price", 0, TooltipProperty.TooltipPropertyFlags.Title);
                propertyBasedTooltipVM.AddProperty(" ", " ", 0, TooltipProperty.TooltipPropertyFlags.None);
                propertyBasedTooltipVM.AddProperty("Buy Price", (string)args[1], 0, TooltipProperty.TooltipPropertyFlags.None);
                propertyBasedTooltipVM.AddProperty("Sell Price", (string)args[2], 0, TooltipProperty.TooltipPropertyFlags.None);
                propertyBasedTooltipVM.AddProperty("Stock Count", (string)args[3], 0, TooltipProperty.TooltipPropertyFlags.None);

            }
        }

        private static void BasicCharacterObjectTooltipAction(PropertyBasedTooltipVM propertyBasedTooltipVM, object[] args)
        {
            SkillObject[] AllDefaultSkills = new SkillObject[]
            {
                DefaultSkills.Engineering,
                DefaultSkills.Medicine,
                DefaultSkills.Leadership,
                DefaultSkills.Steward,
                DefaultSkills.Trade,
                DefaultSkills.Charm,
                DefaultSkills.Roguery,
                DefaultSkills.Scouting,
                DefaultSkills.Tactics,
                DefaultSkills.Crafting,
                DefaultSkills.Athletics,
                DefaultSkills.Riding,
                DefaultSkills.Throwing,
                DefaultSkills.Crossbow,
                DefaultSkills.Bow,
                DefaultSkills.Polearm,
                DefaultSkills.TwoHanded,
                DefaultSkills.OneHanded,
                PersistentEmpireSkills.Farming,
                PersistentEmpireSkills.Gathering,
                PersistentEmpireSkills.Fishing,
                PersistentEmpireSkills.Sailor,
                PersistentEmpireSkills.Cooking,
                PersistentEmpireSkills.Mining,
                PersistentEmpireSkills.WoodCutting,
                PersistentEmpireSkills.Prostetuting,
                PersistentEmpireSkills.Beauty,
                PersistentEmpireSkills.Piety,
                PersistentEmpireSkills.Hunting,
                PersistentEmpireSkills.Endurance,
                PersistentEmpireSkills.Pickpocketing,
                PersistentEmpireSkills.Lockpicking,
                PersistentEmpireSkills.Dismounting
            };
            BasicCharacterObject character = (BasicCharacterObject)args[0];
            int cost = (int)args[1];
            propertyBasedTooltipVM.AddProperty("", character.Name.ToString(), 0, TooltipProperty.TooltipPropertyFlags.Title);
            propertyBasedTooltipVM.AddProperty("", "", 0, TooltipProperty.TooltipPropertyFlags.None);
            propertyBasedTooltipVM.AddProperty("", GameTexts.FindText("str_skills", null).ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
            propertyBasedTooltipVM.AddProperty("", "", 0, TooltipProperty.TooltipPropertyFlags.RundownSeperator);
            foreach (SkillObject skillObject in AllDefaultSkills)
            {
                if (character.GetSkillValue(skillObject) > 0)
                {
                    propertyBasedTooltipVM.AddProperty(skillObject.Name.ToString(), character.GetSkillValue(skillObject).ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
                }
            }
            propertyBasedTooltipVM.AddProperty("", "", 0, TooltipProperty.TooltipPropertyFlags.None);
            propertyBasedTooltipVM.AddProperty("Cost", cost.ToString() + " Denar", 0, TooltipProperty.TooltipPropertyFlags.None);

            // throw new NotImplementedException();
        }
    }
}
