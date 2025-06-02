using System.Reflection;

namespace Unofficial_Unofficial_Balance_Patch
{
	internal static class AccessToolsExtensions
	{
		internal static void SafeSetValue(this FieldInfo field, object? obj, object? value)
		{
			if (obj == null) return;
			field.SetValue(obj, value);
		}
	}
	internal static class StringKeyExtensions
	{
		internal static string ToKey(this string id, string type)
		{
			return PluginInfo.PLUGIN_GUID + "-" + type + "-" + id;
		}
	}
}/*,
		{
			"id": "UUBP_ShardtailGainImpss2",
			"bonus_damage": 20,
			"bonus_hp": 5
		},
		{
			"id": "UUBP_ShardtailGainImps3",
			"bonus_damage": 40,
			"bonus_hp": 5
		},
		{
			"id": "UUBP_ShardtailRallyBuff",
			"bonus_damage": 10,
			"bonus_hp": 5
		},
		{
			"id": "UUBP_ShardtailRallyBuff2",
			"bonus_damage": 15,
			"bonus_hp": 10
		},
		{
			"id": "UUBP_ShardtailRallyBuff3",
			"bonus_damage": 30,
			"bonus_hp": 20
		},
		{
			"id": "UUBP_ShardtailSacrificeImps",
			"bonus_damage": 0,
			"bonus_hp": 10
		},
		{
			"id": "UUBP_ShardtailSacrificeImps2",
			"bonus_damage": 10,
			"bonus_hp": 20
		},
		{
			"id": "UUBP_ShardtailSacrificeImps3",
			"bonus_damage": 30,
			"bonus_hp": 40
		}*/