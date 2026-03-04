# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

ScreenLockApp is a Windows desktop application that locks the screen via a dark overlay. It runs as a system tray app and uses global keyboard hooks to detect lock/unlock shortcuts. Built with C# WinForms targeting .NET Framework 4.8.

## Build Commands

Build in Visual Studio or via command line (requires MSBuild in PATH):

```bash
# Build Debug
msbuild ScreenLockApp.sln /p:Configuration=Debug

# Build Release
msbuild ScreenLockApp.sln /p:Configuration=Release

# Run the compiled binary
./ScreenLockApp/bin/Debug/ScreenLockApp.exe
```

Building the WiX installer requires the WiX Toolset v3.11 to be installed. The installer project (`ScreenLockAppInstaller/`) can be built separately or as part of the full solution.

There are no automated tests in this project.

## Architecture

The application follows a simple controller-view pattern across these components:

- **Form1** — Main controller; runs hidden as a system tray icon. Owns the `KeyboardHook`, manages `LockOverlay` visibility, and hosts the tray context menu (Lock, Settings, Exit).
- **KeyboardHook** — Low-level Windows API hook (`SetWindowsHookEx` with `WH_KEYBOARD_LL`). Fires `KeyPressed` events that Form1 listens to for triggering lock/unlock.
- **LockOverlay** — Fullscreen, always-on-top dark form (opacity 0.85) covering all monitors. Intercepts and blocks keyboard shortcuts (Alt+F4, Alt+Tab, Ctrl+Shift+Esc) and all mouse clicks. Accepts password input to unlock.
- **SettingsForm** — Modal dialog for editing shortcuts and password. Uses keyboard capture fields so users can press a key combo to assign it.
- **SettingsManager** — Serializes/deserializes `AppSettings` to `%APPDATA%\ScreenLockApp\settings.xml`.

### Default Settings (`AppSettings`)

| Setting | Default |
|---|---|
| `LockShortcut` | `Control+Alt+L` |
| `UnlockShortcut` | `Control+Alt+U` |
| `Password` | `1234` |
| `UsePassword` | `true` |
| `UseShortcut` | `true` |

### Key Design Decisions

- Form1 uses `ShowInTaskbar = false` and stays hidden; the app lives entirely in the system tray.
- `LockOverlay` sets `TopMost = true` and spans the combined bounds of all screens to handle multi-monitor setups.
- The keyboard hook operates at the OS level, so it works regardless of which application has focus.
- Settings fall back to defaults silently on any load error.
