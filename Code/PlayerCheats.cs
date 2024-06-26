﻿using HarmonyLib;
using ModCore;
using SmallJson;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DebugMenuCheats
{
	[HarmonyPatch]
	public class PlayerCheats
	{
		private const string defaultColor = "#000000";
		private const string greenColor = "#539a39";
		private const string redColor = "#d94343";

		private List<ItemData> itemData;
		private static float moveSpeedMultiplier = 1;
		private static bool godModeToggled;
		private static bool noClipToggled;
		private static bool likeABossToggled;

		public PlayerCheats()
		{
			DebugMenuManager.AddCommands(this);
		}

		private bool TryGetPlayerEnt(out Entity player)
		{
			player = EntityTag.GetEntityByName("PlayerEnt");
			return player != null;
		}

		private void LogNoIttleMessage()
		{
			DebugMenuManager.LogToConsole("This command requires Ittle to be present in the scene.", DebugMenuManager.TextColor.Error);
		}

		[DebugMenuCommand(commandName: "god", commandAliases: ["godmode"])]
		private void ToggleGodMode(string[] args)
		{
			if (!TryGetPlayerEnt(out Entity player))
			{
				LogNoIttleMessage();
				return;
			}

			godModeToggled = !godModeToggled;

			if (godModeToggled)
			{
				// Restore health
				Killable killable = player.GetEntityComponent<Killable>();
				killable.CurrentHp = killable.MaxHp;
			}

			string output = $"GODMODE {(godModeToggled ? "engaged" : "deactivated")}!";
			DebugMenuManager.LogToConsole(output, godModeToggled ? DebugMenuManager.TextColor.Success : DebugMenuManager.TextColor.Error);
		}

		[DebugMenuCommand(commandName: "noclip")]
		private void ToggleNoClip(string[] args)
		{
			if (!TryGetPlayerEnt(out Entity player))
			{
				LogNoIttleMessage();
				return;
			}

			noClipToggled = !noClipToggled;
			DoNoClip(player);

			string output = $"Noclip {(noClipToggled ? "enabled" : "disabled")}!";
			DebugMenuManager.LogToConsole(output, noClipToggled ? DebugMenuManager.TextColor.Success : DebugMenuManager.TextColor.Error);
		}

		[DebugMenuCommand(commandName: "likeaboss")]
		private void ToggleLikeABoss(string[] args)
		{
			if (!TryGetPlayerEnt(out Entity player))
			{
				LogNoIttleMessage();
				return;
			}

			likeABossToggled = !likeABossToggled;
			string output = $"One Hit Kill {(likeABossToggled ? "enabled" : "disabled")}!";
			DebugMenuManager.LogToConsole(output, likeABossToggled ? DebugMenuManager.TextColor.Success : DebugMenuManager.TextColor.Error);
		}

		[DebugMenuCommand(commandName: "setspeed", commandAliases: ["speed"])]
		private void SetMoveSpeed(string[] args)
		{
			if (!TryGetPlayerEnt(out Entity player))
			{
				LogNoIttleMessage();
				return;
			}

			if (args.Length < 1 || !float.TryParse(args[0], out moveSpeedMultiplier))
			{
				moveSpeedMultiplier = 1;
				DebugMenuManager.LogToConsole("Must specify a number!", DebugMenuManager.TextColor.Error);
				return;
			}

			string output = $"Set Ittle's speed multiplier to {moveSpeedMultiplier}!";
			DebugMenuManager.LogToConsole(output, DebugMenuManager.TextColor.Success);
		}

		[DebugMenuCommand(commandName: "setitems", commandAliases: ["setitem", "giveitems"])]
		private void SetItems(string[] args)
		{
			if (!TryGetPlayerEnt(out Entity player))
			{
				LogNoIttleMessage();
				return;
			}

			if (itemData == null)
				ParseItemDataJson();

			string output;

			if (itemData == null || itemData.Count == 0)
			{
				output = "Something went wrong when parsing item JSON. Please report this!";
				DebugMenuManager.LogToConsole(output, DebugMenuManager.TextColor.Error);
				Plugin.Log.LogError("Failed to parse JSON data for items");
				return;
			}

			if (args.Length == 0)
			{
				output = "Command requires at least 1 argument.";
				DebugMenuManager.LogToConsole(output, DebugMenuManager.TextColor.Error);
				return;
			}

			// Args
			string itemName = args[0];
			int itemLevel;
			bool doSave = !args.Any(arg => arg == "--no-save");

			// Handle melee item names
			switch (itemName)
			{
				case "none":
					foreach (ItemData item in itemData)
						player.SetStateVariable(item.ItemName, 0);

					if (doSave)
						ModCore.Plugin.MainSaver.SaveAll();

					return;
				case "all":
					foreach (ItemData item in itemData)
					{
						switch (item.ItemName)
						{
							case "raft":
							case "shards":
							case "evilKeys":
							case "keys":
								player.SetStateVariable(item.ItemName, item.MaxLevel);
								break;
							default:
								player.SetStateVariable(item.ItemName, 3);
								break;
						}
					}

					if (doSave)
						ModCore.Plugin.MainSaver.SaveAll();

					return;
				case "dev":
					foreach (ItemData item in itemData)
						player.SetStateVariable(item.ItemName, item.MaxLevel);

					if (doSave)
						ModCore.Plugin.MainSaver.SaveAll();

					return;
				case "stick":
					itemName = "melee";
					itemLevel = 0;
					break;
				case "firesword":
				case "sword":
					itemName = "melee";
					itemLevel = 1;
					break;
				case "firemace":
				case "mace":
					itemName = "melee";
					itemLevel = 2;
					break;
				case "efcs":
					itemName = "melee";
					itemLevel = 3;
					break;
				default:
					itemLevel = args.Length > 1 && int.TryParse(args[1], out itemLevel) ? itemLevel : -1;
					break;
			}

			ItemData selectedItem = itemData.Find(x => x.ItemName.ToLower() == itemName || x.Aliases.Contains(itemName));

			if (selectedItem == null)
			{
				output = $"'{itemName}' was not a valid item name.";
				DebugMenuManager.LogToConsole(output, DebugMenuManager.TextColor.Error);
				return;
			}

			if (itemLevel < 0 || itemLevel > selectedItem.MaxLevel)
			{
				output = $"Level is required and must be an number (integer) between 0 and {selectedItem.MaxLevel}.";
				DebugMenuManager.LogToConsole(output, DebugMenuManager.TextColor.Error);
				return;
			}

			player.SetStateVariable(selectedItem.ItemName, itemLevel);
			output = "Ittle's inventory has been updated!";
			DebugMenuManager.LogToConsole(output, DebugMenuManager.TextColor.Success);

			if (doSave && ModCore.Plugin.MainSaver != null)
				ModCore.Plugin.MainSaver.SaveAll();
		}

		private void DoNoClip(Entity player)
		{
			// Disable Ittle's hitbox
			player.GetComponent<BC_ColliderAACylinderN>().IsTrigger = noClipToggled;
		}

		private void ParseItemDataJson()
		{
			if (!ModCore.Utility.TryParseJson(PluginInfo.PLUGIN_NAME, "Data", "itemData.json", out JsonObject rootObj))
				return;

			itemData = new List<ItemData>();

			foreach (JsonObject itemObj in rootObj.GetArray("items").objects.Cast<JsonObject>())
			{
				string itemName = itemObj.GetString("itemName");
				List<string> aliases = new();
				int maxLevel = itemObj.GetInt("maxLevel");

				JsonArray aliasArray = itemObj.GetArray("aliases") ?? new();
				for (int i = 0; i < aliasArray.Length; i++)
				{
					JsonValue alias = (JsonValue)aliasArray.objects[i];
					aliases.Add(alias.GetValue());
				}

				itemData.Add(new ItemData(itemName, aliases, maxLevel));
			}
		}

		#region Events

		public void OnPlayerSpawn(Entity player, GameObject camera, PlayerController controller)
		{
			DoNoClip(player);
		}

		#endregion Events

		#region Patches

		[HarmonyPrefix]
		[HarmonyPatch(typeof(EntityHittable), nameof(EntityHittable.HandleHit))]
		// If godmode is active, disable hits
		public static bool EntityHittable_HandleHit_Patch(ref HitData data, ref EntityHittable __instance)
		{
			// If Entity is not player, don't apply patch
			if (__instance.owner == null || __instance.owner.name != "PlayerEnt")
				return true;

			// If checkpoint, don't apply patch
			if (data.dmg.baseDamage.Length > 0 && data.dmg.baseDamage[0] < 0)
				return true;

			// Skip original method if godmode active
			return !godModeToggled;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Envirodeathable), nameof(Envirodeathable.SendDeath))]
		// If godmode is active, disable envirodeaths
		public static bool Envirodeathable_SendDeath_Patch(ref Envirodeathable __instance)
		{
			// If Entity is not player, don't apply patch
			if (__instance.transform.name != "PlayerEnt")
				return true;

			return !godModeToggled;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Killable), nameof(Killable.HandleHit))]
		// If likeaboss is active, Ittle kills everything in one hit
		public static bool Killable_HandleHit_Patch(ref Killable __instance)
		{
			// If Entity is not player, don't apply patch
			if (__instance.owner.name == "PlayerEnt")
				return true;

			if (likeABossToggled)
			{
				__instance.SignalDeath();
				return false;
			}

			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Entity), nameof(Entity.Move))]
		// If speed is overridden, change return value for Ittle's speed
		public static bool Entity_Move_Patch(Vector3 V, ref Entity __instance)
		{
			if (__instance.name == "PlayerEnt")
			{
				if (__instance.realBody)
				{
					__instance.realBody.SetVelocity(V * moveSpeedMultiplier);
				}
				else
				{
					if (moveSpeedMultiplier == 1)
						__instance.realTrans.position += V * Time.deltaTime;
					else
						__instance.realTrans.position += V * moveSpeedMultiplier * Time.deltaTime;
				}

				return false;
			}

			return true;
		}

		#endregion Patches

		public class ItemData
		{
			public string ItemName { get; }
			public List<string> Aliases { get; }
			public int MaxLevel { get; }

			public ItemData(string itemName, List<string> aliases, int maxLevel)
			{
				ItemName = itemName;
				Aliases = aliases ?? new();
				MaxLevel = maxLevel;
			}
		}
	}
}