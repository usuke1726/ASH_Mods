
using ModdingAPI;
using Sidequel.System;

namespace Sidequel;

internal static class Cont
{
    internal enum Stages
    {
        High, Mid, Low
    }
    internal static Stages Stage => Value switch
    {
        > Const.Cont.MidBorderValue => Stages.High,
        > Const.Cont.LowBorderValue => Stages.Mid,
        _ => Stages.Low
    };
    internal static bool IsHigh => Stage == Stages.High;
    internal static bool IsMid => Stage == Stages.Mid;
    internal static bool IsLow => Stage == Stages.Low;
    internal static bool IsHighOrMid => Stage != Stages.Low;
    internal static bool IsMidOrLow => Stage != Stages.High;
    internal static bool IsEndingCont => Value <= Const.Cont.EndingBorderValue;
    internal static event Action OnReachedEndingCont = null!;

    private static int value;
    internal static int Value
    {
        get
        {
            EnsureLoaded();
            return value;
        }
    }
    private static bool setupDone = false;
    private static void EnsureLoaded()
    {
        if (!State.IsActive || setupDone) return;
        if (State.IsNewGame) Reset();
        else Load();
        setupDone = true;
    }
    internal static void Setup(IModHelper helper)
    {
        helper.Events.Gameloop.GameStarted += (_, _) => EnsureLoaded();
        helper.Events.Gameloop.ReturnedToTitle += (_, _) =>
        {
            setupDone = false;
        };
    }
    private static void Reset()
    {
        value = Const.Cont.InitialValue;
        Save();
    }
    private static void Load()
    {
        if (!STags.TryGetInt(Const.STags.Cont, out value))
        {
            Monitor.Log("Failed to load the cont value", LL.Debug);
        }
    }
    private static void Save()
    {
        STags.SetInt(Const.STags.Cont, value);
    }
    internal static void Add(int val)
    {
        var newValue = Math.Max(value + val, 0);
#if DEBUG
        Assert(val < 0, $"updateCont value is expected to be negative (val: {val})");
        Monitor.Log($"== Cont Updated: {value,3} -> {newValue,3} ({val})", LL.Debug, true);
        Assert(Flags.AfterJA || IsHigh, "It should be only HighCont on the BeforeJA state!");
#endif
        var wasNotEndingCont = !IsEndingCont;
        value = newValue;
        Save();
        if (wasNotEndingCont && IsEndingCont) OnReachedEndingCont?.Invoke();
    }
}

