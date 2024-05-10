using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.SceneScripts
{
    public class PE_Chair : UsableMachine
    {
        // Token: 0x060001BE RID: 446 RVA: 0x0000C464 File Offset: 0x0000A664
        protected override void OnInit()
        {
            base.OnInit();
            foreach (StandingPoint standingPoint in base.StandingPoints)
            {
                standingPoint.AutoSheathWeapons = true;
            }
        }

        // Token: 0x060001BF RID: 447 RVA: 0x0000C4BC File Offset: 0x0000A6BC
        public bool IsAgentFullySitting(Agent usingAgent)
        {
            return base.StandingPoints.Count > 0 && base.StandingPoints.Contains(usingAgent.CurrentlyUsedGameObject) && usingAgent.IsSitting();
        }

        // Token: 0x060001C1 RID: 449 RVA: 0x0000C4EF File Offset: 0x0000A6EF
        public override TextObject GetActionTextForStandingPoint(UsableMissionObject usableGameObject)
        {
            TextObject textObject = new TextObject(this.IsAgentFullySitting(Agent.Main) ? "{=QGdaakYW}{KEY} Get Up" : "{=bl2aRW8f}{KEY} Sit", null);
            textObject.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
            return textObject;
        }

        // Token: 0x060001C2 RID: 450 RVA: 0x0000C530 File Offset: 0x0000A730
        public override string GetDescriptionText(GameEntity gameEntity = null)
        {
            switch (this.ChairType)
            {
                case PE_Chair.SittableType.Log:
                    return new TextObject("{=9pgOGq7X}Log", null).ToString();
                case PE_Chair.SittableType.Sofa:
                    return new TextObject("{=GvLZKQ1U}Sofa", null).ToString();
                case PE_Chair.SittableType.Ground:
                    return new TextObject("{=L7ZQtIuM}Ground", null).ToString();
                default:
                    return new TextObject("{=OgTUrRlR}Chair", null).ToString();
            }
        }

        // Token: 0x060001C3 RID: 451 RVA: 0x0000C5A0 File Offset: 0x0000A7A0
        public override StandingPoint GetBestPointAlternativeTo(StandingPoint standingPoint, Agent agent)
        {
            PE_AnimationPoint animationPoint = standingPoint as PE_AnimationPoint;
            if (animationPoint == null || animationPoint.GroupId < 0)
            {
                return animationPoint;
            }
            WorldFrame userFrameForAgent = standingPoint.GetUserFrameForAgent(agent);
            float num = userFrameForAgent.Origin.GetGroundVec3().DistanceSquared(agent.Position);
            foreach (StandingPoint standingPoint2 in base.StandingPoints)
            {
                PE_AnimationPoint animationPoint2;
                if ((animationPoint2 = (standingPoint2 as PE_AnimationPoint)) != null && standingPoint != standingPoint2 && animationPoint.GroupId == animationPoint2.GroupId && !animationPoint2.IsDisabledForAgent(agent))
                {
                    userFrameForAgent = animationPoint2.GetUserFrameForAgent(agent);
                    float num2 = userFrameForAgent.Origin.GetGroundVec3().DistanceSquared(agent.Position);
                    if (num2 < num)
                    {
                        num = num2;
                        animationPoint = animationPoint2;
                    }
                }
            }
            return animationPoint;
        }

        // Token: 0x060001C4 RID: 452 RVA: 0x0000C684 File Offset: 0x0000A884
        public override OrderType GetOrder(BattleSideEnum side)
        {
            return OrderType.None;
        }

        // Token: 0x040000B8 RID: 184
        public PE_Chair.SittableType ChairType;

        // Token: 0x02000102 RID: 258
        public enum SittableType
        {
            // Token: 0x04000507 RID: 1287
            Chair,
            // Token: 0x04000508 RID: 1288
            Log,
            // Token: 0x04000509 RID: 1289
            Sofa,
            // Token: 0x0400050A RID: 1290
            Ground
        }
    }
}
