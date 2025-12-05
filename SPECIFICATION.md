# Wintermute Engine .NET 10 / MAUI Conversion Specification

**Document Version:** 1.0
**Date:** December 5, 2025
**Project Codename:** WME.NET

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Current State Analysis](#2-current-state-analysis)
3. [Target Architecture](#3-target-architecture)
4. [Technology Stack](#4-technology-stack)
5. [Component Conversion Map](#5-component-conversion-map)
6. [Core Engine Conversion](#6-core-engine-conversion)
7. [Script System Conversion](#7-script-system-conversion)
8. [Graphics System Conversion](#8-graphics-system-conversion)
9. [Audio System Conversion](#9-audio-system-conversion)
10. [Development Tools Conversion](#10-development-tools-conversion)
11. [File Format Compatibility](#11-file-format-compatibility)
12. [Plugin System Conversion](#12-plugin-system-conversion)
13. [Implementation Phases](#13-implementation-phases)
14. [Risk Assessment](#14-risk-assessment)
15. [Success Criteria](#15-success-criteria)
16. [Appendices](#16-appendices)

---

## 1. Executive Summary

### 1.1 Project Overview

This specification defines the complete conversion of **Wintermute Engine 1.x** from its current C++/.NET Framework 3.5 implementation to a modern **C#/.NET 10/MAUI** architecture. The goal is to create a cross-platform, maintainable game engine while preserving backward compatibility with existing Wintermute games.

### 1.2 Objectives

| Objective | Description |
|-----------|-------------|
| **Modernization** | Migrate from legacy C++/DirectX to modern C#/.NET 10 |
| **Cross-Platform** | Support Windows, macOS, Linux, iOS, and Android |
| **Maintainability** | Unified C# codebase with modern patterns |
| **Compatibility** | Run existing WME games without modification |
| **Tooling** | Modern MAUI-based development tools |

### 1.3 Scope

**In Scope:**
- Complete engine rewrite in C#
- All development tools (ProjectMan, SceneEdit, SpriteEdit, WindowEdit, StringTableMgr)
- Script compiler and runtime
- Full file format compatibility
- Plugin API (modernized)

**Out of Scope:**
- WME 2.x features (separate project)
- Mobile-specific game features (touch gestures, etc.) - future enhancement
- Cloud services integration

### 1.4 Key Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Graphics API | Silk.NET (OpenGL/Vulkan) | Cross-platform, modern, C# native |
| Audio API | OpenAL Soft via Silk.NET | Cross-platform, proven |
| UI Framework | .NET MAUI | Cross-platform, modern, Microsoft-supported |
| Script Compiler | ANTLR4 | Industry standard, C# native |
| 2D Rendering | SkiaSharp | Fast, cross-platform, MAUI-integrated |

---

## 2. Current State Analysis

### 2.1 Codebase Statistics

| Component | Current Tech | Files | Complexity |
|-----------|--------------|-------|------------|
| Core Engine | C++ | ~1,271 | High |
| Script Compiler | Flex/Bison | 12 | Medium |
| Development Tools | C#/.NET 3.5 | 21 projects | Medium |
| External Libraries | C/C++ | 28 | Low (replace) |

### 2.2 Core Engine Classes (To Convert)

```
CBGame          → WmeGame
CBObject        → WmeObject
CBScriptable    → WmeScriptable
CBRenderer      → IWmeRenderer
CBSurface       → WmeSurface
CBSprite        → WmeSprite
CBSoundMgr      → WmeSoundManager
CBSoundBuffer   → WmeSoundBuffer
CBFileManager   → WmeFileManager
CScEngine       → WmeScriptEngine
CAdGame         → WmeAdventureGame
CAdScene        → WmeScene
CAdActor        → WmeActor
CAdEntity       → WmeEntity
CAdItem         → WmeItem
```

### 2.3 Critical Interfaces to Preserve

```csharp
// Script property access (must maintain exact signatures)
interface IWmeScriptable
{
    WmeValue GetProperty(string name);
    void SetProperty(string name, WmeValue value);
    WmeValue CallMethod(string name, WmeStack stack);
}

// Plugin compatibility
interface IWmePlugin
{
    void Initialize(IWmeGame game);
    void OnEvent(WmeEvent eventType, object data);
    void Shutdown();
}
```

---

## 3. Target Architecture

### 3.1 Solution Structure

```
WintermuteEngine.sln
│
├── src/
│   ├── WME.Core/                    # Core engine library
│   │   ├── WME.Core.csproj
│   │   ├── Game/
│   │   ├── Objects/
│   │   ├── Scripting/
│   │   ├── Resources/
│   │   └── Persistence/
│   │
│   ├── WME.Graphics/                # Graphics abstraction
│   │   ├── WME.Graphics.csproj
│   │   ├── Rendering/
│   │   ├── Sprites/
│   │   ├── Surfaces/
│   │   └── Effects/
│   │
│   ├── WME.Graphics.OpenGL/         # OpenGL implementation
│   ├── WME.Graphics.Vulkan/         # Vulkan implementation (future)
│   │
│   ├── WME.Audio/                   # Audio abstraction
│   │   ├── WME.Audio.csproj
│   │   ├── SoundManager/
│   │   ├── Codecs/
│   │   └── Streaming/
│   │
│   ├── WME.Audio.OpenAL/            # OpenAL implementation
│   │
│   ├── WME.Adventure/               # Adventure game module
│   │   ├── WME.Adventure.csproj
│   │   ├── Scenes/
│   │   ├── Actors/
│   │   ├── Items/
│   │   ├── Dialogs/
│   │   └── Pathfinding/
│   │
│   ├── WME.Scripting/               # Script compiler & runtime
│   │   ├── WME.Scripting.csproj
│   │   ├── Compiler/
│   │   ├── Runtime/
│   │   ├── Debugger/
│   │   └── Grammar/
│   │
│   ├── WME.Formats/                 # File format handlers
│   │   ├── WME.Formats.csproj
│   │   ├── Packages/
│   │   ├── Scenes/
│   │   ├── Sprites/
│   │   └── SaveGames/
│   │
│   ├── WME.Plugins/                 # Plugin system
│   │   └── WME.Plugins.csproj
│   │
│   └── WME.Runtime/                 # Game runtime executable
│       └── WME.Runtime.csproj
│
├── tools/
│   ├── WME.Tools.Common/            # Shared tool components
│   ├── WME.ProjectManager/          # Project Manager (MAUI)
│   ├── WME.SceneEditor/             # Scene Editor (MAUI)
│   ├── WME.SpriteEditor/            # Sprite Editor (MAUI)
│   ├── WME.WindowEditor/            # Window Editor (MAUI)
│   ├── WME.StringTableManager/      # String Table Manager (MAUI)
│   └── WME.ScriptEditor/            # Script Editor (MAUI)
│
├── tests/
│   ├── WME.Core.Tests/
│   ├── WME.Scripting.Tests/
│   ├── WME.Formats.Tests/
│   └── WME.Integration.Tests/
│
└── samples/
    └── WME.SampleGame/
```

### 3.2 Layer Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Applications Layer                        │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────────────────┐│
│  │ WME.Runtime │ │ MAUI Tools  │ │ Third-Party Games       ││
│  └─────────────┘ └─────────────┘ └─────────────────────────┘│
├─────────────────────────────────────────────────────────────┤
│                    Adventure Layer                           │
│  ┌─────────────────────────────────────────────────────────┐│
│  │ WME.Adventure (Scenes, Actors, Items, Dialogs, AI)      ││
│  └─────────────────────────────────────────────────────────┘│
├─────────────────────────────────────────────────────────────┤
│                    Core Engine Layer                         │
│  ┌───────────────┐ ┌───────────────┐ ┌───────────────────┐  │
│  │ WME.Core      │ │ WME.Scripting │ │ WME.Formats       │  │
│  │ (Game, Objects│ │ (Compiler,    │ │ (DCP, Scenes,     │  │
│  │  Resources)   │ │  Runtime, VM) │ │  SaveGames)       │  │
│  └───────────────┘ └───────────────┘ └───────────────────┘  │
├─────────────────────────────────────────────────────────────┤
│                    Abstraction Layer                         │
│  ┌─────────────────────────┐ ┌─────────────────────────────┐│
│  │ WME.Graphics            │ │ WME.Audio                   ││
│  │ (IRenderer, ISurface)   │ │ (ISoundManager, ICodec)     ││
│  └─────────────────────────┘ └─────────────────────────────┘│
├─────────────────────────────────────────────────────────────┤
│                    Platform Layer                            │
│  ┌───────────────┐ ┌───────────────┐ ┌───────────────────┐  │
│  │ OpenGL/Vulkan │ │ OpenAL Soft   │ │ Platform APIs     │  │
│  │ (Silk.NET)    │ │ (Silk.NET)    │ │ (File I/O, etc.)  │  │
│  └───────────────┘ └───────────────┘ └───────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

### 3.3 Dependency Injection Architecture

```csharp
// Service registration in WME.Runtime
public static class WmeServices
{
    public static IServiceCollection AddWintermuteEngine(
        this IServiceCollection services)
    {
        // Core services
        services.AddSingleton<IWmeGame, WmeGame>();
        services.AddSingleton<IWmeFileManager, WmeFileManager>();
        services.AddSingleton<IWmeResourceManager, WmeResourceManager>();

        // Graphics
        services.AddSingleton<IWmeRenderer, OpenGLRenderer>();
        services.AddSingleton<IWmeSurfaceFactory, OpenGLSurfaceFactory>();

        // Audio
        services.AddSingleton<IWmeSoundManager, OpenALSoundManager>();

        // Scripting
        services.AddSingleton<IWmeScriptEngine, WmeScriptEngine>();
        services.AddSingleton<IWmeScriptCompiler, WmeScriptCompiler>();

        // Adventure module
        services.AddSingleton<IWmeSceneManager, WmeSceneManager>();
        services.AddSingleton<IWmePathfinder, WmePathfinder>();

        return services;
    }
}
```

---

## 4. Technology Stack

### 4.1 Core Technologies

| Component | Technology | Version | Purpose |
|-----------|------------|---------|---------|
| Runtime | .NET | 10.0 | Application framework |
| Language | C# | 13.0 | Primary language |
| UI Framework | .NET MAUI | 10.0 | Cross-platform UI |
| Graphics | Silk.NET | 2.x | OpenGL/Vulkan bindings |
| 2D Rendering | SkiaSharp | 3.x | 2D graphics, sprites |
| Audio | Silk.NET.OpenAL | 2.x | Cross-platform audio |
| Parser Generator | ANTLR4 | 4.x | Script compiler |
| Compression | System.IO.Compression | built-in | ZIP/deflate |
| Image Formats | ImageSharp | 3.x | PNG, JPEG, BMP |
| Video | FFmpeg.AutoGen | 7.x | Theora video playback |
| Serialization | System.Text.Json | built-in | Settings, data |
| Logging | Microsoft.Extensions.Logging | 10.x | Structured logging |
| DI Container | Microsoft.Extensions.DI | 10.x | Dependency injection |
| Testing | xUnit | 2.x | Unit testing |

### 4.2 Platform Support Matrix

| Platform | Runtime | Tools | Min Version |
|----------|---------|-------|-------------|
| Windows | ✅ | ✅ | Windows 10 1809+ |
| macOS | ✅ | ✅ | macOS 12+ |
| Linux | ✅ | ✅ | Ubuntu 22.04+ |
| iOS | ✅ | ❌ | iOS 15+ |
| Android | ✅ | ❌ | Android 10+ |

### 4.3 NuGet Dependencies

```xml
<!-- WME.Core.csproj -->
<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="10.0.0" />
  <PackageReference Include="Microsoft.Extensions.Logging" Version="10.0.0" />
  <PackageReference Include="System.Text.Json" Version="10.0.0" />
</ItemGroup>

<!-- WME.Graphics.OpenGL.csproj -->
<ItemGroup>
  <PackageReference Include="Silk.NET.OpenGL" Version="2.22.0" />
  <PackageReference Include="Silk.NET.Windowing" Version="2.22.0" />
  <PackageReference Include="Silk.NET.Input" Version="2.22.0" />
  <PackageReference Include="SkiaSharp" Version="3.0.0" />
</ItemGroup>

<!-- WME.Audio.OpenAL.csproj -->
<ItemGroup>
  <PackageReference Include="Silk.NET.OpenAL" Version="2.22.0" />
  <PackageReference Include="NVorbis" Version="0.10.5" />
</ItemGroup>

<!-- WME.Scripting.csproj -->
<ItemGroup>
  <PackageReference Include="Antlr4.Runtime.Standard" Version="4.13.1" />
</ItemGroup>

<!-- WME.Formats.csproj -->
<ItemGroup>
  <PackageReference Include="SixLabors.ImageSharp" Version="3.1.0" />
  <PackageReference Include="FFmpeg.AutoGen" Version="7.0.0" />
</ItemGroup>

<!-- MAUI Tools -->
<ItemGroup>
  <PackageReference Include="CommunityToolkit.Maui" Version="10.0.0" />
  <PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.0" />
</ItemGroup>
```

---

## 5. Component Conversion Map

### 5.1 Core Engine Classes

| Original (C++) | New (C#) | Namespace | Notes |
|----------------|----------|-----------|-------|
| `CBGame` | `WmeGame` | WME.Core | Main game class |
| `CBObject` | `WmeObject` | WME.Core.Objects | Base object |
| `CBScriptable` | `WmeScriptable` | WME.Core.Objects | Script-enabled object |
| `CBBase` | `WmeBase` | WME.Core.Objects | Root class with game context |
| `CBPersistMgr` | `WmePersistenceManager` | WME.Core.Persistence | Save/load system |
| `CBFileManager` | `WmeFileManager` | WME.Core.Resources | File I/O |
| `CBResourceManager` | `WmeResourceManager` | WME.Core.Resources | Asset caching |
| `CBParser` | `WmeDefinitionParser` | WME.Formats | Definition file parser |
| `CBDynBuffer` | `WmeBuffer` | WME.Core | Dynamic buffer |
| `CBStringTable` | `WmeStringTable` | WME.Core | Localization |

### 5.2 Graphics Classes

| Original (C++) | New (C#) | Namespace |
|----------------|----------|-----------|
| `CBRenderer` | `IWmeRenderer` | WME.Graphics |
| `CBRendererD3D8/9` | `OpenGLRenderer` | WME.Graphics.OpenGL |
| `CBSurface` | `WmeSurface` | WME.Graphics |
| `CBSurfaceD3D8/9` | `OpenGLSurface` | WME.Graphics.OpenGL |
| `CBSprite` | `WmeSprite` | WME.Graphics.Sprites |
| `CBFrame` | `WmeFrame` | WME.Graphics.Sprites |
| `CBSubFrame` | `WmeSubFrame` | WME.Graphics.Sprites |
| `CBImage` | `WmeImage` | WME.Graphics |
| `C3DCamera` | `WmeCamera3D` | WME.Graphics.3D |
| `C3DModel` | `WmeModel3D` | WME.Graphics.3D |
| `C3DLight` | `WmeLight3D` | WME.Graphics.3D |

### 5.3 Audio Classes

| Original (C++) | New (C#) | Namespace |
|----------------|----------|-----------|
| `CBSoundMgr` | `WmeSoundManager` | WME.Audio |
| `CBSoundBuffer` | `WmeSoundBuffer` | WME.Audio |
| `CBSoundOGG` | `OggDecoder` | WME.Audio.Codecs |
| `CBSoundWAV` | `WavDecoder` | WME.Audio.Codecs |
| `CBSoundTheora` | `TheoraDecoder` | WME.Audio.Codecs |

### 5.4 Adventure Classes

| Original (C++) | New (C#) | Namespace |
|----------------|----------|-----------|
| `CAdGame` | `WmeAdventureGame` | WME.Adventure |
| `CAdScene` | `WmeScene` | WME.Adventure.Scenes |
| `CAdLayer` | `WmeLayer` | WME.Adventure.Scenes |
| `CAdActor` | `WmeActor` | WME.Adventure.Actors |
| `CAdEntity` | `WmeEntity` | WME.Adventure.Entities |
| `CAdItem` | `WmeItem` | WME.Adventure.Items |
| `CAdInventory` | `WmeInventory` | WME.Adventure.Items |
| `CAdInventoryBox` | `WmeInventoryBox` | WME.Adventure.UI |
| `CAdResponseBox` | `WmeResponseBox` | WME.Adventure.Dialogs |
| `CAdPath` | `WmePath` | WME.Adventure.Pathfinding |
| `CAdRegion` | `WmeRegion` | WME.Adventure.Scenes |
| `CAdWaypointGroup` | `WmeWaypointGroup` | WME.Adventure.Pathfinding |

### 5.5 Scripting Classes

| Original (C++) | New (C#) | Namespace |
|----------------|----------|-----------|
| `CScEngine` | `WmeScriptEngine` | WME.Scripting |
| `CScScript` | `WmeScript` | WME.Scripting |
| `CScStack` | `WmeScriptStack` | WME.Scripting.Runtime |
| `CScValue` | `WmeValue` | WME.Scripting.Runtime |
| `lang.l/lang.y` | `WmeScript.g4` | WME.Scripting.Grammar |

### 5.6 UI Classes

| Original (C++) | New (C#) | Namespace |
|----------------|----------|-----------|
| `CUIWindow` | `WmeWindow` | WME.Core.UI |
| `CUIButton` | `WmeButton` | WME.Core.UI |
| `CUIEdit` | `WmeTextBox` | WME.Core.UI |
| `CUIText` | `WmeLabel` | WME.Core.UI |
| `CUIEntity` | `WmeUIEntity` | WME.Core.UI |

---

## 6. Core Engine Conversion

### 6.1 WmeGame Class

```csharp
namespace WME.Core;

public class WmeGame : WmeObject, IWmeGame, IDisposable
{
    // Dependencies (injected)
    private readonly IWmeRenderer _renderer;
    private readonly IWmeSoundManager _soundManager;
    private readonly IWmeScriptEngine _scriptEngine;
    private readonly IWmeFileManager _fileManager;
    private readonly ILogger<WmeGame> _logger;

    // State
    public GameState State { get; private set; }
    public WmeObject? ActiveObject { get; set; }
    public WmeObject? MainObject { get; set; }

    // Collections
    public WmeObjectCollection<WmeWindow> Windows { get; }
    public WmeObjectCollection<WmeScript> Scripts { get; }

    // Timing
    public TimeSpan GameTime { get; private set; }
    public TimeSpan DeltaTime { get; private set; }
    public int TargetFPS { get; set; } = 60;

    // Versioning (for compatibility)
    public static readonly Version EngineVersion = new(1, 10, 2);
    public static readonly int SaveGameVersion = 0x010A01;

    public WmeGame(
        IWmeRenderer renderer,
        IWmeSoundManager soundManager,
        IWmeScriptEngine scriptEngine,
        IWmeFileManager fileManager,
        ILogger<WmeGame> logger)
    {
        _renderer = renderer;
        _soundManager = soundManager;
        _scriptEngine = scriptEngine;
        _fileManager = fileManager;
        _logger = logger;

        Windows = new WmeObjectCollection<WmeWindow>(this);
        Scripts = new WmeObjectCollection<WmeScript>(this);
    }

    public async Task<bool> InitializeAsync(string projectFile)
    {
        _logger.LogInformation("Initializing Wintermute Engine {Version}", EngineVersion);

        // Load project configuration
        if (!await LoadProjectAsync(projectFile))
            return false;

        // Initialize subsystems
        await _renderer.InitializeAsync();
        await _soundManager.InitializeAsync();
        await _scriptEngine.InitializeAsync();

        State = GameState.Running;
        return true;
    }

    public void Update(TimeSpan deltaTime)
    {
        if (State != GameState.Running)
            return;

        DeltaTime = deltaTime;
        GameTime += deltaTime;

        // Update scripts
        _scriptEngine.Update(deltaTime);

        // Update objects
        foreach (var window in Windows)
            window.Update(deltaTime);
    }

    public void Render()
    {
        _renderer.BeginFrame();

        // Render game content
        MainObject?.Render(_renderer);

        // Render windows
        foreach (var window in Windows.Where(w => w.Visible))
            window.Render(_renderer);

        _renderer.EndFrame();
    }

    // Script interface (preserved for compatibility)
    public override WmeValue? GetProperty(string name) => name switch
    {
        "Type" => new WmeValue("game"),
        "State" => new WmeValue((int)State),
        "ActiveObject" => new WmeValue(ActiveObject),
        "MainObject" => new WmeValue(MainObject),
        _ => base.GetProperty(name)
    };

    public override bool SetProperty(string name, WmeValue value)
    {
        switch (name)
        {
            case "State":
                State = (GameState)value.GetInt();
                return true;
            case "MainObject":
                MainObject = value.GetNative<WmeObject>();
                return true;
            default:
                return base.SetProperty(name, value);
        }
    }
}

public enum GameState
{
    Running,
    Frozen,
    SemiFrozen
}
```

### 6.2 WmeObject Base Class

```csharp
namespace WME.Core.Objects;

public abstract class WmeObject : WmeScriptable, IDisposable
{
    public string? Name { get; set; }
    public string? Caption { get; set; }
    public bool Ready { get; protected set; } = true;
    public bool Visible { get; set; } = true;

    public Rectangle BoundingBox { get; protected set; }
    public Point Position { get; set; }

    // Editor support
    public bool EditorSelected { get; set; }
    public Dictionary<string, string> EditorProperties { get; } = new();

    public virtual void Update(TimeSpan deltaTime) { }

    public virtual void Render(IWmeRenderer renderer) { }

    public virtual bool HandleEvent(WmeEvent evt) => false;

    // Persistence
    public virtual void Persist(WmePersistenceManager persist)
    {
        persist.Transfer("Name", ref _name);
        persist.Transfer("Caption", ref _caption);
        persist.Transfer("Visible", ref _visible);
        persist.Transfer("Position", ref _position);
    }

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
```

### 6.3 WmeScriptable Interface

```csharp
namespace WME.Core.Objects;

public abstract class WmeScriptable : WmeBase
{
    private int _refCount;

    // Script variable storage
    protected Dictionary<string, WmeValue> _properties = new();

    public virtual WmeValue? GetProperty(string name)
    {
        return _properties.TryGetValue(name, out var value) ? value : null;
    }

    public virtual bool SetProperty(string name, WmeValue value)
    {
        _properties[name] = value;
        return true;
    }

    public virtual WmeValue? CallMethod(
        WmeScript script,
        WmeScriptStack stack,
        string name)
    {
        // Override in derived classes for specific methods
        return null;
    }

    public void AddRef() => Interlocked.Increment(ref _refCount);

    public void Release()
    {
        if (Interlocked.Decrement(ref _refCount) <= 0)
            Dispose();
    }
}
```

---

## 7. Script System Conversion

### 7.1 ANTLR4 Grammar (WmeScript.g4)

```antlr
grammar WmeScript;

// Parser Rules
script
    : statement* EOF
    ;

statement
    : variableDeclaration
    | functionDeclaration
    | methodDeclaration
    | externalDeclaration
    | eventHandler
    | ifStatement
    | whileStatement
    | forStatement
    | switchStatement
    | returnStatement
    | breakStatement
    | continueStatement
    | expressionStatement
    | block
    ;

variableDeclaration
    : ('var' | 'global' | 'const') IDENTIFIER ('=' expression)? ';'
    ;

functionDeclaration
    : 'function' IDENTIFIER '(' parameterList? ')' block
    ;

methodDeclaration
    : 'method' IDENTIFIER '(' parameterList? ')' block
    ;

externalDeclaration
    : 'external' STRING IDENTIFIER '(' typeList? ')' ';'
    ;

eventHandler
    : 'on' STRING block
    ;

ifStatement
    : 'if' '(' expression ')' statement ('else' statement)?
    ;

whileStatement
    : 'while' '(' expression ')' statement
    ;

forStatement
    : 'for' '(' forInit? ';' expression? ';' forUpdate? ')' statement
    ;

switchStatement
    : 'switch' '(' expression ')' '{' caseClause* defaultClause? '}'
    ;

returnStatement
    : 'return' expression? ';'
    ;

breakStatement
    : 'break' ';'
    ;

continueStatement
    : 'continue' ';'
    ;

block
    : '{' statement* '}'
    ;

expression
    : primary
    | expression '.' IDENTIFIER
    | expression '[' expression ']'
    | expression '(' argumentList? ')'
    | ('++' | '--') expression
    | expression ('++' | '--')
    | ('+' | '-' | '!' | '~') expression
    | expression ('*' | '/' | '%') expression
    | expression ('+' | '-') expression
    | expression ('<' | '>' | '<=' | '>=') expression
    | expression ('==' | '!=' | '===' | '!==') expression
    | expression '&&' expression
    | expression '||' expression
    | expression '?' expression ':' expression
    | expression assignmentOperator expression
    ;

primary
    : 'null'
    | 'true'
    | 'false'
    | 'this'
    | NUMBER
    | STRING
    | IDENTIFIER
    | '(' expression ')'
    | arrayLiteral
    ;

// Lexer Rules
IDENTIFIER  : [a-zA-Z_][a-zA-Z0-9_]* ;
NUMBER      : [0-9]+ ('.' [0-9]+)? ;
STRING      : '"' (~["\r\n] | '\\"')* '"' ;
WS          : [ \t\r\n]+ -> skip ;
COMMENT     : '//' ~[\r\n]* -> skip ;
BLOCK_COMMENT : '/*' .*? '*/' -> skip ;
```

### 7.2 Script Engine Implementation

```csharp
namespace WME.Scripting;

public class WmeScriptEngine : IWmeScriptEngine
{
    private readonly IWmeScriptCompiler _compiler;
    private readonly ILogger<WmeScriptEngine> _logger;

    private readonly List<WmeScript> _scripts = new();
    private readonly Dictionary<string, WmeScriptFunction> _globalFunctions = new();
    private readonly Dictionary<string, WmeValue> _globalVariables = new();

    public WmeScriptEngine(
        IWmeScriptCompiler compiler,
        ILogger<WmeScriptEngine> logger)
    {
        _compiler = compiler;
        _logger = logger;
    }

    public async Task<WmeScript?> LoadScriptAsync(string filename)
    {
        var source = await File.ReadAllTextAsync(filename);
        return CompileScript(source, filename);
    }

    public WmeScript? CompileScript(string source, string filename)
    {
        try
        {
            var bytecode = _compiler.Compile(source, filename);
            var script = new WmeScript(bytecode, this);
            _scripts.Add(script);
            return script;
        }
        catch (WmeCompilationException ex)
        {
            _logger.LogError(ex, "Script compilation failed: {Filename}", filename);
            return null;
        }
    }

    public void Update(TimeSpan deltaTime)
    {
        foreach (var script in _scripts.ToList())
        {
            if (script.State == ScriptState.Running)
            {
                script.Execute(deltaTime);
            }
            else if (script.State == ScriptState.Finished)
            {
                _scripts.Remove(script);
            }
        }
    }

    public WmeValue? CallMethod(
        WmeScriptable target,
        string methodName,
        params WmeValue[] args)
    {
        var stack = new WmeScriptStack();
        foreach (var arg in args.Reverse())
            stack.Push(arg);

        return target.CallMethod(null!, stack, methodName);
    }
}
```

### 7.3 Script Virtual Machine

```csharp
namespace WME.Scripting.Runtime;

public class WmeScriptVM
{
    private readonly WmeScriptStack _stack = new();
    private readonly WmeScriptStack _callStack = new();
    private int _instructionPointer;
    private WmeScriptable? _thisObject;

    public ScriptState State { get; private set; }
    public int CurrentLine { get; private set; }

    public WmeValue? Execute(WmeCompiledScript bytecode, WmeScriptable? thisObj = null)
    {
        _thisObject = thisObj;
        _instructionPointer = 0;
        State = ScriptState.Running;

        while (_instructionPointer < bytecode.Instructions.Count &&
               State == ScriptState.Running)
        {
            var instruction = bytecode.Instructions[_instructionPointer++];
            ExecuteInstruction(instruction, bytecode);
        }

        return _stack.Count > 0 ? _stack.Pop() : null;
    }

    private void ExecuteInstruction(WmeInstruction inst, WmeCompiledScript bytecode)
    {
        switch (inst.OpCode)
        {
            case OpCode.PushInt:
                _stack.Push(new WmeValue((int)inst.Operand!));
                break;

            case OpCode.PushString:
                _stack.Push(new WmeValue((string)inst.Operand!));
                break;

            case OpCode.PushNull:
                _stack.Push(WmeValue.Null);
                break;

            case OpCode.PushVar:
                var varName = (string)inst.Operand!;
                _stack.Push(GetVariable(varName));
                break;

            case OpCode.PopVar:
                var targetVar = (string)inst.Operand!;
                SetVariable(targetVar, _stack.Pop());
                break;

            case OpCode.Add:
                var b = _stack.Pop();
                var a = _stack.Pop();
                _stack.Push(a + b);
                break;

            case OpCode.Call:
                ExecuteCall((string)inst.Operand!, (int)inst.Operand2!);
                break;

            case OpCode.GetProperty:
                var obj = _stack.Pop().GetNative<WmeScriptable>();
                var prop = obj?.GetProperty((string)inst.Operand!);
                _stack.Push(prop ?? WmeValue.Null);
                break;

            case OpCode.SetProperty:
                var value = _stack.Pop();
                var target = _stack.Pop().GetNative<WmeScriptable>();
                target?.SetProperty((string)inst.Operand!, value);
                break;

            case OpCode.Jump:
                _instructionPointer = (int)inst.Operand!;
                break;

            case OpCode.JumpIfFalse:
                if (!_stack.Pop().GetBool())
                    _instructionPointer = (int)inst.Operand!;
                break;

            case OpCode.Return:
                State = ScriptState.Finished;
                break;

            // ... additional opcodes
        }
    }
}

public enum OpCode
{
    // Stack operations
    PushInt, PushFloat, PushString, PushBool, PushNull,
    PushVar, PopVar, PushVarRef, PopEmpty,
    PushThis, PopThis,

    // Arithmetic
    Add, Sub, Mul, Div, Mod, Neg,

    // Comparison
    Equal, NotEqual, StrictEqual, StrictNotEqual,
    Less, Greater, LessEqual, GreaterEqual,

    // Logic
    And, Or, Not,

    // Control flow
    Jump, JumpIfFalse, JumpIfTrue,
    Call, CallMethod, Return, ReturnEvent,

    // Object operations
    GetProperty, SetProperty, CreateObject
}
```

---

## 8. Graphics System Conversion

### 8.1 Renderer Interface

```csharp
namespace WME.Graphics;

public interface IWmeRenderer : IDisposable
{
    int Width { get; }
    int Height { get; }
    bool Windowed { get; }

    Task InitializeAsync();

    void BeginFrame();
    void EndFrame();

    // 2D Operations
    void DrawSurface(IWmeSurface surface, int x, int y,
        Rectangle? sourceRect = null,
        BlendMode blendMode = BlendMode.Normal,
        float alpha = 1.0f,
        bool mirrorX = false, bool mirrorY = false);

    void DrawSurfaceTransform(IWmeSurface surface,
        int x, int y, int hotX, int hotY,
        Rectangle? sourceRect,
        float scaleX, float scaleY,
        float rotation, float alpha,
        BlendMode blendMode,
        bool mirrorX, bool mirrorY);

    void DrawLine(int x1, int y1, int x2, int y2, Color color);
    void DrawRect(Rectangle rect, Color color, int width = 1);
    void FillRect(Rectangle rect, Color color);
    void FadeToColor(Color color, Rectangle? rect = null);

    // 3D Operations
    void Setup3D(WmeCamera3D camera);
    void Setup2D();
    void DrawModel(WmeModel3D model, Matrix4x4 transform);

    // Surface management
    IWmeSurface CreateSurface(int width, int height);
    IWmeSurface LoadSurface(string filename, Color? colorKey = null);

    // Screenshots
    WmeImage TakeScreenshot();

    // Hit testing
    WmeObject? GetObjectAt(int x, int y);
}

public enum BlendMode
{
    Normal,
    Additive,
    Subtractive,
    Multiply
}
```

### 8.2 OpenGL Renderer Implementation

```csharp
namespace WME.Graphics.OpenGL;

public class OpenGLRenderer : IWmeRenderer
{
    private readonly IWindow _window;
    private GL _gl = null!;
    private SKSurface? _skiaSurface;
    private GRContext? _grContext;

    private readonly Dictionary<string, OpenGLSurface> _surfaceCache = new();
    private readonly ILogger<OpenGLRenderer> _logger;

    public int Width => _window.Size.X;
    public int Height => _window.Size.Y;
    public bool Windowed { get; private set; } = true;

    public OpenGLRenderer(ILogger<OpenGLRenderer> logger)
    {
        _logger = logger;

        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(800, 600);
        options.Title = "Wintermute Engine";

        _window = Window.Create(options);
        _window.Load += OnLoad;
        _window.Render += OnRender;
        _window.Resize += OnResize;
    }

    public async Task InitializeAsync()
    {
        await Task.Run(() => _window.Initialize());
        _logger.LogInformation("OpenGL Renderer initialized: {Width}x{Height}",
            Width, Height);
    }

    private void OnLoad()
    {
        _gl = _window.CreateOpenGL();

        // Initialize Skia for 2D rendering
        var grInterface = GRGlInterface.Create();
        _grContext = GRContext.CreateGl(grInterface);

        CreateSkiaSurface();
    }

    private void CreateSkiaSurface()
    {
        var frameBufferInfo = new GRGlFramebufferInfo(0, (uint)GLEnum.Rgba8);
        var backendRenderTarget = new GRBackendRenderTarget(
            Width, Height, 0, 8, frameBufferInfo);

        _skiaSurface = SKSurface.Create(
            _grContext,
            backendRenderTarget,
            GRSurfaceOrigin.BottomLeft,
            SKColorType.Rgba8888);
    }

    public void BeginFrame()
    {
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    }

    public void EndFrame()
    {
        _skiaSurface?.Canvas.Flush();
        _grContext?.Flush();
        _window.SwapBuffers();
    }

    public void DrawSurface(IWmeSurface surface, int x, int y,
        Rectangle? sourceRect = null,
        BlendMode blendMode = BlendMode.Normal,
        float alpha = 1.0f,
        bool mirrorX = false, bool mirrorY = false)
    {
        if (surface is not OpenGLSurface glSurface)
            return;

        var canvas = _skiaSurface?.Canvas;
        if (canvas == null) return;

        using var paint = new SKPaint
        {
            Color = SKColors.White.WithAlpha((byte)(alpha * 255)),
            BlendMode = blendMode switch
            {
                BlendMode.Additive => SKBlendMode.Plus,
                BlendMode.Multiply => SKBlendMode.Multiply,
                _ => SKBlendMode.SrcOver
            }
        };

        var src = sourceRect.HasValue
            ? new SKRect(sourceRect.Value.X, sourceRect.Value.Y,
                sourceRect.Value.Right, sourceRect.Value.Bottom)
            : SKRect.Create(glSurface.Width, glSurface.Height);

        var dest = SKRect.Create(x, y, src.Width, src.Height);

        canvas.Save();

        if (mirrorX || mirrorY)
        {
            canvas.Translate(
                mirrorX ? dest.Right : dest.Left,
                mirrorY ? dest.Bottom : dest.Top);
            canvas.Scale(mirrorX ? -1 : 1, mirrorY ? -1 : 1);
            dest = SKRect.Create(0, 0, dest.Width, dest.Height);
        }

        canvas.DrawImage(glSurface.SkiaImage, src, dest, paint);
        canvas.Restore();
    }

    public IWmeSurface LoadSurface(string filename, Color? colorKey = null)
    {
        if (_surfaceCache.TryGetValue(filename, out var cached))
            return cached;

        var surface = new OpenGLSurface(filename, colorKey);
        _surfaceCache[filename] = surface;
        return surface;
    }

    public void Dispose()
    {
        foreach (var surface in _surfaceCache.Values)
            surface.Dispose();

        _skiaSurface?.Dispose();
        _grContext?.Dispose();
        _window.Dispose();
    }
}
```

### 8.3 Sprite System

```csharp
namespace WME.Graphics.Sprites;

public class WmeSprite : WmeObject
{
    private readonly List<WmeFrame> _frames = new();
    private int _currentFrame;
    private TimeSpan _frameTimer;

    public bool Looping { get; set; } = true;
    public bool Paused { get; set; }
    public bool Finished { get; private set; }
    public int CurrentFrameIndex => _currentFrame;
    public WmeFrame? CurrentFrame =>
        _currentFrame < _frames.Count ? _frames[_currentFrame] : null;

    public async Task<bool> LoadAsync(string filename, IWmeRenderer renderer)
    {
        // Parse sprite definition file
        var parser = new WmeSpriteParser();
        var definition = await parser.ParseAsync(filename);

        foreach (var frameDef in definition.Frames)
        {
            var frame = new WmeFrame
            {
                Delay = TimeSpan.FromMilliseconds(frameDef.Delay),
                HotspotX = frameDef.HotspotX,
                HotspotY = frameDef.HotspotY,
                MoveX = frameDef.MoveX,
                MoveY = frameDef.MoveY,
                IsKeyframe = frameDef.IsKeyframe
            };

            foreach (var subframeDef in frameDef.Subframes)
            {
                var surface = renderer.LoadSurface(
                    subframeDef.Filename,
                    subframeDef.ColorKey);

                frame.Subframes.Add(new WmeSubFrame
                {
                    Surface = surface,
                    SourceRect = subframeDef.SourceRect,
                    HotspotX = subframeDef.HotspotX,
                    HotspotY = subframeDef.HotspotY
                });
            }

            _frames.Add(frame);
        }

        return true;
    }

    public override void Update(TimeSpan deltaTime)
    {
        if (Paused || Finished || _frames.Count == 0)
            return;

        _frameTimer += deltaTime;

        var currentDelay = _frames[_currentFrame].Delay;
        if (_frameTimer >= currentDelay)
        {
            _frameTimer -= currentDelay;
            _currentFrame++;

            if (_currentFrame >= _frames.Count)
            {
                if (Looping)
                    _currentFrame = 0;
                else
                {
                    _currentFrame = _frames.Count - 1;
                    Finished = true;
                }
            }
        }
    }

    public override void Render(IWmeRenderer renderer)
    {
        var frame = CurrentFrame;
        if (frame == null) return;

        foreach (var subframe in frame.Subframes)
        {
            renderer.DrawSurface(
                subframe.Surface,
                Position.X - subframe.HotspotX,
                Position.Y - subframe.HotspotY,
                subframe.SourceRect);
        }
    }

    public void Reset()
    {
        _currentFrame = 0;
        _frameTimer = TimeSpan.Zero;
        Finished = false;
    }
}
```

---

## 9. Audio System Conversion

### 9.1 Sound Manager Interface

```csharp
namespace WME.Audio;

public interface IWmeSoundManager : IDisposable
{
    Task InitializeAsync();

    IWmeSoundBuffer? LoadSound(string filename, SoundType type, bool streamed = false);
    void UnloadSound(IWmeSoundBuffer sound);

    void SetMasterVolume(float volume);
    void SetTypeVolume(SoundType type, float volume);
    float GetTypeVolume(SoundType type);

    void PauseAll(bool includingMusic = false);
    void ResumeAll();
    void StopAll();

    void Update(TimeSpan deltaTime);
}

public interface IWmeSoundBuffer : IDisposable
{
    SoundType Type { get; }
    SoundState State { get; }
    bool IsLooping { get; set; }
    float Volume { get; set; }
    float Pan { get; set; }
    TimeSpan Position { get; set; }
    TimeSpan Duration { get; }

    void Play(bool loop = false);
    void Pause();
    void Resume();
    void Stop();
}

public enum SoundType
{
    SFX,
    Music,
    Speech
}

public enum SoundState
{
    Stopped,
    Playing,
    Paused
}
```

### 9.2 OpenAL Implementation

```csharp
namespace WME.Audio.OpenAL;

public class OpenALSoundManager : IWmeSoundManager
{
    private ALContext _context;
    private readonly List<OpenALSoundBuffer> _sounds = new();
    private readonly Dictionary<SoundType, float> _typeVolumes = new()
    {
        [SoundType.SFX] = 1.0f,
        [SoundType.Music] = 1.0f,
        [SoundType.Speech] = 1.0f
    };
    private float _masterVolume = 1.0f;

    private readonly IWmeFileManager _fileManager;
    private readonly ILogger<OpenALSoundManager> _logger;

    public OpenALSoundManager(
        IWmeFileManager fileManager,
        ILogger<OpenALSoundManager> logger)
    {
        _fileManager = fileManager;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        await Task.Run(() =>
        {
            var device = ALC.OpenDevice(null);
            _context = ALC.CreateContext(device, null);
            ALC.MakeContextCurrent(_context);
        });

        _logger.LogInformation("OpenAL audio initialized");
    }

    public IWmeSoundBuffer? LoadSound(string filename, SoundType type, bool streamed)
    {
        try
        {
            var stream = _fileManager.OpenFile(filename);
            if (stream == null) return null;

            IWmeAudioDecoder decoder = Path.GetExtension(filename).ToLower() switch
            {
                ".ogg" => new OggVorbisDecoder(stream),
                ".wav" => new WavDecoder(stream),
                _ => throw new NotSupportedException($"Audio format not supported: {filename}")
            };

            var buffer = new OpenALSoundBuffer(decoder, type, streamed);
            buffer.Volume = _typeVolumes[type] * _masterVolume;
            _sounds.Add(buffer);

            return buffer;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load sound: {Filename}", filename);
            return null;
        }
    }

    public void SetMasterVolume(float volume)
    {
        _masterVolume = Math.Clamp(volume, 0f, 1f);
        UpdateAllVolumes();
    }

    public void SetTypeVolume(SoundType type, float volume)
    {
        _typeVolumes[type] = Math.Clamp(volume, 0f, 1f);
        UpdateAllVolumes();
    }

    private void UpdateAllVolumes()
    {
        foreach (var sound in _sounds)
        {
            sound.Volume = _typeVolumes[sound.Type] * _masterVolume;
        }
    }

    public void Update(TimeSpan deltaTime)
    {
        // Update streaming buffers
        foreach (var sound in _sounds.Where(s => s.IsStreaming))
        {
            sound.UpdateStream();
        }

        // Clean up finished sounds
        _sounds.RemoveAll(s => s.State == SoundState.Stopped && !s.IsLooping);
    }

    public void Dispose()
    {
        foreach (var sound in _sounds)
            sound.Dispose();

        ALC.DestroyContext(_context);
    }
}
```

---

## 10. Development Tools Conversion

### 10.1 MAUI Architecture for Tools

```
tools/
├── WME.Tools.Common/              # Shared components
│   ├── ViewModels/
│   │   └── BaseViewModel.cs
│   ├── Services/
│   │   ├── IProjectService.cs
│   │   ├── ISettingsService.cs
│   │   └── IDialogService.cs
│   ├── Controls/
│   │   ├── PropertyEditor.cs
│   │   ├── TreeViewEx.cs
│   │   └── WmeCanvas.cs
│   └── Converters/
│
├── WME.WindowEditor/              # Window Editor (MAUI)
│   ├── Views/
│   │   ├── MainPage.xaml
│   │   ├── CanvasView.xaml
│   │   └── PropertyPanel.xaml
│   ├── ViewModels/
│   │   ├── MainViewModel.cs
│   │   ├── WindowViewModel.cs
│   │   └── ControlViewModel.cs
│   ├── Services/
│   │   └── WindowDocumentService.cs
│   └── App.xaml
```

### 10.2 Main ViewModel Pattern

```csharp
namespace WME.WindowEditor.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    private readonly IProjectService _projectService;
    private readonly IWindowDocumentService _documentService;
    private readonly ISettingsService _settingsService;

    [ObservableProperty]
    private WindowDocument? _currentDocument;

    [ObservableProperty]
    private WmeControlViewModel? _selectedControl;

    [ObservableProperty]
    private ObservableCollection<WmeControlViewModel> _controlTree = new();

    [ObservableProperty]
    private bool _isModified;

    public MainViewModel(
        IProjectService projectService,
        IWindowDocumentService documentService,
        ISettingsService settingsService)
    {
        _projectService = projectService;
        _documentService = documentService;
        _settingsService = settingsService;
    }

    [RelayCommand]
    private async Task NewDocumentAsync()
    {
        if (IsModified && !await ConfirmDiscardChangesAsync())
            return;

        CurrentDocument = _documentService.CreateNew();
        BuildControlTree();
        IsModified = false;
    }

    [RelayCommand]
    private async Task OpenDocumentAsync()
    {
        var result = await _dialogService.ShowOpenFileDialogAsync(
            "Window Files (*.window)|*.window");

        if (result != null)
        {
            CurrentDocument = await _documentService.OpenAsync(result);
            BuildControlTree();
            IsModified = false;
        }
    }

    [RelayCommand]
    private async Task SaveDocumentAsync()
    {
        if (CurrentDocument == null) return;

        if (string.IsNullOrEmpty(CurrentDocument.FilePath))
        {
            await SaveAsDocumentAsync();
            return;
        }

        await _documentService.SaveAsync(CurrentDocument);
        IsModified = false;
    }

    [RelayCommand]
    private void AddButton()
    {
        if (CurrentDocument?.Window == null) return;

        var button = new WmeButton
        {
            Name = GenerateUniqueName("button"),
            Position = new Point(10, 10),
            Size = new Size(100, 30)
        };

        CurrentDocument.Window.Children.Add(button);
        BuildControlTree();
        SelectedControl = FindViewModel(button);
        IsModified = true;
    }

    partial void OnSelectedControlChanged(WmeControlViewModel? value)
    {
        // Update property panel
        WeakReferenceMessenger.Default.Send(
            new ControlSelectedMessage(value?.Control));
    }
}
```

### 10.3 Property Editor Control

```csharp
namespace WME.Tools.Common.Controls;

public class PropertyEditor : ContentView
{
    public static readonly BindableProperty SelectedObjectProperty =
        BindableProperty.Create(
            nameof(SelectedObject),
            typeof(object),
            typeof(PropertyEditor),
            null,
            propertyChanged: OnSelectedObjectChanged);

    public object? SelectedObject
    {
        get => GetValue(SelectedObjectProperty);
        set => SetValue(SelectedObjectProperty, value);
    }

    private VerticalStackLayout _propertiesLayout;

    public PropertyEditor()
    {
        _propertiesLayout = new VerticalStackLayout { Spacing = 4 };
        Content = new ScrollView { Content = _propertiesLayout };
    }

    private static void OnSelectedObjectChanged(
        BindableObject bindable,
        object oldValue,
        object newValue)
    {
        ((PropertyEditor)bindable).BuildProperties(newValue);
    }

    private void BuildProperties(object? obj)
    {
        _propertiesLayout.Children.Clear();

        if (obj == null) return;

        var properties = obj.GetType()
            .GetProperties()
            .Where(p => p.GetCustomAttribute<BrowsableAttribute>()?.Browsable != false)
            .OrderBy(p => p.GetCustomAttribute<CategoryAttribute>()?.Category ?? "")
            .ThenBy(p => p.Name);

        string? currentCategory = null;

        foreach (var prop in properties)
        {
            var category = prop.GetCustomAttribute<CategoryAttribute>()?.Category;

            if (category != currentCategory)
            {
                currentCategory = category;
                _propertiesLayout.Children.Add(new Label
                {
                    Text = category ?? "Misc",
                    FontAttributes = FontAttributes.Bold,
                    Margin = new Thickness(0, 8, 0, 4)
                });
            }

            var editor = CreateEditorForProperty(obj, prop);
            _propertiesLayout.Children.Add(editor);
        }
    }

    private View CreateEditorForProperty(object obj, PropertyInfo prop)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star))
            }
        };

        var label = new Label
        {
            Text = prop.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? prop.Name,
            VerticalOptions = LayoutOptions.Center
        };
        grid.Add(label, 0);

        View editor = prop.PropertyType switch
        {
            Type t when t == typeof(string) => CreateStringEditor(obj, prop),
            Type t when t == typeof(int) => CreateIntEditor(obj, prop),
            Type t when t == typeof(bool) => CreateBoolEditor(obj, prop),
            Type t when t == typeof(Color) => CreateColorEditor(obj, prop),
            Type t when t.IsEnum => CreateEnumEditor(obj, prop),
            _ => new Label { Text = prop.GetValue(obj)?.ToString() }
        };

        grid.Add(editor, 1);
        return grid;
    }

    private View CreateStringEditor(object obj, PropertyInfo prop)
    {
        var entry = new Entry
        {
            Text = prop.GetValue(obj)?.ToString()
        };

        entry.TextChanged += (s, e) =>
        {
            if (prop.CanWrite)
                prop.SetValue(obj, e.NewTextValue);
        };

        return entry;
    }

    // Additional editor methods...
}
```

### 10.4 Canvas View for Visual Editing

```csharp
namespace WME.WindowEditor.Views;

public class WmeEditorCanvas : SKCanvasView
{
    private WmeWindow? _window;
    private WmeControl? _selectedControl;
    private bool _isDragging;
    private Point _dragStart;
    private Point _originalPosition;

    public event EventHandler<WmeControl?>? ControlSelected;
    public event EventHandler<WmeControl>? ControlMoved;

    public WmeWindow? Window
    {
        get => _window;
        set
        {
            _window = value;
            InvalidateSurface();
        }
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.DarkGray);

        if (_window == null) return;

        // Draw window background
        using var bgPaint = new SKPaint { Color = SKColors.LightGray };
        canvas.DrawRect(0, 0, _window.Width, _window.Height, bgPaint);

        // Draw controls
        foreach (var control in _window.Children)
        {
            DrawControl(canvas, control);
        }

        // Draw selection handles
        if (_selectedControl != null)
        {
            DrawSelectionHandles(canvas, _selectedControl);
        }
    }

    private void DrawControl(SKCanvas canvas, WmeControl control)
    {
        var rect = new SKRect(
            control.Position.X,
            control.Position.Y,
            control.Position.X + control.Size.Width,
            control.Position.Y + control.Size.Height);

        using var paint = new SKPaint
        {
            Color = control switch
            {
                WmeButton => SKColors.LightBlue,
                WmeTextBox => SKColors.White,
                WmeLabel => SKColors.Transparent,
                _ => SKColors.LightGray
            },
            Style = SKPaintStyle.Fill
        };

        canvas.DrawRect(rect, paint);

        // Draw border
        paint.Style = SKPaintStyle.Stroke;
        paint.Color = SKColors.Black;
        canvas.DrawRect(rect, paint);

        // Draw text
        if (!string.IsNullOrEmpty(control.Text))
        {
            using var textPaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 14
            };
            canvas.DrawText(control.Text, rect.Left + 4, rect.MidY + 5, textPaint);
        }
    }

    private void DrawSelectionHandles(SKCanvas canvas, WmeControl control)
    {
        var rect = new SKRect(
            control.Position.X - 3,
            control.Position.Y - 3,
            control.Position.X + control.Size.Width + 3,
            control.Position.Y + control.Size.Height + 3);

        using var paint = new SKPaint
        {
            Color = SKColors.Blue,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2
        };

        canvas.DrawRect(rect, paint);

        // Corner handles
        var handleSize = 6;
        paint.Style = SKPaintStyle.Fill;

        var handles = new[]
        {
            new SKPoint(rect.Left, rect.Top),
            new SKPoint(rect.Right, rect.Top),
            new SKPoint(rect.Left, rect.Bottom),
            new SKPoint(rect.Right, rect.Bottom)
        };

        foreach (var handle in handles)
        {
            canvas.DrawRect(
                handle.X - handleSize / 2,
                handle.Y - handleSize / 2,
                handleSize, handleSize, paint);
        }
    }
}
```

---

## 11. File Format Compatibility

### 11.1 Package File Reader (DCP)

```csharp
namespace WME.Formats.Packages;

public class DcpPackageReader : IWmePackageReader
{
    private const uint Magic1 = 0xDEC0ADDE;
    private const uint Magic2 = 0x4B4E554A; // "JUNK"
    private const uint CurrentVersion = 0x00000200;

    private readonly Dictionary<string, DcpEntry> _entries = new();
    private Stream? _stream;

    public async Task<bool> OpenAsync(string filename)
    {
        _stream = File.OpenRead(filename);
        using var reader = new BinaryReader(_stream, Encoding.UTF8, leaveOpen: true);

        // Read header
        var magic1 = reader.ReadUInt32();
        var magic2 = reader.ReadUInt32();

        if (magic1 != Magic1 || magic2 != Magic2)
            return false;

        var version = reader.ReadUInt32();
        var gameVersion = reader.ReadUInt32();
        var priority = reader.ReadByte();
        var cd = reader.ReadByte();
        var masterIndex = reader.ReadBoolean();
        var creationTime = reader.ReadInt32();
        var description = reader.ReadFixedString(100);
        var numDirs = reader.ReadUInt32();

        // Read directory entries
        for (int d = 0; d < numDirs; d++)
        {
            var dirNameLength = reader.ReadByte();
            var dirName = reader.ReadFixedString(dirNameLength);
            var dirCd = reader.ReadByte();
            var numEntries = reader.ReadUInt32();

            for (int e = 0; e < numEntries; e++)
            {
                var nameLength = reader.ReadByte();
                var name = reader.ReadFixedString(nameLength);

                var entry = new DcpEntry
                {
                    FullPath = Path.Combine(dirName, name),
                    Offset = reader.ReadUInt32(),
                    Length = reader.ReadUInt32(),
                    CompressedLength = reader.ReadUInt32(),
                    Flags = reader.ReadUInt32(),
                    TimeDate1 = reader.ReadUInt32(),
                    TimeDate2 = version >= 0x00000200 ? reader.ReadUInt32() : 0
                };

                _entries[entry.FullPath.ToLowerInvariant()] = entry;
            }
        }

        return true;
    }

    public Stream? OpenFile(string path)
    {
        if (!_entries.TryGetValue(path.ToLowerInvariant(), out var entry))
            return null;

        _stream!.Seek(entry.Offset, SeekOrigin.Begin);

        var data = new byte[entry.CompressedLength > 0
            ? entry.CompressedLength
            : entry.Length];

        _stream.Read(data, 0, data.Length);

        // Check for ZCMP compression
        if (data.Length >= 4 &&
            BitConverter.ToUInt32(data, 0) == 0x504D435A) // "ZCMP"
        {
            return DecompressZcmp(data, (int)entry.Length);
        }

        return new MemoryStream(data);
    }

    private Stream DecompressZcmp(byte[] data, int uncompressedSize)
    {
        var output = new byte[uncompressedSize];

        using var compressedStream = new MemoryStream(data, 8, data.Length - 8);
        using var deflate = new DeflateStream(compressedStream, CompressionMode.Decompress);

        deflate.Read(output, 0, uncompressedSize);

        return new MemoryStream(output);
    }
}
```

### 11.2 Scene File Parser

```csharp
namespace WME.Formats.Scenes;

public class SceneParser
{
    public async Task<WmeScene> ParseAsync(Stream stream)
    {
        using var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync();

        var tokenizer = new DefinitionTokenizer(content);
        var scene = new WmeScene();

        while (tokenizer.HasMore())
        {
            var token = tokenizer.GetToken();

            switch (token.ToUpperInvariant())
            {
                case "SCENE":
                    ParseSceneBlock(tokenizer, scene);
                    break;
            }
        }

        return scene;
    }

    private void ParseSceneBlock(DefinitionTokenizer tokenizer, WmeScene scene)
    {
        tokenizer.Expect("{");

        while (!tokenizer.IsNext("}"))
        {
            var key = tokenizer.GetToken();

            switch (key.ToUpperInvariant())
            {
                case "NAME":
                    tokenizer.Expect("=");
                    scene.Name = tokenizer.GetString();
                    break;

                case "WIDTH":
                    tokenizer.Expect("=");
                    scene.Width = tokenizer.GetInt();
                    break;

                case "HEIGHT":
                    tokenizer.Expect("=");
                    scene.Height = tokenizer.GetInt();
                    break;

                case "LAYER":
                    var layer = ParseLayer(tokenizer);
                    scene.Layers.Add(layer);
                    break;

                case "WAYPOINTS":
                    var waypoints = ParseWaypointGroup(tokenizer);
                    scene.WaypointGroups.Add(waypoints);
                    break;

                case "SCALE_LEVEL":
                    var scale = ParseScaleLevel(tokenizer);
                    scene.ScaleLevels.Add(scale);
                    break;

                case "SCRIPT":
                    tokenizer.Expect("=");
                    scene.Scripts.Add(tokenizer.GetString());
                    break;
            }
        }

        tokenizer.Expect("}");
    }
}
```

### 11.3 Save Game Compatibility

```csharp
namespace WME.Formats.SaveGames;

public class SaveGameManager
{
    private const int CurrentVersion = 0x010A02; // 1.10.2
    private const int MinSupportedVersion = 0x010A01; // 1.10.1

    public async Task<SaveGameInfo?> LoadAsync(string filename)
    {
        using var stream = File.OpenRead(filename);
        using var reader = new BinaryReader(stream);

        // Read header
        var magic = reader.ReadUInt32();
        if (magic != 0xDEC0ADDE)
            return null;

        var version = reader.ReadInt32();
        if (version < MinSupportedVersion)
        {
            throw new WmeException(
                $"Save game version {version:X} is too old. Minimum supported: {MinSupportedVersion:X}");
        }

        var info = new SaveGameInfo
        {
            Version = version,
            Name = reader.ReadWmeString(),
            Description = reader.ReadWmeString(),
            Timestamp = DateTime.FromBinary(reader.ReadInt64())
        };

        // Read thumbnail if present
        var thumbSize = reader.ReadInt32();
        if (thumbSize > 0)
        {
            var thumbData = reader.ReadBytes(thumbSize);
            info.Thumbnail = await LoadThumbnailAsync(thumbData);
        }

        return info;
    }

    public async Task SaveAsync(IWmeGame game, string filename, string description)
    {
        using var stream = File.Create(filename);
        using var writer = new BinaryWriter(stream);

        // Write header
        writer.Write(0xDEC0ADDE);
        writer.Write(CurrentVersion);
        writer.WriteWmeString(Path.GetFileNameWithoutExtension(filename));
        writer.WriteWmeString(description);
        writer.Write(DateTime.Now.ToBinary());

        // Write thumbnail
        var thumbnail = game.Renderer.TakeScreenshot();
        var thumbData = await CreateThumbnailAsync(thumbnail);
        writer.Write(thumbData.Length);
        writer.Write(thumbData);

        // Serialize game state
        var persist = new WmePersistenceManager(writer, isSaving: true);
        await game.PersistAsync(persist);
    }
}
```

---

## 12. Plugin System Conversion

### 12.1 Modern Plugin Interface

```csharp
namespace WME.Plugins;

/// <summary>
/// Base interface for all WME plugins
/// </summary>
public interface IWmePlugin : IDisposable
{
    /// <summary>Plugin metadata</summary>
    WmePluginInfo Info { get; }

    /// <summary>Initialize the plugin with game context</summary>
    Task InitializeAsync(IWmeGame game);

    /// <summary>Called when plugin is being unloaded</summary>
    Task ShutdownAsync();
}

/// <summary>
/// Plugin that provides scriptable object types
/// </summary>
public interface IWmeObjectPlugin : IWmePlugin
{
    /// <summary>Classes provided by this plugin</summary>
    IEnumerable<string> ProvidedClasses { get; }

    /// <summary>Create instance of plugin class</summary>
    WmeScriptable CreateObject(string className, WmeScriptStack constructorArgs);
}

/// <summary>
/// Plugin that responds to game events
/// </summary>
public interface IWmeEventPlugin : IWmePlugin
{
    /// <summary>Events this plugin subscribes to</summary>
    IEnumerable<WmeEventType> SubscribedEvents { get; }

    /// <summary>Handle game event</summary>
    void OnEvent(WmeEventType eventType, object? data);
}

public record WmePluginInfo(
    string Name,
    string Description,
    Version Version,
    Version MinEngineVersion);

public enum WmeEventType
{
    Update,
    SceneDrawBegin,
    SceneDrawEnd,
    SceneInit,
    SceneShutdown,
    BeforeSave,
    AfterLoad
}
```

### 12.2 Plugin Loader

```csharp
namespace WME.Plugins;

public class WmePluginManager : IWmePluginManager
{
    private readonly List<IWmePlugin> _plugins = new();
    private readonly Dictionary<string, IWmeObjectPlugin> _objectProviders = new();
    private readonly ILogger<WmePluginManager> _logger;

    public WmePluginManager(ILogger<WmePluginManager> logger)
    {
        _logger = logger;
    }

    public async Task LoadPluginsAsync(string pluginDirectory)
    {
        if (!Directory.Exists(pluginDirectory))
            return;

        foreach (var dll in Directory.GetFiles(pluginDirectory, "*.dll"))
        {
            try
            {
                await LoadPluginAsync(dll);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load plugin: {Path}", dll);
            }
        }
    }

    private async Task LoadPluginAsync(string path)
    {
        var context = new PluginLoadContext(path);
        var assembly = context.LoadFromAssemblyPath(path);

        var pluginTypes = assembly.GetTypes()
            .Where(t => typeof(IWmePlugin).IsAssignableFrom(t) && !t.IsAbstract);

        foreach (var type in pluginTypes)
        {
            var plugin = (IWmePlugin)Activator.CreateInstance(type)!;

            _logger.LogInformation(
                "Loaded plugin: {Name} v{Version}",
                plugin.Info.Name,
                plugin.Info.Version);

            _plugins.Add(plugin);

            if (plugin is IWmeObjectPlugin objectPlugin)
            {
                foreach (var className in objectPlugin.ProvidedClasses)
                {
                    _objectProviders[className] = objectPlugin;
                }
            }
        }
    }

    public async Task InitializeAllAsync(IWmeGame game)
    {
        foreach (var plugin in _plugins)
        {
            await plugin.InitializeAsync(game);
        }
    }

    public WmeScriptable? CreatePluginObject(string className, WmeScriptStack args)
    {
        if (_objectProviders.TryGetValue(className, out var provider))
        {
            return provider.CreateObject(className, args);
        }
        return null;
    }

    public void RaiseEvent(WmeEventType eventType, object? data = null)
    {
        foreach (var plugin in _plugins.OfType<IWmeEventPlugin>())
        {
            if (plugin.SubscribedEvents.Contains(eventType))
            {
                plugin.OnEvent(eventType, data);
            }
        }
    }
}
```

---

## 13. Implementation Phases

### Phase 1: Foundation (Core Infrastructure)

**Duration Estimate:** Foundation phase

**Deliverables:**
- [ ] Solution structure and project scaffolding
- [ ] Core interfaces (IWmeGame, IWmeRenderer, IWmeSoundManager)
- [ ] WmeObject, WmeScriptable base classes
- [ ] Basic file manager with package support
- [ ] Settings and configuration system
- [ ] Logging infrastructure
- [ ] Unit test framework setup

**Exit Criteria:**
- All core interfaces defined
- Basic DI container configured
- Can load and read DCP packages
- 80%+ test coverage on core classes

### Phase 2: Graphics System

**Duration Estimate:** Graphics phase

**Deliverables:**
- [ ] OpenGL renderer implementation
- [ ] Surface and texture management
- [ ] Sprite system with animation
- [ ] 2D rendering pipeline
- [ ] Basic 3D rendering (models, cameras)
- [ ] Blend modes and effects
- [ ] Screenshot capture

**Exit Criteria:**
- Can render sprites and animations
- Performance benchmarks meet 60 FPS target
- All blend modes working
- Memory management verified

### Phase 3: Audio System

**Duration Estimate:** Audio phase

**Deliverables:**
- [ ] OpenAL sound manager
- [ ] WAV decoder
- [ ] OGG Vorbis decoder
- [ ] Streaming audio support
- [ ] Volume and pan controls
- [ ] Sound type management (SFX, Music, Speech)

**Exit Criteria:**
- All audio formats playing correctly
- No audio glitches or latency issues
- Streaming works for large files

### Phase 4: Script System

**Duration Estimate:** Scripting phase

**Deliverables:**
- [ ] ANTLR4 grammar for WME script
- [ ] Script compiler
- [ ] Bytecode format (compatible with original)
- [ ] Virtual machine implementation
- [ ] Script debugger hooks
- [ ] External function support

**Exit Criteria:**
- Can compile and run original WME scripts
- All opcodes implemented
- Debugger can step through code
- Performance within 2x of original

### Phase 5: Adventure Module

**Duration Estimate:** Adventure module phase

**Deliverables:**
- [ ] Scene management
- [ ] Actor/entity system
- [ ] Pathfinding (A* implementation)
- [ ] Inventory system
- [ ] Dialog/response system
- [ ] Save/load game functionality

**Exit Criteria:**
- Can load and display original scenes
- Characters move and animate
- Dialogs work correctly
- Save games load in both old and new engine

### Phase 6: UI System

**Duration Estimate:** UI phase

**Deliverables:**
- [ ] Window system
- [ ] Button, text, edit controls
- [ ] Entity display
- [ ] Inventory/response boxes
- [ ] Focus and input handling

**Exit Criteria:**
- All UI control types working
- Input handling correct
- Original UI definitions load correctly

### Phase 7: Development Tools

**Duration Estimate:** Tools phase

**Deliverables:**
- [ ] Common tools library
- [ ] Window Editor (MAUI)
- [ ] Sprite Editor (MAUI)
- [ ] Scene Editor (MAUI)
- [ ] String Table Manager (MAUI)
- [ ] Project Manager (MAUI)

**Exit Criteria:**
- All tools functional on Windows and macOS
- Can create and edit all asset types
- Output compatible with engine

### Phase 8: Integration & Polish

**Duration Estimate:** Integration phase

**Deliverables:**
- [ ] Full game runtime
- [ ] Plugin system
- [ ] Performance optimization
- [ ] Cross-platform testing
- [ ] Documentation
- [ ] Sample game

**Exit Criteria:**
- Existing WME games run without modification
- Performance meets targets on all platforms
- All documentation complete

---

## 14. Risk Assessment

### 14.1 Technical Risks

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Script compatibility issues | Medium | High | Extensive test suite with original scripts |
| Graphics performance | Medium | Medium | Profile early, optimize hot paths |
| Audio latency | Low | Medium | Use proven OpenAL patterns |
| Save game compatibility | Medium | High | Version detection and migration code |
| 3D rendering complexity | High | Medium | Phase 3D features, start with 2D |
| Cross-platform issues | Medium | Medium | CI/CD with multi-platform builds |

### 14.2 Project Risks

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Scope creep | High | High | Strict phase gates, feature freeze |
| Underestimated complexity | Medium | High | Prototype critical paths early |
| Dependency issues | Low | Medium | Pin versions, regular updates |
| Testing gaps | Medium | High | TDD approach, coverage requirements |

### 14.3 Compatibility Risks

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Edge cases in file formats | High | Medium | Comprehensive format test suite |
| Script behavior differences | Medium | High | Behavioral test suite |
| Plugin API changes | Medium | Medium | Adapter layer for old plugins |
| Platform-specific bugs | Medium | Low | Platform abstraction layer |

---

## 15. Success Criteria

### 15.1 Functional Requirements

- [ ] **F1:** Load and run existing WME 1.x games without modification
- [ ] **F2:** All script functions behave identically to original engine
- [ ] **F3:** Save games from original engine load correctly
- [ ] **F4:** All development tools create compatible output
- [ ] **F5:** Plugin API supports existing plugins (with adapter)

### 15.2 Performance Requirements

| Metric | Target | Measurement |
|--------|--------|-------------|
| Frame rate | 60 FPS | Benchmark suite |
| Load time | < 5s for typical scene | Automated tests |
| Memory usage | < 500 MB baseline | Profiling |
| Script execution | Within 2x of original | Benchmark scripts |

### 15.3 Quality Requirements

| Metric | Target |
|--------|--------|
| Unit test coverage | > 80% |
| Integration test coverage | > 70% |
| Static analysis warnings | 0 critical, < 10 warnings |
| Documentation coverage | 100% public APIs |

### 15.4 Platform Support

| Platform | Runtime | Tools |
|----------|---------|-------|
| Windows 10+ | ✅ Required | ✅ Required |
| macOS 12+ | ✅ Required | ✅ Required |
| Linux (Ubuntu 22.04+) | ✅ Required | ✅ Required |
| iOS 15+ | ⭕ Optional | ❌ N/A |
| Android 10+ | ⭕ Optional | ❌ N/A |

---

## 16. Appendices

### Appendix A: Original Class Reference

See `REPOSITORY_ANALYSIS.md` for complete original class documentation.

### Appendix B: File Format Specifications

#### B.1 DCP Package Header
```
Offset  Size  Description
0x00    4     Magic1 (0xDEC0ADDE)
0x04    4     Magic2 (0x4B4E554A)
0x08    4     Package Version
0x0C    4     Game Version
0x10    1     Priority
0x11    1     CD Number
0x12    1     Master Index Flag
0x13    4     Creation Time
0x17    100   Description
0x7B    4     Number of Directories
```

#### B.2 Script Bytecode Header
```
Offset  Size  Description
0x00    4     Magic (0xDEC0ADDE)
0x04    4     Version (0x0102)
0x08    4     Code Start Offset
0x0C    4     Function Table Offset
0x10    4     Symbol Table Offset
0x14    4     Event Table Offset
0x18    4     Externals Table Offset
0x1C    4     Method Table Offset
```

### Appendix C: Script Opcodes

| Code | Name | Description |
|------|------|-------------|
| 0x00 | NOP | No operation |
| 0x01 | PUSH_INT | Push integer constant |
| 0x02 | PUSH_FLOAT | Push float constant |
| 0x03 | PUSH_STRING | Push string constant |
| 0x04 | PUSH_NULL | Push null value |
| 0x05 | PUSH_VAR | Push variable value |
| 0x06 | POP_VAR | Pop to variable |
| 0x10 | ADD | Addition |
| 0x11 | SUB | Subtraction |
| 0x12 | MUL | Multiplication |
| 0x13 | DIV | Division |
| 0x20 | JMP | Unconditional jump |
| 0x21 | JMP_FALSE | Jump if false |
| 0x30 | CALL | Call function |
| 0x31 | RET | Return from function |
| 0x40 | GET_PROP | Get object property |
| 0x41 | SET_PROP | Set object property |

### Appendix D: Glossary

| Term | Definition |
|------|------------|
| WME | Wintermute Engine |
| DCP | Dead:Code Package (compressed game data) |
| Scene | A game location containing layers and objects |
| Actor | An animated character that can move and talk |
| Entity | A static or animated object in a scene |
| Region | A defined area in a scene (walkable, blocked, etc.) |
| Sprite | An animated image sequence |
| Script | WME scripting language source/bytecode |

---

## Document History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-12-05 | Claude | Initial specification |

---

*End of Specification*
