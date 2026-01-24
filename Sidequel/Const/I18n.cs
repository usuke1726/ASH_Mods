
namespace Sidequel;

partial class Const
{
    internal static class I18n
    {
        internal const string NodePrefix = "node";
    }
    internal static readonly Func<string, string> formatJATrigger = s =>
    {
        var key = Flags.JATriggeredByJon ? "system.JATrigger.Jon" : "system.JATrigger.Alex";
        var trigger = I18nLocalize(key);
        return s.Replace("{{JATrigger}}", trigger);
    };
}

