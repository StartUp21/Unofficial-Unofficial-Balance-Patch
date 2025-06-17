using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Unofficial_Unofficial_Balance_Patch.CardEffects
{
    public sealed class CardEffectAddTempUpgradeOnStatusThreshold : CardEffectAddCardUpgradeToUnits
    {

        public override void Setup(CardEffectState cardEffectState)
        {
            this.upgradeLifetime = (UnitUpgradeLifetime)cardEffectState.GetAdditionalParamInt1();
            StatusEffectStackData[] paramStatusEffectStackData = cardEffectState.GetParamStatusEffectStackData();
            this.statusRequirement = paramStatusEffectStackData[0];
            this.upgrade = cardEffectState.GetParamCardUpgradeData();
        }

        protected override void CollectTargetsForUpgrade(CardEffectState cardEffectState, CardEffectParams cardEffectParams, List<CharacterState> upgradeTargets, ICoreGameManagers coreGameManagers)
        {
            upgradeTargets.Clear();
            foreach (CharacterState characterState in cardEffectParams.targets)
            {
                if (this.statusRequirement != null && this.upgrade != null)
                {
                    if (characterState.GetStatusEffectStacks(this.statusRequirement.statusId) == this.statusRequirement.count)
                    {
                        upgradeTargets.Add(characterState);
                    }
                }
            }
        }

        private StatusEffectStackData? statusRequirement;
        private CardUpgradeData? upgrade;
    }
}
