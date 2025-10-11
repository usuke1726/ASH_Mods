
using ModdingAPI;
using ModdingAPI.KeyBind;
using UnityEngine;

namespace Sidequel.Character;

internal partial class Core
{
    private static Player player = null!;
    private static Transform toughBird = null!;
    private static bool setupDone = false;
    private static bool isHalfEye = true;

    internal static void Setup(IModHelper helper)
    {
        helper.Events.Gameloop.GameStarted += (_, _) =>
        {
            ChangeToToughBird();
        };
        helper.Events.Gameloop.ReturnedToTitle += (_, _) =>
        {
            setupDone = false;
        };
        KeyBind.RegisterKeyBind("Alpha0(LeftShift)", () =>
        {
            isHalfEye = !isHalfEye;
            if (isHalfEye) EmoteHalfEyes();
            else EmoteNormalEyes();
        }, name: "(Debug)ToggleEmotion");
    }

    private static void ChangeToToughBird()
    {
        if (setupDone) return;
        if (!Context.TryToGetPlayer(out player))
        {
            Monitor.Log($"player is null", LL.Warning);
            return;
        }
        setupDone = true;
        if (!State.IsActive) return;
        toughBird = GameObject.Find("/LevelObjects/NPCs").transform.Find("ToughBirdNPC (1)");
        CreateMohawk();
        ChangeColors();
        SetupEyes();
        EmoteHalfEyes();
    }
    private static void CreateMohawk()
    {
        var mesh = new Mesh()
        {
            vertices = [.. mohawkVertices],
            subMeshCount = 2,
        };
        mesh.SetTriangles(mohawkTriangles1.ToArray(), 0);
        mesh.SetTriangles(mohawkTriangles2.ToArray(), 1);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        var obj = new GameObject("Mohawk");
        obj.AddComponent<MeshFilter>().mesh = mesh;
        var renderer = toughBird.transform.Find("Fox/Head").GetComponent<SkinnedMeshRenderer>();
        obj.AddComponent<MeshRenderer>().materials = [renderer.materials[0], renderer.materials[2]];
        obj.transform.parent = player.transform.Find("Character/Armature/root/Base/Chest/Head");
        obj.transform.localPosition = new(1.5309f, -0.1382f, 0.2273f);
        obj.transform.localRotation = Quaternion.Euler(357, 270, 90);
        obj.transform.localScale = obj.transform.localScale.SetX(1.05f).SetY(1.0f);
    }
    private static void ChangeColors()
    {
        // Tough bird's color set:
        //   skin: 0.229 0.3108 0.5849 1 (materials[0])
        //   light part of mohawk: 0.3347 0.4022 0.6509 1 (materials[2])
        //   beak: 1 0.6135 0 1 (materials[1])
        //   pupil: 0.1286 0.269 0.5566 1
        //   shirt: 0.0898 0.1321 0.0729 1
        var playerBodyRenderer = player.transform.Find("Character/Body").GetComponent<SkinnedMeshRenderer>();
        var tex = playerBodyRenderer.material.mainTexture as Texture2D;
        if (tex == null)
        {
            Monitor.Log($"texutre is null!", LL.Warning);
            return;
        }
        var tex2 = Util.EditableTexture(tex);
        for (int x = 0; x < tex2.width; x++)
        {
            for (int y = 0; y < tex2.height; y++)
            {
                // The texture of material of playerBodyRenderer is composed of the following 3 colors:
                // (1, 1, 1, 1) : unused color
                // (0.271, 0.243, 0.400, 1.000) : skin color
                // (1.000, 0.820, 0.000, 1.000) : beak color
                // In this code, the skin color and beak color are detected by using B value of RGB.
                var p = tex2.GetPixel(x, y);
                if (p.b < 0.2f)
                {
                    // The beak color
                    tex2.SetPixel(x, y, new(1, 0.6135f, 0, 1));
                }
                else if (p.b < 0.6f)
                {
                    // The skin color
                    tex2.SetPixel(x, y, new(0.229f, 0.3108f, 0.5849f, 1));
                }
            }
        }
        tex2.Apply();
        playerBodyRenderer.material.mainTexture = tex2;

        // Without the following code, the color of limbs becomes lighter than the one of head (idk why)
        playerBodyRenderer.material.mainTextureScale = new(0.99f, 0.99f);

        player.fullShirtColor = player.emptyShirtColor = new(0.0898f, 0.1321f, 0.0729f, 1);
    }

    private static Transform eyeL = null!;
    private static Transform eyeR = null!;
    private static Transform pupilL = null!;
    private static Transform pupilR = null!;
    private static Texture2D textureEyeL = null!;
    private static Texture2D textureEyeR = null!;
    private static Texture2D texturePupilL = null!;
    private static Texture2D texturePupilR = null!;
    private static Vector3 defaultPupilLPosition;
    private static Vector3 defaultPupilRPosition;
    private static void SetupEyes()
    {
        var playerHead = player.transform.Find("Character/Armature/root/Base/Chest/Head");
        var birdHead = toughBird.transform.Find("Fox/Armature/root/Base/Chest/Head_0");
        eyeL = playerHead.Find("EyeL");
        eyeR = playerHead.Find("EyeR");
        pupilL = playerHead.Find("EyeL/Pupil");
        pupilR = playerHead.Find("EyeR/Pupil");
        textureEyeL = eyeL.GetComponent<SpriteRenderer>().sprite.texture;
        textureEyeR = eyeR.GetComponent<SpriteRenderer>().sprite.texture;
        texturePupilL = pupilL.GetComponent<SpriteRenderer>().sprite.texture;
        texturePupilR = pupilR.GetComponent<SpriteRenderer>().sprite.texture;
        Color pupilColor = new(0.1286f, 0.269f, 0.5566f, 1);
        pupilL.GetComponent<SpriteRenderer>().material.color = pupilColor;
        pupilR.GetComponent<SpriteRenderer>().material.color = pupilColor;
        defaultPupilLPosition = pupilL.localPosition;
        defaultPupilRPosition = pupilR.localPosition;
    }
    internal static void EmoteHalfEyes()
    {
        if (!setupDone || !State.IsActive) return;
        SetTexture(eyeL, Mask(textureEyeL, (x, y) => x * 3 >= textureEyeL.width));
        SetTexture(eyeR, Mask(textureEyeR, (x, y) => x * 3 >= textureEyeR.width));
        SetTexture(pupilL, Mask(texturePupilL, (x, y) => x * 3 >= texturePupilL.width - 10));
        SetTexture(pupilR, Mask(texturePupilR, (x, y) => x * 3 >= texturePupilR.width - 10));
        // slightly shift pupil positions upward to match the ToughBird NPC
        pupilL.localPosition = defaultPupilLPosition.SetX(-0.08f);
        pupilR.localPosition = defaultPupilRPosition.SetX(-0.08f);
        isHalfEye = true;
    }
    internal static void EmoteNormalEyes()
    {
        if (!setupDone || !State.IsActive) return;
        SetTexture(eyeL, Mask(textureEyeL, (x, y) => true));
        SetTexture(eyeR, Mask(textureEyeR, (x, y) => true));
        SetTexture(pupilL, Mask(texturePupilL, (x, y) => true));
        SetTexture(pupilR, Mask(texturePupilR, (x, y) => true));
        // considering whether to restore the pupil positions
        //pupilL.localPosition = defaultPupilLPosition;
        //pupilR.localPosition = defaultPupilRPosition;
        isHalfEye = false;
    }
    private static void SetTexture(Transform obj, Texture2D texture)
    {
        var renderer = obj.GetComponent<SpriteRenderer>();
        renderer.sprite = Sprite.Create(texture, renderer.sprite.rect, new(0.5f, 0.5f), renderer.sprite.pixelsPerUnit);
    }
    private static Texture2D Mask(Texture2D tex, Func<int, int, bool> show)
    {
        var newTex = Util.EditableTexture(tex);
        for (int x = 0; x < tex.width; x++)
        {
            for (int y = 0; y < tex.height; y++)
            {
                if (!show(x, y)) newTex.SetPixel(x, y, new(0, 0, 0, 0));
            }
        }
        newTex.Apply();
        return newTex;
    }
}

