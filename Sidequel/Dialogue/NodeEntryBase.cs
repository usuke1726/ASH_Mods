
using System.Collections;
using ModdingAPI;
using Sidequel.Dialogue.Actions;
using Sidequel.System;
using UnityEngine;

namespace Sidequel.Dialogue;

internal abstract class NodeEntryBase
{
    protected abstract Node[] Nodes { get; }
    internal abstract void Setup();

    protected static string Player => Character.Player;
    protected static string Original => Character.Original;
    protected static int LastSelected => DialogueController.instance.LastSelected;

#pragma warning disable IDE1006
    protected static IEnumerable<int> range(int minInclusive, int maxInclusive) => Enumerable.Range(minInclusive, Math.Max(maxInclusive + 1 - minInclusive, 1));
    protected static CommandAction command(Action action, bool hideBox = true, string? anchor = null) => new(action, hideBox, anchor);
    protected static CommandAction command(Func<IEnumerator> coroutine, bool hideBox = true, string? anchor = null) => new(coroutine, hideBox, anchor);
    protected static GetItemAction item(string itemId, string? anchor = null) => new(() => itemId, anchor);
    protected static GetItemAction item(Func<string> getItemId, string? anchor = null) => new(getItemId, anchor);
    protected static AddItemAction item(string itemId, int amount, string? anchor = null) => new(() => itemId, () => amount, anchor);
    protected static AddItemAction item(Func<string> getItemId, int amount, string? anchor = null) => new(getItemId, () => amount, anchor);
    protected static AddItemAction item(Func<string> getItemId, Func<int> getAmount, string? anchor = null) => new(getItemId, getAmount, anchor);
    protected static AddMultipleItemsAction item(string[] itemIds, int[] amounts, string? anchor = null) => new(() => itemIds, () => amounts, anchor);
    protected static AddMultipleItemsAction item(Func<string[]> getItemIds, Func<int[]> getAmounts, string? anchor = null) => new(getItemIds, getAmounts, anchor);
    protected static EmoteAction emote(Emotes emotion, string speaker, string? anchor = null) => new(emotion, speaker, anchor);
    protected static GotoAction @goto(string target, string? anchor = null) => new(target, anchor);
    protected static IfAction @if(Func<bool> condition, string? trueAnchor, string? falseAnchor = null, string? anchor = null) => new(condition, trueAnchor, falseAnchor, anchor);
    protected static IfSingleAction @if(Func<bool> condition, IInvokableInAction? trueAction, IInvokableInAction? falseAction = null, string? anchor = null) => new(condition, trueAction, falseAction, anchor);
    protected static SwitchAction @switch(Func<int> getIndex, IEnumerable<string?> anchors, string? anchor = null) => new(getIndex, anchors, anchor);
    protected static SwitchAction @switch(Func<string> getAnchor, string? anchor = null) => new(getAnchor, anchor);
    protected static LineAction line(string line, string speaker, Func<bool>? condition = null, Func<string, string>? replacer = null, bool useId = true, string? anchor = null) => new(line, speaker, condition, replacer, useId, anchor);
    protected static LineAction line(int index, string speaker, Func<bool>? condition = null, Func<string, string>? replacer = null, bool useId = true, string? anchor = null) => new($"{index:00}", speaker, condition, replacer, useId, anchor);
    protected static LineIfAction lineif(Func<bool> condition, string trueLine, string falseLine, string speaker, Func<string, string>? replacer = null, bool useId = true, string? anchor = null) => new(condition, trueLine, falseLine, speaker, replacer, useId, anchor);
    protected static RangedLinesAction lines(int minInclusive, int maxInclusive, Func<int, string> getI18nKey, string speaker, List<Tuple<int, IInvokableInAction>>? actions = null, Func<string, string>? replacer = null, bool useId = true, string? anchor = null) => new(minInclusive, maxInclusive, getI18nKey, speaker, actions, replacer, useId, anchor);
    protected static RangedLinesAction lines(int minInclusive, int maxInclusive, Func<int, string> getI18nKey, Func<int, string> getSpeaker, List<Tuple<int, IInvokableInAction>>? actions = null, Func<string, string>? replacer = null, bool useId = true, string? anchor = null) => new(minInclusive, maxInclusive, getI18nKey, getSpeaker, actions, replacer, useId, anchor);
    protected static RangedLinesAction lines(int minInclusive, int maxInclusive, Func<int, string> getI18nKey, HashSet<int> playerIndexes, List<Tuple<int, IInvokableInAction>>? actions = null, Func<string, string>? replacer = null, bool useId = true, string? anchor = null) => new(minInclusive, maxInclusive, getI18nKey, playerIndexes, actions, replacer, useId, anchor);
    protected static IndexedLinesAction lines(Func<int, string> getI18nKey, string speaker, List<Tuple<int, IInvokableInAction>>? actions = null, Func<string, string>? replacer = null, bool useId = true, string? anchor = null) => new(getI18nKey, speaker, actions, replacer, useId, anchor);
    protected static IndexedLinesAction lines(Func<int, string> getI18nKey, Func<int, string> getSpeaker, List<Tuple<int, IInvokableInAction>>? actions = null, Func<string, string>? replacer = null, bool useId = true, string? anchor = null) => new(getI18nKey, getSpeaker, actions, replacer, useId, anchor);
    protected static IndexedLinesAction lines(Func<int, string> getI18nKey, HashSet<int> playerIndexes, List<Tuple<int, IInvokableInAction>>? actions = null, Func<string, string>? replacer = null, bool useId = true, string? anchor = null) => new(getI18nKey, playerIndexes, actions, replacer, useId, anchor);
    protected static OptionAction option(string[] options, Func<string, string>? replacer = null, bool useId = true, string? anchor = null) => new(options, replacer, useId, anchor);
    protected static Anchor anchor(string anchor) => new(anchor);
    protected static NodeCompleteAction end(Func<bool>? condition = null, string? anchor = null) => new(condition, anchor);
    protected static TagAction tag(string id, object value, string? anchor = null) => new(id, value, anchor);
    protected static TagAction tag(string id, Func<object> getValue, string? anchor = null) => new(id, getValue, anchor);
    protected static TransitionAction transition(Action action, bool hideBox = true, string? anchor = null) => new(action, hideBox, anchor);
    protected static WaitAction wait(float time, bool hideBox = true, string? anchor = null) => new(time, hideBox, anchor);
    protected static UpdateContAction cont(int value, string? anchor = null) => new(value, anchor);
    protected static UpdateNodeStateAction state(NodeStates state, string? anchor = null) => new(state, anchor);
    protected static UpdateNodeStateAction state(string nodeId, NodeStates state, string? anchor = null) => new(nodeId, state, anchor);
    protected static UpdateNodeStateAction done() => state(NodeStates.Done);
    protected static UpdateNodeStateAction done(string nodeId) => state(nodeId, NodeStates.Done);
    protected static string digit1(int i) => $"{i}";
    protected static string digit2(int i) => $"{i:00}";
    protected static string digit3(int i) => $"{i:000}";
    protected static Func<int, string> digit1(string id, string sep = ".") => i => $"{id}{sep}{i}";
    protected static Func<int, string> digit2(string id, string sep = ".") => i => $"{id}{sep}{i:00}";
    protected static Func<int, string> digit3(string id, string sep = ".") => i => $"{id}{sep}{i:000}";
    protected static bool _H => Cont.IsHigh;
    protected static bool _M => Cont.IsMid;
    protected static bool _L => Cont.IsLow;
    protected static bool _HM => Cont.IsHighOrMid;
    protected static bool _ML => Cont.IsMidOrLow;
    protected static bool _bJA => Flags.BeforeJA;
    protected static bool _aJA => Flags.AfterJA;
    protected static bool _JAJon => Flags.JATriggeredByJon;
    protected static Tags _tags => Context.globalData.gameData.tags;
#pragma warning restore IDE1006

    sealed public override string ToString() => base.ToString();
    sealed public override bool Equals(object obj) => base.Equals(obj);
    sealed public override int GetHashCode() => base.GetHashCode();

    internal virtual void OnGameStarted() { }

    protected static ModdingAPI.Character Ch(string id)
    {
        if (Character.TryGetCharacter(id, out var ch)) return ch;
        Debug("character with id \"{id}\" not found!!!");
        return null!;
    }
    protected static ModdingAPI.Character Ch(Characters ch) => Character.Get(ch);
    protected static void Pose(string id, Poses pose) => Pose(Ch(id).transform, pose);
    protected static void Pose(Characters ch, Poses pose) => Pose(Ch(ch).transform, pose);
    protected static void Pose(Characters ch, RuntimeAnimatorController controller) => Pose(Ch(ch).transform, controller);
    protected static void Pose(Transform target, Poses pose) => Sidequel.Character.Pose.Set(target, pose);
    protected static void Pose(Transform target, RuntimeAnimatorController controller) => Sidequel.Character.Pose.Set(target, controller);
    protected static void Move(string id, Vector3? position = null, Vector3? rotation = null) => Move(Ch(id).transform, position, rotation);
    protected static void Move(Characters ch, Vector3? position = null, Vector3? rotation = null) => Move(Ch(ch).transform, position, rotation);
    protected static void Move(Transform target, Vector3? position = null, Vector3? rotation = null)
    {
        if (position != null) target.position = (Vector3)position;
        if (rotation != null) target.localRotation = Quaternion.Euler((Vector3)rotation);
    }
    protected static void SetNext(string nodeId, Characters? ch) => DialogueController.instance.SetNext(nodeId, ch);
    protected static bool NodeDone(string id) => Flags.NodeDone(id);
    protected static bool NodeYet(string id) => Flags.NodeYet(id);
    protected static bool NodeIP(string id) => Flags.NodeIP(id);
    protected static bool NodeRefused(string id) => Flags.NodeRefused(id);
    protected static bool GetBool(string id, bool defaultValue = false) => STags.GetBool(id, defaultValue);
    protected static string GetString(string id, string? defaultValue = null) => STags.GetString(id, defaultValue);
    protected static int GetInt(string id, int defaultValue = 0) => STags.GetInt(id, defaultValue);
    protected static float GetFloat(string id, float defaultValue = 0) => STags.GetFloat(id, defaultValue);

    internal enum NodeStates
    {
        NotYet = 0,
        InProgress = 1,
        Refused = 2,
        Done = 3,
    }
}

