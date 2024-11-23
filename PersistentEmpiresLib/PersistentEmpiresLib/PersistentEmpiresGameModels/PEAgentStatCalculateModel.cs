using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib.SceneScripts;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.PersistentEmpiresGameModels
{
    public class PEAgentStatCalculateModel : AgentStatCalculateModel
    {
        public float GetEffectiveArmorEncumbrance(Agent agent)
        {
            // float num4 = agent.Character.GetSkillValue(DefaultSkills.Athletics) > 0 ? agent.Character.GetSkillValue(DefaultSkills.Athletics) : 10;
            float effectiveWeight = 0f;
            float skill = agent.Character.GetSkillValue(PersistentEmpireSkills.Endurance) < 10 ? 10 : agent.Character.GetSkillValue(PersistentEmpireSkills.Endurance);

            for (EquipmentIndex equipmentIndex = agent.IsHuman ? EquipmentIndex.NumAllWeaponSlots : EquipmentIndex.HorseHarness; equipmentIndex < (agent.IsHuman ? EquipmentIndex.ArmorItemEndSlot : EquipmentIndex.NumEquipmentSetSlots); equipmentIndex++)
            {
                EquipmentElement equipmentElement = agent.SpawnEquipment[equipmentIndex];
                if (!equipmentElement.IsEmpty)
                {
                    int tier = (int)equipmentElement.Item.Tier;
                    int requiredEndurance = (tier + 1) * 10;
                    if (skill >= requiredEndurance)
                    {
                        effectiveWeight += equipmentElement.GetEquipmentElementWeight();
                    }
                    else
                    {
                        effectiveWeight += equipmentElement.GetEquipmentElementWeight() * 75;
                    }
                }
            }
            float totalWeightOfArmor = effectiveWeight; // 20 -> agent 
            return MathF.Max(0f, totalWeightOfArmor);
        }
        public override bool CanAgentRideMount(Agent agent, Agent targetMount)
        {
            return agent.CheckSkillForMounting(targetMount);
        }

        public override float GetDifficultyModifier()
        {
            return 0.5f;
        }

        public override float GetDismountResistance(Agent agent)
        {
            BasicCharacterObject characterObject = agent.Character;
            if (characterObject != null)
            {
                float effectiveSkill = characterObject.GetSkillValue(DefaultSkills.Riding);
                return 0.0025f * (float)effectiveSkill;
            }
            return float.MaxValue;

        }

        public override float GetKnockBackResistance(Agent agent)
        {
            return 0.25f;
        }

        public override float GetKnockDownResistance(Agent agent, StrikeType strikeType = StrikeType.Invalid)
        {
            float num = 0.5f;
            if (agent.HasMount)
            {
                num += 0.1f;
            }
            else if (strikeType == StrikeType.Thrust)
            {
                num += 0.25f;
            }
            return num;
        }

        private static void InitializeHorseAgentStats(Agent agent, Equipment spawnEquipment, AgentDrivenProperties agentDrivenProperties)
        {
            agentDrivenProperties.AiSpeciesIndex = agent.Monster.FamilyType;
            float num = 0.8f;
            EquipmentElement equipmentElement = spawnEquipment[EquipmentIndex.HorseHarness];
            agentDrivenProperties.AttributeRiding = num + ((equipmentElement.Item != null) ? 0.2f : 0f);
            float num2 = 0f;
            for (int i = 1; i < 12; i++)
            {
                equipmentElement = spawnEquipment[i];
                if (equipmentElement.Item != null)
                {
                    float num3 = num2;
                    equipmentElement = spawnEquipment[i];
                    num2 = num3 + (float)equipmentElement.GetModifiedMountBodyArmor();
                }
            }
            agentDrivenProperties.ArmorTorso = num2;
            equipmentElement = spawnEquipment[EquipmentIndex.ArmorItemEndSlot];
            // HorseComponent horseComponent = equipmentElement.Item.HorseComponent;
            EquipmentElement equipmentElement2 = spawnEquipment[EquipmentIndex.ArmorItemEndSlot];
            equipmentElement = spawnEquipment[EquipmentIndex.HorseHarness];
            agentDrivenProperties.MountChargeDamage = (float)equipmentElement2.GetModifiedMountCharge(equipmentElement) * 0.01f;
            agentDrivenProperties.MountDifficulty = (float)equipmentElement2.Item.Difficulty;
        }
        public override void InitializeAgentStats(Agent agent, Equipment spawnEquipment, AgentDrivenProperties agentDrivenProperties, AgentBuildData agentBuildData)
        {
            agentDrivenProperties.ArmorEncumbrance = spawnEquipment.GetTotalWeightOfArmor(agent.IsHuman);
            if (!agent.IsHuman)
            {
                PEAgentStatCalculateModel.InitializeHorseAgentStats(agent, spawnEquipment, agentDrivenProperties);
                return;
            }
            agentDrivenProperties = this.InitializeAgentHumanStats(agent, agent.SpawnEquipment, agentDrivenProperties, agentBuildData);
        }
        public override float GetEffectiveMaxHealth(Agent agent)
        {
            MultiplayerClassDivisions.MPHeroClass mpheroClassForCharacter = MultiplayerClassDivisions.GetMPHeroClassForCharacter(agent.Character);
            if (mpheroClassForCharacter != null)
            {
                return (float)mpheroClassForCharacter.Health;
            }
            return 100f;
        }
        private AgentDrivenProperties InitializeAgentHumanStats(Agent agent, Equipment spawnEquipment, AgentDrivenProperties agentDrivenProperties, AgentBuildData agentBuildData)
        {
            var isAgentWounded = WoundingBehavior.Instance.IsAgentWounded(agent);
            if (isAgentWounded)
            {
                ApplyWoundedAgentProperties(agent, agentDrivenProperties);

                return agentDrivenProperties;
            }
            // Bloklama yarrak gibi olursa buraya bak
            // agentDrivenProperties.SetStat(DrivenProperty.UseRealisticBlocking, MultiplayerOptions.OptionType.UseRealisticBlocking.GetBoolValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions) ? 1f : 0f);
            MultiplayerClassDivisions.MPHeroClass heroClass = MultiplayerClassDivisions.GetMPHeroClassForCharacter(agent.Character);
            agentDrivenProperties.ArmorHead = spawnEquipment.GetHeadArmorSum();
            agentDrivenProperties.ArmorTorso = spawnEquipment.GetHumanBodyArmorSum();
            agentDrivenProperties.ArmorLegs = spawnEquipment.GetLegArmorSum();
            agentDrivenProperties.ArmorArms = spawnEquipment.GetArmArmorSum();
            agent.BaseHealthLimit = this.GetEffectiveMaxHealth(agent);
            agent.HealthLimit = agent.BaseHealthLimit;
            agent.Health = agent.HealthLimit;
            float managedParameter = ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.BipedalCombatSpeedMinMultiplier);
            float managedParameter2 = ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.BipedalCombatSpeedMaxMultiplier);
            float num = heroClass.IsTroopCharacter(agent.Character) ? heroClass.TroopCombatMovementSpeedMultiplier : heroClass.HeroCombatMovementSpeedMultiplier;
            agentDrivenProperties.CombatMaxSpeedMultiplier = managedParameter + (managedParameter2 - managedParameter) * num;
            return agentDrivenProperties;
        }
        private void FillAgentStatsFromData(ref AgentDrivenProperties agentDrivenProperties, Agent agent, MultiplayerClassDivisions.MPHeroClass heroClass, MissionPeer missionPeer, MissionPeer owningMissionPeer)
        {
            MissionPeer missionPeer2 = missionPeer ?? owningMissionPeer;
            if (missionPeer2 != null)
            {
                MPPerkObject.MPOnSpawnPerkHandler onSpawnPerkHandler = MPPerkObject.GetOnSpawnPerkHandler(missionPeer2);
                bool isPlayer = missionPeer != null;
                for (int i = 0; i < 55; i++)
                {
                    DrivenProperty drivenProperty = (DrivenProperty)i;
                    float stat = agentDrivenProperties.GetStat(drivenProperty);
                    if (drivenProperty == DrivenProperty.ArmorHead || drivenProperty == DrivenProperty.ArmorTorso || drivenProperty == DrivenProperty.ArmorLegs || drivenProperty == DrivenProperty.ArmorArms)
                    {
                        agentDrivenProperties.SetStat(drivenProperty, stat + (float)heroClass.ArmorValue + onSpawnPerkHandler.GetDrivenPropertyBonusOnSpawn(isPlayer, drivenProperty, stat));
                    }
                    else
                    {
                        agentDrivenProperties.SetStat(drivenProperty, stat + onSpawnPerkHandler.GetDrivenPropertyBonusOnSpawn(isPlayer, drivenProperty, stat));
                    }
                }
            }
            float topSpeedReachDuration = heroClass.IsTroopCharacter(agent.Character) ? heroClass.TroopTopSpeedReachDuration : heroClass.HeroTopSpeedReachDuration;
            agentDrivenProperties.TopSpeedReachDuration = topSpeedReachDuration;
            float managedParameter = ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.BipedalCombatSpeedMinMultiplier);
            float managedParameter2 = ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.BipedalCombatSpeedMaxMultiplier);
            float num = heroClass.IsTroopCharacter(agent.Character) ? heroClass.TroopCombatMovementSpeedMultiplier : heroClass.HeroCombatMovementSpeedMultiplier;
            agentDrivenProperties.CombatMaxSpeedMultiplier = managedParameter + (managedParameter2 - managedParameter) * num;
        }
        public override void UpdateAgentStats(Agent agent, AgentDrivenProperties agentDrivenProperties)
        {
            if (agent.IsHuman)
            {
                this.UpdateHumanAgentStats(agent, agentDrivenProperties);
                return;
            }
            if (agent.IsMount)
            {
                this.UpdateMountAgentStats(agent, agentDrivenProperties);
            }
        }
        private void UpdateMountAgentStats(Agent agent, AgentDrivenProperties agentDrivenProperties)
        {
            var isAgentWounded = WoundingBehavior.Instance.IsAgentWounded(agent);

            // 0 Set Wounded flags
            if (isAgentWounded)
            {
                agentDrivenProperties.MountManeuver = 0.5f;
                agentDrivenProperties.MountSpeed = 0.5f;
                agentDrivenProperties.TopSpeedReachDuration = 10f;
                agentDrivenProperties.MountDashAccelerationMultiplier = 0.5f;

                return;
            }

            //var tmp = Mission.Current.GetActiveEntitiesWithScriptComponentOfType<PE_AttachToAgent>().ToList().Select(x => x.GetFirstScriptOfType<PE_AttachToAgent>());
            //if (tmp.Any(x => x.AttachedTo == agent))
            //{
            //    agentDrivenProperties.MountManeuver = 0.8f;
            //    agentDrivenProperties.MountSpeed = 0.8f;
            //    agentDrivenProperties.TopSpeedReachDuration = 10f;
            //    agentDrivenProperties.MountDashAccelerationMultiplier = 0.8f;

            //    return;
            //}

            MPPerkObject.MPPerkHandler perkHandler = MPPerkObject.GetPerkHandler(agent.RiderAgent);
            EquipmentElement equipmentElement = agent.SpawnEquipment[EquipmentIndex.ArmorItemEndSlot];
            EquipmentElement equipmentElement2 = agent.SpawnEquipment[EquipmentIndex.HorseHarness];
            agentDrivenProperties.MountManeuver = (float)equipmentElement.GetModifiedMountManeuver(equipmentElement2) * (1f + ((perkHandler != null) ? perkHandler.GetMountManeuver() : 0f));
            agentDrivenProperties.MountSpeed = (float)(equipmentElement.GetModifiedMountSpeed(equipmentElement2) + 1) * 0.22f * (1f + ((perkHandler != null) ? perkHandler.GetMountSpeed() : 0f));
            Agent riderAgent = agent.RiderAgent;
            int num = (riderAgent != null) ? riderAgent.Character.GetSkillValue(DefaultSkills.Riding) : 100;
            agentDrivenProperties.TopSpeedReachDuration = Game.Current.BasicModels.RidingModel.CalculateAcceleration(equipmentElement, equipmentElement2, num);
            agentDrivenProperties.MountSpeed *= 1f + (float)num * 0.0032f;
            agentDrivenProperties.MountManeuver *= 1f + (float)num * 0.0035f;
            float num2 = equipmentElement.Weight / 2f + (equipmentElement2.IsEmpty ? 0f : equipmentElement2.Weight);
            agentDrivenProperties.MountDashAccelerationMultiplier = ((num2 > 200f) ? ((num2 < 300f) ? (1f - (num2 - 200f) / 111f) : 0.1f) : 1f);            
        }

        private int GetMinSkillForTier(ItemObject.ItemTiers tier)
        {
            if (tier > ItemObject.ItemTiers.Tier1)
            {
                return 25 * (int)tier;
            }
            return 0;
        }
        private int GetMinSkillForWeapon(ItemObject item)
        {
            return this.GetMinSkillForTier(item.Tier);
        }
        private float GetBaseSpeedMultiplier()
        {

            return 0.5f;
        }
        private bool IsAgentWearingAboveItsSkill(Agent agent)
        {
            float num4 = agent.Character.GetSkillValue(PersistentEmpireSkills.Endurance) > 0 ? agent.Character.GetSkillValue(PersistentEmpireSkills.Endurance) : 10;

            for (EquipmentIndex equipmentIndex = agent.IsHuman ? EquipmentIndex.NumAllWeaponSlots : EquipmentIndex.HorseHarness; equipmentIndex < (agent.IsHuman ? EquipmentIndex.ArmorItemEndSlot : EquipmentIndex.NumEquipmentSetSlots); equipmentIndex++)
            {
                EquipmentElement equipmentElement = agent.SpawnEquipment[equipmentIndex];
                if (!equipmentElement.IsEmpty)
                {
                    int tier = (int)equipmentElement.Item.Tier;
                    int requiredSkillForNoExtraWeight = (tier + 1) * 10;
                    if (requiredSkillForNoExtraWeight > num4) return true;
                }
            }
            return false;
        }

        private void UpdateHumanAgentStats(Agent agent, AgentDrivenProperties agentDrivenProperties)
        {
            var isAgentWounded = WoundingBehavior.Instance.IsAgentWounded(agent);
            
            // 0 Set Wounded flags
            if (isAgentWounded)
            {
                ApplyWoundedAgentProperties(agent, agentDrivenProperties);

                base.SetAiRelatedProperties(agent, agentDrivenProperties, null, null);
                return;
            }
            
            var perkHandler = MPPerkObject.GetPerkHandler(agent);
            var character = agent.Character;
            var equipment = agent.Equipment;
            var weaponsTotalWeight = equipment.GetTotalWeightOfWeapons();
            
            weaponsTotalWeight *= 1f + ((perkHandler != null) ? perkHandler.GetEncumbrance(true) : 0f);

            var mainHandWieldedItemIndex = agent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
            var mainWeaponItem = (mainHandWieldedItemIndex != EquipmentIndex.None) ? equipment[mainHandWieldedItemIndex].Item : null;
            var mainWeaponWeaponComponentData = (mainHandWieldedItemIndex != EquipmentIndex.None) ? equipment[mainHandWieldedItemIndex].CurrentUsageItem : null;
            var offHandWieldedItemIndex = agent.GetWieldedItemIndex(Agent.HandIndex.OffHand);
            var offHandWieldedWeaponComponentData = (offHandWieldedItemIndex != EquipmentIndex.None) ? equipment[offHandWieldedItemIndex].CurrentUsageItem : null;

            // 1. Calculate WeaponsEncumbrance
            if (mainHandWieldedItemIndex != EquipmentIndex.None)
            {
                var mainWeaponWeaponComponent = mainWeaponItem.WeaponComponent;
                var mainWeaponRealWeaponLength = mainWeaponWeaponComponent.PrimaryWeapon.GetRealWeaponLength();
                var num2 = ((mainWeaponWeaponComponent.GetItemType() == ItemObject.ItemTypeEnum.Bow) ? 4f : 1.5f) * mainWeaponItem.Weight * MathF.Sqrt(mainWeaponRealWeaponLength);
                num2 *= 1f + ((perkHandler != null) ? perkHandler.GetEncumbrance(false) : 0f);
                weaponsTotalWeight += num2;
            }
            if (offHandWieldedItemIndex != EquipmentIndex.None)
            {
                var offHandWieldedItem = equipment[offHandWieldedItemIndex].Item;
                var num3 = 1.5f * offHandWieldedItem.Weight;
                num3 *= 1f + ((perkHandler != null) ? perkHandler.GetEncumbrance(false) : 0f);
                weaponsTotalWeight += num3;
            }
            // 1. Set WeaponsEncumbrance
            agentDrivenProperties.WeaponsEncumbrance = weaponsTotalWeight;

            // 2. Calculate TopSpeedReachDuration
            var heroClass = MultiplayerClassDivisions.GetMPHeroClassForCharacter(agent.Character);
            var topSpeedReachDuration = heroClass.IsTroopCharacter(agent.Character) ? heroClass.TroopTopSpeedReachDuration : heroClass.HeroTopSpeedReachDuration;
            // 2. Set TopSpeedReachDuration
            agentDrivenProperties.TopSpeedReachDuration = topSpeedReachDuration;

            // 3. Calculate and Update SwingSpeedMultiplier
            if (mainWeaponItem != null)
            {
                int minTierSkill = this.GetMinSkillForWeapon(mainWeaponItem);
                int requiredSkill = mainWeaponItem.Difficulty;
                if (requiredSkill > 0)
                {
                    agentDrivenProperties.SwingSpeedMultiplier = (float)this.GetSkillValueForItem(character, mainWeaponItem) >= requiredSkill ? 0.93f + 0.0007f * (float)this.GetSkillValueForItem(character, mainWeaponItem) : 0.5f;
                }
                else if (minTierSkill > 0)
                {
                    agentDrivenProperties.SwingSpeedMultiplier = (float)this.GetSkillValueForItem(character, mainWeaponItem) >= minTierSkill ? 0.93f + 0.0007f * (float)this.GetSkillValueForItem(character, mainWeaponItem) : 0.5f;
                }
            }
            else
            {
                agentDrivenProperties.SwingSpeedMultiplier = 0.93f + 0.0007f * (float)this.GetSkillValueForItem(character, mainWeaponItem);
            }

            // 4. Calculate and Update Armors
            agentDrivenProperties.ArmorHead = agent.SpawnEquipment.GetHeadArmorSum();
            agentDrivenProperties.ArmorTorso = agent.SpawnEquipment.GetHumanBodyArmorSum();
            agentDrivenProperties.ArmorLegs = agent.SpawnEquipment.GetLegArmorSum();
            agentDrivenProperties.ArmorArms = agent.SpawnEquipment.GetArmArmorSum();

            // 5. Calculate and Update other properties
            agentDrivenProperties.ThrustOrRangedReadySpeedMultiplier = agentDrivenProperties.SwingSpeedMultiplier;
            agentDrivenProperties.HandlingMultiplier = 1f;
            agentDrivenProperties.ShieldBashStunDurationMultiplier = 1f;
            agentDrivenProperties.KickStunDurationMultiplier = 1f;
            agentDrivenProperties.ReloadSpeed = (float)this.GetSkillValueForItem(character, mainWeaponItem) >= 25 ? 0.93f + 0.0007f * (float)this.GetSkillValueForItem(character, mainWeaponItem) : 0.3f;
            agentDrivenProperties.MissileSpeedMultiplier = 1f;
            agentDrivenProperties.ReloadMovementPenaltyFactor = 1f;

            // 6. SetAllWeaponInaccuracy
            base.SetAllWeaponInaccuracy(agent, agentDrivenProperties, (int)mainHandWieldedItemIndex, mainWeaponWeaponComponentData);

            // 7. Calculate maxSpeedMultiplier
            var athleticsSkill = character.GetSkillValue(DefaultSkills.Athletics) > 0 ? character.GetSkillValue(DefaultSkills.Athletics) : 80;
            var b = 100 + (weaponsTotalWeight / 2) + (this.GetEffectiveArmorEncumbrance(agent) / 3);
            var maxSpeedMultiplier = ((2 * athleticsSkill) - b) / 100;
            if (this.IsAgentWearingAboveItsSkill(agent)) maxSpeedMultiplier = 0.01f;
            else if (maxSpeedMultiplier < 0.7f && !this.IsAgentWearingAboveItsSkill(agent)) maxSpeedMultiplier = 0.7f;
            // 7. Set maxSpeedMultiplier
            agentDrivenProperties.MaxSpeedMultiplier = maxSpeedMultiplier;

            //float maxSpeedMultiplier = (athleticsSkill / (100f + (num / 5) + (this.GetEffectiveArmorEncumbrance(agent) / 5)));
            // eray istedi 0.8f yapıldı bash speed

            // 8. Handle Weapon specific flags
            var ridingSkillValue = character.GetSkillValue(DefaultSkills.Riding);
            var flag = false;
            var flag2 = false;

            if (mainWeaponWeaponComponentData != null)
            {
                var effectiveSkillForWeapon = this.GetEffectiveSkillForWeapon(agent, (WeaponComponentData)mainWeaponWeaponComponentData);

                if (perkHandler != null)
                {
                    agentDrivenProperties.MissileSpeedMultiplier *= perkHandler.GetThrowingWeaponSpeed(mainWeaponWeaponComponentData) + 1f;
                }
                if (mainWeaponWeaponComponentData.IsRangedWeapon)
                {
                    int thrustSpeed = mainWeaponWeaponComponentData.ThrustSpeed;
                    if (!agent.HasMount)
                    {
                        var num5 = MathF.Max(0f, 1f - (float)effectiveSkillForWeapon / 500f);
                        agentDrivenProperties.WeaponMaxMovementAccuracyPenalty = 0.125f * num5;
                        agentDrivenProperties.WeaponMaxUnsteadyAccuracyPenalty = 0.1f * num5;
                    }
                    else
                    {
                        float num6 = MathF.Max(0f, (1f - (float)effectiveSkillForWeapon / 500f) * (1f - (float)ridingSkillValue / 1800f));
                        agentDrivenProperties.WeaponMaxMovementAccuracyPenalty = 0.025f * num6;
                        agentDrivenProperties.WeaponMaxUnsteadyAccuracyPenalty = 0.06f * num6;
                    }
                    // Make sure at least 0
                    agentDrivenProperties.WeaponMaxMovementAccuracyPenalty = MathF.Max(0f, agentDrivenProperties.WeaponMaxMovementAccuracyPenalty);
                    agentDrivenProperties.WeaponMaxUnsteadyAccuracyPenalty = MathF.Max(0f, agentDrivenProperties.WeaponMaxUnsteadyAccuracyPenalty);

                    if (mainWeaponWeaponComponentData.RelevantSkill == DefaultSkills.Bow)
                    {
                        float num7 = ((float)thrustSpeed - 60f) / 75f;
                        num7 = MBMath.ClampFloat(num7, 0f, 1f);
                        agentDrivenProperties.WeaponMaxMovementAccuracyPenalty *= 6f;
                        agentDrivenProperties.WeaponMaxUnsteadyAccuracyPenalty *= 4.5f / MBMath.Lerp(0.75f, 2f, num7, 1E-05f);
                    }
                    else if (mainWeaponWeaponComponentData.RelevantSkill == DefaultSkills.Throwing)
                    {
                        float num8 = ((float)thrustSpeed - 85f) / 17f;
                        num8 = MBMath.ClampFloat(num8, 0f, 1f);
                        agentDrivenProperties.WeaponMaxUnsteadyAccuracyPenalty *= 3.5f * MBMath.Lerp(1.5f, 0.8f, num8, 1E-05f);
                    }
                    else if (mainWeaponWeaponComponentData.RelevantSkill == DefaultSkills.Crossbow)
                    {
                        agentDrivenProperties.WeaponMaxMovementAccuracyPenalty *= 2.5f;
                        agentDrivenProperties.WeaponMaxUnsteadyAccuracyPenalty *= 1.2f;
                    }
                    if (mainWeaponWeaponComponentData.WeaponClass == WeaponClass.Bow)
                    {
                        flag = true;
                        agentDrivenProperties.WeaponBestAccuracyWaitTime = 0.3f + (95.75f - (float)thrustSpeed) * 0.005f;
                        float num9 = ((float)thrustSpeed - 60f) / 75f;
                        num9 = MBMath.ClampFloat(num9, 0f, 1f);
                        agentDrivenProperties.WeaponUnsteadyBeginTime = 0.1f + (float)effectiveSkillForWeapon * 0.01f * MBMath.Lerp(1f, 2f, num9, 1E-05f);
                        if (agent.IsAIControlled)
                        {
                            agentDrivenProperties.WeaponUnsteadyBeginTime *= 4f;
                        }
                        agentDrivenProperties.WeaponUnsteadyEndTime = 2f + agentDrivenProperties.WeaponUnsteadyBeginTime;
                        agentDrivenProperties.WeaponRotationalAccuracyPenaltyInRadians = 0.1f;
                    }
                    else if (mainWeaponWeaponComponentData.WeaponClass == WeaponClass.Javelin || mainWeaponWeaponComponentData.WeaponClass == WeaponClass.ThrowingAxe || mainWeaponWeaponComponentData.WeaponClass == WeaponClass.ThrowingKnife)
                    {
                        agentDrivenProperties.WeaponBestAccuracyWaitTime = 0.4f + (89f - (float)thrustSpeed) * 0.03f;
                        agentDrivenProperties.WeaponUnsteadyBeginTime = 2.5f + (float)effectiveSkillForWeapon * 0.01f;
                        agentDrivenProperties.WeaponUnsteadyEndTime = 10f + agentDrivenProperties.WeaponUnsteadyBeginTime;
                        agentDrivenProperties.WeaponRotationalAccuracyPenaltyInRadians = 0.025f;
                        if (mainWeaponWeaponComponentData.WeaponClass == WeaponClass.ThrowingAxe)
                        {
                            agentDrivenProperties.WeaponInaccuracy *= 6.6f;
                        }
                    }
                    else
                    {
                        agentDrivenProperties.WeaponBestAccuracyWaitTime = 0.1f;
                        agentDrivenProperties.WeaponUnsteadyBeginTime = 0f;
                        agentDrivenProperties.WeaponUnsteadyEndTime = 0f;
                        agentDrivenProperties.WeaponRotationalAccuracyPenaltyInRadians = 0.1f;
                    }
                }
                else if (mainWeaponWeaponComponentData.WeaponFlags.HasAllFlags(WeaponFlags.WideGrip))
                {
                    flag2 = true;
                    agentDrivenProperties.WeaponUnsteadyBeginTime = 1f + (float)effectiveSkillForWeapon * 0.005f;
                    agentDrivenProperties.WeaponUnsteadyEndTime = 3f + (float)effectiveSkillForWeapon * 0.01f;
                }
            }

            // 9. Handle AttributeShieldMissileCollisionBodySizeAdder
            agentDrivenProperties.AttributeShieldMissileCollisionBodySizeAdder = 0.3f;

            // 10. Handle horse
            var mountAgent = agent.MountAgent;
            float num10 = (mountAgent != null) ? mountAgent.GetAgentDrivenPropertyValue(DrivenProperty.AttributeRiding) : 1f;
            agentDrivenProperties.AttributeRiding = (float)ridingSkillValue * num10;
            agentDrivenProperties.AttributeHorseArchery = MissionGameModels.Current.StrikeMagnitudeModel.CalculateHorseArcheryFactor(character);
            agentDrivenProperties.BipedalRangedReadySpeedMultiplier = ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.BipedalRangedReadySpeedMultiplier);
            agentDrivenProperties.BipedalRangedReloadSpeedMultiplier = ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.BipedalRangedReloadSpeedMultiplier);

            if (perkHandler != null)
            {
                for (int i = 55; i < 84; i++)
                {
                    DrivenProperty drivenProperty = (DrivenProperty)i;
                    if (((drivenProperty != DrivenProperty.WeaponUnsteadyBeginTime && drivenProperty != DrivenProperty.WeaponUnsteadyEndTime) || flag || flag2) && (drivenProperty != DrivenProperty.WeaponRotationalAccuracyPenaltyInRadians || flag))
                    {
                        float stat = agentDrivenProperties.GetStat(drivenProperty);
                        agentDrivenProperties.SetStat(drivenProperty, stat + perkHandler.GetDrivenPropertyBonus(drivenProperty, stat));
                    }
                }
            }
            base.SetAiRelatedProperties(agent, agentDrivenProperties, mainWeaponWeaponComponentData, offHandWieldedWeaponComponentData);
        }

        private static void ApplyWoundedAgentProperties(Agent agent, AgentDrivenProperties agentDrivenProperties)
        {
            agentDrivenProperties.TopSpeedReachDuration = 0.1f;
            agentDrivenProperties.MaxSpeedMultiplier = 0.2f; // 0.01f
            agentDrivenProperties.MissileSpeedMultiplier = 0.1f;
            agentDrivenProperties.SwingSpeedMultiplier = 0.2f;

            agentDrivenProperties.ArmorHead = agent.SpawnEquipment.GetHeadArmorSum();
            agentDrivenProperties.ArmorTorso = agent.SpawnEquipment.GetHumanBodyArmorSum();
            agentDrivenProperties.ArmorLegs = agent.SpawnEquipment.GetLegArmorSum();
            agentDrivenProperties.ArmorArms = agent.SpawnEquipment.GetArmArmorSum();

            agentDrivenProperties.ThrustOrRangedReadySpeedMultiplier = 0.1f; ;
            agentDrivenProperties.HandlingMultiplier = 0.1f;
            agentDrivenProperties.ShieldBashStunDurationMultiplier = 0.1f;
            agentDrivenProperties.KickStunDurationMultiplier = 0.1f;
            agentDrivenProperties.ReloadSpeed = 0.1f;
            agentDrivenProperties.MissileSpeedMultiplier = 0.5f;
            agentDrivenProperties.ReloadMovementPenaltyFactor = 0.1f;

            agentDrivenProperties.WeaponMaxMovementAccuracyPenalty = 0.1f;
            agentDrivenProperties.WeaponMaxUnsteadyAccuracyPenalty = 0.1f;

            agentDrivenProperties.WeaponInaccuracy = 10f;
            agentDrivenProperties.WeaponBestAccuracyWaitTime = 0.1f;
            agentDrivenProperties.WeaponUnsteadyBeginTime = 0f;
            agentDrivenProperties.WeaponUnsteadyEndTime = 1f;
            agentDrivenProperties.WeaponRotationalAccuracyPenaltyInRadians = 2f;

            agentDrivenProperties.AttributeRiding = 0.5f;
            agentDrivenProperties.AttributeHorseArchery = 0.1f;
            agentDrivenProperties.BipedalRangedReadySpeedMultiplier = 0.01f;
            agentDrivenProperties.BipedalRangedReloadSpeedMultiplier = 0.01f;
        }

        private int GetSkillValueForItem(BasicCharacterObject characterObject, ItemObject primaryItem)
        {
            return characterObject.GetSkillValue((primaryItem != null) ? primaryItem.RelevantSkill : DefaultSkills.Athletics);
        }

        public override float GetWeaponDamageMultiplier(Agent agent, WeaponComponentData weapon)
        {
            return 1f;
        }
    }
}
