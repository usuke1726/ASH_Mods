
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

    internal static void Setup(IModHelper helper)
    {
        helper.Events.Gameloop.GameStarted += (_, _) =>
        {
            if (!State.IsActive) return;
            NodeState.Load();
        };
    }
    internal static NodeStates GetNodeState(string id) => NodeState.Get(id);
    internal static void SetNodeState(string id, NodeStates state) => NodeState.Set(id, state);
    private static class NodeState
    {
        private static readonly Dictionary<string, int> states = [];
        internal static NodeStates Get(string id)
        {
            if (states.TryGetValue(id, out var v)) return (NodeStates)v;
            return (NodeStates)(states[id] = 0);
        }
        internal static void Set(string id, NodeStates state)
        {
            states[id] = (int)state;
            STags.SetString(Const.STags.NodeStates, Serialize());
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

