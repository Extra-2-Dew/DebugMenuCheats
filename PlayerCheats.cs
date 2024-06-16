using HarmonyLib;
using ModCore;
using UnityEngine;

namespace DebugMenuCheats
{
	[HarmonyPatch]
	public class PlayerCheats
	{
		private const string defaultColor = "#000000";
		private const string greenColor = "#539a39";
		private const string redColor = "#d94343";

		private static float moveSpeedMultiplier = 1;
		private static bool godModeToggled;
		private static bool noClipToggled;
		private static bool likeABossToggled;

		private bool TryGetPlayerEnt(out Entity player)
		{
			player = EntityTag.GetEntityByName("PlayerEnt");
			return player != null;
		}

		private void LogNoIttleMessage()
		{
			LogToConsole("This requires Ittle to be present in the scene.", redColor);
		}

		private void LogToConsole(string message, string color = defaultColor)
		{
			string output = ModCore.Utility.ColorText(message, color);
			DebugMenuManager.Instance.UpdateOutput(output);
		}

		[Cheat(commandName: "god", commandAliases: ["godmode"])]
		private void ToggleGodMode(string[] args)
		{
			if (args.Length > 0 && args[0] == "help")
			{
				LogToConsole("Toggles invincibility for Ittle.\nYou will not receive damage or knockback, and can't void out.");
				return;
			}

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

			LogToConsole($"GODMODE {(godModeToggled ? "engaged" : "deactivated")}!", godModeToggled ? greenColor : redColor);
		}

		[Cheat(commandName: "noclip")]
		private void ToggleNoClip(string[] args)
		{
			if (args.Length > 0 && args[0] == "help")
			{
				LogToConsole("Toggles Ittle's main collider to disable her hitbox,\nallowing you to walk through walls.");
				return;
			}

			if (!TryGetPlayerEnt(out Entity player))
			{
				LogNoIttleMessage();
				return;
			}

			noClipToggled = !noClipToggled;
			DoNoClip(player);

			LogToConsole($"Noclip {(noClipToggled ? "enabled" : "disabled")}!", noClipToggled ? greenColor : redColor);
		}

		[Cheat(commandName: "likeaboss")]
		private void ToggleLikeABoss(string[] args)
		{
			if (args.Length > 0 && args[0] == "help")
			{
				LogToConsole("Toggles one hit kill mode for Ittle.\nEverything will die in one hit, including invincibile enemies");
				return;
			}

			if (!TryGetPlayerEnt(out Entity player))
			{
				LogNoIttleMessage();
				return;
			}

			likeABossToggled = !likeABossToggled;
			LogToConsole($"One Hit Kill {(likeABossToggled ? "enabled" : "disabled")}!", likeABossToggled ? greenColor : redColor);
		}

		[Cheat(commandName: "setspeed", commandAliases: ["speed"])]
		private void SetMoveSpeed(string[] args)
		{
			if (args.Length < 1)
			{
				LogToConsole("Must specify a number!", redColor);
				return;
			}

			if (!TryGetPlayerEnt(out Entity player))
			{
				LogNoIttleMessage();
				return;
			}

			if (args[0] == "help")
			{
				LogToConsole("Set Ittle's speed multiplier. Requires a number");
				return;
			}

			if (args[0] == "default" || args[0] == "reset" || args[0] == "def")
			{
				moveSpeedMultiplier = 1;
				LogToConsole($"Reset Ittle's speed multiplier");
				return;
			}

			if (float.TryParse(args[0], out moveSpeedMultiplier))
			{
				LogToConsole($"Set Ittle's speed multiplier to {moveSpeedMultiplier}!", greenColor);
				return;
			}
		}

		private void DoNoClip(Entity player)
		{
			// Disable Ittle's hitbox
			player.GetComponent<BC_ColliderAACylinderN>().IsTrigger = noClipToggled;
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
					__instance.realTrans.position += V * moveSpeedMultiplier * Time.deltaTime;

				return false;
			}

			return true;
		}

		#endregion Patches
	}
}