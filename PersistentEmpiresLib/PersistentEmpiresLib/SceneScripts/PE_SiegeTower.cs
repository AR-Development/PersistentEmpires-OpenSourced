using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.SceneScripts
{
    public class PE_SiegeTower : SiegeTower
    {
        public override BattleSideEnum Side
        {
            get
            {
                return BattleSideEnum.None;
            }
        }
        protected override void OnInit()
        {
            base.OnInit();
            base.DestructionComponent.BattleSide = BattleSideEnum.None;
        }
    }
}
