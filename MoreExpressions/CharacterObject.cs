
using ModdingAPI;
using UnityEngine;

namespace MoreExpressions;

#pragma warning disable IDE1006

internal enum Eye { L, R }
internal class CharacterObject
{
    internal CustomExpression? currentExpression = null;
    internal bool isEmoting { get => currentExpression != null; }
    private static Dictionary<Characters, CharacterObject> characters = [];
    internal static void Setup(IModHelper helper)
    {
        Character.OnSetupDone(CreateCharacterObjects);
    }
    private CharacterObject(Characters ch, GameObject obj, string headQuery)
    {
        character = ch;
        _character = obj;
        var head = obj.transform.Find(headQuery)?.gameObject;
        if (head == null) throw new Exception($"Head of character {ch} is null");
        _head = head;
        _eyeL = _head.transform.Find("EyeL").gameObject;
        _eyeR = _head.transform.Find("EyeR").gameObject;
        _pupilL = _eyeL.transform.Find("Pupil").gameObject;
        _pupilR = _eyeR.transform.Find("Pupil").gameObject;
        _happyL = _eyeL.transform.Find("HappyEyes").gameObject;
        _happyR = _eyeR.transform.Find("HappyEyes").gameObject;
        _blinkL = _eyeL.transform.Find("Blink").gameObject;
        _blinkR = _eyeR.transform.Find("Blink").gameObject;
        foreach (Parts p in Enum.GetValues(typeof(Parts)))
        {
            var renderer = Obj(p).GetComponent<SpriteRenderer>();
            defaultSprites[p] = renderer.sprite;
            defaultColors[p] = renderer.material.color;
        }
        animator = _head.GetComponent<Animator>();
        var npcAnimator = obj.GetComponentInChildren<NPCIKAnimator>();
        if (npcAnimator != null)
        {
            emotionAnimator = npcAnimator;
            return;
        }
        var playerAnimator = obj.GetComponentInChildren<PlayerIKAnimator>();
        if (playerAnimator != null)
        {
            emotionAnimator = playerAnimator;
            return;
        }
        emotionAnimator = null!;
        Monitor.Log($"EmotionAnimator of character {ch} not found!!", LL.Warning);
    }
    internal Characters character { get; private set; }
    internal GameObject _character { get; private set; }
    internal GameObject _head { get; private set; }
    internal GameObject _eyeL { get; private set; }
    internal GameObject _eyeR { get; private set; }
    internal GameObject _pupilL { get; private set; }
    internal GameObject _pupilR { get; private set; }
    internal GameObject _happyL { get; private set; }
    internal GameObject _happyR { get; private set; }
    internal GameObject _blinkL { get; private set; }
    internal GameObject _blinkR { get; private set; }
    internal readonly Animator animator;
    private readonly Dictionary<Parts, Sprite> defaultSprites = [];
    private readonly Dictionary<Parts, Color> defaultColors = [];
    internal readonly IEmotionAnimator emotionAnimator;
    private List<Tuple<GameObject, Part>> partList = [];

    public void Update()
    {
        foreach (var part in partList)
        {
            if (part.Item2.color != null)
            {
                part.Item1.GetComponent<SpriteRenderer>().material.color = (Color)part.Item2.color;
            }
        }
    }

    public void Deactivate()
    {
        foreach (Parts p in Enum.GetValues(typeof(Parts)))
        {
            var renderer = Obj(p).GetComponent<SpriteRenderer>();
            renderer.sprite = defaultSprites[p];
            renderer.material.color = defaultColors[p];
        }
    }

    private StackResourceSortingKey? key = null;
    public void ShowVanillaEmotion(Emotion emotion)
    {
        key = emotionAnimator.ShowEmotion(emotion);
    }
    public void ShowEmotion(CustomExpression expression)
    {
        ResetEmotion();
        currentExpression = expression;
        expression.Activate(this);
    }
    public void ToggleEmotion(CustomExpression expression)
    {
        if (isEmoting) ResetEmotion();
        else ShowEmotion(expression);
    }
    public void ResetEmotion()
    {
        partList = [];
        key?.ReleaseResource();
        key = null;
        if (currentExpression == null) return;
        currentExpression.Deactivate(this);
        currentExpression = null;
    }

    private static readonly string AnimationClipName = "HeadIdle";
    internal void StopBlink()
    {
        if (!_character.activeSelf) return;
        animator.Play(AnimationClipName, 0, 0);
        animator.enabled = false;
    }
    internal void RestartBlink()
    {
        if (!_character.activeSelf) return;
        animator.enabled = true;
    }
    internal void ToggleBlink()
    {
        if (!_character.activeSelf) return;
        if (animator.enabled) StopBlink(); else RestartBlink();
    }

    internal GameObject Obj(Parts parts)
    {
        return parts switch
        {
            Parts.eyeL => _eyeL,
            Parts.eyeR => _eyeR,
            Parts.pupilL => _pupilL,
            Parts.pupilR => _pupilR,
            Parts.happyL => _happyL,
            Parts.happyR => _happyR,
            Parts.blinkL => _blinkL,
            Parts.blinkR => _blinkR,
            _ => throw new Exception()
        };
    }

    internal Textures GetTextures(IEnumerable<Parts> required)
    {
        Textures ret = new();
        foreach (var p in required)
        {
            var tex = ReadableTexture2D(Obj(p).GetComponent<SpriteRenderer>().sprite.texture);
            switch (p)
            {
                case Parts.eyeL: ret.eyeL.texture = tex; break;
                case Parts.eyeR: ret.eyeR.texture = tex; break;
                case Parts.pupilL: ret.pupilL.texture = tex; break;
                case Parts.pupilR: ret.pupilR.texture = tex; break;
                case Parts.happyL: ret.happyL.texture = tex; break;
                case Parts.happyR: ret.happyR.texture = tex; break;
                case Parts.blinkL: ret.blinkL.texture = tex; break;
                case Parts.blinkR: ret.blinkR.texture = tex; break;
            }
        }
        return ret;
    }
    private void SetTexture(GameObject obj, Texture2D tex)
    {
        var spriteRenderer = obj.GetComponent<SpriteRenderer>();
        var sprite = spriteRenderer.sprite;
        var newSprite = Sprite.Create(tex, sprite.rect, new Vector2(0.5f, 0.5f), sprite.pixelsPerUnit);
        spriteRenderer.sprite = newSprite;
    }
    private void SetPartList(Textures tex)
    {
        partList = [
            new(_eyeL, tex.eyeL),
            new(_eyeR, tex.eyeR),
            new(_pupilL, tex.pupilL),
            new(_pupilR, tex.pupilR),
            new(_happyL, tex.happyL),
            new(_happyR, tex.happyR),
            new(_blinkL, tex.blinkL),
            new(_blinkR, tex.blinkR),
        ];
    }
    internal void SetTextures(Textures tex)
    {
        SetPartList(tex);
        foreach (var t in partList)
        {
            if (t.Item2.texture != null) SetTexture(t.Item1, t.Item2.texture);
        }
    }
    internal static Texture2D ReadableTexture2D(Texture2D texture)
    {
        RenderTexture renderTexture = RenderTexture.GetTemporary(
            texture.width,
            texture.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear
        );
        Graphics.Blit(texture, renderTexture);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTexture;
        Texture2D readableTextur2D = new Texture2D(texture.width, texture.height);
        readableTextur2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        readableTextur2D.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTexture);
        return readableTextur2D;
    }

    private static Dictionary<Characters, CharacterObject> objects = [];
    internal static IEnumerable<CharacterObject> GetCharacterObjects() => objects.Values;
    internal static void ForEach(Action<CharacterObject> callback)
    {
        foreach (var obj in GetCharacterObjects()) callback(obj);
    }
    internal static CharacterObject Get(Characters ch) => objects[ch];
    private static string QueryFrom(string type, string head)
    {
        return $"{type}/Armature/root/Base/Chest/{head}";
    }
    private static readonly Dictionary<Characters, string> querys = new()
    {
        [Characters.Claire] = QueryFrom("Character", "Head"),
        [Characters.AuntMay] = QueryFrom("Bird", "Head_0"),
        [Characters.RangerJon] = QueryFrom("Bird", "Head_0"),
        [Characters.DadBoatDeer] = QueryFrom("Fox", "Head_0"),
        [Characters.KidBoatDeer] = QueryFrom("Fox", "Head_0"),
        [Characters.ShipWorker] = QueryFrom("Fox", "Head_0"),
        [Characters.RumorGuy] = QueryFrom("Rabbit", "Head_0"),
        [Characters.Charlie] = QueryFrom("Fox", "Head_0"),
        [Characters.Tim] = QueryFrom("Rabbit", "Head_0"),
        [Characters.ClimbingRhino] = QueryFrom("Fox", "Head_0"),
        [Characters.GlideKid] = QueryFrom("Fox", "Head_0"),
        [Characters.Jen] = QueryFrom("Fox", "Head_0"),
        [Characters.Bill] = QueryFrom("Bird", "Head_0"),
        [Characters.OutlookPointGuy] = QueryFrom("Rabbit", "Head_0"),
        [Characters.ToughBird] = QueryFrom("Fox", "Head_0"),
        [Characters.SunhatDeer] = QueryFrom("Bird", "Head_0"),
        [Characters.DiveKid] = QueryFrom("Bird", "Head_0"),
        [Characters.RunningLizard] = QueryFrom("Rabbit", "Head_0"),
        [Characters.RunningNephew] = QueryFrom("Rabbit", "Head_0"),
        [Characters.RunningRabbit] = QueryFrom("Rabbit", "Head_0"),
        [Characters.RunningGoat] = QueryFrom("Rabbit", "Head_0"),
        [Characters.BreakfastKid] = QueryFrom("Fox", "Head_0"),
        [Characters.Camper] = QueryFrom("Bird", "Head_0"),
        [Characters.ShovelKid] = QueryFrom("Fox", "Head_0"),
        [Characters.CompassFox] = QueryFrom("Rabbit", "Head_0"),
        [Characters.PictureFox] = QueryFrom("Fox", "Head_0"),
        [Characters.Taylor] = QueryFrom("Rabbit", "Head_0"),
        [Characters.Sue] = QueryFrom("Rabbit", "Head_0"),
        [Characters.WatchGoat] = QueryFrom("Fox", "Head_0"),
        [Characters.Julie] = QueryFrom("Bird", "Head_0"),
        [Characters.BeachstickballKid] = QueryFrom("Character", "Head"),
        [Characters.Avery] = QueryFrom("Character", "Head"),
        [Characters.Artist] = QueryFrom("StandingNPC/Fox", "Head_0"),
        [Characters.HydrationDog] = QueryFrom("Rabbit", "Head_0"),
    };
    private static void CreateCharacterObjects()
    {
        foreach (var info in Character.GetCharacters())
        {
            objects[info.character] = new(info.character, info.gameObject, querys[info.character]);
        }
    }
}

#pragma warning restore IDE1006

