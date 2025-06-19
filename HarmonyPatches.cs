using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Unofficial_Unofficial_Balance_Patch
{
    internal class HarmonyPatches
    {
        [HarmonyPrefix, HarmonyPatch(typeof(CardManager), "DiscardCard")]
        public static bool CardManager_DiscardCard_Prefix(CardManager __instance, CardManager.DiscardCardParams discardCardParams, bool fromNaturalPlay)
        {
            if (discardCardParams.discardCard.HasTrait(typeof(CardTraitFreeze)))
            {
                return false;
            }
            return true;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CardEffectChooseDiscard), "CardFilterFunc")]
        public static void CardEffectChooseDiscard_CardFilterFunc_Postfix(ref bool __result, CardState cardState)
        {
            if (cardState.HasTrait<CardTraitFreeze>())
            {
                __result = true;
                Debug.Log("Filtered Frozen");
            }
        }

        //[HarmonyPrefix, HarmonyPatch(typeof(CardEffectRandomDiscard), "ApplyEffect")]
        //public static bool CardEffectRandomDiscard_ApplyEffect_Prefix(CardState ___sourceCardState, CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers)
        //{
        //    CardManager cardManager = coreGameManagers.GetCardManager();
        //    AllGameData allGameData = coreGameManagers.GetAllGameData();
        //    List<CardState> hand = cardManager.GetHand(true);
        //    if (cardEffectParams.playedCard != null)
        //    {
        //        hand.Remove(cardEffectParams.playedCard);
        //    }
        //    int intInRange = cardEffectState.GetIntInRange();
        //    int num = Mathf.Max(0, hand.Count - intInRange);
        //    for (int i = 0; i < num; i++)
        //    {
        //        int index = RandomManager.Range(0, hand.Count, RngId.Battle);
        //        hand.RemoveAt(index);
        //    }
        //    float effectDelay = allGameData.GetBalanceData().GetAnimationTimingData().cardEffectDiscardAnimationDelay;
        //    CardManager.DiscardCardParams discardCardParams = new CardManager.DiscardCardParams();
        //    foreach (CardState discardCard in hand)
        //    {
        //        discardCardParams.effectDelay = effectDelay;
        //        discardCardParams.discardCard = discardCard;
        //        discardCardParams.triggeredByCard = true;

        //        discardCardParams.triggeredCard = ___sourceCardState;
        //        discardCardParams.wasPlayed = false;
        //        cardManager.DiscardCard(discardCardParams, false);
        //        effectDelay += allGameData.GetBalanceData().GetAnimationTimingData().cardEffectDiscardAnimationDelay;
        //    }
        //    Debug.Log("Custom Discard Method Used");
        //    return false;
        //}
    }
}
