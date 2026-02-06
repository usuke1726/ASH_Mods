
using ModdingAPI;
using UnityEngine;

namespace Sidequel.Character;

internal class Leo : Core.NPC
{
    internal override ModdingAPI.Character Character => ch;
    private ModdingAPI.Character ch = null!;
    internal override bool Match(string name, bool matchAsObjectName)
    {
        if (name == Const.Object.Leo) return true;
        if (matchAsObjectName) return false;
        return name == "Leo";
    }
    private static readonly ColorMapChanger colorChanger = new([
        new( new(0.000f, 0.000f, 0.000f, 1.000f), new(0.33f, 0.14f, 0, 1) ),
        new( new(0.290f, 0.541f, 0.161f, 1.000f), new(0, 0.48f, 0, 1) ),
        new( new(0.451f, 0.843f, 0.259f, 1.000f), new(0.7f, 0, 0, 1) ),
        new( new(0.722f, 0.902f, 0.267f, 1.000f), new(0.20f, 0.19f, 0.50f, 1) ),
        new( new(0.741f, 0.953f, 0.647f, 1.000f), new(0.93f, 0.75f, 0.69f, 1) ),
        new( new(0.871f, 0.875f, 0.290f, 1.000f), new(0.20f, 0.19f, 0.50f, 1) ),
        new( new(0.906f, 0.843f, 0.161f, 1.000f), new(0.4f, 0.4f, 0.4f, 1) ),
        new( new(0.906f, 0.859f, 0.161f, 1.000f), new(0.20f, 0.19f, 0.50f, 1) ),
        new( new(0.937f, 0.843f, 0.290f, 1.000f), new(0.20f, 0.19f, 0.50f, 1) ),
        new( new(1.000f, 0.000f, 0.000f, 1.000f), new(0.4f, 0.4f, 0.4f, 1) ),
    ])
    {
        eyeColors = new()
        {
            eye = new(1, 1, 0.657f, 1),
            blink = new(1, 1, 0.657f, 1),
            happyEye = new(1, 1, 0.657f, 1),
        }
    };
    internal override void Create()
    {
        var NPCs = GameObject.Find("NPCs");
        if (NPCs == null) return;
        var croc = NPCs.transform.GetChildren().FirstOrDefault(child =>
            child.name == "RunningNPC" && child.GetComponent<DialogueInteractable>()?.startNode == "Runner2Start"
        );
        if (croc == null) return;
        var obj = croc.gameObject.Clone();
        GameObject.Destroy(obj.GetComponent<NavMeshNavigator>());
        var path = obj.GetComponent<PathNPCMovement>();
        path.pathParent = CreatePath();
        path.maxSpeed = 5.0f;
        obj.name = Const.Object.Leo;
        ch = new ModdingAPI.Character((Characters)Const.Object.LeoObjectId, obj.transform);

        obj.transform.parent = NPCs.transform;
        obj.transform.position = pathNodes[^1].position;

        string[] parts = ["Body", "Arms", "Head", "Legs"];
        colorChanger
            .WithMaterials(parts.Select(s => obj.transform.Find($"Rabbit/{s}").GetComponent<SkinnedMeshRenderer>().material))
            .WithHead(ch.transform.Find("Rabbit/Armature/root/Base/Chest/Head_0"))
            .Apply();

        Pose.Set(ch.transform, Poses.Walking);
    }
    private static Transform CreatePath()
    {
        var root = GameObject.Find("/Paths").transform;
        var prefab = root.Find("MeteorOverlookPath/Node (0)").gameObject;
        var container = new GameObject("Sidequel_LeosPath").transform;
        container.parent = root;
        int count = 0;
        foreach (var node in pathNodes)
        {
            var child = prefab.Clone().transform;
            child.position = node.position;
            child.GetComponent<PathNode>().waitTime = node.waitTime;
            child.name = $"Node ({count++})";
            child.parent = container;
        }
        return container;
    }

    private static readonly NodeData start = new(219.5338f, 60.9636f, 178.5871f, 5);
    private static readonly NodeData turning = new(316.1212f, 47.1807f, 77.6857f, 5);
    private static readonly NodeData[] bridgeSide = [
        new(222.8058f, 60.9614f, 181.8729f),
        new(242.6634f, 60.3796f, 176.0946f),
        new(241.023f, 58.4602f, 169.3922f),
        new(241.0233f, 46.6152f, 97.648f),
        new(242.9015f, 45.0885f, 90.4296f),
    ];
    private static readonly NodeData[] beachSide = [
        new(240.166f, 46.8474f, 76.1302f),
        new(251.7951f, 48.4018f, 66.3176f),
        new(255.9276f, 48.4209f, 50.8192f),
        new(274.8341f, 48.2191f, 55.7165f),
        new(286.592f, 48.0219f, 60.9046f),
        new(312.6506f, 46.7049f, 80.31f),
    ];
    private static readonly NodeData[] pathNodes = [
        .. bridgeSide,
        new(237.136f, 46.8087f, 80.2693f),
        .. beachSide,
        turning,
        .. beachSide.Reverse(),
        new(228.0177f, 44.8733f, 80.2878f, 3),
        .. bridgeSide.Reverse(),
        start
    ];
    private class NodeData(float x, float y, float z, float waitTime = 0)
    {
        internal readonly Vector3 position = new Vector3(x, y, z) + Vector3.up * 1.0f;
        internal readonly float waitTime = waitTime;
    }
}

