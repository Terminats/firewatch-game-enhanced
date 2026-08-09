# Firewatch Community Patch

A small collection of fixes for the Windows version of Firewatch.

Right now it fixes a movement bug at high frame rates. With VSync disabled, the
player could slow down, stop moving, or become unable to move in one direction. This plugin fixes it.

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
