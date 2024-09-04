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

using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.SceneScripts
{
    public class PE_MoneyChest : PE_UsableFromDistance
    {
        public delegate void MoneyChestAccessedHandler(PE_MoneyChest chest);
        public static event MoneyChestAccessedHandler OnMoneyChestAccessed;
        public long Gold = 0;
        public int CastleId = 0;
        public bool Lockpickable = true;
        public bool NoPerm = false;
        protected override void OnInit()
        {
            base.OnInit();
            TextObject actionMessage = new TextObject("Money Chest");
            base.ActionMessage = actionMessage;
            TextObject descriptionMessage = new TextObject("Press {KEY} To Access");
            descriptionMessage.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
            base.DescriptionMessage = descriptionMessage;
        }
        public override string GetDescriptionText(GameEntity gameEntity = null)
        {
            return "Money Chest";
        }
        public void UpdateGold(long gold)
        {
            if (GameNetwork.IsServer)
            {
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new UpdateMoneychestGold(this, gold));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            }
            this.Gold = gold;
        }

        public void AddTax(int amount)
        {
            this.UpdateGold(this.Gold + amount);
        }

        public bool IsBroken()
        {
            PE_RepairableDestructableComponent destructComponent = base.GameEntity.GetFirstScriptOfType<PE_RepairableDestructableComponent>();
            if (destructComponent == null) return false;
            return destructComponent.IsBroken;
        }

        protected override bool OnHit(Agent attackerAgent, int damage, Vec3 impactPosition, Vec3 impactDirection, in MissionWeapon weapon, ScriptComponentBehavior attackerScriptComponentBehavior, out bool reportDamage)
        {
            reportDamage = false;
            if (this.Lockpickable == false) return false;
            if (this.CastleId == -1) return false;

            bool lockPickSuccess = LockpickingBehavior.Instance.Lockpick(attackerAgent, weapon);
            if (lockPickSuccess)
            {
                // this.WithdrawGold(attackerAgent.MissionPeer.GetNetworkPeer(), this.Gold > 1000000 ? 1000000 : (int)this.Gold);
                PersistentEmpireRepresentative persistentEmpireRepresentative = attackerAgent.MissionPeer.GetNetworkPeer().GetComponent<PersistentEmpireRepresentative>();
                long amount = this.Gold;
                if (amount > 1000000)
                {
                    amount = 1000000;
                }
                else
                {
                    amount = this.Gold;
                }
                persistentEmpireRepresentative.GoldGain((int)amount);
                this.UpdateGold(this.Gold - amount);
            }

            return false;
        }

        public void WithdrawGold(NetworkCommunicator withdrawer, int amount)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = withdrawer.GetComponent<PersistentEmpireRepresentative>();

            if ((this.GetFaction() == null || (this.GetFaction().lordId != withdrawer.VirtualPlayer.Id.ToString() && this.IsBroken() == false)) && this.NoPerm != true)
            {
                InformationComponent.Instance.SendMessage("You don't have the keys", TaleWorlds.Library.Color.ConvertStringToColor("#ff0000ff").ToUnsignedInteger(), withdrawer);
                return;
            }
            if (amount > this.Gold)
            {
                InformationComponent.Instance.SendMessage("Chest don't have that amount", TaleWorlds.Library.Color.ConvertStringToColor("#ff0000ff").ToUnsignedInteger(), withdrawer);
                return;
            }
            persistentEmpireRepresentative.GoldGain(amount);
            this.UpdateGold(this.Gold - amount);
        }

        public bool CanUserUse(NetworkCommunicator player)
        {
            if (this.NoPerm) return true;
            if (this.GetFaction() == null) return false;
            if (this.IsBroken()) return true;
            if (this.GetFaction().lordId != player.VirtualPlayer.Id.ToString()) return false;
            return true;
        }

        public void DepositGold(NetworkCommunicator depositer, int amount)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = depositer.GetComponent<PersistentEmpireRepresentative>();

            if (this.NoPerm == false)
            {
                if (this.GetFaction() == null || (this.GetFaction().lordId != depositer.VirtualPlayer.Id.ToString() && this.IsBroken() == false))
                {
                    InformationComponent.Instance.SendMessage("You don't have the keys", TaleWorlds.Library.Color.ConvertStringToColor("#ff0000ff").ToUnsignedInteger(), depositer);
                    return;
                }
            }

            if (persistentEmpireRepresentative.ReduceIfHaveEnoughGold(amount) == false)
            {
                InformationComponent.Instance.SendMessage("You don't have enough gold", TaleWorlds.Library.Color.ConvertStringToColor("#ff0000ff").ToUnsignedInteger(), depositer);
                return;
            }
            this.UpdateGold(this.Gold + amount);
        }


        public PE_CastleBanner GetCastleBanner()
        {
            // FactionsBehavior factionBehavior = Mission.Current.GetMissionBehavior<FactionsBehavior>();
            CastlesBehavior castleBehaviors = Mission.Current.GetMissionBehavior<CastlesBehavior>();
            if (castleBehaviors.castles.ContainsKey(this.CastleId))
            {
                return castleBehaviors.castles[this.CastleId];
            }
            return null;
        }

        public Faction GetFaction()
        {
            if (this.GetCastleBanner() == null) return null;
            return this.GetCastleBanner().GetOwnerFaction();
        }
        public void OpenUI()
        {
            PE_MoneyChest.OnMoneyChestAccessed(this);
        }
        public override void OnUse(Agent userAgent)
        {
            if (!base.IsUsable(userAgent))
            {
                userAgent.StopUsingGameObjectMT(false);
                return;
            }
            base.OnUse(userAgent);
            if (userAgent.IsMine && OnMoneyChestAccessed != null)
            {
                OnMoneyChestAccessed(this);
            }
            if (GameNetwork.IsServer)
            {
                userAgent.StopUsingGameObjectMT(true);
            }
        }
    }
}
