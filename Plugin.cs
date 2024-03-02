using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using ModCore;
using System;
using System.Linq;
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
		internal static DebugMenuCommands DMC { get { return DebugMenuCommands.Instance; } }

		private void Awake()
		{
			instance = this;
			Log = Logger;
			Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

			DebugMenuCommands.OnDebugMenuInitialized += AddCommands;
			AddEventHooks();

			Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
		}

		// Adds cheat commands to debug menu
		private void AddCommands()
		{
			Type[] types = Assembly.GetExecutingAssembly().GetTypes()
				.Where(type => string.Equals(type.Namespace, GetType().Namespace, StringComparison.Ordinal))
				.ToArray();

			foreach (Type type in types)
			{
				MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
				object instance = null;

				foreach (MethodInfo method in methods)
				{
					object[] attributes = method.GetCustomAttributes(typeof(CheatAttribute), true);

					if (attributes.Length > 0)
					{
						if (instance == null)
							instance = Activator.CreateInstance(type);

						CheatAttribute cheat = (CheatAttribute)attributes[0];
						DebugMenuCommands.CommandFunc commandDelegate = args => method.Invoke(instance, [args]);
						DebugMenuCommands.Instance.AddCommand(cheat.CommandName, commandDelegate, cheat.CommandAliases);
					}
				}
			}
		}

		// Adds event subscriptions
		private void AddEventHooks()
		{
			Events.OnPlayerSpawn += playerCheats.OnPlayerSpawn;
		}
	}
}