
using IMoreExpressions;
using ModdingAPI;
using ModdingAPI.KeyBind;
using UnityEngine;

namespace MoreExpressions;

#pragma warning disable IDE1006
internal abstract class CustomExpression
{
    private Textures? defaultTextures = null;
    private readonly Dictionary<Characters, Textures> textures = [];

    protected void SetDefaultTextures(Textures tex) => defaultTextures = tex;
    protected void SetTextures(Textures tex) => SetDefaultTextures(tex);
    protected void SetTextures(Characters ch, Textures tex) => textures[ch] = tex;

    private static readonly Dictionary<Expression, CustomExpression> expressions = [];
    private static readonly Dictionary<string, CustomExpression> addedExpressions = [];

    public static void ShowEmotion(Characters ch, Expression type)
    {
        var obj = CharacterObject.Get(ch);
        if (expressions.TryGetValue(type, out var expression)) obj.ShowEmotion(expression);
        else Monitor.Log($"type {type} is not set!!", LL.Error, true);
    }

    public static void ToggleEmotion(Characters ch, Expression type)
    {
        var obj = CharacterObject.Get(ch);
        if (expressions.TryGetValue(type, out var expression)) obj.ToggleEmotion(expression);
        else Monitor.Log($"type {type} is not set!!", LL.Error, true);
    }
    public static void ToggleEmotion(Characters ch, string name)
    {
        var obj = CharacterObject.Get(ch);
        if (addedExpressions.TryGetValue(name, out var expression)) obj.ToggleEmotion(expression);
        else if (Enum.TryParse<Expression>(name, true, out var type)) ToggleEmotion(ch, type);
    }
    public static void ShowEmotion(Characters ch, string name)
    {
        var obj = CharacterObject.Get(ch);
        if (addedExpressions.TryGetValue(name, out var expression)) obj.ShowEmotion(expression);
        else if (Enum.TryParse<Expression>(name, true, out var type)) ShowEmotion(ch, type);
    }
    public static void ResetEmotion(Characters ch) => CharacterObject.Get(ch).ResetEmotion();

    protected static Textures GetTextures(IEnumerable<Parts> required) => GetTextures(Characters.Claire, required);
    protected static Textures GetTextures(Characters ch, IEnumerable<Parts> required) => CharacterObject.Get(ch).GetTextures(required);
    protected static Textures GetTexturesMain() => GetTexturesMain(Characters.Claire);
    protected static Textures GetTexturesMain(Characters ch) => CharacterObject.Get(ch).GetTextures([Parts.eyeL, Parts.eyeR, Parts.pupilL, Parts.pupilR]);

    protected void SetTextures(Characters ch)
    {
        var tex = textures.TryGetValue(ch, out var t) ? t : defaultTextures;
        if (tex == null) return;
        CharacterObject.Get(ch).SetTextures(tex);
    }

    protected static void WriteTexture(ref Part part, Func<int, int, bool> show)
    {
        if (part.texture == null) return;
        MaskTexture(ref part.texture, show);
    }
    protected static void WriteTexture(ref Texture2D texture, Func<int, int, bool> show)
    {
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                Color color = show(x, y) ? new(1, 1, 1, 1) : new(0, 0, 0, 0);
                texture.SetPixel(x, y, color);
            }
        }
        texture.Apply();
    }
    protected static void MaskTexture(ref Part part, Func<int, int, bool> show)
    {
        if (part.texture == null) return;
        MaskTexture(ref part.texture, show);
    }
    protected static void MaskTexture(ref Texture2D texture, Func<int, int, bool> show)
    {
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                if (!show(x, y))
                {
                    texture.SetPixel(x, y, new(0, 0, 0, 0));
                }
            }
        }
        texture.Apply();
    }

    protected virtual void Update(CharacterObject obj) => obj.Update();
    internal virtual void Activate(CharacterObject obj)
    {
        _SetupTextures();
        SetTextures(obj.character);
    }
    internal virtual void Deactivate(CharacterObject obj) => obj.Deactivate();

    protected bool setupTexturesDone = false;
    private void _SetupTextures()
    {
        if (!setupTexturesDone)
        {
            setupTexturesDone = true;
            SetupTextures();
        }
    }
    protected virtual void SetupTextures() { }


    private static bool firstUpdateDone = false;
    private static void OnFirstUpdate()
    {
        if (firstUpdateDone) return;
        firstUpdateDone = true;
        expressions[Expression.Happy] = new Expressions.Happy();
        expressions[Expression.Surprise] = new Expressions.Surprise();
        expressions[Expression.EyesClosed] = new Expressions.EyesClosed();
        expressions[Expression.Sad] = new Expressions.Sad();
        expressions[Expression.Angry] = new Expressions.Angry();
        expressions[Expression.HalfEye] = new Expressions.HalfEye();
        expressions[Expression.Thin] = new Expressions.Thin();
        expressions[Expression.SadThin] = new Expressions.SadThin();
        expressions[Expression.AngryThin] = new Expressions.AngryThin();
        expressions[Expression.Sparkle] = new Expressions.Sparkle();
        expressions[Expression.BitterSmile] = new Expressions.BitterSmile();
        expressions[Expression.ImpishSmile] = new Expressions.ImpishSmile();
    }
    internal static void Setup(IMod mod)
    {
        CharacterObject.Setup(mod.Helper);
        mod.Helper.Events.Gameloop.PlayerUpdated += (_, _) => UpdateStatic();
        mod.Helper.Events.Gameloop.ReturnedToTitle += (_, _) =>
        {
            CharacterObject.ForEach(obj => obj.ResetEmotion());
            expressions.Clear();
            addedExpressions.Clear();
            firstUpdateDone = false;
        };
        //DebugKeybinds();
    }
    private static void UpdateStatic()
    {
        OnFirstUpdate();
        CharacterObject.ForEach(obj => obj.currentExpression?.Update(obj));
    }

    private static void DebugKeybinds()
    {
        IEnumerable<Characters> chs = [
            Characters.Claire,
            Characters.Tim1,
            Characters.ClimbingRhino1,
            Characters.RumorGuy,
        ];
        Func<Expression, Action> emote = e => () =>
        {
            foreach (var ch in chs) ToggleEmotion(ch, e);
        };
        KeyBind.RegisterKeyBind("Alpha1(F9)", emote(Expression.Sad), name: "Emotion Sad");
        KeyBind.RegisterKeyBind("Alpha2(F9)", emote(Expression.Angry), name: "Emotion Angry");
        KeyBind.RegisterKeyBind("Alpha3(F9)", emote(Expression.HalfEye), name: "Emotion HalfEye");
        KeyBind.RegisterKeyBind("Alpha4(F9)", emote(Expression.Sparkle), name: "Emotion Sparkle");
        KeyBind.RegisterKeyBind("Alpha5(F9)", emote(Expression.Happy), name: "Emotion Happy");
        KeyBind.RegisterKeyBind("Alpha6(F9)", emote(Expression.BitterSmile), name: "Emotion BitterSmile");
        KeyBind.RegisterKeyBind("Alpha7(F9)", emote(Expression.ImpishSmile), name: "Emotion ImpishSmile");
        KeyBind.RegisterKeyBind("Alpha8(F9)", emote(Expression.EyesClosed), name: "Emotion EyeClosed");
        KeyBind.RegisterKeyBind("Alpha9(F9)", emote(Expression.Surprise), name: "Emotion Surprise");

        KeyBind.RegisterKeyBind("Alpha1(F10)", emote(Expression.Thin), name: "Emotion Thin");
        KeyBind.RegisterKeyBind("Alpha2(F10)", emote(Expression.SadThin), name: "Emotion SadThin");
        KeyBind.RegisterKeyBind("Alpha3(F10)", emote(Expression.AngryThin), name: "Emotion AngryThin");
    }
}
#pragma warning restore IDE1006

