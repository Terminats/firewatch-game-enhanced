# Firewatch Enhanced

A small collection of fixes for Firewatch on Windows and Linux.


## Features
- Fixes player movement at high frame rates with VSync disabled.
- Adds an optional FPS counter. It is disabled by default.
- Adds an option to remove the time limit from dialogue choices. It is disabled
  by default.
- Adds a subtitle size slider with a range of 75% to 200%. The original size is
  100%.
- Adds a Field of View slider with a range of 30 to 110. The original value is
  55.
- Exposes the game's hidden Mouse Acceleration setting in the Controls menu. You need to disable that option if you're experiencing an issue with high mouse sensitivity.
- Includes Ignore Pads option which is set by default, this fixes issue with controllers causing automatic rotation of character.
- Adds a New Game (Skip Intro) button that skips the opening text sequence.
- Includes a simple noclip mode for unstucking your character.
- Improves save loading and reduces some asset-streaming stalls.
- Reduces unnecessary player interaction checks above 60 FPS.
- Prevents the Free Roam day/night cycle from getting stuck after leaving the
  cave.

The new options are available in the existing General, Graphics, and Controls settings.

If an existing Free Roam save already has broken cave lighting, install the
patch, enter the cave again and move through gate until you drop on the first stone, after that go you can go back and the day/night cycle should be restored. The exit event
will restore the day/night cycle and the next save should load correctly.

## Installation

### Windows

Download the Windows release and extract it into the Firewatch folder, next to
`Firewatch.exe`. The release includes BepInEx 5, so no separate mod loader is
needed.

The current build targets the 64-bit GOG version of the game.

### Native Linux and SteamOS

Download the `linux-x64` release. Make sure Steam is using the native Linux
build of Firewatch rather than a forced Proton version, then:

1. Open **Installed Files > Browse** and extract the archive into the game
   directory, next to `fw.x86_64`.
2. Open a terminal in that directory and run:

   ```sh
   chmod +x run_bepinex.sh
   ```

3. In **Properties > General**, enter this in **Launch Options**:

   ```text
   ./run_bepinex.sh %command%
   ```

4. Start the game normally through Steam.

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

## Unused voice extractor

The repository includes an optional script for extracting English voice
recordings that have no dialogue definition or direct reference in the shipped
game data.

Install its Python dependency and run it from the repository directory:

```powershell
python -m pip install wem2ogg
python scripts\extract_unused_voices.py
```

The resulting OGG files and `index.csv` are written to `unused-voice`. These
files are probably unused recordings or dynamically constructed audio events. I did not verify that but if somebody has time they can dig through and see if there is anything interesting in there.

## License

The patch source code is available under the MIT License. BepInEx is a separate
project and is distributed under its own LGPL-2.1 license.
