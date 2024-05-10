using TaleWorlds.Engine;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.SceneScripts
{
    public class PE_UsablePlace : UsableMachine
    {
        // Token: 0x060001EA RID: 490 RVA: 0x0000D2D9 File Offset: 0x0000B4D9
        public override string GetDescriptionText(GameEntity gameEntity = null)
        {
            StandingPoint pilotStandingPoint = base.PilotStandingPoint;
            if (pilotStandingPoint == null)
            {
                return null;
            }
            return pilotStandingPoint.DescriptionMessage.ToString();
        }

        // Token: 0x060001EB RID: 491 RVA: 0x0000D2F1 File Offset: 0x0000B4F1
        public override TextObject GetActionTextForStandingPoint(UsableMissionObject usableGameObject)
        {
            StandingPoint pilotStandingPoint = base.PilotStandingPoint;
            if (pilotStandingPoint == null)
            {
                return null;
            }
            return pilotStandingPoint.ActionMessage;
        }
    }
}
