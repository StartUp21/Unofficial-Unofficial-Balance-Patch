using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace Unofficial_Unofficial_Balance_Patch
{
    internal class HarmonyPatches
    {
        [HarmonyPatch(typeof(CardManager), nameof(CardManager.DiscardCard))]
        public static bool CardManager_DiscardCard_Prefix(CardManager __instance, CardManager.DiscardCardParams discardCardParams, bool fromNaturalPlay = false)
        {
            if (discardCardParams.discardCard.HasTrait(typeof(CardTraitFreeze)))
            {
                return false;
            }
            return true;
        }
    }
}
