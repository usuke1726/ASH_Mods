
using System.Reflection;
using System.Text.RegularExpressions;

namespace ModdingAPI.IO;

public class ConfigFile<Poco> : ConfigFileBase
    where Poco : class
{
    private readonly Dictionary<string, Type> properties;
    public ConfigFile(IMod mod, string baseName, string? comments = null, Dictionary<string, string>? propertyComments = null, Func<Poco>? getDefaultValue = null) : base(mod, baseName, () => InitialContents(comments, propertyComments, getDefaultValue), comments)
    {
        properties = GetProperties();
    }
    private static Dictionary<string, Type> GetProperties()
    {
        return new(typeof(Poco)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(prop => prop.CanRead && prop.CanWrite && allowedTypes.Contains(prop.PropertyType))
            .Select(prop => new KeyValuePair<string, Type>(prop.Name, prop.PropertyType)));
    }
    private static string InitialContents(string? comments, Dictionary<string, string>? propertyComments, Func<Poco>? getDefaultValue)
    {
        var commentStr = CommentsToStr(comments);
        List<string> contents = [];
        Poco? obj = getDefaultValue?.Invoke() ?? typeof(Poco).GetConstructor(Type.EmptyTypes).Invoke(null) as Poco;
        if (obj == null) throw new Exception($"obj is null");
        var properties = GetProperties();
        foreach (var name in properties.Keys)
        {
            var type = properties[name];
            var prop = typeof(Poco).GetProperty(name);
            string? s = null;
            if (type == typeof(int))
            {
                s = ToString((int)prop.GetValue(obj));
            }
            else if (type == typeof(float))
            {
                s = ToString((float)prop.GetValue(obj));
            }
            else if (type == typeof(bool))
            {
                s = ToString((bool)prop.GetValue(obj));
            }
            else if (type == typeof(string))
            {
                s = (string)prop.GetValue(obj) ?? "";
            }
            if (s == null) continue;
            if (propertyComments?.TryGetValue(name, out var comment) ?? false)
            {
                var comS = string.Join("\n", comment.Replace("\r", "").Split('\n').Select(c => $"# {c}"));
                s = $"{comS}\n{name} = {s}";
            }
            else
            {
                s = $"{name} = {s}";
            }
            contents.Add(s);
        }
        return $"{commentStr}\n\n{string.Join("\n\n", contents)}";
    }
    public async Task<bool> Save(Poco obj)
    {
        await ReadTask;
        try
        {
            foreach (var name in properties.Keys)
            {
                var type = properties[name];
                var prop = typeof(Poco).GetProperty(name);
                if (type == typeof(int))
                {
                    Set(name, (int)prop.GetValue(obj));
                }
                else if (type == typeof(float))
                {
                    Set(name, (float)prop.GetValue(obj));
                }
                else if (type == typeof(bool))
                {
                    Set(name, (bool)prop.GetValue(obj));
                }
                else if (type == typeof(string))
                {
                    Set(name, (string)prop.GetValue(obj));
                }
            }
            if (await Write())
            {
                Monitor.SLog("Saved config successfully", LogLevel.Debug);
                return true;
            }
            else
            {
                Monitor.SLog("Failed writing config", LogLevel.Warning);
                return false;
            }
        }
        catch (Exception e)
        {
            Monitor.SLog($"Failed writing config:\n{e}", LogLevel.Error);
            return false;
        }
    }
    public async Task<Poco?> Get(Poco defaultValue)
    {
        await ReadTask;
        try
        {
            Poco? obj = typeof(Poco).GetConstructor(Type.EmptyTypes).Invoke(null) as Poco;
            if (obj == null) throw new Exception($"obj is null");
            foreach (var name in properties.Keys)
            {
                var type = properties[name];
                var prop = typeof(Poco).GetProperty(name);
                if (type == typeof(int))
                {
                    prop.SetValue(obj, GetAsInt(name) ?? prop.GetValue(defaultValue));
                }
                else if (type == typeof(float))
                {
                    prop.SetValue(obj, GetAsFloat(name) ?? prop.GetValue(defaultValue));
                }
                else if (type == typeof(bool))
                {
                    prop.SetValue(obj, GetAsBool(name) ?? prop.GetValue(defaultValue));
                }
                else if (type == typeof(string))
                {
                    prop.SetValue(obj, GetAsString(name) ?? prop.GetValue(defaultValue));
                }
            }
            return obj;
        }
        catch (Exception e)
        {
            Monitor.SLog($"Failed get the {typeof(Poco).Name} object:\n{e}", LogLevel.Error);
            return null;
        }
    }

    private static readonly HashSet<Type> allowedTypes = [typeof(int), typeof(bool), typeof(float), typeof(string)];
    public static bool IsValidPocoClass() => IsValidPocoClass(out var _, out var _);
    public static bool IsValidPocoClass(out List<string> errors) => IsValidPocoClass(out errors, out var _);
    public static bool IsValidPocoClass(out List<string> errors, out bool isEmpty)
    {
        bool ret = true;
        isEmpty = true;
        errors = [];
        if (typeof(Poco).GetConstructor(Type.EmptyTypes) == null)
        {
            errors.Add("No constructor without arguments has been defined");
            ret = false;
        }
        var namePattern = new Regex(@"^[a-zA-Z_][a-zA-Z_0-9]*$");
        foreach (var prop in typeof(Poco).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var isValid = true;
            if (!namePattern.IsMatch(prop.Name))
            {
                errors.Add($"Invalid name format (name: {prop.Name})");
                isValid = false;
            }
            if (!prop.CanRead)
            {
                errors.Add($"Cannot read the property {prop.Name}");
                isValid = false;
            }
            if (!prop.CanWrite)
            {
                errors.Add($"Cannot write the property {prop.Name}");
                isValid = false;
            }
            Type type = prop.PropertyType;
            if (!allowedTypes.Contains(type))
            {
                errors.Add($"Invalid type (property: {prop.Name}, type: {type})");
                isValid = false;
            }
            ret = ret && isValid;
            isEmpty = isEmpty && !isValid;
        }
        return ret;
    }
}

partial class FileWrapper
{
    public static ConfigFile<T> ConfigFile<T>(this IMod mod, string baseName, string? comments = null, Dictionary<string, string>? propertyComments = null, Func<T>? getDefaultValue = null)
        where T : class => new(mod, baseName, comments, propertyComments, getDefaultValue);
}

