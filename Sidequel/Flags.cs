
using ModdingAPI;
using Sidequel.System;
using NodeStates = Sidequel.Dialogue.NodeEntryBase.NodeStates;

namespace Sidequel;

internal static class Flags
{
    internal static bool BeforeJA => !AfterJA;
    internal static bool AfterJA => STags.GetBool(Const.STags.JADone);
    internal static bool JATriggeredByJon => STags.GetBool(Const.STags.JATriggeredByJon);

    internal static bool NodeYet(string id) => GetNodeState(id) == NodeStates.NotYet;
    internal static bool NodeIP(string id) => GetNodeState(id) == NodeStates.InProgress;
    internal static bool NodeRefused(string id) => GetNodeState(id) == NodeStates.Refused;
    internal static bool NodeDone(string id) => GetNodeState(id) == NodeStates.Done;
    internal static bool NodeIs(string id, NodeStates state) => GetNodeState(id) == state;

    internal static void Setup(IModHelper helper)
    {
        helper.Events.Gameloop.GameStarted += (_, _) =>
        {
            if (!State.IsActive) return;
            NodeState.Load();
        };
        helper.Events.Gameloop.ReturnedToTitle += (_, _) =>
        {
            NodeState.ResetWatchers();
        };
    }
    internal static NodeStates GetNodeState(string id) => NodeState.Get(id);
    internal static void SetNodeState(string id, NodeStates state) => NodeState.Set(id, state);
    internal static void OnStateChanged(string id, Action<StateChangeInfo> action) => NodeState.OnStateChanged(id, action);
    internal static void OnStateChanged(string id, Action action) => NodeState.OnStateChanged(id, action);
    internal readonly struct StateChangeInfo(NodeStates value, NodeStates prev)
    {
        internal readonly NodeStates value = value;
        internal readonly NodeStates prev = prev;
    }
    private static class NodeState
    {
        delegate void OnStateChangedAction(StateChangeInfo info);
        private static readonly Dictionary<string, int> states = [];
        private static readonly Dictionary<string, OnStateChangedAction> actions = [];
        internal static NodeStates Get(string id)
        {
            if (states.TryGetValue(id, out var v)) return (NodeStates)v;
            return (NodeStates)(states[id] = 0);
        }
        internal static void Set(string id, NodeStates state)
        {
            var prev = Get(id);
            states[id] = (int)state;
            if (actions.TryGetValue(id, out var action)) action(new(state, prev));
            STags.SetString(Const.STags.NodeStates, Serialize());
        }
        internal static void OnStateChanged(string id, Action action) => OnStateChanged(id, _ => action());
        internal static void OnStateChanged(string id, Action<StateChangeInfo> action)
        {
            var d = new OnStateChangedAction(action);
            if (actions.ContainsKey(id)) actions[id] += d;
            else actions[id] = d;
        }
        internal static void ResetWatchers()
        {
            actions.Clear();
        }
        private static string Serialize()
        {
            return string.Join(";", states.Select(pair => $"{pair.Key}:{pair.Value}"));
        }
        internal static void Load()
        {
            states.Clear();
            var data = STags.GetString(Const.STags.NodeStates);
            if (data == null) return;
            foreach (var d in data.Split(";"))
            {
                var l = d.Split(":", 2);
                if (l.Length < 2) continue;
                if (!int.TryParse(l[1], out var v)) continue;
                states[l[0]] = v;
            }
        }
    }
}

