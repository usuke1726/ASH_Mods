
using UnityEngine;

namespace SideStory.Dialogue;

internal static class NodeSelector
{
    private static readonly List<Node> nodes = [];
    internal static Node? Find(Transform? speaker)
    {
        if (ModdingAPI.Character.TryGet(speaker!, out var character))
        {
            Debug($"== talking to the character {character.character}");
        }
        return nodes.Find(n => n.condition());
    }
    internal static void OnSetupDone()
    {
        nodes.Sort((n1, n2) => n2.priority.CompareTo(n1.priority));
    }
    internal static void RegisterNode(Node node) => nodes.Add(node);
}

