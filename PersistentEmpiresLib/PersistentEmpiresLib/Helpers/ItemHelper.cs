using NetworkMessages.FromServer;
using System.Reflection;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.Helpers
{
    public static class ItemHelper
    {
        // Token: 0x06000037 RID: 55 RVA: 0x000043E8 File Offset: 0x000025E8
        public static int GetMaximumAmmo(ItemObject item)
        {
            int ammo = 0;
            if (item != null && item.Weapons != null)
            {
                foreach (WeaponComponentData weaponComponentData in item.Weapons)
                {
                    bool isConsumable = weaponComponentData.IsConsumable;
                    if (isConsumable || weaponComponentData.IsRangedWeapon || weaponComponentData.WeaponFlags.HasAnyFlag(WeaponFlags.HasHitPoints))
                    {
                        ammo = weaponComponentData.MaxDataValue;
                    }
                }
            }
            return ammo;
        }

        public static bool IsWeaponComparableWithUsage(ItemObject item, string comparedUsageId)
        {
            for (int i = 0; i < item.Weapons.Count; i++)
            {
                if (item.Weapons[i].WeaponDescriptionId == comparedUsageId || (comparedUsageId == "OneHandedBastardSword" && item.Weapons[i].WeaponDescriptionId == "OneHandedSword") || (comparedUsageId == "OneHandedSword" && item.Weapons[i].WeaponDescriptionId == "OneHandedBastardSword"))
                {
                    return true;
                }
            }
            return false;
        }

        // Token: 0x06000038 RID: 56 RVA: 0x00004480 File Offset: 0x00002680
        public static bool IsWeaponComparableWithUsage(ItemObject item, string comparedUsageId, out int comparableUsageIndex)
        {
            comparableUsageIndex = -1;
            for (int i = 0; i < item.Weapons.Count; i++)
            {
                if (item.Weapons[i].WeaponDescriptionId == comparedUsageId || (comparedUsageId == "OneHandedBastardSword" && item.Weapons[i].WeaponDescriptionId == "OneHandedSword") || (comparedUsageId == "OneHandedSword" && item.Weapons[i].WeaponDescriptionId == "OneHandedBastardSword"))
                {
                    comparableUsageIndex = i;
                    return true;
                }
            }
            return false;
        }

        // Token: 0x06000039 RID: 57 RVA: 0x0000451C File Offset: 0x0000271C
        public static bool CheckComparability(ItemObject item, ItemObject comparedItem)
        {
            if (item == null || comparedItem == null)
            {
                return false;
            }
            if (item.PrimaryWeapon != null && comparedItem.PrimaryWeapon != null && ((item.PrimaryWeapon.IsMeleeWeapon && comparedItem.PrimaryWeapon.IsMeleeWeapon) || (item.PrimaryWeapon.IsRangedWeapon && item.PrimaryWeapon.IsConsumable && comparedItem.PrimaryWeapon.IsRangedWeapon && comparedItem.PrimaryWeapon.IsConsumable) || (!item.PrimaryWeapon.IsRangedWeapon && item.PrimaryWeapon.IsConsumable && !comparedItem.PrimaryWeapon.IsRangedWeapon && comparedItem.PrimaryWeapon.IsConsumable) || (item.PrimaryWeapon.IsShield && comparedItem.PrimaryWeapon.IsShield)))
            {
                WeaponComponentData primaryWeapon = item.PrimaryWeapon;
                return ItemHelper.IsWeaponComparableWithUsage(comparedItem, primaryWeapon.WeaponDescriptionId);
            }
            return item.Type == comparedItem.Type;
        }

        // Token: 0x0600003A RID: 58 RVA: 0x00004608 File Offset: 0x00002808
        public static bool CheckComparability(ItemObject item, ItemObject comparedItem, int usageIndex)
        {
            if (item == null || comparedItem == null)
            {
                return false;
            }
            if (item.PrimaryWeapon != null && ((item.PrimaryWeapon.IsMeleeWeapon && comparedItem.PrimaryWeapon.IsMeleeWeapon) || (item.PrimaryWeapon.IsRangedWeapon && item.PrimaryWeapon.IsConsumable && comparedItem.PrimaryWeapon.IsRangedWeapon && comparedItem.PrimaryWeapon.IsConsumable) || (!item.PrimaryWeapon.IsRangedWeapon && item.PrimaryWeapon.IsConsumable && !comparedItem.PrimaryWeapon.IsRangedWeapon && comparedItem.PrimaryWeapon.IsConsumable) || (item.PrimaryWeapon.IsShield && comparedItem.PrimaryWeapon.IsShield)))
            {
                WeaponComponentData weaponComponentData = item.Weapons[usageIndex];
                return ItemHelper.IsWeaponComparableWithUsage(comparedItem, weaponComponentData.WeaponDescriptionId);
            }
            return item.Type == comparedItem.Type;
        }

        // Token: 0x0600003B RID: 59 RVA: 0x000046EF File Offset: 0x000028EF
        private static TextObject GetDamageDescription(int damage, DamageTypes damageType)
        {
            TextObject textObject = new TextObject("{=vvCwVo7i}{DAMAGE} {DAMAGE_TYPE}", null);
            textObject.SetTextVariable("DAMAGE", damage);
            textObject.SetTextVariable("DAMAGE_TYPE", GameTexts.FindText("str_damage_types", damageType.ToString()));
            return textObject;
        }

        // Token: 0x0600003C RID: 60 RVA: 0x0000472C File Offset: 0x0000292C
        public static TextObject GetSwingDamageText(WeaponComponentData weapon, ItemModifier itemModifier)
        {
            int modifiedSwingDamage = weapon.GetModifiedSwingDamage(itemModifier);
            DamageTypes swingDamageType = weapon.SwingDamageType;
            return ItemHelper.GetDamageDescription(modifiedSwingDamage, swingDamageType);
        }

        // Token: 0x0600003D RID: 61 RVA: 0x00004750 File Offset: 0x00002950
        public static TextObject GetMissileDamageText(WeaponComponentData weapon, ItemModifier itemModifier)
        {
            int modifiedMissileDamage = weapon.GetModifiedMissileDamage(itemModifier);
            DamageTypes damageType = (weapon.WeaponClass == WeaponClass.ThrowingAxe) ? weapon.SwingDamageType : weapon.ThrustDamageType;
            return ItemHelper.GetDamageDescription(modifiedMissileDamage, damageType);
        }

        // Token: 0x0600003E RID: 62 RVA: 0x00004784 File Offset: 0x00002984
        public static TextObject GetThrustDamageText(WeaponComponentData weapon, ItemModifier itemModifier)
        {
            int modifiedThrustDamage = weapon.GetModifiedThrustDamage(itemModifier);
            DamageTypes thrustDamageType = weapon.ThrustDamageType;
            return ItemHelper.GetDamageDescription(modifiedThrustDamage, thrustDamageType);
        }

        // Token: 0x0600003F RID: 63 RVA: 0x000047A5 File Offset: 0x000029A5
        public static TextObject NumberOfItems(int number, ItemObject item)
        {
            TextObject textObject = new TextObject("{=siWNDxgo}{.%}{?NUMBER_OF_ITEM > 1}{NUMBER_OF_ITEM} {PLURAL(ITEM)}{?}one {ITEM}{\\?}{.%}", null);
            textObject.SetTextVariable("ITEM", item.Name);
            textObject.SetTextVariable("NUMBER_OF_ITEM", number);
            return textObject;
        }


        public static GameEntity SpawnWeaponWithNewEntityAux(Scene scene, MissionWeapon weapon, Mission.WeaponSpawnFlags spawnFlags, MatrixFrame frame, int forcedSpawnIndex, MissionObject attachedMissionObject, bool hasLifeTime)
        {
            GameEntity gameEntity = GameEntityExtensions.Instantiate(scene, weapon, spawnFlags.HasAnyFlag(Mission.WeaponSpawnFlags.WithHolster), true);
            gameEntity.CreateAndAddScriptComponent(typeof(SpawnedItemEntity).Name);
            SpawnedItemEntity firstScriptOfType = gameEntity.GetFirstScriptOfType<SpawnedItemEntity>();
            if (forcedSpawnIndex >= 0)
            {
                firstScriptOfType.Id = new MissionObjectId(forcedSpawnIndex, true);
            }
            if (attachedMissionObject != null)
            {
                attachedMissionObject.GameEntity.AddChild(gameEntity, false);
            }
            if (attachedMissionObject != null)
            {
                GameEntity gameEntity2 = gameEntity;
                MatrixFrame matrixFrame = attachedMissionObject.GameEntity.GetGlobalFrame();
                matrixFrame = matrixFrame.TransformToParent(frame);
                gameEntity2.SetGlobalFrame(matrixFrame);
            }
            else
            {
                gameEntity.SetGlobalFrame(frame);
            }
            if (GameNetwork.IsServerOrRecorder)
            {
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new SpawnWeaponWithNewEntity(weapon, spawnFlags, firstScriptOfType.Id.Id, frame, attachedMissionObject == null ? MissionObjectId.Invalid : attachedMissionObject.Id, true, hasLifeTime));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord, null);
                for (int i = 0; i < weapon.GetAttachedWeaponsCount(); i++)
                {
                    GameNetwork.BeginBroadcastModuleEvent();
                    GameNetwork.WriteMessage(new AttachWeaponToSpawnedWeapon(weapon.GetAttachedWeapon(i), firstScriptOfType.Id, weapon.GetAttachedWeaponFrame(i)));
                    GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord, null);
                }
            }
            Vec3 zero = Vec3.Zero;
            var original = typeof(Mission).GetMethod("SpawnWeaponAux", BindingFlags.NonPublic | BindingFlags.Instance);
            original.Invoke(Mission.Current, new object[] {
                gameEntity, weapon, spawnFlags, zero, zero, hasLifeTime
            });
            return gameEntity;
        }
    }
}
