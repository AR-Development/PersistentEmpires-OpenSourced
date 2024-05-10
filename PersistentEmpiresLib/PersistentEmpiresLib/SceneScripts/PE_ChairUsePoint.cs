using System;
using System.Collections.Generic;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.SceneScripts
{
    public class PE_ChairUsePoint : PE_AnimationPoint
    {
        // Token: 0x06000255 RID: 597 RVA: 0x00010008 File Offset: 0x0000E208
        protected override void SetActionCodes()
        {
            base.SetActionCodes();
            this._loopAction = ActionIndexCache.Create(this.LoopStartAction);
            this._pairLoopAction = ActionIndexCache.Create(this.PairLoopStartAction);
            this._nearTableLoopAction = ActionIndexCache.Create(this.NearTableLoopAction);
            this._nearTablePairLoopAction = ActionIndexCache.Create(this.NearTablePairLoopAction);
            this._drinkLoopAction = ActionIndexCache.Create(this.DrinkLoopAction);
            this._drinkPairLoopAction = ActionIndexCache.Create(this.DrinkPairLoopAction);
            this._eatLoopAction = ActionIndexCache.Create(this.EatLoopAction);
            this._eatPairLoopAction = ActionIndexCache.Create(this.EatPairLoopAction);
            this.SetChairAction(this.GetRandomChairAction());
        }

        // Token: 0x06000256 RID: 598 RVA: 0x000100B0 File Offset: 0x0000E2B0
        protected override bool ShouldUpdateOnEditorVariableChanged(string variableName)
        {
            return base.ShouldUpdateOnEditorVariableChanged(variableName) || variableName == "NearTable" || variableName == "Drink" || variableName == "Eat" || variableName == "NearTableLoopAction" || variableName == "DrinkLoopAction" || variableName == "EatLoopAction";
        }

        // Token: 0x06000257 RID: 599 RVA: 0x00010114 File Offset: 0x0000E314
        public override void OnUse(Agent userAgent)
        {
            PE_ChairUsePoint.ChairAction chairAction = base.CanAgentUseItem(userAgent) ? this.GetRandomChairAction() : PE_ChairUsePoint.ChairAction.None;
            this.SetChairAction(chairAction);
            base.OnUse(userAgent);
        }

        // Token: 0x06000258 RID: 600 RVA: 0x00010144 File Offset: 0x0000E344
        private PE_ChairUsePoint.ChairAction GetRandomChairAction()
        {
            List<PE_ChairUsePoint.ChairAction> list = new List<PE_ChairUsePoint.ChairAction>
            {
                PE_ChairUsePoint.ChairAction.None
            };
            if (this.NearTable && this._nearTableLoopAction != ActionIndexCache.act_none)
            {
                list.Add(PE_ChairUsePoint.ChairAction.LeanOnTable);
            }
            if (this.Drink && this._drinkLoopAction != ActionIndexCache.act_none)
            {
                list.Add(PE_ChairUsePoint.ChairAction.Drink);
            }
            if (this.Eat && this._eatLoopAction != ActionIndexCache.act_none)
            {
                list.Add(PE_ChairUsePoint.ChairAction.Eat);
            }
            return list[new Random().Next(list.Count)];
        }

        // Token: 0x06000259 RID: 601 RVA: 0x000101D8 File Offset: 0x0000E3D8
        private void SetChairAction(PE_ChairUsePoint.ChairAction chairAction)
        {
            switch (chairAction)
            {
                case PE_ChairUsePoint.ChairAction.None:
                    this.LoopStartActionCode = this._loopAction;
                    this.PairLoopStartActionCode = this._pairLoopAction;
                    base.SelectedRightHandItem = this.RightHandItem;
                    base.SelectedLeftHandItem = this.LeftHandItem;
                    return;
                case PE_ChairUsePoint.ChairAction.LeanOnTable:
                    this.LoopStartActionCode = this._nearTableLoopAction;
                    this.PairLoopStartActionCode = this._nearTablePairLoopAction;
                    base.SelectedRightHandItem = string.Empty;
                    base.SelectedLeftHandItem = string.Empty;
                    return;
                case PE_ChairUsePoint.ChairAction.Drink:
                    this.LoopStartActionCode = this._drinkLoopAction;
                    this.PairLoopStartActionCode = this._drinkPairLoopAction;
                    base.SelectedRightHandItem = this.DrinkRightHandItem;
                    base.SelectedLeftHandItem = this.DrinkLeftHandItem;
                    return;
                case PE_ChairUsePoint.ChairAction.Eat:
                    this.LoopStartActionCode = this._eatLoopAction;
                    this.PairLoopStartActionCode = this._eatPairLoopAction;
                    base.SelectedRightHandItem = this.EatRightHandItem;
                    base.SelectedLeftHandItem = this.EatLeftHandItem;
                    return;
                default:
                    return;
            }
        }

        // Token: 0x0600025A RID: 602 RVA: 0x000102C0 File Offset: 0x0000E4C0
        protected override void OnTick(float dt)
        {
            base.OnTick(dt);
            if (base.UserAgent != null && !base.UserAgent.IsAIControlled && base.UserAgent.EventControlFlags.HasAnyFlag(Agent.EventControlFlag.Crouch | Agent.EventControlFlag.Stand))
            {
                base.UserAgent.StopUsingGameObject(true, Agent.StopUsingGameObjectFlags.AutoAttachAfterStoppingUsingGameObject);
            }
        }

        // Token: 0x04000122 RID: 290
        public bool NearTable;

        // Token: 0x04000123 RID: 291
        public string NearTableLoopAction = "";

        // Token: 0x04000124 RID: 292
        public string NearTablePairLoopAction = "";

        // Token: 0x04000125 RID: 293
        public bool Drink;

        // Token: 0x04000126 RID: 294
        public string DrinkLoopAction = "";

        // Token: 0x04000127 RID: 295
        public string DrinkPairLoopAction = "";

        // Token: 0x04000128 RID: 296
        public string DrinkRightHandItem = "";

        // Token: 0x04000129 RID: 297
        public string DrinkLeftHandItem = "";

        // Token: 0x0400012A RID: 298
        public bool Eat;

        // Token: 0x0400012B RID: 299
        public string EatLoopAction = "";

        // Token: 0x0400012C RID: 300
        public string EatPairLoopAction = "";

        // Token: 0x0400012D RID: 301
        public string EatRightHandItem = "";

        // Token: 0x0400012E RID: 302
        public string EatLeftHandItem = "";

        // Token: 0x0400012F RID: 303
        private ActionIndexCache _loopAction;

        // Token: 0x04000130 RID: 304
        private ActionIndexCache _pairLoopAction;

        // Token: 0x04000131 RID: 305
        private ActionIndexCache _nearTableLoopAction;

        // Token: 0x04000132 RID: 306
        private ActionIndexCache _nearTablePairLoopAction;

        // Token: 0x04000133 RID: 307
        private ActionIndexCache _drinkLoopAction;

        // Token: 0x04000134 RID: 308
        private ActionIndexCache _drinkPairLoopAction;

        // Token: 0x04000135 RID: 309
        private ActionIndexCache _eatLoopAction;

        // Token: 0x04000136 RID: 310
        private ActionIndexCache _eatPairLoopAction;

        // Token: 0x0200010D RID: 269
        private enum ChairAction
        {
            // Token: 0x04000541 RID: 1345
            None,
            // Token: 0x04000542 RID: 1346
            LeanOnTable,
            // Token: 0x04000543 RID: 1347
            Drink,
            // Token: 0x04000544 RID: 1348
            Eat
        }
    }
}
