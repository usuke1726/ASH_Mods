
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using UnityEngine;

namespace ModdingAPI;
internal abstract class MonitorServer
{
    protected abstract Process? Launch();

    private static MonitorServer? server = null;
    private const string ServerDomain = "localhost";
    public static bool HasSetupDone { get; private set; } = false;
    protected static readonly string ScriptPath = $"{ModdingApiInfo.PLUGIN_PATH}/server.js";
    private static Process? serverProcess = null;
    private static ClientWebSocket? client = null;
    protected static ManualLogSource Logger { get; private set; } = null!;
    private static readonly List<string> bufferBeforeLaunched = [];
    private static readonly HashSet<WebSocketState> closedStates = [WebSocketState.Closed, WebSocketState.Aborted, WebSocketState.CloseReceived];
    public static async void Setup(ManualLogSource _logger)
    {
        Logger = _logger;
        if (!Config.UseMonitorServer && !Config.UseMonitorClient)
        {
            HasSetupDone = true;
            return;
        }
        if (!VerifyConfig())
        {
            HasSetupDone = true;
            return;
        }
        LaunchServer();
        await LaunchClient();
    }
    private static bool VerifyConfig()
    {
        if (!IsValidPort())
        {
            Logger.LogError(I18n_.Localize("MonitorServer.Error.InvalidPort"));
            return false;
        }
        return true;
    }
    private static void SetServer()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WindowsEditor:
                server = new Server.MonitorServer_Windows();
                break;
            default:
                Logger.LogError(I18n_.Localize("MonitorServer.Error.UnsupportedPlatform", Application.platform));
                break;
        }
    }
    private static void LaunchServer()
    {
        if (!Config.UseMonitorServer)
        {
            Logger.LogInfo(I18n_.Localize("MonitorServer.Info.ServerDisabled"));
            return;
        }
        SetServer();
        serverProcess = server?.Launch();
        if (!IsServerActive())
        {
            Logger.LogError(I18n_.Localize("MonitorServer.Error.FailedLaunchingServer"));
        }
    }
    protected static bool ExistsServerScript()
    {
        if (!File.Exists(ScriptPath))
        {
            Logger.LogError(I18n_.Localize("MonitorServer.Error.ScriptNotFound", ScriptPath));
            return false;
        }
        return true;
    }
    private static bool IsValidPort()
    {
        var port = Config.MonitorServerPort;
        return (port >= 1024 && port <= 65535) || port == 80;
    }
    private static bool IsServerActive()
    {
        return serverProcess != null && !serverProcess.HasExited;
    }
    private static async Task LaunchClient()
    {
        if (!Config.UseMonitorClient)
        {
            Logger.LogInfo(I18n_.Localize("MonitorServer.Info.ClientDisabled"));
            HasSetupDone = true;
            return;
        }
        else if (Config.UseMonitorServer && !IsServerActive())
        {
            Logger.LogInfo(I18n_.Localize("MonitorServer.Info.DisableClientOnFailedLaunchServer"));
            HasSetupDone = true;
            return;
        }
        if (await TryToLaunchWebSocketClient())
        {
            HasSetupDone = true;
            await ShowSampleMessage();
            foreach (var mes in bufferBeforeLaunched)
            {
                await Send(mes);
            }
            bufferBeforeLaunched.Clear();
        }
        else
        {
            HasSetupDone = true;
            Logger.LogError(I18n_.Localize("MonitorServer.Error.FailedConnectingServer"));
        }
    }
    public static async Task Send(string message)
    {
        if (!HasSetupDone)
        {
            bufferBeforeLaunched.Add(message);
            return;
        }
        if (client == null) return;
        if (closedStates.Contains(client.State))
        {
            client = null;
            Logger.LogWarning(I18n_.Localize("MonitorServer.Warn.ServerClosed"));
            return;
        }
        try
        {
            var segment = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
            await client.SendAsync(segment, WebSocketMessageType.Text, true, new());
        }
        catch (Exception e)
        {
            Logger.LogError(I18n_.Localize("MonitorServer.Error.FailedSending", e));
        }
    }
    public static async Task Send(string message, Style style) => await Send(style.Str(message));
    public static async Task Send(IEnumerable<string> texts) => await Send(Style.Format(texts));
    public static async Task Send(IEnumerable<StyleText> styleTexts) => await Send(Style.Format(styleTexts));
    public static async Task Send(IEnumerable<IEnumerable<string>> styleTexts) => await Send(Style.Format(styleTexts));

    private const int MaxTrialNum = 10;
    private static async Task<bool> TryToLaunchWebSocketClient()
    {
        client = new ClientWebSocket();
        var uri = new Uri($"ws://{ServerDomain}:{Config.MonitorServerPort}");
        CancellationToken token = new();
        for (int i = 0; i < MaxTrialNum; i++)
        {
            try
            {
                await client.ConnectAsync(uri, token);
                return true;
            }
            catch
            {
                await Task.Delay(1000);
            }
        }
        client = null;
        return false;
    }
    private static async Task ShowSampleMessage()
    {
        await Send([
                ["(Sample Message) ", ""],
                ["Successed ", "red, bold"],
                ["WebSocket ", "yellow, italic"],
                ["Connection", "blue, underline"],
                ["!!!!!!!!!", "green, italic, bold, underline, invert"]
                ]);
    }

    public static async Task Deactivate()
    {
        if (client != null)
        {
            await client.CloseAsync(WebSocketCloseStatus.Empty, "", new());
        }
        if (serverProcess != null && !serverProcess.HasExited)
        {
            Logger.LogWarning(I18n_.Localize("MonitorServer.Info.KillCmd"));
            try
            {
                serverProcess.Kill();
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }
        serverProcess = null;
    }
}

