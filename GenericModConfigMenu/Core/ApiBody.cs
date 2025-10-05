
using ModdingAPI;
using ModdingAPI.IO;

namespace GenericModConfigMenu.Core;
using Options;

internal class ApiBody<IPoco> : ApiBase
    where IPoco : class
{
    private readonly List<IOption> options = [];
    private readonly ConfigFile<IPoco>? file = null;
    private IPoco? loadedObj = null;
    private readonly Action reset;
    private readonly Action<IPoco> import;
    private readonly Func<IPoco> export;
    public ApiBody(IMod mod, Action reset, Action<IPoco> import, Func<IPoco> export, string? displayName, string? comments, Dictionary<string, string>? propertyComments, bool isEmptyPoco) : base(mod.UniqueID, displayName ?? mod.Name)
    {
        this.reset = reset;
        this.import = import;
        this.export = export;
        Reset();
        var defaultValue = export();
        if (!isEmptyPoco)
        {
            file = mod.ConfigFile<IPoco>(ConfigFileName, comments, propertyComments, getDefaultValue: () => defaultValue);
            _ = Setup(defaultValue);
        }
    }
    private async Task Setup(IPoco defaultValue)
    {
        if (file == null) return;
        if (!file.FileCreated)
        {
            loadedObj = await file.Get(defaultValue);
            if (loadedObj != null) import(loadedObj);
        }
    }
    public void Reset() => reset();
    public async Task<bool> Save() => file != null ? await file.Save(export()) : true;
    internal override void AddMenu(OptionsMenu menu, List<SubmenuItemEntry> list)
    {
        list.Add(new(DisplayName, getModMenu => () =>
        {
            var modMenu = getModMenu();
            MainMenu.ShowSubMenu(menu, options.Where(o => o.Enabled).Select(o => o.MenuItem(menu, modMenu)), onClosed: () =>
            {
                if (options.Any(o => o.Unsaved)) _ = Save();
                foreach (var o in options) o.Unsaved = false;
            });
        }));
    }
    internal override void OpenModOptionMenu(Action onClosed)
    {
        MainMenu.ShowSubMenu(null, options.Where(o => o.Enabled).Select(o => o.MenuItem(null, null)), onClosed: () =>
        {
            if (options.Any(o => o.Unsaved)) _ = Save();
            foreach (var o in options) o.Unsaved = false;
            onClosed?.Invoke();
        });
    }
    internal override void AddBoolOption(IMod mod, Func<bool> getValue, Action<bool> setValue, Func<string> name, Func<bool, string>? formatValue)
    {
        options.Add(new BoolOption(getValue, setValue, name, formatValue));
    }
    internal override void AddNumberOption(IMod mod, Func<int> getValue, Action<int> setValue, Func<string> name, int interval, int? min, int? max, Func<int, string>? formatValue)
    {
        options.Add(new NumberOptionInt(getValue, setValue, name, interval, min, max, formatValue));
    }
    internal override void AddNumberOption(IMod mod, Func<float> getValue, Action<float> setValue, Func<string> name, float interval, float? min, float? max, int? digit, Func<string, string>? formatValue)
    {
        options.Add(new NumberOptionFloat(getValue, setValue, name, interval, min, max, digit, formatValue));
    }
    internal override void AddSelectOption(IMod mod, Func<string> getValue, Action<string> setValue, Func<string> name, string[] selection, Func<string, string>? formatValue)
    {
        options.Add(new SelectOptionString(getValue, setValue, name, selection, formatValue));
    }
    internal override void AddSelectOption(IMod mod, Func<int> getValue, Action<int> setValue, Func<string> name, int[] selection, Func<int, string>? formatValue)
    {
        options.Add(new SelectOptionInt(getValue, setValue, name, selection, formatValue));
    }
    internal override void AddSelectOption(IMod mod, Func<float> getValue, Action<float> setValue, Func<string> name, float[] selection, Func<float, string>? formatValue)
    {
        options.Add(new SelectOptionFloat(getValue, setValue, name, selection, formatValue));
    }
    internal override void AddAction(IMod mod, Action action, Func<string> name, bool closeMenu, Action<Action>? beforeClose, Func<bool>? condition)
    {
        options.Add(new ActionOption(action, name, closeMenu, beforeClose, condition));
    }
}

