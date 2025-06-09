using ShinyShoe;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Unofficial_Unofficial_Balance_Patch.CardEffects
{
	public sealed class CardEffectAbilityUserFeederRules : CardEffectBase
	{
		public override PropDescriptions CreateEditorInspectorDescriptions()
		{
			return new PropDescriptions {};
		}

		public override bool TestEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers)
		{
			CombatManager combatManager = coreGameManagers.GetCombatManager();
			return combatManager != null && combatManager.GetTurnCount() != 0;
		}

		public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers)
		{
			CombatManager combatManager = coreGameManagers.GetCombatManager();
			if (combatManager == null || combatManager.GetTurnCount() == 0 || !CombatFeederRules.GetAnyFoodUnits(cardEffectParams.targets))
			{
				yield break;
			}
			int numTimesToTrigger = 1;
			CardState parentCardState = cardEffectState.GetParentCardState();
			if (parentCardState != null)
			{
				foreach (CardTraitState item in (IEnumerable<CardTraitState>)parentCardState.GetTraitStates())
				{
					if (item is CardTraitScalingFeederRules cardTraitScalingFeederRules)
					{
						numTimesToTrigger = cardTraitScalingFeederRules.GetAdditionalTriggers(coreGameManagers.GetCardStatistics());
						break;
					}
				}
			}
			List<CharacterState> targetsCache;
			using (GenericPools.GetList(out targetsCache, cardEffectParams.targets))
			{
				List<CharacterState> roomMonsterUnits;
				using (GenericPools.GetList(out roomMonsterUnits))
				{
					yield return CombatFeederRules.RunFoodRules(coreGameManagers.GetSaveManager(), coreGameManagers.GetCombatManager(), coreGameManagers.GetStatusEffectManager(),
						targetsCache, roomMonsterUnits, numTimesToTrigger, null, cardEffectParams.selfTarget);
				}
			}
		}
	}
}
