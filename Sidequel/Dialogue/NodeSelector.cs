
using ModdingAPI;
using UnityEngine;

namespace Sidequel.Dialogue;

internal static class NodeSelector
{
    private static readonly Dictionary<Characters, List<Node>> nodes = [];
    private static readonly List<Node> nullNodes = [];
    private static readonly List<Node> globalNodes = [];
    private static readonly Dictionary<string, List<Node>> nodesFromStartNode = [];
    internal static Node? Find(DialogueInteractable? dialogue)
    {
        if (dialogue != null && TryToFindFromStartNode(dialogue.startNode, out var sNode)) return sNode;
        var speaker = dialogue?.transform;
        var character = SpeakerToCharacter(speaker);
        Node? node = null;
        if (character != null)
        {
            //Debug($"== talking to the character {character.character}");
            node = nodes.TryGetValue(character.character, out var list) ? list.Find(n => n.condition()) : null;
        }
        else
        {
            node = nullNodes.Find(n => n.condition());
        }
        return node ?? globalNodes.Find(n => n.condition());
    }
    internal static bool TryToFindFromStartNode(string startNode, out Node? result)
    {
        result = null;
        if (nodesFromStartNode.TryGetValue(startNode, out var sNodes))
        {
            result = sNodes.Find(n => n.condition());
            return true;
        }
        return false;
    }
    private static ModdingAPI.Character? SpeakerToCharacter(Transform? speaker)
    {
        if (speaker != null && Sidequel.Character.Core.TryGetNPC(speaker.name, true, out var npc)) return npc;
        return ModdingAPI.Character.TryGet(speaker!, out var ch) ? ch : null;
    }
    internal static Node? Find(string id, Characters? character)
    {
        var list = character == null ? nullNodes : nodes[(Characters)character];
        return list.Find(n => n.id == id) ?? globalNodes.Find(n => n.id == id);
    }
    private static readonly HashSet<string> vanillaDialogueNodes = [
        "ToMainIslandStart",
        "ToSecretIslandStart",
        "MeteorLakeSignStart",
        "MeteorLakeLeftSignStart",
        "KioskSignStart",
        "VistorCenterCrossroadsStart",
        "WhiteCoastSign",
        "BeachHutSign",
        "HawkPeakRiverSignStart",
        "RoyalRidgeSignStart",
        "OutlookPointSignStart",
        "HawkPeakSignStart",
        "FireTowerSnowSignStart",
        "OrangeIslandsSign",
        "VisitorsCenterSignStart",
        "HawkPeakAltSignStart",
        "HomeSignStart",
    ];
    internal static bool UseVanillaNode(DialogueInteractable dialogue)
    {
        return vanillaDialogueNodes.Contains(dialogue.startNode);
    }
    internal static void OnSetupDone()
    {
        static int Compare(Node n1, Node n2) => n2.priority.CompareTo(n1.priority);
        foreach (var list in nodes.Values) list.Sort(Compare);
        foreach (var list in nodesFromStartNode.Values) list.Sort(Compare);
        nullNodes.Sort(Compare);
        globalNodes.Sort(Compare);
    }
    internal static void RegisterNode(Node node) => globalNodes.Add(node);
    internal static void RegisterNodeFromStartNode(string startNode, Node node)
    {
        nodesFromStartNode.TryAdd(startNode, []);
        nodesFromStartNode[startNode].Add(node);
    }
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

