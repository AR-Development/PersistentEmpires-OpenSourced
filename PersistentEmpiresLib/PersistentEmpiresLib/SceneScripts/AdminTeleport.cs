/*
 *  Persistent Empires Open Sourced - A Mount and Blade: Bannerlord Mod
 *  Copyright (C) 2024  Free Software Foundation, Inc.
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.

 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib.SceneScripts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.SceneScripts
{
    public class AdminTeleport : MissionObject
    {
        
        public string Description = "";
        public int Id = 0;

        protected override void OnEditorInit()
        {
            if (Id == 0)
            {
                Id = GenerateId();
            }
        }

        private int GenerateId()
        {
            List<GameEntity> reference = new List<GameEntity>();
            base.Scene.GetAllEntitiesWithScriptComponent<AdminTeleport>(ref reference);

            var ids = reference.Select(r => r.GetFirstScriptOfType<AdminTeleport>().Id).OrderByDescending(r=> r);
            var idMax = ids.FirstOrDefault();

            return ++idMax;
        }

        protected override void OnInit()
        {
            base.OnInit();

            AdminClientBehavior.Register(new AdminTp(Id, GameEntity.GlobalPosition, Description));
        }

        protected bool ValidateValues()
        {
            List<GameEntity> reference = new List<GameEntity>();
            base.Scene.GetAllEntitiesWithScriptComponent<AdminTeleport>(ref reference);
            List<AdminTeleport> sameId = reference.Select(r => r.GetFirstScriptOfType<AdminTeleport>()).Where(r => r.Id == Id && r != this).ToList();
            if (sameId.Count() > 0)
            {
                MBEditor.AddEntityWarning(GameEntity, Id + " has a same id with another admin teleports");
                return false;
            }
            return true;
        }
        protected override void OnSceneSave(string saveFolder)
        {
            ValidateValues();
        }
        protected override bool OnCheckForProblems()
        {
            return ValidateValues();
        }
    }

    public class AdminTp
    {
        public int Id;

        public Vec3 SpawnPosition;

        public string Description;
        private Vec3 globalPosition;

        public AdminTp(int id, Vec3 globalPosition, string description)
        {
            Id = id;
            SpawnPosition = globalPosition;
            Description = description;
        }
    }
}