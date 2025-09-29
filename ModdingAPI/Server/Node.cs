
using System.Diagnostics;

namespace ModdingAPI.Server;

internal class MonitorServer_Node : MonitorServer
{
    protected override Process? Launch()
    {
        if (!ExistsServerScript()) return null;
        ProcessStartInfo psi = new()
        {
            FileName = "node",
            Arguments = $"\"{ScriptPath}\" {Config.MonitorServerPort}",
            WindowStyle = ProcessWindowStyle.Normal,
            UseShellExecute = false,
            RedirectStandardInput = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            CreateNoWindow = false,
        };
        try
        {
            return Process.Start(psi);
        }
        catch { return null; }
    }
}

