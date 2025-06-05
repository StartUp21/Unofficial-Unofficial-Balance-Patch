using HarmonyLib;
using ShinyShoe;
using ShinyShoe.Loading;
using ShinyShoe.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unofficial_Unofficial_Balance_Patch.CardEffects
{
	[HarmonyPatch]
	public sealed class CardEffectDraftBattleCard : CardEffectBase
	{
		private static readonly List<Action<GrantResult>> callbacks = new List<Action<GrantResult>>();
		[HarmonyPrefix, HarmonyPatch(typeof(CardDraftScreen), "ApplyDraft")]
		private static bool DoNotAddToDeck(bool __runOriginal, CardDraftScreen __instance, IDraftableUI draftedItem)
		{
			if (!__runOriginal) return false;
			var @this = new Traverse(__instance);
			var cardChosenCallback_f = @this.Field("cardChosenCallback");
			var cardChosenCallback = cardChosenCallback_f.GetValue<Action<GrantResult>>();
			if (!callbacks.Contains(cardChosenCallback)) return true;
			callbacks.Remove(cardChosenCallback);
			(draftedItem as CardUI).SetEdgeFxActive(CardUI.EdgeHighlight.Off);
			// base_ApplyDraft(__instance, draftedItem);
			CardState cardState = (draftedItem as CardUI).GetCardState();
			var currentExtraCopies = @this.Field<int>("currentExtraCopies").Value;
			var saveManager = @this.Field<SaveManager>("saveManager").Value;
			if (saveManager)
			{
				CardData cardData = saveManager.GetAllGameData().FindCardData(cardState.GetCardDataID());
				@this.Method("ReplayRecordDraftChoice", draftedItem, cardData).GetValue(draftedItem, cardData);
			}
			cardChosenCallback(new GrantResult(cardState.GetCardDataID(), cardState.GetAssetName(), "CardDraft"));
			cardChosenCallback_f.SetValue(null);
			return false;
		}
		private bool _canPlayWhenHandFull = true;
		public override bool CanPlayAfterBossDead => false;
		public override bool CanApplyInPreviewMode => false;
		public override bool CanPlayWhenHandFull => _canPlayWhenHandFull;
		public override void Setup(CardEffectState cardEffectState)
		{
			base.Setup(cardEffectState);
			_canPlayWhenHandFull = !cardEffectState.GetParamBool();
		}
		public override PropDescriptions CreateEditorInspectorDescriptions()
		{
			return new PropDescriptions
			{
				[CardEffectFieldNames.ParamInt.GetFieldName()] = new PropDescription("Target Card Pile", "", typeof(CardPile)),
				[CardEffectFieldNames.AdditionalParamInt.GetFieldName()] = new PropDescription("Num Drafts"),
				[CardEffectFieldNames.AdditionalParamInt1.GetFieldName()] = new PropDescription("Num Choices"),
				[CardEffectFieldNames.ParamCardUpgradeData.GetFieldName()] = new PropDescription("Optional Upgrade"),
				[CardEffectFieldNames.FilterBasedOnMainSubClass.GetFieldName()] = new PropDescription("Filter Based On Main Subclass"),
				[CardEffectFieldNames.CopyModifiersFromSource.GetFieldName()] = new PropDescription("Copy Modifiers From Source"),
				[CardEffectFieldNames.ParamCardPool.GetFieldName()] = new PropDescription("Card Pool"),
				[CardEffectFieldNames.ParamCardFilter.GetFieldName()] = new PropDescription("Card Filter"),
				[CardEffectFieldNames.ParamBool.GetFieldName()] = new PropDescription("Require Space In Hand", "If enabled, will check if the player's hand is full")
			};
		}
		public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers)
		{
			/*
			 * ParamInt:							cardPile
			 * AdditionalParamInt:		numDrafts
			 * AdditionalParamInt1:		numChoices
			 * 
			 * 
			 */
			if (coreGameManagers.GetSaveManager().PreviewMode) yield break;
			var cardPile = (CardPile)cardEffectState.GetParamInt();
			int numChoices = cardEffectState.GetAdditionalParamInt1();
			var cardManager = coreGameManagers.GetCardManager();
			int numDrafts = Mathf.Max(1, cardEffectState.GetAdditionalParamInt());
			int numDraftsEffective = Mathf.Min(
				numDrafts,
				cardManager.GetMaxHandSize() - cardManager.GetNumCardsInHand());
			for (int d = 0; d < numDrafts; d++)
			{
				if (coreGameManagers.GetCardManager().GetBelowHandSize())
				{
					yield return ShowCardDraft(numDraftsEffective, d, numChoices, cardPile, cardEffectState, cardEffectParams, coreGameManagers, sysManagers);
				}
				else
				{
					coreGameManagers.GetCardManager().ShowCardsDrawnErrorMessage(CommonSelectionBehavior.SelectionError.HandFull);
				}
			}
		}
		private IEnumerator ShowCardDraft(int draftCount, int currentDraft, int numChoices, CardPile cardPile,
			CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers)
		{
			var saveManager = coreGameManagers.GetSaveManager();
			yield return CoreUtil.WaitForSecondsOrBreak(saveManager.GetActiveTiming().PostBattlePause);
			using (GenericPools.GetList(out List<CardData> choicePool))
			{
				if (!cardEffectState.GetFilteredCardListFromPool(coreGameManagers.GetRelicManager(), ref choicePool))
					yield break;
				if (cardEffectState.GetFilterBasedOnMainSubClass())
				{
					var mainClass = saveManager.GetMainClass();
					var subClass = saveManager.GetSubClass();
					choicePool.RemoveAll(x => x.GetLinkedClass() != null && x.GetLinkedClass() != mainClass && x.GetLinkedClass() != subClass);
				}
				using (GenericPools.GetList(out List<CardData> choices))
				{
					while (choicePool.Count > 0 && choices.Count < numChoices)
					{
						var choice = choicePool.RandomElement(RngId.Battle);
						while (choicePool.Remove(choice)) { }
						choices.Add(choice);
					}
					GrantResult? draftResult = null;
					var screenManager = sysManagers.GetScreenManager();
					var cardManager = coreGameManagers.GetCardManager();
					LoadingScreen.AddTask(new LoadAdditionalCards(choices, false, LoadingScreen.DisplayStyle.Spinner, () =>
					{
						screenManager.ShowScreen(ScreenName.Draft, s =>
						{
							var screen = s as CardDraftScreen;
							Action<GrantResult> callback = res =>
							{
								draftResult = res;
								screen.Close();
							};
							callbacks.Add(callback);
							screen?.Setup(choices, title: "Mokobun", setExtraCopies: 0, setCanSkip: false, setFromRewardNode: false, cardManager, callback);
						});
					}));
					yield return new WaitUntil(() => draftResult != null);
					var cardData = choices.FirstOrDefault(x => x.GetID() == draftResult?.itemAnalyticsId);
					if (cardData == null) yield break;
					AddCard(cardData, cardPile, draftCount, currentDraft, cardEffectState, cardEffectParams, coreGameManagers);
				}
			}
			yield return CoreUtil.WaitForSecondsOrBreak(1 - saveManager.GetActiveTiming().PostBattlePause);
		}

		private void AddCard(CardData cardData, CardPile cardPile, int draftCount, int currentDraft,
			CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers managers)
		{
			int current = 0;
			int max = 0;
			var selectedRoom = cardEffectParams.GetSelectedRoom(managers.GetRoomManager());
			var selfTarget = cardEffectParams.selfTarget;
			using (GenericPools.GetList(out List<CharacterState> characters))
			{
				if (selfTarget != null)
				{
					selectedRoom?.AddCharactersToList(characters, selfTarget.GetTeamType());
				}
				else
				{
					current = currentDraft;
					max = draftCount;
				}
				for (int j = 0; j < characters.Count; j++)
				{
					CharacterState characterState = characters[j];
					foreach (CharacterTriggerState trigger in characterState.GetTriggers())
					{
						List<CardEffectState> effects = trigger.GetEffects();
						bool flag = false;
						foreach (CardEffectState item in effects)
						{
							if (item.GetCardEffect() is CardEffectAddBattleCard)
							{
								if (selfTarget == characterState)
								{
									current = max;
								}
								flag = true;
								max++;
								break;
							}
						}
						if (flag)
						{
							break;
						}
					}
				}
				if (current == 0 && max == 0)
				{
					current = currentDraft;
					max = draftCount;
				}
				var upgrades = new CardManager.AddCardUpgradingInfo();
				if (cardEffectState.GetParamCardUpgradeData() != null)
					upgrades.upgradeDatas.Add(cardEffectState.GetParamCardUpgradeData());
				upgrades.tempCardUpgrade = true;
				upgrades.upgradingCardSource = cardEffectState.GetParentCardState();
				if (cardEffectState.GetCopyModifiersFromSource())
				{
					upgrades.ignoreTempUpgrades = cardEffectState.GetIgnoreTempModifiersFromSource();
					upgrades.copyModifiersFromCard = cardEffectParams.playedCard;
					if (cardEffectParams.selfTarget != null && cardEffectParams.selfTarget.GetSpawnerCard() != null)
						upgrades.copyModifiersFromCard = cardEffectParams.selfTarget.GetSpawnerCard();
				}
				managers.GetCardManager().AddCard(cardData, cardPile, current, max, fromRelic: false, permanent: false, upgrades);
			}
		}

	}
}
