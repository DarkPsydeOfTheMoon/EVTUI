# EVTUI

## Setup

For basically any command-line interface, cloning and entering the repository directory should work the same way:

```
# 1. Clone and enter repository
git clone https://github.com/DarkPsydeOfTheMoon/EVTUI.git
cd EVTUI
```

Then, assuming you've cloned and are within the repository's local directory...

### Windows

```
# 2. Set up dependencies
.\scripts\bootstrap_win.bat
```

### Debian Linux

```
# 2. Set up dependencies
./scripts/bootstrap_debian.sh
```

### Other Linux

TBD.

## Credits

- [AvaloniaUI](https://github.com/AvaloniaUI)'s [Avalonia](https://github.com/AvaloniaUI/Avalonia) (License: MIT)
- [TGE](https://github.com/tge-was-taken)'s [Atlus-Script-Tools](https://github.com/tge-was-taken/Atlus-Script-Tools) (License: GPL-3.0)
- [Sewer56](https://github.com/Sewer56)'s [CriFsV2Lib](https://github.com/Sewer56/CriFsV2Lib) (License: LGPL-3.0)
- [TGE](https://github.com/tge-was-taken)'s [GFD-Studio](https://github.com/tge-was-taken/GFD-Studio)
- [VideoLAN](https://github.com/videolan)'s [LibVLCSharp](https://github.com/videolan/libvlcsharp) (License: LGPL-2.1)
- [LazyBone152](https://github.com/LazyBone152)'s [XV2-Tools](https://github.com/LazyBone152/XV2-Tools) (License: MIT)

Although [TGE](https://github.com/tge-was-taken)'s [EvtTool](https://github.com/tge-was-taken/EvtTool) (especially the contributions by [Secre-C](https://github.com/Secre-C)) is not explicitly used in this project, it was *heavily* referenced for the EVT-parsing functionality.

The `Serialization` library is a port of the forthcoming `exbip` Python library — both original and port by [Pherakki](https://github.com/Pherakki).
