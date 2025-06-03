using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using TrainworksReloaded.Base;
using TrainworksReloaded.Base.CardUpgrade;
using TrainworksReloaded.Core;
using TrainworksReloaded.Core.Extensions;
using TrainworksReloaded.Core.Impl;
using TrainworksReloaded.Core.Interfaces;

namespace Unofficial_Unofficial_Balance_Patch
{

	[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
	public class Plugin : BaseUnityPlugin
	{
		// private readonly Dictionary<string, FileInfo> _jsonFiles = new Dictionary<string, FileInfo>();
		private void Awake()
		{
			var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			// var files = GetJsonFilesInDirectory(new DirectoryInfo(Path.Combine(Path.GetDirectoryName(assembly), "balance_patches")));
			// files.Do(x => _jsonFiles.Add(x.Directory.Name, x));
			Railhead.GetBuilder().Configure(PluginInfo.PLUGIN_GUID, c =>
			{
				c.AddMergedJsonFile(GetJsonFilesInDirectory(new DirectoryInfo(Path.Combine(assemblyFolder, "balance_patches")))
					.Select(x => Path.GetRelativePath(assemblyFolder, x.FullName)).ToList());
			});
			Railend.ConfigurePostAction(c =>
			{
				if (c.GetInstance<GameDataClient>().TryGetProvider<SaveManager>(out var saveManager))
				{
					/*
					var upgrades = c.GetInstance<PluginAtlas>().PluginDefinitions[PluginInfo.PLUGIN_GUID].Configuration
						.GetSection("upgrades")?
						.GetChildren()
						.Select(x => x.GetSection("id").ParseString())
						.Cast<string>().ToList() ?? new List<string>();
					*/
					var allGameData = saveManager.GetAllGameData();
					foreach (var upgradeTree in allGameData.GetAllClassDatas()
						.SelectMany(x => (List<ChampionData>)AccessTools.Field(typeof(ClassData), "champions").GetValue(x))
						.SelectMany(x => x.upgradeTree.GetUpgradeTrees()))
					{
						var list = upgradeTree.GetCardUpgrades();
						for (int i = 0; i < list.Count; i++) {
							CardUpgradeData? upgrade = list[i];
							if (upgrade == null) continue;
							if (c.GetInstance<IRegister<CardUpgradeData>>().TryGetValue($"UUBP_{upgrade.name}".ToKey("Upgrade"), out var data))
							{
								list[i] = data;
							}
						}
					}
				}
			});
			new Harmony(PluginInfo.PLUGIN_GUID + "-Harmony").PatchAll(Assembly.GetExecutingAssembly());
			/*
			Railend.ConfigurePostAction(c =>
			{
				if (c.GetInstance<GameDataClient>().TryGetProvider<SaveManager>(out var saveManager))
				{
					var allGameData = saveManager.GetAllGameData();
					CharacterData impQueenUnit = allGameData.FindCardDataByName("ImpQueenChampion").GetSpawnCharacterData();
					// AccessTools.Field(typeof(CharacterData), "attackDamage")
					// 	.SetValue(impQueenUnit, 5);
					// AccessTools.Field(typeof(CharacterData), "unitAbility")
					// 	.SetValue(impQueenUnit,	allGameData.FindCardDataByName("UUBP_ImpQueenChampionAbility".ToKey("Card")));
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
			*/
		}
		private IEnumerable<FileInfo> GetJsonFilesInDirectory(DirectoryInfo directory)
		{
			if (!directory.Exists) yield break;
			foreach (var file in directory.GetFiles().Where(x => x.Extension == ".json"))
				yield return file;
			foreach (var file in directory.GetDirectories().SelectMany(GetJsonFilesInDirectory))
				yield return file;
		}
	}
}
