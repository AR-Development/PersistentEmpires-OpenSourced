using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using static TaleWorlds.Core.ItemObject;

namespace PersistentEmpiresLib.PersistentEmpiresGameModels
{
    public class PEAgentApplyDamageModel : MultiplayerAgentApplyDamageModel
    {
        public bool DontOverrideMangonelHit = false;



        private int GetMinSkillForTier(ItemObject.ItemTiers tier)
        {
            if (tier > ItemObject.ItemTiers.Tier1)
            {
                return 25 * (int)tier;
            }
            return 0;
        }

        #if SERVER
        public override bool DecideAgentDismountedByBlow(Agent attackerAgent, Agent victimAgent, in AttackCollisionData collisionData, WeaponComponentData attackerWeapon, in Blow blow)
        {
            if (attackerAgent == null || attackerWeapon == null || victimAgent == null)
            {
                return base.CanWeaponDismount(attackerAgent, attackerWeapon, blow, collisionData);
            }

            if (attackerWeapon.WeaponFlags.HasFlag(WeaponFlags.CanHook) && attackerWeapon.WeaponFlags.HasFlag(WeaponFlags.CanDismount))
            {
                SkillObject skill = PersistentEmpireSkills.Dismounting;
                if (attackerAgent.Character.GetSkillValue(skill) > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return base.CanWeaponDismount(attackerAgent, attackerWeapon, blow, collisionData);
        }

        public override float CalculateDamage(in AttackInformation attackInformation, in AttackCollisionData collisionData, in MissionWeapon weapon, float baseDamage)
        {
            // if(WoundingBehavior.Instance.WoundingEnabled && WoundingBehavior.Instance.IsPlayerWounded)

            if (weapon.IsEmpty)
            {
                return baseDamage;
            }
            if (weapon.Item != null && weapon.Item.WeaponComponent != null && weapon.Item.WeaponComponent.PrimaryWeapon.WeaponClass == WeaponClass.Boulder && attackInformation.IsAttackerAgentNull == false)
            {
                bool isThrownBySiegeWeapon = false;
                foreach (var missile in Mission.Current.Missiles.ToList())
                {
                    if (missile.Index == collisionData.AffectorWeaponSlotOrMissileIndex && missile.MissionObjectToIgnore != null)
                    {
                        isThrownBySiegeWeapon = true;
                        break;
                    }
                }
                if (!isThrownBySiegeWeapon)
                {
                    return 2;
                }

                if (isThrownBySiegeWeapon && attackInformation.IsVictimAgentNull == false && ConfigManager.DontOverrideMangonelHit == false)
                {
                    return 75;
                }
            }

            if (
                weapon.Item.ItemType == ItemTypeEnum.OneHandedWeapon ||
                weapon.Item.ItemType == ItemTypeEnum.TwoHandedWeapon ||
                weapon.Item.ItemType == ItemTypeEnum.Polearm ||
                weapon.Item.ItemType == ItemTypeEnum.Bow ||
                weapon.Item.ItemType == ItemTypeEnum.Crossbow)
            {
                if (weapon.Item.StringId == DoctorBehavior.Instance.ItemId)
                {
                    SkillObject medicineSkill = MBObjectManager.Instance.GetObject<SkillObject>("Medicine");
                    if (attackInformation.AttackerAgentCharacter.GetSkillValue(medicineSkill) >= 50) return 5;
                }
                if (weapon.Item.StringId == PickpocketingBehavior.Instance.ItemId)
                {
                    SkillObject pickpocketingSkill = MBObjectManager.Instance.GetObject<SkillObject>("Pickpocketing");
                    if (attackInformation.AttackerAgentCharacter.GetSkillValue(pickpocketingSkill) >= 10) return 1;
                }


                SkillObject skillObject = weapon.Item.RelevantSkill;
                int effectiveSkill = attackInformation.AttackerAgentCharacter.GetSkillValue(skillObject);

                int requiredSkill = weapon.Item.Difficulty;
                if (requiredSkill > 0 && effectiveSkill < requiredSkill)
                {
                    return baseDamage / 4;
                }

                int minSkill = this.GetMinSkillForTier(weapon.Item.Tier);
                if (minSkill > 0 && effectiveSkill < minSkill)
                {
                    return baseDamage / 4;
                }
                else if (minSkill == 0 && effectiveSkill < 25)
                {
                    return baseDamage / 2;
                }
                return baseDamage;
            }


            return baseDamage;
        }
#endif
    }
}