
using ModdingAPI;
using UnityEngine;

namespace SideStory.Dialogue;

internal static class NodeSelector
{
    private static readonly Dictionary<Characters, List<Node>> nodes = [];
    private static readonly List<Node> nullNodes = [];
    private static readonly List<Node> globalNodes = [];
    internal static Node? Find(Transform? speaker)
    {
        var character = ModdingAPI.Character.TryGet(speaker!, out var ch) ? ch : null;
        Node? node = null;
        if (character != null)
        {
            Debug($"== talking to the character {character.character}");
            node = nodes.TryGetValue(character.character, out var list) ? list.Find(n => n.condition()) : null;
        }
        else
        {
            node = nullNodes.Find(n => n.condition());
        }
        return node ?? globalNodes.Find(n => n.condition());
    }
    internal static void OnSetupDone()
    {
        static int Compare(Node n1, Node n2) => n2.priority.CompareTo(n1.priority);
        foreach (var list in nodes.Values) list.Sort(Compare);
        globalNodes.Sort(Compare);
    }
    internal static void RegisterNode(Node node) => globalNodes.Add(node);
    internal static void RegisterNode(Characters? character, Node node)
    {
        if (character == null) nullNodes.Add(node);
        else
        {
            var ch = (Characters)character;
            nodes.TryAdd(ch, []);
            nodes[ch].Add(node);
        }
    }
}

