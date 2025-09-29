
My mods for A Short Hike.

> [!WARNING]
> - These mods are alpha versions under development. **Contents and specifications may be changed without notice**.
> - These mods depend on the following and **they might be incompatible with platforms other than Xbox PC**.
>   - Unity version `2021.3.27` (version of `<gamePath>/UnityPlayer.dll`)
>   - .NET Standard 2.1 (version of `<gamePath>/A Short Hike_Data/Managed/netstandard.dll`)
>   - BepInEx version `5.4.23`

*`<gamePath>` represents the directory where there is `A Short Hike.exe`.

## How to install

1. Install BepInEx
    - see https://docs.bepinex.dev/articles/user_guide/installation/index.html
    - Make sure to "do a first run to generate configuration files" (step 3).
1. Install ModdingAPI
    1. Download [`/ModdingAPI/dist/ModdingAPI_<version>.zip`](ModdingAPI/dist) and unzip it.
    1. Make the directory `<gamePath>/BepInEx/plugins/ModdingAPI` and place the extracted contents on there.
1. Install other mods
    1. Make the directory `<gamePath>/Mods`
    1. Download `/<modName>/dist/<modName>_<version>.zip`, unzip it, and place the contents on the directory `<gamePath>/Mods/<modName>`.

*The files beginning `.<modName>` (s.t. `.<modName>.deps.json` and `.<modName>.sha512.txt`) are unnecessary for mod running. They can be removed.

## Changing mods profile

When you launch the game once after installing ModdingAPI, a config file `<gamePath>/BepInEx/plugins/ModdingAPI/config.cfg` will be created.

Rewriting the property `ModsPath` in the file enables change mods folder loaded.

## Changing logging monitor

You can use another logging monitor instead of the original BepInEx monitor.

In this case, a WebSocket server will be needed. See [the sample `/ModdingAPI/MonitorServer`](ModdingAPI/MonitorServer).

## License

All mods and contents in this repository are licensed under the MIT license.
