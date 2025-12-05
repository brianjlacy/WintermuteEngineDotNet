# Wintermute Engine .NET Conversion

This repository contains the modernization effort to convert **Wintermute Engine 1.x** from C++/.NET Framework 3.5 to C#/.NET 8.0/MAUI.

## Project Status

🚧 **Phase 1: Foundation (Core Infrastructure)** - In Progress

### Completed
- ✅ Solution structure created (WintermuteEngine.sln)
- ✅ Project scaffolding for all 22 projects
- ✅ .NET 8.0 target framework configured
- ✅ NuGet package references configured per specification
- ✅ TODO.md tracking document created

### In Progress
- Core interfaces and base classes implementation

## Project Structure

```
WintermuteEngine.sln
├── src/                      # Core engine libraries
│   ├── WME.Core/            # Core functionality
│   ├── WME.Graphics/        # Graphics abstraction layer
│   ├── WME.Graphics.OpenGL/ # OpenGL renderer implementation
│   ├── WME.Audio/           # Audio abstraction layer
│   ├── WME.Audio.OpenAL/    # OpenAL audio implementation
│   ├── WME.Adventure/       # Adventure game module
│   ├── WME.Scripting/       # Script compiler & runtime
│   ├── WME.Formats/         # File format handlers
│   ├── WME.Plugins/         # Plugin system
│   └── WME.Runtime/         # Game runtime executable
├── tools/                    # Development tools (MAUI)
│   ├── WME.Tools.Common/
│   ├── WME.ProjectManager/
│   ├── WME.SceneEditor/
│   ├── WME.SpriteEditor/
│   ├── WME.WindowEditor/
│   ├── WME.StringTableManager/
│   └── WME.ScriptEditor/
├── tests/                    # Test projects (xUnit)
│   ├── WME.Core.Tests/
│   ├── WME.Scripting.Tests/
│   ├── WME.Formats.Tests/
│   └── WME.Integration.Tests/
└── samples/
    └── WME.SampleGame/
```

## Requirements

- .NET 8.0 SDK or later
- Visual Studio 2022+ / VS Code / Rider (recommended)
- Windows 10+ / macOS 12+ / Linux (Ubuntu 22.04+)

## Building

```bash
dotnet restore
dotnet build WintermuteEngine.sln
```

## Documentation

- **SPECIFICATION.md** - Complete technical specification for the conversion
- **TODO.md** - Task tracking document with all phases and tasks
- **INSTRUCTIONS.md** - Development workflow and guidelines

## Original Wintermute Engine

This is the .NET conversion project. For the original Wintermute Engine 1.x information:
- Forum: http://forum.dead-code.org/index.php?board=28.0
- Original repository content preserved in `src/` subdirectories

## License

See license.txt for license information.

---

**Note:** This project is under active development. The current focus is on Phase 1 (Foundation/Core Infrastructure).
