# Wintermute Engine .NET 10 / MAUI Conversion - TODO

**Project Codename:** WME.NET
**Document Version:** 1.0
**Last Updated:** 2025-12-05

---

## Overview

This document tracks all phases and tasks required to convert Wintermute Engine from C++/.NET Framework 3.5 to C#/.NET 10/MAUI as defined in SPECIFICATION.md.

**Legend:**
- [ ] = Not started
- [✅] = Completed and verified

---

## Phase 1: Foundation (Core Infrastructure)

### Project Structure

- [✅] Create solution structure (WintermuteEngine.sln)
- [✅] Create WME.Core project
- [✅] Create WME.Graphics project
- [✅] Create WME.Graphics.OpenGL project
- [✅] Create WME.Audio project
- [✅] Create WME.Audio.OpenAL project
- [✅] Create WME.Adventure project
- [✅] Create WME.Scripting project
- [✅] Create WME.Formats project
- [✅] Create WME.Plugins project
- [✅] Create WME.Runtime project
- [✅] Configure .NET 10 target framework for all projects
- [✅] Add NuGet package references (Microsoft.Extensions.DependencyInjection, etc.)

### Core Interfaces

- [✅] Define IWmeGame interface
- [ ] Define IWmeRenderer interface
- [ ] Define IWmeSoundManager interface
- [ ] Define IWmeScriptEngine interface
- [ ] Define IWmeFileManager interface
- [ ] Define IWmeResourceManager interface
- [ ] Define IWmeSurface interface
- [ ] Define IWmeSoundBuffer interface

### Base Classes

- [ ] Implement WmeBase class
- [ ] Implement WmeScriptable class
- [ ] Implement WmeObject class
- [ ] Implement WmeObjectCollection<T> class
- [ ] Implement WmeValue class (for scripting)
- [ ] Implement WmeScriptStack class

### File Management

- [ ] Implement WmeFileManager class
- [ ] Implement DCP package reader (DcpPackageReader)
- [ ] Implement file system abstraction
- [ ] Implement resource caching (WmeResourceManager)
- [ ] Add support for ZIP/deflate compression
- [ ] Implement ZCMP decompression

### Configuration & Logging

- [ ] Set up Microsoft.Extensions.Logging infrastructure
- [ ] Implement settings/configuration system
- [ ] Create configuration file format
- [ ] Implement IWmeSettings interface

### Dependency Injection

- [ ] Create WmeServices registration class
- [ ] Configure DI container
- [ ] Register all core services

### Unit Tests

- [ ] Create WME.Core.Tests project
- [ ] Set up xUnit framework
- [ ] Add tests for WmeBase class
- [ ] Add tests for WmeScriptable class
- [ ] Add tests for WmeObject class
- [ ] Add tests for DcpPackageReader
- [ ] Add tests for WmeFileManager
- [ ] Verify 80%+ test coverage for Phase 1

---

## Phase 2: Graphics System

### Renderer Interface Implementation

- [ ] Implement IWmeRenderer with Silk.NET OpenGL
- [ ] Create OpenGLRenderer class
- [ ] Initialize OpenGL context
- [ ] Implement window creation with Silk.NET.Windowing
- [ ] Implement BeginFrame/EndFrame methods

### Skia Integration

- [ ] Integrate SkiaSharp for 2D rendering
- [ ] Create GRContext for OpenGL
- [ ] Implement SKSurface management
- [ ] Create framebuffer integration

### Surface Management

- [ ] Implement IWmeSurface interface
- [ ] Create OpenGLSurface class
- [ ] Implement surface creation
- [ ] Implement surface loading from files
- [ ] Add color key support
- [ ] Implement surface caching
- [ ] Add texture management

### Image Loading

- [ ] Integrate SixLabors.ImageSharp
- [ ] Add PNG format support
- [ ] Add JPEG format support
- [ ] Add BMP format support
- [ ] Add TGA format support (if needed)

### 2D Rendering Operations

- [ ] Implement DrawSurface method
- [ ] Implement DrawSurfaceTransform method (rotation, scaling)
- [ ] Implement DrawLine method
- [ ] Implement DrawRect method
- [ ] Implement FillRect method
- [ ] Implement FadeToColor method
- [ ] Add mirror X/Y support

### Blend Modes

- [ ] Implement Normal blend mode
- [ ] Implement Additive blend mode
- [ ] Implement Subtractive blend mode
- [ ] Implement Multiply blend mode

### Sprite System

- [ ] Implement WmeSprite class
- [ ] Implement WmeFrame class
- [ ] Implement WmeSubFrame class
- [ ] Create sprite definition parser (WmeSpriteParser)
- [ ] Implement frame animation system
- [ ] Add hotspot support
- [ ] Add keyframe support
- [ ] Implement sprite looping

### 3D Rendering (Basic)

- [ ] Implement WmeCamera3D class
- [ ] Implement WmeModel3D class
- [ ] Implement WmeLight3D class
- [ ] Implement Setup3D method
- [ ] Implement Setup2D method
- [ ] Implement DrawModel method

### Utilities

- [ ] Implement screenshot capture (TakeScreenshot)
- [ ] Implement hit testing (GetObjectAt)

### Graphics Tests

- [ ] Create WME.Graphics.Tests project
- [ ] Add rendering tests
- [ ] Add sprite animation tests
- [ ] Add blend mode tests
- [ ] Performance benchmarks (60 FPS target)
- [ ] Memory leak tests

---

## Phase 3: Audio System

### Sound Manager

- [ ] Implement IWmeSoundManager with OpenAL
- [ ] Create OpenALSoundManager class
- [ ] Initialize OpenAL context
- [ ] Implement device enumeration
- [ ] Implement InitializeAsync method

### Sound Buffer

- [ ] Implement IWmeSoundBuffer interface
- [ ] Create OpenALSoundBuffer class
- [ ] Implement Play/Pause/Resume/Stop methods
- [ ] Implement volume control
- [ ] Implement pan control
- [ ] Add looping support

### Audio Decoders

- [ ] Create IWmeAudioDecoder interface
- [ ] Implement WAV decoder (WavDecoder)
- [ ] Implement OGG Vorbis decoder (OggVorbisDecoder) with NVorbis
- [ ] Add Theora video audio support (TheoraDecoder) with FFmpeg.AutoGen

### Streaming Audio

- [ ] Implement streaming buffer management
- [ ] Add buffer queuing for OpenAL
- [ ] Implement UpdateStream method
- [ ] Test with large audio files

### Sound Type Management

- [ ] Implement SoundType enum (SFX, Music, Speech)
- [ ] Implement per-type volume control
- [ ] Implement master volume control
- [ ] Add SetTypeVolume method
- [ ] Add GetTypeVolume method

### Audio Management

- [ ] Implement PauseAll method
- [ ] Implement ResumeAll method
- [ ] Implement StopAll method
- [ ] Implement sound cleanup
- [ ] Add memory management for audio buffers

### Audio Tests

- [ ] Create WME.Audio.Tests project
- [ ] Add decoder tests for WAV
- [ ] Add decoder tests for OGG
- [ ] Test streaming functionality
- [ ] Test volume controls
- [ ] Test for audio glitches/latency

---

## Phase 4: Script System

### Grammar Definition

- [ ] Install ANTLR4 NuGet package
- [ ] Create WmeScript.g4 grammar file
- [ ] Define parser rules (statements, expressions)
- [ ] Define lexer rules (tokens, keywords)
- [ ] Generate C# parser/lexer code
- [ ] Test grammar with original WME scripts

### Script Compiler

- [ ] Implement IWmeScriptCompiler interface
- [ ] Create WmeScriptCompiler class
- [ ] Implement ANTLR4 visitor for AST traversal
- [ ] Implement bytecode generation
- [ ] Create WmeCompiledScript class
- [ ] Define WmeInstruction struct
- [ ] Implement OpCode enum

### Bytecode Format

- [ ] Define bytecode header format
- [ ] Implement function table
- [ ] Implement symbol table
- [ ] Implement event table
- [ ] Implement externals table
- [ ] Implement method table
- [ ] Ensure compatibility with original bytecode

### Virtual Machine

- [ ] Implement WmeScriptVM class
- [ ] Implement instruction execution loop
- [ ] Implement stack operations (PUSH/POP)
- [ ] Implement arithmetic operations (ADD, SUB, MUL, DIV, MOD)
- [ ] Implement comparison operations (EQUAL, LESS, GREATER, etc.)
- [ ] Implement logical operations (AND, OR, NOT)
- [ ] Implement control flow (JUMP, JUMP_IF_FALSE, JUMP_IF_TRUE)
- [ ] Implement function calls (CALL, RETURN)
- [ ] Implement method calls (CALL_METHOD)
- [ ] Implement property access (GET_PROPERTY, SET_PROPERTY)

### Script Engine

- [ ] Implement IWmeScriptEngine interface
- [ ] Create WmeScriptEngine class
- [ ] Implement script loading (LoadScriptAsync)
- [ ] Implement script compilation (CompileScript)
- [ ] Implement script execution management
- [ ] Add global function registration
- [ ] Add global variable management
- [ ] Implement Update method for script processing

### Script Types

- [ ] Implement WmeScript class
- [ ] Implement WmeScriptFunction class
- [ ] Implement ScriptState enum
- [ ] Add script reference counting

### External Functions

- [ ] Create external function registration system
- [ ] Implement built-in functions (Game.*, Debug.*, etc.)
- [ ] Add external function calling from scripts

### Debugger Support

- [ ] Add breakpoint support (hooks)
- [ ] Add step-through capability
- [ ] Implement variable inspection
- [ ] Add call stack tracking

### Script Tests

- [ ] Create WME.Scripting.Tests project
- [ ] Add grammar tests
- [ ] Add compiler tests
- [ ] Add VM instruction tests
- [ ] Test with original WME scripts
- [ ] Performance benchmarks (within 2x of original)
- [ ] Add error handling tests

---

## Phase 5: Adventure Module

### Scene Management

- [ ] Implement IWmeSceneManager interface
- [ ] Create WmeSceneManager class
- [ ] Implement WmeScene class
- [ ] Implement WmeLayer class
- [ ] Create scene file parser (SceneParser)
- [ ] Implement scene loading
- [ ] Implement scene rendering
- [ ] Add viewport management

### Regions

- [ ] Implement WmeRegion class
- [ ] Add region types (walkable, blocked, decoration)
- [ ] Implement point-in-region testing
- [ ] Add region scripts

### Scale Levels

- [ ] Implement WmeScaleLevel class
- [ ] Add scale calculation based on Y position
- [ ] Integrate with actor rendering

### Actors

- [ ] Implement WmeActor class
- [ ] Add actor movement
- [ ] Add actor animation (walk, talk, idle)
- [ ] Implement talk() method
- [ ] Implement turn/face direction
- [ ] Add actor scripts

### Entities

- [ ] Implement WmeEntity class
- [ ] Add entity activation
- [ ] Add entity animation
- [ ] Add entity scripts

### Items

- [ ] Implement WmeItem class
- [ ] Implement WmeInventory class
- [ ] Implement WmeInventoryBox class (UI)
- [ ] Add item pickup/drop
- [ ] Add item usage
- [ ] Add item combining

### Pathfinding

- [ ] Implement IWmePathfinder interface
- [ ] Create WmePathfinder class (A* algorithm)
- [ ] Implement WmePath class
- [ ] Implement WmeWaypointGroup class
- [ ] Add waypoint parsing
- [ ] Integrate pathfinding with actors

### Dialogs

- [ ] Implement WmeResponseBox class
- [ ] Implement dialog system
- [ ] Add response selection
- [ ] Add dialog branching
- [ ] Integrate with scripts

### Save/Load System

- [ ] Implement WmePersistenceManager class
- [ ] Create save game format
- [ ] Implement Transfer method (bidirectional serialization)
- [ ] Implement Persist methods for all game objects
- [ ] Add save game metadata (thumbnail, timestamp)
- [ ] Ensure compatibility with original save games
- [ ] Test save/load functionality

### Adventure Module Tests

- [ ] Create WME.Adventure.Tests project
- [ ] Add scene loading tests
- [ ] Add pathfinding tests
- [ ] Add actor movement tests
- [ ] Add inventory tests
- [ ] Test save/load functionality

---

## Phase 6: UI System

### Window System

- [ ] Implement WmeWindow class
- [ ] Add window positioning
- [ ] Add window visibility
- [ ] Implement window scripts
- [ ] Add modal window support

### UI Controls

- [ ] Implement WmeButton class
- [ ] Implement WmeTextBox class (WmeEdit)
- [ ] Implement WmeLabel class (WmeText)
- [ ] Implement WmeUIEntity class
- [ ] Add control positioning and sizing

### Input Handling

- [ ] Integrate Silk.NET.Input
- [ ] Implement mouse input handling
- [ ] Implement keyboard input handling
- [ ] Add focus management
- [ ] Add click detection
- [ ] Add hover states

### Control Scripts

- [ ] Add script integration for controls
- [ ] Implement event handlers (onClick, onMouseOver, etc.)

### UI Tests

- [ ] Add UI control tests
- [ ] Test input handling
- [ ] Test focus management

---

## Phase 7: Development Tools

### Common Tools Library

- [ ] Create WME.Tools.Common project
- [ ] Implement BaseViewModel class
- [ ] Create IProjectService interface
- [ ] Create ISettingsService interface
- [ ] Create IDialogService interface
- [ ] Implement PropertyEditor control
- [ ] Implement TreeViewEx control
- [ ] Implement WmeCanvas control
- [ ] Add value converters

### Project Manager

- [ ] Create WME.ProjectManager project (MAUI)
- [ ] Implement project creation
- [ ] Implement project settings
- [ ] Add package management
- [ ] Add build functionality
- [ ] Create project file format

### Window Editor

- [ ] Create WME.WindowEditor project (MAUI)
- [ ] Implement MainPage UI
- [ ] Implement CanvasView for visual editing
- [ ] Implement PropertyPanel
- [ ] Add control toolbox
- [ ] Add drag-and-drop support
- [ ] Implement WmeEditorCanvas
- [ ] Add selection handles
- [ ] Add control alignment tools
- [ ] Implement save/load window definitions

### Sprite Editor

- [ ] Create WME.SpriteEditor project (MAUI)
- [ ] Implement sprite preview
- [ ] Add frame management
- [ ] Add subframe editing
- [ ] Add hotspot editing
- [ ] Implement animation preview
- [ ] Add timing controls
- [ ] Implement save/load sprite definitions

### Scene Editor

- [ ] Create WME.SceneEditor project (MAUI)
- [ ] Implement scene canvas
- [ ] Add layer management
- [ ] Add entity placement
- [ ] Add region editing
- [ ] Add waypoint editing
- [ ] Add scale level editing
- [ ] Implement scene preview
- [ ] Implement save/load scene definitions

### String Table Manager

- [ ] Create WME.StringTableManager project (MAUI)
- [ ] Implement string table editor
- [ ] Add language management
- [ ] Add import/export functionality
- [ ] Implement save/load string tables

### Script Editor

- [ ] Create WME.ScriptEditor project (MAUI)
- [ ] Implement syntax highlighting
- [ ] Add code completion
- [ ] Add error highlighting
- [ ] Integrate compiler
- [ ] Add debugging support (optional)

### Tools Tests

- [ ] Test all tools on Windows
- [ ] Test all tools on macOS
- [ ] Test all tools on Linux
- [ ] Verify output compatibility with engine

---

## Phase 8: Integration & Polish

### Game Runtime

- [ ] Create WME.Runtime executable project
- [ ] Implement game initialization
- [ ] Implement main game loop
- [ ] Add command-line argument parsing
- [ ] Add configuration file support
- [ ] Implement full-screen/windowed mode
- [ ] Add resolution configuration

### Plugin System

- [ ] Implement IWmePlugin interface
- [ ] Implement IWmeObjectPlugin interface
- [ ] Implement IWmeEventPlugin interface
- [ ] Create WmePluginManager class
- [ ] Implement plugin loading (AssemblyLoadContext)
- [ ] Add plugin event system
- [ ] Test plugin compatibility
- [ ] Create plugin adapter for legacy plugins

### Format Compatibility

- [ ] Implement DefinitionTokenizer class
- [ ] Implement DefinitionParser class
- [ ] Add all file format parsers
- [ ] Test with original game assets
- [ ] Verify backward compatibility

### Performance Optimization

- [ ] Profile rendering performance
- [ ] Profile script execution performance
- [ ] Optimize hot paths
- [ ] Implement object pooling where appropriate
- [ ] Optimize memory allocations
- [ ] Add performance benchmarks
- [ ] Verify 60 FPS target on all platforms

### Cross-Platform Testing

- [ ] Set up CI/CD pipeline
- [ ] Test on Windows 10/11
- [ ] Test on macOS 12+
- [ ] Test on Linux (Ubuntu 22.04+)
- [ ] Test on iOS (optional)
- [ ] Test on Android (optional)
- [ ] Fix platform-specific issues

### Integration Tests

- [ ] Create WME.Integration.Tests project
- [ ] Test complete game initialization
- [ ] Test scene loading and rendering
- [ ] Test script execution
- [ ] Test save/load with real games
- [ ] Test plugin system
- [ ] Run existing WME games

### Documentation

- [ ] Update README.md with setup instructions
- [ ] Create API documentation
- [ ] Document all public interfaces
- [ ] Create migration guide for game developers
- [ ] Create plugin development guide
- [ ] Add code examples
- [ ] Document file formats

### Sample Game

- [ ] Create WME.SampleGame project
- [ ] Implement simple demo game
- [ ] Add to samples/ directory
- [ ] Document sample game

### Final Verification

- [ ] Run all unit tests (>80% coverage)
- [ ] Run all integration tests (>70% coverage)
- [ ] Run static analysis (0 critical warnings)
- [ ] Test with multiple original WME games
- [ ] Verify all success criteria from SPECIFICATION.md
- [ ] Performance validation
- [ ] Final code review

---

## Additional Tasks (To Be Added As Needed)

- [ ] (Add additional tasks here as they are discovered during implementation)

---

## Notes

- Tasks should be completed in order within each phase
- Each task must be tested before marking as complete
- Commit and push frequently (at least after each major task)
- Review SPECIFICATION.md before starting each phase
- Update this document if new tasks are discovered

---

**End of TODO.md**
