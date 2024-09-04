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

using System;
using System.Linq;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class DrowningBehavior : MissionLogic
    {
        public bool IsSetProperly = false;
        public float UpperLimit;
        public float LowerLimit;

        public long LastCheckedAt = 0;
        public long Duration = 10;

        public override void AfterStart()
        {

            GameEntity upperLimit = base.Mission.Scene.FindEntityWithTag("drowning_upper_limit");
            GameEntity lowerLimit = base.Mission.Scene.FindEntityWithTag("drowning_lower_limit");

            if (upperLimit == null || lowerLimit == null)
            {
                this.IsSetProperly = false;
                return;
            }
            this.IsSetProperly = true;
            this.UpperLimit = upperLimit.GetGlobalFrame().origin.Z;
            this.LowerLimit = lowerLimit.GetGlobalFrame().origin.Z;
            this.LastCheckedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public override void OnMissionTick(float dt)
        {
            if ((this.LastCheckedAt + this.Duration < DateTimeOffset.UtcNow.ToUnixTimeSeconds()) && this.IsSetProperly)
            {
                this.LastCheckedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                foreach (Agent agent in base.Mission.Agents.ToList())
                {
                    if (agent.IsActive() && agent.Position.Z < UpperLimit && agent.Position.Z > LowerLimit)
                    {
                        Blow blow = new Blow(agent.Index);
                        blow.DamageType = TaleWorlds.Core.DamageTypes.Pierce;
                        blow.BoneIndex = agent.Monster.HeadLookDirectionBoneIndex;
                        blow.GlobalPosition = agent.Position;
                        blow.GlobalPosition.z = blow.GlobalPosition.z + agent.GetEyeGlobalHeight();
                        blow.BaseMagnitude = 40;
                        blow.WeaponRecord.FillAsMeleeBlow(null, null, -1, -1);
                        blow.InflictedDamage = 40;
                        blow.SwingDirection = agent.LookDirection;
                        MatrixFrame frame = agent.Frame;
                        blow.SwingDirection = frame.rotation.TransformToParent(new Vec3(-1f, 0f, 0f, -1f));
                        blow.SwingDirection.Normalize();
                        blow.Direction = blow.SwingDirection;
                        blow.DamageCalculated = true;
                        sbyte mainHandItemBoneIndex = agent.Monster.MainHandItemBoneIndex;
                        AttackCollisionData attackCollisionDataForDebugPurpose = AttackCollisionData.GetAttackCollisionDataForDebugPurpose(false, false, false, true, false, false, false, false, false, false, false, false, CombatCollisionResult.StrikeAgent, -1, 0, 2, blow.BoneIndex, BoneBodyPartType.Head, mainHandItemBoneIndex, Agent.UsageDirection.AttackLeft, -1, CombatHitResultFlags.NormalHit, 0.5f, 1f, 0f, 0f, 0f, 0f, 0f, 0f, Vec3.Up, blow.Direction, blow.GlobalPosition, Vec3.Zero, Vec3.Zero, agent.Velocity, Vec3.Up);
                        agent.RegisterBlow(blow, attackCollisionDataForDebugPurpose);
                    }
                }
            }
        }
    }
}
