
using ModdingAPI;
using ModdingAPI.IO;

namespace MultiSave;

internal static class UpdateTimeHandler
{
    private static readonly Dictionary<int, DateTime> modifiedTimeCache = [];
    private static readonly string cacheFileName = "LastModifiedDates.txt";
    private static bool setupDone = false;
    private static TextFile file = null!;

    private static string FormatKeyValue(int slot, DateTime time) => $"{slot}:{Date.ToString(time)}";

    internal static void Setup(IMod mod)
    {
        try
        {
            file = new(mod, cacheFileName, isCache: true, initialContents: "");
            ReadCacheContents(file.ReadLines());
            setupDone = true;
        }
        catch (Exception e)
        {
            Monitor.Log($"== Error on setup ==\n{e}", LL.Error);
        }
    }
    private static void ReadCacheContents(IEnumerable<string> lines)
    {
        foreach (var line in lines)
        {
            if (line.Trim().Length == 0) continue;
            try
            {
                var parts = line.Split(':', 2);
                if (parts.Length != 2) throw new Exception();
                var slot = int.Parse(parts[0]);
                var time = Date.ToDate(parts[1]);
                if (slot < SaveMenu.MaxSaveSlots && slot >= 0)
                {
                    modifiedTimeCache[slot] = time;
                }
                else
                {
                    Monitor.Log($"Out of bound of slot number: {slot}", LL.Warning);
                }
            }
            catch
            {
                Monitor.Log($"Invalid cache data format:\n\"{line}\"");
            }
        }
        Monitor.Log("Cache loaded successfully");
    }
    private static async void UpdateCache()
    {
        try
        {
            await file.WriteLines(modifiedTimeCache.Select(pair => FormatKeyValue(pair.Key, pair.Value)));
            Monitor.Log("Cache updated successfully");
        }
        catch (Exception e)
        {
            Monitor.Log($"== Failed cache updating ==\n{e}", LL.Error);
        }
    }
    private static int SlotForContainerName(string name)
    {
        for (int i = 0; i < SaveMenu.MaxSaveSlots; i++)
        {
            if (GlobalData.GetFilenameForSaveSlot(i) == name) return i;
        }
        return -1;
    }
    internal static DateTime RequestLastModifiedTime(string containerName)
    {
        if (!setupDone)
        {
            Monitor.Log("Cache setup has not been done yet", LL.Warning);
            return DateTime.MinValue;
        }
        var slot = SlotForContainerName(containerName);
        if (!modifiedTimeCache.ContainsKey(slot)) return DateTime.MinValue;
        return modifiedTimeCache[slot];
    }

    internal static void Update(int slot, DateTime time)
    {
        modifiedTimeCache[slot] = time;
        UpdateCache();
    }

    private static class Date
    {
        internal static string ToString(DateTime time) => time.ToString("yyyy/MM/dd HH:mm:ss");
        internal static DateTime ToDate(string time) => DateTime.Parse(time);
    }
}

