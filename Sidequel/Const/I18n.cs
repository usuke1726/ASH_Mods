
namespace Sidequel;

partial class Const
{
    internal static class I18n
    {
        internal const string NodePrefix = "node";
    }
    internal static readonly Func<string, string> formatJATrigger = s =>
    {
        var key = Flags.JATriggeredByJon ? "JATrigger.Jon" : "JATrigger.Alex";
        var trigger = I18n_.Localize(key);
        return s.Replace("{{JATrigger}}", trigger);
    };
}

