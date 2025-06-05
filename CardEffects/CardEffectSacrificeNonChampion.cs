using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unofficial_Unofficial_Balance_Patch.CardEffects
{
	[HarmonyPatch]
	public sealed class CardEffectSacrificeNonChampion : CardEffectBase
	{
		internal const string CHAMPION_KEY = "SubtypesData_Champion_83f21cbe-9d9b-4566-a2c3-ca559ab8ff34";
		[HarmonyPostfix, HarmonyPatch(typeof(CardEffectBase), "IsTargetValid")]
		private static bool IsValidTarget(bool __result, CardEffectBase __instance, CharacterState target, bool doSubtypeCheck)
		{
			if (!(__instance is CardEffectSacrificeNonChampion)) return __result;
			return __result && !target.GetCharacterManager().DoesCharacterPassSubtypeCheck(target, SubtypeManager.GetSubtypeData(CHAMPION_KEY));
		}
		[HarmonyPostfix, HarmonyPatch(typeof(TargetHelper), "PruneEffectTargets")]
		private static void PruneEffectTargets(List<CardEffectState> effects, List<CharacterState> targets)
		{
			if (effects.Any(x => x?.GetCardEffect() is CardEffectSacrificeNonChampion))
				targets.RemoveAll(x => x.GetCharacterManager().DoesCharacterPassSubtypeCheck(x, SubtypeManager.GetSubtypeData(CHAMPION_KEY)));
		}
		private CardEffectState cachedState;
		public override bool CanRandomizeTargetMode => false;
		public override PropDescriptions CreateEditorInspectorDescriptions()
		{
			return new PropDescriptions
			{
				[CardEffectFieldNames.ParamStr.GetFieldName()] = new PropDescription("Targets Status Effect Id", "For target mode FrontWithStatus, gets the front target with this status effect."),
				[CardEffectFieldNames.ParamBool.GetFieldName()] = new PropDescription("Do Not Fully Remove", "Whether to immediately remove the dead unit from the room.")
			};
		}
		public override void Setup(CardEffectState cardEffectState)
		{
			cachedState = cardEffectState;
			base.Setup(cardEffectState);
		}
		private void RecollectTargets(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers)
		{
			if (cardEffectState.GetTargetMode() != TargetMode.RandomInRoom)
				return;
			SaveManager saveManager = coreGameManagers.GetSaveManager();
			TargetHelper.CollectTargets(cardEffectState, cardEffectParams, coreGameManagers, saveManager.PreviewMode);
			cardEffectParams.targets.RemoveAll(x => x.GetCharacterManager().DoesCharacterPassSubtypeCheck(x, SubtypeManager.GetSubtypeData(CHAMPION_KEY)));
			if (cardEffectParams.targets.Count > 0)
			{
				RngId rngId = (saveManager.PreviewMode ? RngId.BattleTest : RngId.Battle);
				CharacterState item = cardEffectParams.targets[RandomManager.Range(0, cardEffectParams.targets.Count, rngId)];
				cardEffectParams.targets.Clear();
				cardEffectParams.targets.Add(item);
			}
		}
		public override bool TestEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers)
		{
			RecollectTargets(cardEffectState, cardEffectParams, coreGameManagers);
			return cardEffectParams.targets.Count > 0;
		}
		public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers)
		{
			RecollectTargets(cardEffectState, cardEffectParams, coreGameManagers);
			CharacterState triggeredCharacter = null;
			if (cardEffectParams.playedCard != null && cardEffectParams.playedCard.IsUnitAbility() && cardEffectParams.characterThatActivatedAbility != null)
			{
				triggeredCharacter = cardEffectParams.characterThatActivatedAbility;
				if (cardEffectParams.targets.Contains(triggeredCharacter))
				{
					yield return coreGameManagers.GetCombatManager().QueueAndRunTrigger(triggeredCharacter, CharacterTriggerData.Trigger.OnOwnAbilityActivated);
				}
			}
			foreach (CharacterState target in cardEffectParams.targets)
			{
				bool paramBool = cardEffectState.GetParamBool();
				yield return target.Sacrifice(cardEffectParams.playedCard, ignoreTriggers: false, paramBool, triggeredCharacter);
			}
		}
		public string GetTooltipBaseKey(CardEffectState cardEffectState)
		{
			return "CardEffectSacrifice";
		}
		public override string GetDescriptionAsTrait(CardEffectState cardEffectState)
		{
			return ("CardTraitData_descriptionKey-" + "UUBP_SacrificeNonChampionAnywhere".ToKey("Trait")).Localize();
		}
		public int GetDynamicStrength(CardEffectState cardEffectState, RelicManager relicManager)
		{
			return 0;
		}
	}
}
