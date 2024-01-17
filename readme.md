# OpenXR Runtime Manager

Small utility program that permit see the current OpenXR runtime, and select another one.

![Small GIF showing the UI of this program](./ui.gif)

> This program needs to be ran as administrator, as it will edit a registry key in `HKLM\SOFTWARE\Khronos\OpenXR\1`.

## Compatibility

This program relies both on the OpenXR standard runtime enumeration scheme on Windows, and on a list of "well known" manifest file paths and currently support the following runtimes:

 - SteamVR
 - Oculus OpenXR
 - MixedRearlityRuntime
 - VarjoOpenXR

If a runtime is not being recognized (path not presentin the `AvailableRuntimes` registry key, or other reasons). Please open an issue on this repository, and try to include the expected path to the JSON manifest for the runtime.

*Note: This program currently do not handle configuring 32bit runtimes in a 64bit build, due to the way the registry gets shadowed for 32 bit application on windows. SysWow64 is prettymuch a "legacy" platform at this point. If you happen to need this, please open an issue to discuss it.*

## Legal

Copyright :copyright: 2021-2024 Arthur Brainville (Ybalrid) - Licensed under the terms of the MIT License agreement.

*OpenXR™ is a trademark owned by The Khronos Group Inc. and is registered as a trademark in China, the European Union, Japan and the United Kingdom.*
