using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Unofficial_Unofficial_Balance_Patch.CardEffects
{

	public sealed class CardEffectAddTempUpgradeToCurrentCard : CardEffectBase
	{
		public override bool CanPlayAfterBossDead => false;
		public override bool CanApplyInPreviewMode => false;
		public override PropDescriptions CreateEditorInspectorDescriptions()
		{
			return new PropDescriptions { [CardEffectFieldNames.ParamCardUpgradeData.GetFieldName()] = new PropDescription("Card Upgrade") };
		}
		public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers)
		{
			CardState playedCard = cardEffectParams.playedCard;
			if (playedCard == null)
			{
				yield break;
			}
			CardManager cardManager = coreGameManagers.GetCardManager();
			if (CardUpgradeHelper.TryApplyUpgradeToCard(playedCard, cardEffectState.GetParamCardUpgradeData(), isTemporaryUpgrade: true, cardEffectParams.isFromHiddenTrigger, coreGameManagers, out var _))
			{
				playedCard.UpdateCardBodyText();
				cardManager.RefreshCardInHand(playedCard, cleanupTweens: false);
				CardUI cardInHand = cardManager.GetCardInHand(playedCard);
				if (cardInHand != null)
				{
					cardInHand?.ShowEnhanceFX();
				}
			}
		}
		public override void GetTooltipsStatusList(CardEffectState cardEffectState, ref List<string> outStatusIdList)
		{
			GetTooltipsStatusList(cardEffectState.GetSourceCardEffectData(), ref outStatusIdList);
		}
		public static void GetTooltipsStatusList(CardEffectData cardEffectData, ref List<string> outStatusIdList)
		{
			foreach (StatusEffectStackData statusEffectUpgrade in cardEffectData.GetParamCardUpgradeData().GetStatusEffectUpgrades())
			{
				outStatusIdList.Add(statusEffectUpgrade.statusId);
			}
		}
	}

}
