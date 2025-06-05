using System.Collections;
using System.Collections.Generic;

namespace Unofficial_Unofficial_Balance_Patch.CardEffects
{
	public sealed class CardEffectAddCardUpgradeToUnitsAnywhere : CardEffectAddCardUpgradeToUnits
	{
		public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers)
		{
			if (!coreGameManagers.GetSaveManager().PreviewMode || cardEffectParams.selectedRoom == cardEffectParams.characterThatActivatedAbility?.GetCurrentRoomIndex())
				yield return base.ApplyEffect(cardEffectState, cardEffectParams, coreGameManagers, sysManagers);
		}
	}
}
