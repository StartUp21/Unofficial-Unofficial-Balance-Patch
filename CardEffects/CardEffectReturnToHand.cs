using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using static HandUI;

namespace Unofficial_Unofficial_Balance_Patch.CardEffects
{
	public class CardEffectReturnToHand : CardEffectBase
	{

		public override bool CanPlayAfterBossDead => true;
		public override bool CanApplyInPreviewMode => false;
		public override bool CanPlayWhenHandFull => true;
		public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers)
		{
			var cardManager = coreGameManagers.GetCardManager();
			Traverse _cardManager = Traverse.Create(cardManager);
			var standByPile = _cardManager.Field<Dictionary<CardState, RemoveFromStandByCondition>>("pileStandBy").Value;
			var standbyCardsBeingProcessed = _cardManager.Field<List<CardState>>("standbyCardsBeingProcessed").Value;
			var handUI = _cardManager.Field<HandUI>("handUI").Value;
			var character = cardEffectParams.selfTarget;
			var card = character.GetSpawnerCard();
			standbyCardsBeingProcessed.Add(card);
			card.ClearRemoveFromStandByPileOverride();
			coreGameManagers.GetCardManager().GetHand().Insert(0, card);
			handUI.DrawCard(card, 0f,	instantly: false, skipDrawAnimation: false, DrawSource.Deck);
			yield return standByPile[card].OnReturned(card, standByPile[card].GetReturnLocation(), character);
			standByPile.Remove(card);
			standbyCardsBeingProcessed.Remove(card);
		}
		public override PropDescriptions CreateEditorInspectorDescriptions()
		{
			return new PropDescriptions
			{
			};
		}
	}
}
