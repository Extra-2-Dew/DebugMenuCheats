using HarmonyLib;
using ModCore;
using UnityEngine;

namespace DebugMenuCheats
{
	[HarmonyPatch]
	public class PlayerCheats
	{
		private static float moveSpeedMultiplier = 1;

		private static bool godModeToggled;
		private static bool noClipToggled;
		private static bool likeABossToggled;

		[Cheat(commandName: "god", commandAliases: ["godmode"])]
		private void ToggleGodMode(string[] args)
		{
			if (args.Length > 0 && args[0] == "help")
			{
				DebugMenuCommands.Instance.UpdateOutput("Toggles invincibility for Ittle.\nYou will not receive damage or knockback, and can't void out.");
				return;
			}

			godModeToggled = !godModeToggled;

			if (godModeToggled)
			{
				// Restore health
				Killable killable = EntityTag.GetEntityByName("PlayerEnt").GetEntityComponent<Killable>();
				killable.CurrentHp = killable.MaxHp;
			}

			DebugMenuCommands.Instance.UpdateOutput(ModCore.Utility.ColorText($"GODMODE {(godModeToggled ? "engaged" : "deactivated")}!", godModeToggled ? "#6ed948" : "#d94343"));
		}

		[Cheat(commandName: "noclip")]
		private void ToggleNoClip(string[] args)
		{
			if (args.Length > 0 && args[0] == "help")
			{
				DebugMenuCommands.Instance.UpdateOutput("Toggles Ittle's main collider to disable her hitbox,\nallowing you to walk through walls.");
				return;
			}

			noClipToggled = !noClipToggled;
			DoNoClip();

			DebugMenuCommands.Instance.UpdateOutput(ModCore.Utility.ColorText($"Noclip {(noClipToggled ? "enabled" : "disabled")}!", noClipToggled ? "#6ed948" : "#d94343"));
		}

		[Cheat(commandName: "likeaboss")]
		private void ToggleLikeABoss(string[] args)
		{
			if (args.Length > 0 && args[0] == "help")
			{
				DebugMenuCommands.Instance.UpdateOutput("Toggles one hit kill mode for Ittle.\nEverything will die in one hit, including invincibile enemies");
				return;
			}

			likeABossToggled = !likeABossToggled;
			DebugMenuCommands.Instance.UpdateOutput(ModCore.Utility.ColorText($"One Hit Kill {(likeABossToggled ? "enabled" : "disabled")}!", likeABossToggled ? "#6ed948" : "#d94343"));
		}

		[Cheat(commandName: "setspeed", commandAliases: ["speed"])]
		private void SetMoveSpeed(string[] args)
		{
			if (args.Length > 0)
			{
				if (args[0] == "help")
				{
					DebugMenuCommands.Instance.UpdateOutput("Set Ittle's speed multiplier. Requires a number");
					return;
				}

				if (args[0] == "default" || args[0] == "reset" || args[0] == "def")
				{
					moveSpeedMultiplier = 1;
					DebugMenuCommands.Instance.UpdateOutput($"Reset Ittle's speed multiplier");
					return;
				}

				if (float.TryParse(args[0], out moveSpeedMultiplier))
				{
					DebugMenuCommands.Instance.UpdateOutput(ModCore.Utility.ColorText($"Set Ittle's speed multiplier to {moveSpeedMultiplier}!", "#6ed948"));
					return;
				}

				DebugMenuCommands.Instance.UpdateOutput(ModCore.Utility.ColorText($"Noclip {(noClipToggled ? "enabled" : "disabled")}!", noClipToggled ? "#6ed948" : "#d94343"));
			}

			DebugMenuCommands.Instance.UpdateOutput(ModCore.Utility.ColorText("Must specify a number!", Color.red));
		}

		private void DoNoClip()
		{
			// Disable Ittle's hitbox
			EntityTag.GetEntityByName("PlayerEnt").GetComponent<BC_ColliderAACylinderN>().IsTrigger = noClipToggled;
		}

		#region Events

		public void OnPlayerSpawn(Entity player, UnityEngine.GameObject camera, PlayerController controller)
		{
			DoNoClip();
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