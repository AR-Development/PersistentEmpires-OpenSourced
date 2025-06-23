using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.Helpers;
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

            if ((this.GetFaction() == null || (this.GetFaction().lordId != withdrawer.VirtualPlayer.ToPlayerId() && this.IsBroken() == false)) && this.NoPerm != true)
            {
                InformationComponent.Instance.SendMessage(GameTexts.FindText("PE_Dont_Have_Keys", null).ToString(), TaleWorlds.Library.Color.ConvertStringToColor("#ff0000ff").ToUnsignedInteger(), withdrawer);
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
            if (this.GetFaction().lordId != player.VirtualPlayer.ToPlayerId()) return false;
            return true;
        }

        public void DepositGold(NetworkCommunicator depositer, int amount)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = depositer.GetComponent<PersistentEmpireRepresentative>();

            if (this.NoPerm == false)
            {
                if (this.GetFaction() == null || (this.GetFaction().lordId != depositer.VirtualPlayer.ToPlayerId() && this.IsBroken() == false))
                {
                    InformationComponent.Instance.SendMessage(GameTexts.FindText("PE_Dont_Have_Keys", null).ToString(), TaleWorlds.Library.Color.ConvertStringToColor("#ff0000ff").ToUnsignedInteger(), depositer);
                    return;
                }
            }

            if (persistentEmpireRepresentative.ReduceIfHaveEnoughGold(amount) == false)
            {
                InformationComponent.Instance.SendMessage(GameTexts.FindText("PE_Not_Enough_Gold", null).ToString(), TaleWorlds.Library.Color.ConvertStringToColor("#ff0000ff").ToUnsignedInteger(), depositer);
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
