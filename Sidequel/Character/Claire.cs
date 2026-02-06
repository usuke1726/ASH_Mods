
using ModdingAPI;
using UnityEngine;

namespace Sidequel.Character;

internal class Claire : Core.NPC
{
    internal override ModdingAPI.Character Character => ch;
    private ModdingAPI.Character ch = null!;
    internal override bool Match(string name, bool matchAsObjectName)
    {
        if (name == Const.Object.Claire) return true;
        if (matchAsObjectName) return false;
        return name == "Claire";
    }
    internal override void Create()
    {
        var NPCs = GameObject.Find("NPCs");
        if (NPCs == null) return;
        var auntMay = NPCs.transform.Find("AuntMayNPC").gameObject;
        if (auntMay == null) return;
        var claireObj = auntMay.Clone();
        claireObj.name = Const.Object.Claire;
        claireObj.GetComponentInChildren<Animator>().speed = 0.6f;
        ch = new ModdingAPI.Character((Characters)Const.Object.ClaireObjectId, claireObj.transform);

        claireObj.transform.parent = NPCs.transform;
        claireObj.transform.position = new(651.1263f, 20.47f, 338.8536f);
        claireObj.transform.rotation = Quaternion.Euler(0, 115.3961f, 0);

        Color skinColor = new(0.271f, 0.243f, 0.400f, 1);
        Color shirtColor = new(0.8585f, 0, 0.144f, 1);
        Color beakColor = new(1, 0.820f, 0, 1);

        var bird = claireObj.transform.Find("Bird");
        bird.Find("Arms").GetComponent<SkinnedMeshRenderer>().material.color = skinColor;
        bird.Find("Body").GetComponent<SkinnedMeshRenderer>().material.color = shirtColor;
        bird.Find("Head").GetComponent<SkinnedMeshRenderer>().materials[0].color = skinColor;
        bird.Find("Head").GetComponent<SkinnedMeshRenderer>().materials[1].color = beakColor;
        bird.Find("Head").GetComponent<SkinnedMeshRenderer>().materials[2].color = skinColor;
        bird.Find("Legs").GetComponent<SkinnedMeshRenderer>().materials[0].color = skinColor;
        bird.Find("Legs").GetComponent<SkinnedMeshRenderer>().materials[1].color = skinColor;

        var head = claireObj.transform.Find("Bird/Armature/root/Base/Chest/Head_0/");
        if (head == null)
        {
            Monitor.Log($"The head of Claire Object is null!!", LL.Warning);
            return;
        }
        ModdingAPI.Character.OnSetupDone(() =>
        {
            var deerHat = ModdingAPI.Character.Get(Characters.SunhatDeer).gameObject.transform.Find("Bird/Armature/root/Base/Chest/Head_0/Hat").gameObject;
            head.Find("Hat").gameObject.SetActive(false);
            var sunhat = deerHat.Clone();
            sunhat.name = "SunHat";
            sunhat.SetActive(true);
            sunhat.transform.parent = head;
            sunhat.transform.localPosition = new(-2.435f, 0.0515f, 0.1987f);
            sunhat.transform.localRotation = Quaternion.Euler(0, 271.3634f, 0);
        });
    }
}

