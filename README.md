# Debug Menu Cheats

This mod adds several cheat commands

## Installation

1. Download the latest version of [ModCore](https://github.com/Extra-2-Dew/ModCore) along with its dependencies.
2. Download the latest release (the zip file) from the [Releases page](https://github.com/Extra-2-Dew/DebugMenuCheats/releases).
3. Unzip the release and drop it into your Bepinex plugins folder.

## Commands List
| Command Name          | Description |
| --------------------- | ----------- |
| [god](#god)           | Toggles invincibility for Ittle. You will not receive damage or knockback, and won't be able to void out.
| likeaboss             | Toggles one-hit-kill mode for Ittle. Everything you hit will die in one hit, including some invincible enemies.
| noclip                | Toggle's Ittle's hitbox. While her hitbox is disabled, you can walk through walls.
| [setspeed](#setspeed) | Set Ittle's movement speed
| [setitems](#setitems) | Give yourself items
| [goto](#goto)         | Instantly teleport to any scene, room, or spawn point your heart desires

## Command Documentation

### Player Commands
Player cheats require Ittle to be present in the scene.

___

<details id="god">
<summary><b>god</b></summary>

**Alias(es):**
- `godmode`
</details>

___

<details id="setspeed">
<summary><b>setspeed</b></summary>

**Alias(es):**
- `speed`

**Argument(s):**

| Index | Type    | Explanation |
| ----- | ------- | ----------- |
| 0     | decimal | The amount to multiply Ittle's default speed by

**Examples:**
- `setspeed 2` will double Ittle's default speed
- `speed 1` will reset Ittle's speed to default
- `speed -1` will reverse Ittle's movement directions
</details>

___

<details id="setitems">
<summary><b>setitems</b></summary>

**Alias(es):**
- `giveitems`

**Argument(s):**

| Index | Type   | Explanation |
| ----- | ------ | ----------- |
| 0     | text   | The item to set, the name of the melee item (`stick`, `sword`, `mace`, or `efcs`), or a shorthand for setting multiple (`all`, `dev`, or `none`)
| 1     | number | The level/count for the item

**Flag(s):**

| Flag        | Explanation |
| ----------- | ----------- |
| `--no-save` | Don't save the items to the save file automatically.

**Examples:**
- `setitems ice 4` will give you Ice Ring level 4
- `setitems mace` will give you Fire Mace
- `giveitems melee 0` will give you Stick
- `giveitems all` will give you all items at max (non-dev) level/counts
- `setitems dev --no-save` will give you all items at max level/counts, but it won't save them

<details>
<summary>Accepted values</summary>

| Item name | Alias(es)        | Level/Count Range |
| --------- | ---------------- | ----------------- |
| stick     |                  |                   |
| firesword | sword            |                   |
| firemace  | mace             |                   |
| efcs      |                  |                   |
| melee     |                  | 0-3               |
| forcewand | force, wand      | 0-4               |
| dynamite  | dyna             | 0-4               |
| icering   | ice, ring        | 0-4               |
| raft      |                  | 0-8               |
| shards    | secretshards     | 0-24              |
| keys      | lockpicks, picks | 0-99              |
| evilkeys  | forbiddenkeys    | 0-4               |
| headband  |                  | 0-3               |
| tome      |                  | 0-3               |
| amulet    |                  | 0-3               |
| chain     |                  | 0-3               |
| tracker   |                  | 0-3               |
| loot      |                  | 0-1               |
</details>
</details>

___

### Non-Player Commands
These commands will work anywhere, regardless of if Ittle is present

___

<details id="goto">
<summary><b>goto</b></summary>

**Argument(s):**

| Index | Type        | Conditional | Explanation |
| ----- | ----------- | ----------- | ----------- |
| 0     | text        |             | The name or alias for the scene
| 1     | text/number |             | The name of the spawn, room, or the 0-based index position for the spawn point
| 2     | number      | ✔          | (If room is given for arg 1) The 0-based index for the door within the room. If not given, door will default to index 0

**Flag(s):**

| Flag        | Explanation |
| ----------- | ----------- |
| `--no-save` | Don't save the items to the save file automatically.

**Examples:**
- `goto fluffy restorept1`: Warps to Fluffy Fields checkpoint
- `goto ff c`: Warps to Fluffy Fields room C
- `goto ff`: Warps to Fluffy Fields' first spawn in list (checkpoint)
- `goto fluffyfields b 1`: Warps to Fluffy Fields room B at the 2nd door in the room

</details>

___