
using ModdingAPI.IO;

namespace ModdingAPI;

internal class ConfigPoco
{
    public string ModsPath { get; set; } = "Mods";
    public string Language { get; set; } = "system";
    public bool UseMonitorClient { get; set; } = false;
    public bool UseMonitorServer { get; set; } = false;
    public int MonitorServerPort { get; set; } = 8080;
    public bool PrintKeyBindListOnGameLaunched { get; set; } = true;
}
internal static class Config
{
    public static string ModsPath { get; private set; } = null!;
    public static string Language { get; private set; } = null!;
    public static bool UseMonitorClient { get; private set; }
    public static bool UseMonitorServer { get; private set; }
    public static int MonitorServerPort { get; private set; }
    public static bool PrintKeyBindListOnGameLaunched { get; private set; }
    private static void SetConfig(ConfigPoco config)
    {
        ModsPath = config.ModsPath;
        Language = config.Language.Trim();
        UseMonitorClient = config.UseMonitorClient;
        UseMonitorServer = config.UseMonitorServer;
        MonitorServerPort = config.MonitorServerPort;
        PrintKeyBindListOnGameLaunched = config.PrintKeyBindListOnGameLaunched;
    }

    private static readonly string configFileName = "config.cfg";
    private static readonly string fileComment = """
        
        """;
    private static readonly Dictionary<string, string> propertyComments = new()
    {
    };

    public static async Task ReadConfig(ManualLogSource logger)
    {
        var configFile = new ConfigFile<ConfigPoco>(
            ModdingApiMod.instance,
            configFileName,
            comments: fileComment,
            propertyComments: propertyComments,
            getDefaultValue: () => new()
        );
        var config = await configFile.Get(new());
        if (config == null)
        {
            Monitor.SLog($"failed to load moddingAPI config", LogLevel.Warning);
            config = new();
        }
        SetConfig(config);
        API_I18n.SetLanguage(Language, logger);
    }
}

