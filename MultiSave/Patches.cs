
using HarmonyLib;
using ModdingAPI;

namespace MultiSave;

[HarmonyPatch(typeof(OptionsMenu))]
internal class SaveMenu
{
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(OptionsMenu), "ShowSaveMenu")]
    internal static void ShowSaveMenu(object instance) => throw new NotImplementedException();

    [HarmonyReversePatch]
    [HarmonyPatch(typeof(OptionsMenu), "GetSaveSlotText")]
    internal static string GetSaveSlotText(object instance) => throw new NotImplementedException();

    [HarmonyReversePatch]
    [HarmonyPatch(typeof(OptionsMenu), "BuildSimpleMenu")]
    internal static LinearMenu BuildSimpleMenu(object instance, List<MenuItem> menuItems) => throw new NotImplementedException();

    public static readonly int MaxSaveSlotsMin = 3;
    public static readonly int MaxSaveSlotsMax = 10;
    private static int maxSaveSlots = 5;
    public static int MaxSaveSlots
    {
        get => maxSaveSlots; set
        {
            maxSaveSlots = Math.Max(Math.Min(value, MaxSaveSlotsMax), MaxSaveSlotsMin);
        }
    }

    [HarmonyPrefix()]
    [HarmonyPatch(typeof(OptionsMenu), "ShowSaveSlotsMenu")]
    internal static bool ShowSaveSlotsMenu(LinearMenu parentMenu, BasicMenuItem menuItem, OptionsMenu __instance)
    {
        LinearMenu submenu = null!;
        List<MenuItem> list = [];
        for (int i = 0; i < MaxSaveSlots; i++)
        {
            int cachedIndex = i;
            string filenameForSaveSlot = GlobalData.GetFilenameForSaveSlot(i);
            string text = "";
            if (FileSystem.Exists(filenameForSaveSlot))
            {
                text = " <color=#AAA>(" + FileSystem.LastModified(filenameForSaveSlot).ToString(I18n.STRINGS.dateFormat) + ")</color>";
            }
            list.Add(new MenuItem(string.Format(I18n.STRINGS.numberedSaveSlot, i + 1) + text, (Action)delegate
            {
                GameSettings.saveSlot = cachedIndex;
                UI.SetGenericText(menuItem.gameObject, GetSaveSlotText(__instance));
                OptionsMenu.PositionMenu(parentMenu.transform);
                submenu.Kill();
                TitleScreen titleScreen = UnityEngine.Object.FindObjectOfType<TitleScreen>();
                if ((bool)titleScreen)
                {
                    titleScreen.UpdateTitleScreenMenuItems();
                }
            }));
        }
        submenu = BuildSimpleMenu(__instance, list);
        return false;
    }
}

[HarmonyPatch(typeof(XGameSaveWrapper))]
internal class SavingHooks
{
    [HarmonyPrefix]
    [HarmonyPatch("Update")]
    static bool Update(string containerName, IDictionary<string, byte[]> blobsToSave, IList<string> blobsToDelete)
    {
        var now = DateTime.Now;
        var slot = GameSettings.saveSlot;
        UpdateTimeHandler.Update(slot, now);
        Debug($"== now saved! ({now.ToString(I18n.STRINGS.dateFormat)})");
        Debug($"slot: {slot}, containerName: {containerName}");
        return true;
    }
    [HarmonyPrefix]
    [HarmonyPatch("UpdateAsync")]
    static bool UpdateAsync(string containerName, IDictionary<string, byte[]> blobsToSave, IList<string> blobsToDelete, object callback)
    {
        var now = DateTime.Now;
        var slot = GameSettings.saveSlot;
        UpdateTimeHandler.Update(slot, now);
        Debug($"== now saved! ({now.ToString(I18n.STRINGS.dateFormat)}, ASYNC)");
        Debug($"slot: {slot}, containerName: {containerName}");
        return true;
    }
}

[HarmonyPatch(typeof(GameCoreFileSystem))]
internal class PatchedFileSystem
{
    [HarmonyPrefix]
    [HarmonyPatch("LastModified")]
    static bool PreFix(string fileName, ref DateTime __result)
    {
        Debug("== FileSystemModified called!");
        Debug($"  file: '{fileName}'");
        __result = UpdateTimeHandler.RequestLastModifiedTime(fileName);
        return false;
    }
}

