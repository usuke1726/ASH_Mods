
using HarmonyLib;
using ModdingAPI;

namespace Sidequel.System;

internal class SaveData
{
    private static readonly string SaveDataExistsKey = "ModRegistry_Quicker1726_Sidequel_SaveData";
    private static Dictionary<int, bool> saveDataExists = null!;
    public static bool DoesSaveDataExists()
    {
        EnsureDataLoaded();
        return saveDataExists.TryGetValue(GameSettings.saveSlot, out var v) && v;
    }
    public static void Setup(IModHelper helper)
    {
        helper.Events.System.BeforeSaving += (_, _) =>
        {
            //Monitor.Log($"BEFORE SAVING", LL.Warning);
            BeforeSaving();
        };
    }
    public static void OnVanillaNewGameStarted()
    {
        EnsureDataLoaded();
        var saveSlot = GameSettings.saveSlot;
        saveDataExists[saveSlot] = false;
        Save();
    }
    public static void BeforeSaving()
    {
        if (!State.IsActive) return;
        EnsureDataLoaded();
        var saveSlot = GameSettings.saveSlot;
        saveDataExists[saveSlot] = true;
        Save();
    }
    private static void Save()
    {
        var data = Serialize();
        //Debug($"saving saveData: {data}");
        PlayerPrefsAdapter.SetString(SaveDataExistsKey, data);
    }
    private static void EnsureDataLoaded()
    {
        if (saveDataExists == null) SetupData();
    }
    private static void SetupData()
    {
        var data = PlayerPrefsAdapter.GetString(SaveDataExistsKey, null);
        if (data == null) saveDataExists = [];
        else saveDataExists = Deserialize(data);
        Debug($"loaded saveData: {Serialize()}");
    }
    private static string Serialize()
    {
        return string.Join(";", saveDataExists.Select(p => $"{p.Key}:{(p.Value ? "1" : "0")}"));
    }
    private static Dictionary<int, bool> Deserialize(string data)
    {
        Dictionary<int, bool> ret = [];
        foreach (var d in data.Split(";"))
        {
            var a = d.Split(":", 2);
            if (a.Length < 2) continue;
            if (!int.TryParse(a[0], out var key)) continue;
            ret[key] = a[0] != "0";
        }
        return ret;
    }
}

