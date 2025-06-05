using ShinyShoe;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Unofficial_Unofficial_Balance_Patch.CardEffects
{
	public sealed class CardEffectTransferStatsFromUnit : CardEffectBase
	{
		public override PropDescriptions CreateEditorInspectorDescriptions()
		{
			return new PropDescriptions
			{
			};
		}
		public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers)
		{
			if (cardEffectParams.targets.Count == 0)
			{
				yield break;
			}
			CharacterState characterState = cardEffectParams.targets[0];
			Team.Type teamType = characterState.GetTeamType();
			RoomState roomState = characterState.GetCurrentRoom();
			if (roomState == null)
			{
				roomState = cardEffectParams.GetSelectedRoom(coreGameManagers.GetRoomManager());
			}
			if (roomState == null)
			{
				yield break;
			}
			List<CharacterState> list;
			using (GenericPools.GetList(out list))
			{
				CharacterState lastAbilityActivatorCharacter = coreGameManagers.GetCombatManager().GetLastAbilityActivatorCharacter();
				if (!(lastAbilityActivatorCharacter == null) && !lastAbilityActivatorCharacter.HasStatusEffect("equalizer"))
				{
					yield return AddStatToTarget(lastAbilityActivatorCharacter, characterState.GetAttackDamage(), characterState.GetMaxHP(), 
						lastAbilityActivatorCharacter.GetCurrentRoom() == characterState.GetCurrentRoom(), coreGameManagers);
				}
			}
		}
		private IEnumerator AddStatToTarget(CharacterState target, int attack, int health, bool display_effect, ICoreGameManagers coreGameManagers)
		{
			if (attack > 0)
			{
				if (!coreGameManagers.GetSaveManager().PreviewMode || display_effect)
				{
					CardUpgradeState cardUpgradeState = new CardUpgradeState();
					cardUpgradeState.Setup();
					cardUpgradeState.SetAttackDamage(attack);
					cardUpgradeState.SetAdditionalHP(health);
					yield return target.ApplyCardUpgrade(cardUpgradeState);
				}
				if (!coreGameManagers.GetSaveManager().PreviewMode && !target.HasStatusEffect("cardless"))
				{
					CardUpgradeState cardUpgradeState2 = new CardUpgradeState();
					cardUpgradeState2.Setup();
					cardUpgradeState2.SetAttackDamage(attack);
					cardUpgradeState2.SetAdditionalHP(health);
					CardEffectAddStatsFromSelf.ApplyCardUpgradeToTarget(target, cardUpgradeState2, coreGameManagers.GetCardManager());
				}
			}
		}
	}
}
