using System;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace PersistentEmpiresLib.PersistentEmpiresGameModels
{
    public class PersistentEmpireSkills
    {
        public static SkillObject Farming;
        public static SkillObject Gathering;
        public static SkillObject Fishing;
        public static SkillObject Sailor;
        public static SkillObject Cooking;
        public static SkillObject Mining;
        public static SkillObject WoodCutting;
        public static SkillObject Prostetuting;
        public static SkillObject Beauty;
        public static SkillObject Piety;
        public static SkillObject Hunting;
        public static SkillObject Endurance;
        public static SkillObject Pickpocketing;
        public static SkillObject Lockpicking;
        public static SkillObject Dismounting;
        public static SkillObject Chemist;
        public static void Initialize(Game game)
        {
            PersistentEmpireSkills.Farming = InitializeSkill(game, "Farming", "Farming", String.Empty);
            PersistentEmpireSkills.Gathering = InitializeSkill(game, "Gathering", "Gathering", String.Empty);
            PersistentEmpireSkills.Fishing = InitializeSkill(game, "Fishing", "Fishing", String.Empty);
            PersistentEmpireSkills.Sailor = InitializeSkill(game, "Sailor", "Sailor", String.Empty);
            PersistentEmpireSkills.Cooking = InitializeSkill(game, "Cooking", "Cooking", String.Empty);
            PersistentEmpireSkills.Mining = InitializeSkill(game, "Mining", "Mining", String.Empty);
            PersistentEmpireSkills.WoodCutting = InitializeSkill(game, "WoodCutting", "WoodCutting", String.Empty);
            PersistentEmpireSkills.Prostetuting = InitializeSkill(game, "Prostetuting", "Prostetuting", String.Empty);
            PersistentEmpireSkills.Beauty = InitializeSkill(game, "Beauty", "Beauty", String.Empty);
            PersistentEmpireSkills.Piety = InitializeSkill(game, "Piety", "Piety", String.Empty);
            PersistentEmpireSkills.Hunting = InitializeSkill(game, "Hunting", "Hunting", String.Empty);
            PersistentEmpireSkills.Endurance = InitializeSkill(game, "Endurance", "Endurance", String.Empty);
            PersistentEmpireSkills.Pickpocketing = InitializeSkill(game, "Pickpocketing", "Pickpocketing", String.Empty);
            PersistentEmpireSkills.Lockpicking = InitializeSkill(game, "Lockpicking", "Lockpicking", String.Empty);
            PersistentEmpireSkills.Dismounting = InitializeSkill(game, "Dismounting", "Dismounting", String.Empty);
            PersistentEmpireSkills.Chemist = InitializeSkill(game, "Chemist", "Chemist", String.Empty);
        }

        private static SkillObject InitializeSkill(Game game, string stringId, string name, string description)
        {
            SkillObject skillObject = game.ObjectManager.RegisterPresumedObject<SkillObject>(new SkillObject(stringId));
            skillObject.Initialize(new TextObject(name, null), new TextObject(description, null), SkillObject.SkillTypeEnum.Personal);
            return skillObject;
        }
    }
}
