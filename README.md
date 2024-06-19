# Debug Menu Cheats

This mod adds several cheat commands

## Commands List
| Command Name                  | Description |
| ----------------------------- | ----------- |
| [god](#god)                   | Toggles invincibility for Ittle. You will not receive damage or knockback, and won't be able to void out.
| likeaboss                     | Toggles one-hit-kill mode for Ittle. Everything you hit will die in one hit, including some invincible enemies.
| noclip                        | Toggle's Ittle's hitbox. While her hitbox is disabled, you can walk through walls.
| [setspeed](#setspeed) | Set Ittle's movement speed
| [setitems](#setitems)         | Give yourself items

## Command Documentation

### Player Cheats
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
</details>

___