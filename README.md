# Firewatch Enhanced

A small collection of fixes for the Windows version of Firewatch.


## Features
- Fixes player movement at high frame rates with VSync disabled.
- Adds an optional FPS counter. It is disabled by default.
- Adds an option to remove the time limit from dialogue choices. It is disabled
  by default.
- Adds a Field of View slider with a range of 30 to 110. The original value is
  55.
- Exposes the game's hidden Mouse Acceleration setting in the Controls menu. You need to disable that option if you're experiencing an issue with high mouse sensitivity.
- Includes a simple noclip mode for unstucking your character.

The new options are available in the existing General, Graphics, and Controls settings.

## Installation

Download a release and extract it into the Firewatch folder, next to
`Firewatch.exe`. The release includes BepInEx 5, so no separate mod loader is
needed.

The current build targets the 64-bit GOG version of the game.

## Noclip

Sometimes when colliding with some walls or rocks you may find that Henry is stuck. Press `F8` to toggle noclip to unstuck yourself.

- `WASD` - move
- `Space` / `Left Ctrl` - move up or down
- `Left Shift` - move faster

Turn it off in an open space above solid ground. Turning collisions back on
inside terrain can leave the player stuck.

## Building

Open `FirewatchHighFpsFix.sln` in Visual Studio and build the Release
configuration. The game path is set in `Directory.Build.props`; change it if
Firewatch is installed somewhere else.

The compiled plugin is written to:

```text
src\FirewatchHighFpsFix\bin\Release\FirewatchHighFpsFix.dll
```

Run `Package.ps1` after building to create the distributable archive.

## License

The patch source code is available under the MIT License. BepInEx is a separate
project and is distributed under its own LGPL-2.1 license.
