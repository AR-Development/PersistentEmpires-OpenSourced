using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.SceneScripts
{
    public class PE_TeleportDoor : PE_UsableFromDistance
    {
        // private PE_TeleportDoor LinkedDoor { get; set; }
        private PE_TeleportDoor LinkedDoor { get; set; }
        public string LinkText = "Same Unique Text With Other Door";
        public int CastleId = -1;
        public bool Lockpickable = true;
        private bool AllowMembersWithoutKeys = false;
        public void SetLinkedDoor(PE_TeleportDoor LinkedDoor)
        {
            this.LinkedDoor = LinkedDoor;
        }
        protected override void OnInit()
        {
            base.OnInit();
            base.ActionMessage = new TextObject("Door");
            TextObject descriptionMessage = new TextObject("Press {KEY} To Use");
            descriptionMessage.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
            base.DescriptionMessage = descriptionMessage;
        }
        public override string GetDescriptionText(GameEntity gameEntity = null)
        {
            return "Use Door";
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
        protected override bool OnHit(Agent attackerAgent, int damage, Vec3 impactPosition, Vec3 impactDirection, in MissionWeapon weapon, ScriptComponentBehavior attackerScriptComponentBehavior, out bool reportDamage)
        {
            reportDamage = false;
            if (this.Lockpickable == false) return false;
            if (this.CastleId == -1) return false;

            /*bool lockPickSuccess = LockpickingBehavior.Instance.Lockpick(attackerAgent, weapon);
            if (lockPickSuccess)
            {
                GameEntity teleportPosEntity = this.LinkedDoor.GameEntity.GetFirstChildEntityWithTag("position");
                attackerAgent.TeleportToPosition(teleportPosEntity.GlobalPosition);
            }*/

            return false;
        }

        public bool CanPlayerUse(Agent userAgent)
        {
            if (this.CastleId == -1) return true;
            Faction f = this.GetCastleBanner().GetOwnerFaction();
            NetworkCommunicator player = userAgent.MissionPeer.GetNetworkPeer();
            PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
            if (this.AllowMembersWithoutKeys && persistentEmpireRepresentative.GetFaction() == f) return true;
            if (f.doorManagers.Contains(player.VirtualPlayer.ToPlayerId()) || f.marshalls.Contains(player.VirtualPlayer.ToPlayerId()) || f.lordId == player.VirtualPlayer.ToPlayerId()) return true;
            return false;
        }
        public override void OnUse(Agent userAgent)
        {
            Debug.Print("[USING LOG] AGENT USE " + this.GetType().Name);

            if (!base.IsUsable(userAgent))
            {
                userAgent.StopUsingGameObjectMT(false);
                return;
            }
            base.OnUse(userAgent);
            userAgent.StopUsingGameObjectMT(true);
            if (GameNetwork.IsServer)
            {
                // TODO: Add Faction Checks Later
                if (LinkedDoor != null)
                {
                    bool canPlayerUse = this.CanPlayerUse(userAgent);
                    NetworkCommunicator player = userAgent.MissionPeer.GetNetworkPeer();
                    if (this.CastleId > -1)
                    {
                        PE_RepairableDestructableComponent destructComponent = base.GameEntity.GetFirstScriptOfType<PE_RepairableDestructableComponent>();
                        if (destructComponent != null && destructComponent.IsBroken) canPlayerUse = true;
                    }
                    if (canPlayerUse)
                    {
                        GameEntity teleportPosEntity = this.LinkedDoor.GameEntity.GetFirstChildEntityWithTag("position");
                        userAgent.TeleportToPosition(teleportPosEntity.GlobalPosition);
                    }
                    else
                    {
                        Faction f = this.GetCastleBanner().GetOwnerFaction();
                        InformationComponent.Instance.SendMessage("This door is locked by " + f.name, 0x0606c2d9, player);
                    }
                }
            }
        }
    }
}
