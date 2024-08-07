﻿using BepInEx;
using ModCore;
using SmallJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DebugMenuCheats
{
	public class GotoCommand
	{
		private List<SceneData> allSceneData;
		private FadeEffectData sceneFadeData;
		private FadeEffectData roomFadeData;
		private const string dataFileName = "gotoData.json";
		private GotoData gotoData;

		public GotoCommand()
		{
			DebugMenuManager.AddCommands(this);
		}

		[DebugMenuCommand(commandName: "goto")]
		private void Goto(string[] args)
		{
			gotoData = new();
			string output;

			// If no args given
			if (args == null || args.Length < 1)
			{
				output = "Must specify a scene name to be your destination!";
				DebugMenuManager.LogToConsole(output, DebugMenuManager.TextColor.Error);
				return;
			}

			// Create scene data first time
			if (allSceneData == null)
			{
				// If failed to parson JSON
				if (!TryParseJson(out JsonObject rootObj))
				{
					output = "Something went wrong with the parsing of the goto data JSON! Please report this!";
					DebugMenuManager.LogToConsole(output, DebugMenuManager.TextColor.Error);
					Plugin.Log.LogError("Goto command failed to parse its JSON data file. Does it exist?");
					return;
				}

				allSceneData = CreateSceneData(rootObj);
			}

			string sceneArg = args[0];

			// If scene is invalid
			if (!TryValidateScene(sceneArg, allSceneData, out SceneData sceneData))
			{
				output = $"There is no scene with the name '{sceneArg}'. A typo perhaps?";
				DebugMenuManager.LogToConsole(output, DebugMenuManager.TextColor.Error);
				return;
			}

			string spawnOrRoomArg = args.Length > 1 ? args[1] : string.Empty;
			gotoData.sceneName = sceneData.SceneName;
			gotoData.spawnName = sceneData.SpawnNames.Count > 0 ? sceneData.SpawnNames[0] : string.Empty; // Default to first spawn in list
			gotoData.doSave = sceneData.DoSave;

			// Create FadeEffectData
			if (sceneFadeData == null)
			{
				sceneFadeData = new()
				{
					_targetColor = Color.black,
					_fadeOutTime = 0.5f,
					_fadeInTime = 1.25f,
					_faderName = "ScreenCircleWipe",
					_useScreenPos = true
				};
			}

			// If no spawn given, load scene
			if (string.IsNullOrEmpty(spawnOrRoomArg))
			{
				LoadScene();
				return;
			}

			// If spawn given
			if (!int.TryParse(spawnOrRoomArg, out int spawnOrRoomIndex))
			{
				gotoData.spawnName = sceneData.SpawnNames.FirstOrDefault(x => x.ToLower() == spawnOrRoomArg.ToLower());

				if (gotoData.spawnName == null)
					gotoData.spawnName = "";

				// If no spawn found, check if it's a room
				if (string.IsNullOrEmpty(gotoData.spawnName))
				{
					SceneData.RoomData room = sceneData.Rooms.FirstOrDefault(x => x.RoomName.ToLower() == spawnOrRoomArg.ToLower());
					gotoData.roomName = room != null ? room.RoomName : string.Empty;
					gotoData.doorData = room?.Doors[0];

					// If room is valid
					if (!string.IsNullOrEmpty(gotoData.roomName))
					{
						// If door index given
						if (args.Length > 2)
						{
							if (int.TryParse(args[2], out int doorIndex))
							{
								if (doorIndex < room.Doors.Count)
									gotoData.doorData = room.Doors[doorIndex];
								else
								{
									output = $"Door index {args[2]} is out of range for room {gotoData.roomName}.\nThere are {room.Doors.Count} doors in this room.";
									DebugMenuManager.LogToConsole(output, DebugMenuManager.TextColor.Error);
									return;
								}
							}
						}
					}
				}

				// If no spawn or room found, it's invalid
				if (string.IsNullOrEmpty(gotoData.spawnName) && string.IsNullOrEmpty(gotoData.roomName))
				{
					output = $"Spawn or room '{spawnOrRoomArg}' does not exist on the scene '{sceneData.SceneName}'.\nA typo perhaps?";
					DebugMenuManager.LogToConsole(output, DebugMenuManager.TextColor.Error);
					return;
				}
			}
			// If index given
			else if (spawnOrRoomIndex < sceneData.SpawnNames.Count)
				gotoData.spawnName = sceneData.SpawnNames[spawnOrRoomIndex];
			// If index is out of bounds
			else if (spawnOrRoomIndex >= sceneData.SpawnNames.Count)
			{
				output = $"Spawn index {spawnOrRoomIndex} was out of range for scene '{sceneData.SceneName}'\nThere are {sceneData.SpawnNames.Count} spawns for this scene.";
				DebugMenuManager.LogToConsole(output, DebugMenuManager.TextColor.Error);
				return;
			}

			// If going to spawn, change scenes
			if (!string.IsNullOrEmpty(gotoData.spawnName))
				LoadScene();
			// If going to room, change rooms
			else if (!string.IsNullOrEmpty(gotoData.roomName))
			{
				// If changing scenes, change scenes first, then do room
				if (SceneManager.GetActiveScene().name != gotoData.sceneName)
				{
					LoadScene();
					Events.OnPlayerSpawn += OnPlayerSpawned;
				}
				else
					SwitchRooms();
			}
		}

		private void LoadScene(float fadeInTime = 1.25f)
		{
			DebugMenuManager.LogToConsole($"Warping to scene \"{gotoData.sceneName}\"", DebugMenuManager.TextColor.Success);
			DebugMenuManager.Instance.ToggleMenuVisibility();
			sceneFadeData._fadeInTime = fadeInTime;
			SceneDoor.StartLoad(gotoData.sceneName, gotoData.spawnName, sceneFadeData, gotoData.doSave ? ModCore.Plugin.MainSaver : null);
			sceneFadeData._fadeInTime = 1.25f;
		}

		private void SwitchRooms(bool doFade = true)
		{
			// Setup room transition data
			Entity player = EntityTag.GetEntityByName("PlayerEnt");
			LevelRoom fromRoom = LevelRoom.GetRoomForPosition(player.transform.position);
			LevelRoom toRoom = LevelRoom.currentRooms.Find(x => x.RoomName == gotoData.roomName);
			Vector3 position = gotoData.doorData.SpawnPosition;
			DebugMenuManager.LogToConsole($"Warping to room {toRoom.gameObject.name}", DebugMenuManager.TextColor.Success);

			// Hide Debug Menu
			DebugMenuManager.Instance.ToggleMenuVisibility();

			if (roomFadeData == null)
			{
				roomFadeData = new FadeEffectData()
				{
					_targetColor = Color.black,
					_fadeOutTime = 0.5f,
					_fadeInTime = 1.25f,
					_faderName = "ScreenFade",
					_useScreenPos = false
				};
			}

			// Start room transition
			player.GetEntityComponent<RoomSwitchable>().StartWarpTransition(position, position, fromRoom, toRoom, Vector3.zero, Vector3.zero, "warp", roomFadeData);
			Events.OnRoomChanged += OnRoomChange;
		}

		private void OnPlayerSpawned(Entity player, GameObject camera, PlayerController controller)
		{
			Plugin.Instance.StartCoroutine(ArbitraryRoomSwitchDelay());
			Events.OnPlayerSpawn -= OnPlayerSpawned;
		}

		private void OnRoomChange(Entity entity, LevelRoom toRoom, LevelRoom fromRoom, EntityEventsOwner.RoomEventData data)
		{
			// This fixes issue with reloading current room
			toRoom.SetRoomActive(true, false);
			Entity player = EntityTag.GetEntityByName("PlayerEnt");
			Vector3 facingDirection = new(0, gotoData.doorData.FacingDirection, 0);
			player.transform.localEulerAngles = facingDirection;

			Events.OnRoomChanged -= OnRoomChange;
		}

		// Fixes issue with switching rooms right after scene transition
		// Note: Waiting one frame doesn't fix it
		private IEnumerator ArbitraryRoomSwitchDelay()
		{
			yield return new WaitForSeconds(0.01f);
			SwitchRooms(false);
		}

		/// <summary>
		/// Creates the <see cref="SceneData"/> object from the JSON data
		/// </summary>
		/// <param name="rootObj">The root object within the JSON</param>
		/// <returns>The created <see cref="SceneData"/> object</returns>
		private List<SceneData> CreateSceneData(JsonObject rootObj)
		{
			List<SceneData> sceneData = new();

			// For each scene object
			foreach (JsonObject sceneObj in rootObj.GetArray("scenes").objects.Cast<JsonObject>())
			{
				string sceneName = sceneObj.GetString("scene");
				List<string> allSceneNames = new() { sceneName.ToLower() };
				List<string> spawnNames = new();
				List<SceneData.RoomData> roomData = new();
				bool doSave = string.IsNullOrEmpty(sceneObj.GetString("doSave")) ? true : bool.Parse(sceneObj.GetString("doSave"));

				// For each name value
				JsonArray nameArray = sceneObj.GetArray("names");
				for (int i = 0; i < nameArray.Length; i++)
				{
					JsonValue name = (JsonValue)nameArray.objects[i];
					allSceneNames.Add(name.GetValue().ToLower());
				}

				// For each spawn value
				JsonArray spawnArray = sceneObj.GetArray("spawns");
				for (int i = 0; i < spawnArray.Length; i++)
				{
					JsonValue spawn = (JsonValue)spawnArray.objects[i];
					spawnNames.Add(spawn.GetValue());
				}

				// For each room object
				JsonArray roomArray = sceneObj.GetArray("rooms");
				for (int i = 0; i < roomArray.Length; i++)
				{
					JsonObject roomObj = (JsonObject)roomArray.objects[i];
					List<SceneData.RoomData.DoorData> doorData = new();
					string roomName = roomObj.GetString("roomName");

					// For each door object
					JsonArray doorArray = roomObj.GetArray("doors");
					for (int j = 0; j < doorArray.Length; j++)
					{
						JsonObject doorObj = (JsonObject)doorArray.objects[j];

						// Parse position to Vector3
						if (!ModCore.Utility.TryParseVector3(doorObj.GetString("position"), out Vector3 spawnPos))
						{
							Plugin.Log.LogWarning($"Spawn position for Room{roomName} failed to parse to Vector3!");
							continue;
						}

						// Parse facing direction to int
						if (!int.TryParse(doorObj.GetString("angle"), out int facingDir))
						{
							Plugin.Log.LogWarning($"Facing angle for Room{roomName} failed to parse to int!");
							continue;
						}

						// Add door data
						doorData.Add(new SceneData.RoomData.DoorData(spawnPos, facingDir));
					}

					// Add room data
					roomData.Add(new SceneData.RoomData(roomName, doorData));
				}

				// Add scene data
				if (doSave)
					sceneData.Add(new SceneData(sceneName, allSceneNames, spawnNames, roomData, true));
				else
					sceneData.Add(new SceneData(sceneName, allSceneNames, spawnNames, roomData, false));
			}

			return sceneData;
		}

		// Tries to parse the JSON
		private bool TryParseJson<T>(out T rootObj) where T : JsonBase
		{
			rootObj = null;

			try
			{
				string jsonPath = BepInEx.Utility.CombinePaths(Paths.PluginPath, PluginInfo.PLUGIN_NAME, "Data", dataFileName);

				// If file doesn't exist, do nothing
				if (!File.Exists(jsonPath))
				{
					Plugin.Log.LogWarning($"WARNING in GOTO command: {dataFileName} does not exist!");
					return false;
				}

				rootObj = JsonBase.Decode<JsonObject>(File.ReadAllText(jsonPath)) as T;
				return rootObj != null;
			}
			catch (Exception ex)
			{
				Plugin.Log.LogError("ERROR in GOTO command!!\n" + ex.Message);
				return false;
			}
		}

		private bool TryValidateScene(string arg, List<SceneData> scenes, out SceneData sceneData)
		{
			sceneData = scenes.FirstOrDefault(x => x.AllSceneNames.Contains(arg.ToLower()));
			return sceneData != null;
		}

		private class SceneData
		{
			/// <summary>
			/// The Unity scene name for the scene, case-sensitive
			/// </summary>
			public string SceneName { get; }
			/// <summary>
			/// All valid names for the scene, including shorthand and abbreviations
			/// (eg. FluffyFields, Fluffy, FF)
			/// </summary>
			public List<string> AllSceneNames { get; }
			/// <summary>
			/// The names of all the spawn points for the scene, case-sensitive
			/// </summary>
			public List<string> SpawnNames { get; }
			/// <summary>
			/// List of rooms
			/// </summary>
			public List<RoomData> Rooms { get; }
			/// <summary>
			/// Should the scene trigger a save when loaded to?
			/// </summary>
			public bool DoSave { get; }

			public SceneData(string sceneName, List<string> allSceneNames, List<string> spawnNames, List<RoomData> rooms, bool doSave = true)
			{
				SceneName = sceneName;
				AllSceneNames = allSceneNames;
				SpawnNames = spawnNames;
				Rooms = rooms;
				DoSave = doSave;
			}

			public class RoomData
			{
				/// <summary>
				/// The name of the room
				/// </summary>
				public string RoomName { get; }
				/// <summary>
				/// List of all doors in that room
				/// </summary>
				public List<DoorData> Doors { get; }

				public RoomData(string roomName, List<DoorData> doors)
				{
					RoomName = roomName;
					Doors = doors;
				}

				public class DoorData
				{
					/// <summary>
					/// The end position for the door (at end of transition animation)
					/// </summary>
					public Vector3 SpawnPosition { get; }
					/// <summary>
					/// The facing direction
					/// </summary>
					public int FacingDirection { get; }

					public DoorData(Vector3 spawnPosition, int facingDirection)
					{
						SpawnPosition = spawnPosition;
						FacingDirection = facingDirection;
					}
				}
			}
		}

		private class GotoData
		{
			public string sceneName;
			public string spawnName;
			public string roomName;
			public SceneData.RoomData.DoorData doorData;
			public bool doSave;
		}
	}
}