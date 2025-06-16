using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TrainworksReloaded.Base;
using TrainworksReloaded.Base.Card;
using TrainworksReloaded.Base.CardUpgrade;
using TrainworksReloaded.Base.Effect;
using TrainworksReloaded.Base.Extensions;
using TrainworksReloaded.Base.Prefab;
using TrainworksReloaded.Base.Room;
using TrainworksReloaded.Core;
using TrainworksReloaded.Core.Extensions;
using TrainworksReloaded.Core.Impl;
using TrainworksReloaded.Core.Interfaces;
using UnityEngine;

namespace Unofficial_Unofficial_Balance_Patch
{
	[HarmonyPatch]
	[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
	public class Plugin : BaseUnityPlugin
	{
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
			/*
			Railend.ConfigurePostAction(c =>
			{
				if (c.GetInstance<GameDataClient>().TryGetProvider<SaveManager>(out var saveManager))
				{
					
					var upgrades = c.GetInstance<PluginAtlas>().PluginDefinitions[PluginInfo.PLUGIN_GUID].Configuration
						.GetSection("upgrades")?
						.GetChildren()
						.Select(x => x.GetSection("id").ParseString())
						.Cast<string>().ToList() ?? new List<string>();
					
					
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
			*/
			new Harmony(PluginInfo.PLUGIN_GUID + "-Harmony").PatchAll(Assembly.GetExecutingAssembly());
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
