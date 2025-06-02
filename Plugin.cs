using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TrainworksReloaded.Base;
using TrainworksReloaded.Core;
using TrainworksReloaded.Core.Extensions;
using TrainworksReloaded.Core.Impl;
using UnityEngine;

namespace Unofficial_Unofficial_Balance_Patch
{

	[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
	public class Plugin : BaseUnityPlugin
	{
		private void Awake()
		{
			Railhead.GetBuilder().Configure(PluginInfo.PLUGIN_GUID, c =>
			{
				c.AddMergedJsonFile(new List<string>() { "balance_patches.json" });
			});

			Railend.ConfigurePostAction(c =>
			{
				if (c.GetInstance<GameDataClient>().TryGetProvider<SaveManager>(out var saveManager))
				{
					var allGameData = saveManager.GetAllGameData();
					CharacterData impQueenUnit = allGameData.FindCardDataByName("ImpQueenChampion").GetSpawnCharacterData();
					AccessTools.Field(typeof(CharacterData), "attackDamage")
						.SetValue(impQueenUnit, 5);
					AccessTools.Field(typeof(CharacterData), "unitAbility")
						.SetValue(impQueenUnit,	allGameData.FindCardDataByName("UUBP_ImpQueenChampionAbility".ToKey("Card")));
					List<CardUpgradeTreeData.UpgradeTree> newUpdatedTree = new List<CardUpgradeTreeData.UpgradeTree>();
					CardUpgradeTreeData impQueenChampionUpgradeTree = allGameData.FindClassData("c595c344-d323-4cf1-9ad6-41edc2aebbd0").GetUpgradeTree(1);
					foreach (var upgradeTree in impQueenChampionUpgradeTree.GetUpgradeTrees())
					{
						var newTree = new CardUpgradeTreeData.UpgradeTree();
						newUpdatedTree.Add(newTree);
						AccessTools.Field(typeof(CardUpgradeTreeData.UpgradeTree), "cardUpgrades").SetValue(upgradeTree,
							upgradeTree.GetCardUpgrades()
							.Select(x => allGameData.GetAllCardUpgradeData().LastOrDefault(y => y.name == $"UUBP_{x.name}".ToKey("Upgrade")) ?? x)
							.Cast<CardUpgradeData>().ToList());
					}
				}
			});

			new Harmony(PluginInfo.PLUGIN_GUID + "-Harmony").PatchAll(Assembly.GetExecutingAssembly());
		}
	}
}
