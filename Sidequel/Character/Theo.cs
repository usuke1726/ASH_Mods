
using ModdingAPI;
using UnityEngine;

namespace Sidequel.Character;

internal class Theo : Core.NPC
{
    internal override ModdingAPI.Character Character => ch;
    private ModdingAPI.Character ch = null!;
    internal override bool Match(string name, bool matchAsObjectName)
    {
        if (name == Const.Object.Theo) return true;
        if (matchAsObjectName) return false;
        return name == "Theo";
    }
    private static readonly ColorMapChanger colorChanger = new([
        new( new(0.192f, 0.204f, 0.322f, 1.000f), new(0.5f, 0.3f, 0.3f, 1) ),
        new( new(0.451f, 0.729f, 0.741f, 1.000f), new(0.35f, 0.24f, 0.15f, 1) ),
        new( new(0.502f, 0.651f, 0.047f, 1.000f), new(0.45f, 0.31f, 0.20f, 1) ),
        new( new(1.000f, 0.804f, 0.937f, 1.000f), new(0.4f, 0.2f, 0.2f, 1) ),

        new( new(0.580f, 0.573f, 0.741f, 1.000f), new(0f, 0f, 0.1f, 1) ),
        new( new(0.580f, 0.576f, 0.741f, 1.000f), new(0f, 0f, 0.1f, 1) ),
        new( new(0.698f, 0.694f, 0.827f, 1.000f), new(0f, 0f, 0.1f, 1) ),
        // 色指定上では肌色は黒だが、実際は紺色に見える。これはGameCameraにあるColorCorrectionCurvesによるもの。(実際これをdisabledにすると黒くなる)
    ])
    {
        eyeColors = new()
        {
            eye = new(1, 1, 0.657f, 1),
            blink = new(0.3f, 0.3f, 0.3f, 1),
            happyEye = new(1, 1, 0.657f, 1),
        }
    };
    internal override void Create()
    {
        var NPCs = GameObject.Find("NPCs");
        if (NPCs == null) return;
        var prefab = NPCs.transform.Find("CamperNPC").gameObject;
        if (prefab == null) return;
        var obj = prefab.Clone();
        obj.name = Const.Object.Theo;
        obj.GetComponentInChildren<Animator>().speed = 0.6f;
        GameObject.Destroy(obj.GetComponent<FishingPermitEvent>());
        ch = new ModdingAPI.Character((Characters)Const.Object.TheoObjectId, obj.transform);

        obj.transform.parent = NPCs.transform;
        obj.transform.position = new(422.063f, 13.8907f, 1076.111f);
        obj.transform.localRotation = Quaternion.Euler(0, 5.5787f, 0);

        var head = obj.transform.Find("Bird/Armature/root/Base/Chest/Head_0");
        colorChanger
            .WithMaterials(obj.transform.Find("Bird/Body").GetComponent<SkinnedMeshRenderer>().material)
            .WithHead(head)
            .Apply();

        Pose.Set(ch.transform, Poses.Standing);
        head.GetComponent<Animator>().speed = 0.5f;

        var circleSprite = NPCs.transform.Find("AuntMayNPC/Bird/Armature/root/Base/Chest/Head_0/EyeL").GetComponent<SpriteRenderer>().sprite;
        var eyeR = head.Find("EyeR");
        var eyeL = head.Find("EyeL");
        eyeR.GetComponent<SpriteRenderer>().sprite = circleSprite;
        eyeL.GetComponent<SpriteRenderer>().sprite = circleSprite;
        eyeR.Find("Pupil").GetComponent<SpriteRenderer>().sprite = circleSprite;
        eyeL.Find("Pupil").GetComponent<SpriteRenderer>().sprite = circleSprite;

        float z = 268.5268f;
        eyeL.localRotation = Quaternion.Euler(eyeL.localRotation.eulerAngles with { z = z });
        eyeR.localRotation = Quaternion.Euler(eyeR.localRotation.eulerAngles with { z = z });
        var happyL = eyeL.Find("HappyEyes");
        var happyR = eyeR.Find("HappyEyes");
        float z2 = 358.9214f;
        happyL.localRotation = Quaternion.Euler(happyL.localRotation.eulerAngles with { z = z2 });
        happyR.localRotation = Quaternion.Euler(happyR.localRotation.eulerAngles with { z = z2 });

        TheosUmbrella.Create();
    }
    private class TheosUmbrella
    {
        private static readonly UmbrellaColorChanger colorChanger = new();
        internal static void Create()
        {
            var prefab = GameObject.Find("Umbrella (3)");
            if (prefab == null)
            {
                Debug($"Umbrella is null", LL.Error);
                return;
            }
            var obj = prefab.Clone();
            obj.name = "Sidequel_TheosUmbrella";
            obj.transform.position = new(420.7063f, 13.7506f, 1074.572f);
            obj.transform.localRotation = Quaternion.Euler(295.2865f, 88.0798f, 92.6738f);
            obj.GetComponent<BouncyCollider>().bounceSpeed = 30f;
            colorChanger.WithMaterials(obj.GetComponentInChildren<MeshRenderer>().material).Apply();
        }
        private class UmbrellaColorChanger : ColorChanger
        {
            protected override Color Change(Color color)
            {
                if (color.g > 0.9f) return new(0.3f, 0.3f, 0.3f, 1);
                if (color.r > 0.9f) return new(0.2f, 0.2f, 0.2f, 1);
                return color with { r = color.r * 0.6f, g = color.g * 0.6f, b = color.b * 0.6f };
            }
        }
    }
}

