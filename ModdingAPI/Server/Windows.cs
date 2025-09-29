
using System.Diagnostics;

namespace ModdingAPI.Server;

internal class MonitorServer_Windows : MonitorServer
{
    private const uint GameFocusDelay = 500;
    protected override Process? Launch()
    {
        if (!ExistsServerScript()) return null;
        ProcessStartInfo psi = new()
        {
            FileName = "cmd",
            Arguments = $"/c start \"ModdingAPI\" \"node.exe\" \"{ScriptPath}\" {Config.MonitorServerPort}",
            WindowStyle = ProcessWindowStyle.Normal,
            UseShellExecute = false,
            RedirectStandardInput = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            CreateNoWindow = true,
        };
        ReserveGameWindowFocus();
        try
        {
            return Process.Start(psi);
        }
        catch { return null; }
    }

    private void ReserveGameWindowFocus()
    {
        try
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = "pwsh.exe",
                Arguments = $"-Command \"Start-Sleep -Milliseconds {GameFocusDelay}; Add-Type -AssemblyName System.Windows.Forms; [System.Windows.Forms.SendKeys]::SendWait('%{{ESC}}')\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardInput = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                CreateNoWindow = true,
            });
        }
        catch { }
    }
}

