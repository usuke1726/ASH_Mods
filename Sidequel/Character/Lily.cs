
using ModdingAPI;
using UnityEngine;

namespace Sidequel.Character;

internal class Lily : Core.NPC
{
    internal override ModdingAPI.Character Character => ch;
    private ModdingAPI.Character ch = null!;
    internal override bool Match(string name, bool matchAsObjectName)
    {
        if (name == Const.Object.Lily) return true;
        if (matchAsObjectName) return false;
        return name == "Lily";
    }
    private static readonly ColorMapChanger colorChanger = new([
        new( new(0.420f, 0.349f, 1.000f, 1.000f), new(0.55f, 0.59f, 0.37f, 1) ),
        new( new(0.431f, 0.361f, 1.000f, 1.000f), new(0.61f, 0.66f, 0.41f) ),
        new( new(0.561f, 0.333f, 1.000f, 1.000f), new(0.29f, 0.59f, 0.28f, 1) ),
        new( new(1.000f, 0.459f, 0.678f, 1.000f), new(0.65f, 0.37f, 0.14f, 1) ),
        new( new(1.000f, 0.953f, 0.906f, 1.000f), new(0.907f, 0.721f, 0.48f, 1) ),
    ])
    {
        eyeColors = new()
        {
            eye = new(1, 0.867f, 0.740f, 1),
            happyEye = new(1, 0.867f, 0.740f, 1),
        }
    };
    internal override void Create()
    {
        var NPCs = GameObject.Find("NPCs");
        if (NPCs == null) return;
        var auntMay = NPCs.transform.Find("Bunny_WalkingNPC (1)").gameObject;
        if (auntMay == null) return;
        var obj = auntMay.Clone();
        obj.name = Const.Object.Lily;
        obj.GetComponentInChildren<Animator>().speed = 0.6f;
        GameObject.Destroy(obj.GetComponent<WanderNPCMovement>());
        obj.GetComponent<NavMeshNavigator>().enabled = false;
        ch = new ModdingAPI.Character((Characters)Const.Object.LilyObjectId, obj.transform);

        obj.transform.parent = NPCs.transform;
        obj.transform.position = new(75.2075f, 72.0033f, 794.7939f);
        obj.transform.localRotation = Quaternion.Euler(0, 357.6292f, 0);

        obj.transform.Find("Rabbit/Head").gameObject.SetActive(false);

        colorChanger
            .WithMaterials(obj.transform.Find("Rabbit/Body").GetComponent<SkinnedMeshRenderer>().material)
            .WithHead(obj.transform.Find("Rabbit/Armature/root/Base/Chest/Head_0"))
            .Apply();

        Pose.Set(ch.transform, Poses.Sitting);
    }
}

