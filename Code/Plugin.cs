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
		private PlayerCheats playerCheats = new();

		internal static Plugin Instance { get { return instance; } }
		internal static ManualLogSource Log { get; private set; }
		internal static DebugMenuManager DMM { get { return DebugMenuManager.Instance; } }

		private void Awake()
		{
			instance = this;
			Log = Logger;
			Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

			DebugMenuManager.AddCommands();
			AddEventHooks();

			Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
		}

		// Adds event subscriptions
		private void AddEventHooks()
		{
			Events.OnPlayerSpawn += playerCheats.OnPlayerSpawn;
		}
	}
}