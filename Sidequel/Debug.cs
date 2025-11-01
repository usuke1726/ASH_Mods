
global using static Sidequel.Debug;
using StackTraceObj = System.Diagnostics.StackTrace;

namespace Sidequel;

internal class Debug
{
    [Conditional("DEBUG")]
    internal static void StackTrace() => Monitor.Log(StackTraceMes(), LL.Warning, true);
    [Conditional("DEBUG")]
    internal static void AssertCall(bool condition)
    {
        if (condition) return;
        var name = new StackFrame(1, false).GetMethod().Name;
        Monitor.Log($"{name} called at unexpected timing!!\n{StackTraceMes()}", LL.Error);
    }
    [Conditional("DEBUG")]
    internal static void Assert(bool condition, string errorMes = "")
    {
        if (condition) return;
        Monitor.Log($"Assertion failed!! ({errorMes})\n{StackTraceMes()}", LL.Error);
    }
    private static string StackTraceMes()
    {
        var fs = new StackTraceObj(false).GetFrames();
        Dictionary<Type, string> map = new()
        {
            [typeof(bool)] = "bool",
            [typeof(int)] = "int",
            [typeof(float)] = "float",
            [typeof(object)] = "object",
            [typeof(string)] = "string",
        };
        string ttos(Type type) => map.TryGetValue(type, out var t) ? t : $"{type}";
        var mes = string.Join("\n", fs[2..Math.Min(fs.Length, 22)]
            .Select(f =>
            {
                var m = f.GetMethod();
                var pstr = string.Join(", ", m.GetParameters().Select(p => $"{ttos(p.ParameterType)} {p.Name}"));
                return $"  at {m.DeclaringType.FullName}.{m.Name}({pstr})";
            })
        );
        return $"=== STACKTRACE ===\n\n{mes}\n\n{new string('=', 18)}\n";
    }
}

