# EVTUI

*A WIP tool for viewing, editing, and creating story events in P5R.*

## Setup

For basically any command-line interface, cloning and entering the repository directory should work the same way:

```
git clone https://github.com/DarkPsydeOfTheMoon/EVTUI.git
cd EVTUI
```

Then, assuming you've cloned and are within the repository's local directory, run the setup script for your current OS:

### Windows

```
.\scripts\bootstrap_win.bat
```

### Debian Linux

```
./scripts/bootstrap_debian.sh
```

### Other Linux

TBD.

## Building

Although this project is a WIP and has no official release, you can see the current state by building a standalone executable for your target OS like so (or just `build` and `run` like any .NET Core application):

### Windows

```
dotnet publish --runtime win-x86
```

### Linux

```
dotnet publish --runtime linux-x64
```

## Credits

- [AvaloniaUI](https://github.com/AvaloniaUI)'s [Avalonia](https://github.com/AvaloniaUI/Avalonia) (License: MIT)
- [TGE](https://github.com/tge-was-taken)'s [Atlus-Script-Tools](https://github.com/tge-was-taken/Atlus-Script-Tools) (License: GPL-3.0)
- [Sewer56](https://github.com/Sewer56)'s [CriFsV2Lib](https://github.com/Sewer56/CriFsV2Lib) (License: LGPL-3.0)
- [TGE](https://github.com/tge-was-taken)'s [GFD-Studio](https://github.com/tge-was-taken/GFD-Studio)
- [VideoLAN](https://github.com/videolan)'s [LibVLCSharp](https://github.com/videolan/libvlcsharp) (License: LGPL-2.1)

The ACB parsing code from [LazyBone152](https://github.com/LazyBone152)'s [XV2-Tools](https://github.com/LazyBone152/XV2-Tools) (License: MIT) was originally included as a patched module, but this dependency has since been deprecated. Even so, the current ACB code was heavily based on it. The same is true of the ADX parsing/decrypting from [Thealexbarney](https://github.com/Thealexbarney)'s [VGAudio](https://github.com/Thealexbarney/VGAudio) (License: MIT).

Although [TGE](https://github.com/tge-was-taken)'s [EvtTool](https://github.com/tge-was-taken/EvtTool) (especially the contributions by [Secre-C](https://github.com/Secre-C)) is not explicitly used in this project, it was *heavily* referenced for the EVT-parsing functionality.

The `Serialization` library is a port of the forthcoming `exbip` Python library — both original and port by [Pherakki](https://github.com/Pherakki).
