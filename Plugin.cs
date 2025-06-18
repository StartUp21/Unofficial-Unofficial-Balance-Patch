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
			Railhead.GetBuilder().Configure(PluginInfo.PLUGIN_GUID, c =>
			{
				c.AddMergedJsonFile(GetJsonFilesInDirectory(new DirectoryInfo(Path.Combine(assemblyFolder, "balance_patches")))
				.Select(x => Path.GetRelativePath(assemblyFolder, x.FullName)).ToList());
			});
			Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID + "-Harmony");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
			try
			{
                harmony.Patch(original: typeof(CardManager).GetMethod("DiscardCard", AccessTools.all),
                        prefix: new HarmonyMethod(typeof(HarmonyPatches).GetMethod("CardManager_DiscardCard_Prefix")));
            }
            catch (Exception ex)
            {
                Debug.Log(ex + "Bingaze");
            }
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
