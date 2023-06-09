# ONI multiplayer mod (WORK IN PROGRESS / PROTOTYPE)

This is a mod which adds multiplayer support to Oxygen not included game.

## Current stage and status

Status: In development
Stage: Early WIP and proof of concept

Working functionality:

- Currently tested in Vanilla only (NO DLC)
- Main menu UI
  - Join/Load/Create MP game
- Steam overlay support
  - Ability to join/invite friends
- Synced tools (bottom toolbar)
  - dig/build/mop/harvest/etc and other from bottom toolbar
- Synced game settings
  - warp settings
  - pause
- Synced colony settings
  - Consumables screen
  - Research tree
- Additional in-game UI
  - Active players info
      - Cursors of all players
  - Additional diagnostic showing amount of synchronization error
    - Always 0 on the server side
    - Huge on any client :)
- Every mornings hard syncs to avoid accumulated errors
- Hard sync on each server save action

## Game mechanics

All players play all together and share controls over a single colony. Order given by a player might be overruled by
another.
<p>There is no difference between different players, all players are equal.
It is possible to look and control different asteroids at the same time as well.

## Under the hood mechanics idea

Idea is based on the assumption that the game engine will run ~~the same~~ **similar** without any user input even on
different
machines.
<p>So if user input will be the same on different machines - then their separate simulations should run the same.
<p>To avoid accumulated errors (if any) it is proposed to do periodic (one per game day) hard syncs by loading game save
files.
<p>Additional support is required for minions since their logic is separate from the world state their behavior is differnt on different machines.

<p>[Optional] To make smoother experience or if simulations will be running too different it is feasible to do periodic
small world syncs for smaller areas of different layers (e.g. sync gases within an area 16x16 every 30 seconds).

## How to install

### Automatic way (from release 0.2.0-alpha)
* Be aware that mod will be installed to %USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\MultiplayerMod.
- Download latest release from https://github.com/zuev93/oni_multiplayer/releases/latest
- Unzip mod to any folder
- Double click on install.bat
    
### Manual way
- Download latest release from https://github.com/zuev93/oni_multiplayer/releases/latest
- Unzip mod to any folder
- Copy release content (mod.yaml, mod_info.yaml and MultiplayerMod.dll) to %USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\MultiplayerMod (if
some folders are missing, please create them).

## How to use

### Host:

- Host a game either via 'New multiplayer game' or via 'Load multiplayer game'
- Wait until the game is loaded and overlay is opened automatically
- Invite friends via Steam overlay

### Player/Friend:

- Run the game first
- Join invite (from already opened game) or join via Steam overlay
- Wait until game is loaded

# What is next
We are trying to keep planned work and known bugs in github issues
https://github.com/zuev93/oni_multiplayer/milestones
<p>
And also we're using a GitHub project for task management

https://github.com/users/zuev93/projects/1

# Contacts
If you have any suggestions or questions feel free to create discussion or issue in GitHub or join our discord server https://discord.gg/3TQ97w8Qwq

# How to develop

For a guide and more info, refer to the [Contributing.md](docs/CONTRIBUTING.md)
