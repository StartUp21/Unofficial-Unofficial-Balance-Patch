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
}