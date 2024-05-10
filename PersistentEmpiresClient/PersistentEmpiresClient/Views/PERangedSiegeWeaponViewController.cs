using PersistentEmpiresLib.SceneScripts;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.SiegeWeapon;

namespace PersistentEmpires.Views.Views
{
    public class PERangedSiegeWeaponViewController : MissionView
    {
        // Token: 0x060004A3 RID: 1187 RVA: 0x00023584 File Offset: 0x00021784
        public override void OnObjectUsed(Agent userAgent, UsableMissionObject usedObject)
        {
            base.OnObjectUsed(userAgent, usedObject);
            if (userAgent.IsMainAgent && usedObject is StandingPoint)
            {
                UsableMachine usableMachineFromPoint = this.GetUsableMachineFromPoint(usedObject as StandingPoint);
                if (usableMachineFromPoint is RangedSiegeWeapon)
                {
                    RangedSiegeWeapon rangedSiegeWeapon = usableMachineFromPoint as RangedSiegeWeapon;
                    if (rangedSiegeWeapon.GetComponent<RangedSiegeWeaponView>() == null)
                    {
                        this.AddRangedSiegeWeaponView(rangedSiegeWeapon);
                    }
                }
            }
        }

        // Token: 0x060004A4 RID: 1188 RVA: 0x000235D4 File Offset: 0x000217D4
        private UsableMachine GetUsableMachineFromPoint(StandingPoint standingPoint)
        {
            GameEntity gameEntity = standingPoint.GameEntity;
            while (gameEntity != null && !gameEntity.HasScriptOfType<RangedSiegeWeapon>())
            {
                gameEntity = gameEntity.Parent;
            }
            if (gameEntity != null)
            {
                UsableMachine firstScriptOfType = gameEntity.GetFirstScriptOfType<RangedSiegeWeapon>();
                if (firstScriptOfType != null)
                {
                    return firstScriptOfType;
                }
            }
            return null;
        }

        // Token: 0x060004A5 RID: 1189 RVA: 0x00023618 File Offset: 0x00021818
        private void AddRangedSiegeWeaponView(RangedSiegeWeapon rangedSiegeWeapon)
        {
            PERangedSiegeWeaponView rangedSiegeWeaponView;
            /*if (rangedSiegeWeapon is Trebuchet)
			{
				rangedSiegeWeaponView = new TrebuchetView();
			}
			else*/
            if (rangedSiegeWeapon is PE_Mangonel)
            {
                rangedSiegeWeaponView = new PE_MangonelView();
            }
            /*else if (rangedSiegeWeapon is Ballista)
			{
				rangedSiegeWeaponView = new BallistaView();
			}*/
            else
            {
                rangedSiegeWeaponView = new PERangedSiegeWeaponView();
            }
            rangedSiegeWeaponView.Initialize(rangedSiegeWeapon, base.MissionScreen);
            rangedSiegeWeapon.AddComponent(rangedSiegeWeaponView);
        }
    }
}
