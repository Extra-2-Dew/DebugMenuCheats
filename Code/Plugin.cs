using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using ModCore;
using System.Reflection;

namespace DebugMenuCheats
{
	[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
	[BepInDependency("ModCore")]
	public class Plugin : BaseUnityPlugin
	{
		private static Plugin instance;

		internal static Plugin Instance { get { return instance; } }
		internal static ManualLogSource Log { get; private set; }

		private void Awake()
		{
			instance = this;
			Log = Logger;
			Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

			DebugMenuManager.Instance.OnDebugMenuInitialized += () =>
			{
				PlayerCheats playerCheats = new();
				new GotoCommand();
				Events.OnPlayerSpawn += playerCheats.OnPlayerSpawn;
				Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
			};
		}
	}
}