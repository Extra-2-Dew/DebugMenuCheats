using HarmonyLib;
using ModCore;

namespace DebugMenuCheats
{
	[HarmonyPatch]
	public class PlayerCheats
	{
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

			if (noClipToggled)
			{
				Events.OnPlayerSpawn += OnPlayerSpawn;
			}
			else
				Events.OnPlayerSpawn -= OnPlayerSpawn;

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

		private void DoNoClip()
		{
			// Disable Ittle's hitbox
			EntityTag.GetEntityByName("PlayerEnt").GetComponent<BC_ColliderAACylinderN>().IsTrigger = noClipToggled;
		}

		#region Events

		private void OnPlayerSpawn(Entity player, UnityEngine.GameObject camera, PlayerController controller)
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
			if (__instance.owner.name != "PlayerEnt")
				return true;

			// If checkpoint, don't apply patch
			if (data.dmg.baseDamage[0] < 0)
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

		#endregion Patches
	}
}