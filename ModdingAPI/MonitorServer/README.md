
the ModdingAPI Monitor Server (a WebSocket logging server) using Node.js.

## ModdingAPI settings related with monitor server

Properties of `<gamePath>/BepInEx/plugins/ModdingAPI/config.cfg`:

### `UseMonitorServer`

Automatically launches the monitor server. 

Only available on Windows now.

If you set this to `true`, make sure that Node.js and cmd.exe are installed.

### `UseMonitorClient`

Uses the WebSocket client to log.

### `MonitorServerPort`

Port number of WebSocket connection.
