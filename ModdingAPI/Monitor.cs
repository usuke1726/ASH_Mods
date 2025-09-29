
//#define LogPrefix_UseLogLevel

namespace ModdingAPI;

using L = LogLevel;
using T1 = string;
using T2 = IEnumerable<string>;
using T3 = IEnumerable<StyleText>;
using T4 = IEnumerable<IEnumerable<string>>;

public interface IMonitor
{
    void Log(T1 message, L level = L.Info, bool onlyMonitor = false);
    void Log(T2 message, L level = L.Info, bool onlyMonitor = false);
    void Log(T3 message, L level = L.Info, bool onlyMonitor = false);
    void Log(T4 message, L level = L.Info, bool onlyMonitor = false);
    Task LogAsync(T1 message, L level = L.Info, bool onlyMonitor = false);
    Task LogAsync(T2 message, L level = L.Info, bool onlyMonitor = false);
    Task LogAsync(T3 message, L level = L.Info, bool onlyMonitor = false);
    Task LogAsync(T4 message, L level = L.Info, bool onlyMonitor = false);
    void LogOnce(T1 message, L level = L.Info, bool onlyMonitor = false);
    void LogOnce(T2 message, L level = L.Info, bool onlyMonitor = false);
    void LogOnce(T3 message, L level = L.Info, bool onlyMonitor = false);
    void LogOnce(T4 message, L level = L.Info, bool onlyMonitor = false);
    Task LogOnceAsync(T1 message, L level = L.Info, bool onlyMonitor = false);
    Task LogOnceAsync(T2 message, L level = L.Info, bool onlyMonitor = false);
    Task LogOnceAsync(T3 message, L level = L.Info, bool onlyMonitor = false);
    Task LogOnceAsync(T4 message, L level = L.Info, bool onlyMonitor = false);
    void LogNoDup(string messageId, T1 message, L level = L.Info, bool onlyMonitor = false);
    void LogNoDup(string messageId, T2 message, L level = L.Info, bool onlyMonitor = false);
    void LogNoDup(string messageId, T3 message, L level = L.Info, bool onlyMonitor = false);
    void LogNoDup(string messageId, T4 message, L level = L.Info, bool onlyMonitor = false);
    Task LogNoDupAsync(string messageId, T1 message, L level = L.Info, bool onlyMonitor = false);
    Task LogNoDupAsync(string messageId, T2 message, L level = L.Info, bool onlyMonitor = false);
    Task LogNoDupAsync(string messageId, T3 message, L level = L.Info, bool onlyMonitor = false);
    Task LogNoDupAsync(string messageId, T4 message, L level = L.Info, bool onlyMonitor = false);
    void LogNoDup(T1 message, L level = L.Info, bool onlyMonitor = false);
    void LogNoDup(T2 message, L level = L.Info, bool onlyMonitor = false);
    void LogNoDup(T3 message, L level = L.Info, bool onlyMonitor = false);
    void LogNoDup(T4 message, L level = L.Info, bool onlyMonitor = false);
    Task LogNoDupAsync(T1 message, L level = L.Info, bool onlyMonitor = false);
    Task LogNoDupAsync(T2 message, L level = L.Info, bool onlyMonitor = false);
    Task LogNoDupAsync(T3 message, L level = L.Info, bool onlyMonitor = false);
    Task LogNoDupAsync(T4 message, L level = L.Info, bool onlyMonitor = false);
    void LogBepIn(T1 message, L level = L.Info);
    void LogBepIn(T2 message, L level = L.Info);
    void LogBepIn(T3 message, L level = L.Info);
    void LogBepIn(T4 message, L level = L.Info);
    ManualLogSource BepInLogger { get; }
}

internal class Monitor : IMonitor
{
    private static readonly Hook hook = new();
    private class Hook : UnityLogSource
    {
        public Hook()
        {
            LogEvent += (sender, e) =>
            {
                switch (e.Level)
                {
                    case L.Error or L.Fatal:
                        var prefix = LogPrefix("Unity Log", e.Level);
                        _ = MonitorServer.Send($"{prefix}{e.Data}", Style.From(e.Level));
                        break;
                }
            };
        }
    }
    private static Monitor ApiLogger { get => (Monitor)ModdingApiMod.instance.Monitor; }
    private static readonly HashSet<string> IDs = [];
    private static readonly Dictionary<string, Dictionary<string, string>> LastLog = [];
    private static readonly Dictionary<string, HashSet<string>> MessageLogged = [];
    public readonly string ID;
    public readonly string DisplayName;
    public readonly ManualLogSource Logger;
    public ManualLogSource BepInLogger { get => Logger; }
    public Monitor(string ID, string? displayName = null)
    {
        if (IDs.Contains(ID))
        {
            ApiLogger?.Log($"ID {ID} has already been registered!", L.Warning);
        }
        this.ID = ID;
        DisplayName = displayName ?? ID;
        IDs.Add(ID);
        LastLog[ID] = [];
        MessageLogged[ID] = [];
        Logger = BepInEx.Logging.Logger.CreateLogSource(DisplayName);
    }
    internal static void SLogBepIn(object message, L level = L.Info)
    {
        ApiLogger?.BepInLogger.Log(level, message);
    }
    internal static void SLog(object message, L level = L.Info, bool onlyMonitor = false) => ApiLogger?._Log(message, level, onlyMonitor);
    internal static async Task SLogAsync(object message, L level = L.Info, bool onlyMonitor = false)
    {
        if (ApiLogger != null) await ApiLogger._LogAsync(message, level, onlyMonitor);
    }
    internal static void SLogOnce(object message, L level = L.Info, bool onlyMonitor = false) => ApiLogger?._LogOnce(message, level, onlyMonitor);
    internal static async Task SLogOnceAsync(object message, L level = L.Info, bool onlyMonitor = false)
    {
        if (ApiLogger != null) await ApiLogger._LogOnceAsync(message, level, onlyMonitor);
    }
    internal static void SLogNoDup(string mesId, object message, L level = L.Info, bool onlyMonitor = false) => ApiLogger?._LogNoDup(mesId, message, level, onlyMonitor);
    internal static void SLogNoDup(object message, L level = L.Info, bool onlyMonitor = false) => ApiLogger?._LogNoDup(message, level, onlyMonitor);
    internal static async Task SLogNoDupAsync(object message, L level = L.Info, bool onlyMonitor = false) => await SLogNoDupAsync("", message, level, onlyMonitor);
    internal static async Task SLogNoDupAsync(string mesId, object message, L level = L.Info, bool onlyMonitor = false)
    {
        if (ApiLogger != null) await ApiLogger._LogNoDupAsync(mesId, message, level, onlyMonitor);
    }

    private static string LogPrefix(string displayName, L level)
    {
#if LogPrefix_UseLogLevel
        var levelstr = level switch
        {
            LogLevel.Fatal => "Fatal  ",
            LogLevel.Error => "Error  ",
            LogLevel.Warning => "Warning",
            LogLevel.Info => "Info   ",
            LogLevel.Message => "Message",
            LogLevel.Debug => "Debug  ",
            LogLevel.All => "All    ",
            _ => "None   "
        };
        return $"[{levelstr}: {displayName}]";
#else
        return $"[{displayName}] ";
#endif
    }
    private async Task Send(string message, L level)
    {
        await MonitorServer.Send($"{LogPrefix(DisplayName, level)}{message}", Style.From(level));
    }
    private void GetMessage(object message, out string styledMessage, out string rawMessage)
    {
        styledMessage = Style.Format(message);
        rawMessage = Style.NoStyle(message);
    }

#pragma warning disable IDE1006
    private async Task _LogAsync(object message, L level = L.Info, bool onlyMonitor = false)
    {
        GetMessage(message, out var styledMessage, out var rawMessage);
        if (!onlyMonitor) Logger.Log(level, rawMessage);
        await Send(styledMessage, level);
    }
    private void _Log(object message, L level = L.Info, bool onlyMonitor = false)
    {
        var _ = _LogAsync(message, level, onlyMonitor);
    }
    private async Task _LogOnceAsync(object message, L level = L.Info, bool onlyMonitor = false)
    {
        GetMessage(message, out var styledMessage, out var rawMessage);
        if (!MessageLogged[ID].Contains(rawMessage))
        {
            MessageLogged[ID].Add(rawMessage);
            if (!onlyMonitor) Logger.Log(level, rawMessage);
            await Send(styledMessage, level);
        }
    }
    private void _LogOnce(object message, L level = L.Info, bool onlyMonitor = false)
    {
        var _ = _LogOnceAsync(message, level, onlyMonitor);
    }
    private async Task _LogNoDupAsync(string mesId, object message, L level = L.Info, bool onlyMonitor = false)
    {
        GetMessage(message, out var styledMessage, out var rawMessage);
        if (LastLog[ID].TryGetValue(mesId, out var s) && s == rawMessage) return;
        LastLog[ID][mesId] = rawMessage;
        if (!onlyMonitor) Logger.Log(level, rawMessage);
        await Send(styledMessage, level);
    }
    private async Task _LogNoDupAsync(object message, L level = L.Info, bool onlyMonitor = false) => await _LogNoDupAsync("", message, level, onlyMonitor);
    private void _LogNoDup(string mesId, object message, L level = L.Info, bool onlyMonitor = false)
    {
        var _ = _LogNoDupAsync(mesId, message, level, onlyMonitor);
    }
    private void _LogNoDup(object message, L level = L.Info, bool onlyMonitor = false) => _LogNoDup("", message, level, onlyMonitor);
    private void _LogBepIn(object message, L level = L.Info)
    {
        GetMessage(message, out var styledMessage, out var rawMessage);
        Logger.Log(level, rawMessage);
    }
#pragma warning restore IDE1006

    public void Log(T1 m, L l = L.Info, bool onlyMonitor = false) => _Log(m, l, onlyMonitor);
    public void Log(T2 m, L l = L.Info, bool onlyMonitor = false) => _Log(m, l, onlyMonitor);
    public void Log(T3 m, L l = L.Info, bool onlyMonitor = false) => _Log(m, l, onlyMonitor);
    public void Log(T4 m, L l = L.Info, bool onlyMonitor = false) => _Log(m, l, onlyMonitor);
    public async Task LogAsync(T1 m, L l = L.Info, bool onlyMonitor = false) => await _LogAsync(m, l, onlyMonitor);
    public async Task LogAsync(T2 m, L l = L.Info, bool onlyMonitor = false) => await _LogAsync(m, l, onlyMonitor);
    public async Task LogAsync(T3 m, L l = L.Info, bool onlyMonitor = false) => await _LogAsync(m, l, onlyMonitor);
    public async Task LogAsync(T4 m, L l = L.Info, bool onlyMonitor = false) => await _LogAsync(m, l, onlyMonitor);
    public void LogOnce(T1 m, L l = L.Info, bool onlyMonitor = false) => _LogOnce(m, l, onlyMonitor);
    public void LogOnce(T2 m, L l = L.Info, bool onlyMonitor = false) => _LogOnce(m, l, onlyMonitor);
    public void LogOnce(T3 m, L l = L.Info, bool onlyMonitor = false) => _LogOnce(m, l, onlyMonitor);
    public void LogOnce(T4 m, L l = L.Info, bool onlyMonitor = false) => _LogOnce(m, l, onlyMonitor);
    public async Task LogOnceAsync(T1 m, L l = L.Info, bool onlyMonitor = false) => await _LogOnceAsync(m, l, onlyMonitor);
    public async Task LogOnceAsync(T2 m, L l = L.Info, bool onlyMonitor = false) => await _LogOnceAsync(m, l, onlyMonitor);
    public async Task LogOnceAsync(T3 m, L l = L.Info, bool onlyMonitor = false) => await _LogOnceAsync(m, l, onlyMonitor);
    public async Task LogOnceAsync(T4 m, L l = L.Info, bool onlyMonitor = false) => await _LogOnceAsync(m, l, onlyMonitor);
    public void LogNoDup(string i, T1 m, L l = L.Info, bool onlyMonitor = false) => _LogNoDup(i, m, l, onlyMonitor);
    public void LogNoDup(string i, T2 m, L l = L.Info, bool onlyMonitor = false) => _LogNoDup(i, m, l, onlyMonitor);
    public void LogNoDup(string i, T3 m, L l = L.Info, bool onlyMonitor = false) => _LogNoDup(i, m, l, onlyMonitor);
    public void LogNoDup(string i, T4 m, L l = L.Info, bool onlyMonitor = false) => _LogNoDup(i, m, l, onlyMonitor);
    public async Task LogNoDupAsync(string i, T1 m, L l = L.Info, bool onlyMonitor = false) => await _LogNoDupAsync(i, m, l, onlyMonitor);
    public async Task LogNoDupAsync(string i, T2 m, L l = L.Info, bool onlyMonitor = false) => await _LogNoDupAsync(i, m, l, onlyMonitor);
    public async Task LogNoDupAsync(string i, T3 m, L l = L.Info, bool onlyMonitor = false) => await _LogNoDupAsync(i, m, l, onlyMonitor);
    public async Task LogNoDupAsync(string i, T4 m, L l = L.Info, bool onlyMonitor = false) => await _LogNoDupAsync(i, m, l, onlyMonitor);
    public void LogNoDup(T1 m, L l = L.Info, bool onlyMonitor = false) => _LogNoDup(m, l, onlyMonitor);
    public void LogNoDup(T2 m, L l = L.Info, bool onlyMonitor = false) => _LogNoDup(m, l, onlyMonitor);
    public void LogNoDup(T3 m, L l = L.Info, bool onlyMonitor = false) => _LogNoDup(m, l, onlyMonitor);
    public void LogNoDup(T4 m, L l = L.Info, bool onlyMonitor = false) => _LogNoDup(m, l, onlyMonitor);
    public async Task LogNoDupAsync(T1 m, L l = L.Info, bool onlyMonitor = false) => await _LogNoDupAsync(m, l, onlyMonitor);
    public async Task LogNoDupAsync(T2 m, L l = L.Info, bool onlyMonitor = false) => await _LogNoDupAsync(m, l, onlyMonitor);
    public async Task LogNoDupAsync(T3 m, L l = L.Info, bool onlyMonitor = false) => await _LogNoDupAsync(m, l, onlyMonitor);
    public async Task LogNoDupAsync(T4 m, L l = L.Info, bool onlyMonitor = false) => await _LogNoDupAsync(m, l, onlyMonitor);
    public void LogBepIn(T1 m, L l = L.Info) => _LogBepIn(m, l);
    public void LogBepIn(T2 m, L l = L.Info) => _LogBepIn(m, l);
    public void LogBepIn(T3 m, L l = L.Info) => _LogBepIn(m, l);
    public void LogBepIn(T4 m, L l = L.Info) => _LogBepIn(m, l);
}

