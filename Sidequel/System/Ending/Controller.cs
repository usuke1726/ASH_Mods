
//#define DEBUG_ADJUST_SPOT_POSITIONS
#define ENDING_MESSAGE_ENABLED
//#define DEBUG_PRINT_TIME

using System.Collections;
using ModdingAPI;
using QuickUnityTools.Input;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Sidequel.System.Ending;

internal class Controller : MonoBehaviour
{
#if DEBUG && DEBUG_ADJUST_SPOT_POSITIONS
    private const float EndTime = 1200f;
#else
    private const float EndTime = 149f;
#endif
    private static readonly Color textColor = new(0.1417f, 0.138f, 0.2075f, 1);
    private static readonly List<Schedule> schedules = [
        new(0, Music.PlayUpdraftMusic),
        new(0f, () => PopUp(26, 0.8f)),
        new(1.2f, () => PopUp(27, 1.2f)),
        new(2.7f, () => PopUp(28, 1.4f)),
        new(4.2f, () => {
            Continue();
            CutsceneController.SetFlying();
        }),
        new(9.5f, () => FadeOutScreen.FadeOut(1.5f)),
        new(11.8f, () => {
            CutsceneController.Next(12.5f);
            Music.MuteExceptBGM();
            SkipKeyWatcher.Create();
            GameObject.Destroy(player!.gameObject);
        }),
        new(12f, () => {
            CutsceneController.Ready();
            FadeOutScreen.FadeIn();
        }),
        new(11.8f, () => FadeOutScreen.SetTextColor(textColor)),
        new(134.5f, Ship.Activate),
        new(134.5f, SkipKeyWatcher.Deactivate),
        new(EndTime - 12f, () => FadeOutScreen.SetTextColor(null)),
        new(EndTime - 10f, () => FadeOutScreen.FadeOut(3f)),
        new(EndTime - 7f, SkipKeyWatcher.PrepareEndMessage),
        new(EndTime, () => {
            Debug($"MOVIE END");
        }),
        .. Texts([
            new(EndTime - 10f, "the end", 3f),
        ]),
        .. Spots([
            26f,
            40f,
            54f,
            68f,
            82f,
            94.7f,
            110f,
            120f,
            135f,
        ]),
#if DEBUG && DEBUG_ADJUST_SPOT_POSITIONS
        //new(20f, () => CutsceneController.canMove = true),
#endif
    ];
    private class TextEntry(float time, string? text, float? transition = null)
    {
        internal readonly float time = time;
        internal readonly string? text = text;
        internal readonly float? transition = transition;
    }
    private static IEnumerable<Schedule> Texts(TextEntry[] items)
    {
        int num = items.Length;
        return [
            .. Enumerable.Range(0, num)
                .Where(i => items[i].text != null)
                .Select(i => new Schedule(items[i].time, () => FadeOutScreen.FadeInText(items[i].text!, items[i].transition))),
            .. Enumerable.Range(1, num - 1)
                .Where(i => items[i-1].text != null)
                .Select(i => new Schedule(items[i].time - 2, FadeOutScreen.FadeOutText)),
        ];
    }
    private static IEnumerable<Schedule> Spots(float[] times)
    {
        int num = times.Length;
        Func<int, float> getTime = i => i < num - 1 ? times[i + 1] - times[i] : 8f;
        return [
            .. Enumerable.Range(0, num)
            .Select(i => new Schedule(times[i], () => {
                CutsceneController.Ready();
                FadeOutScreen.FadeIn();
            })),
            .. Enumerable.Range(0, num)
            .Select(i => new Schedule(times[i] - 3f, FadeOutScreen.FadeOut)),
            .. Enumerable.Range(0, num)
            .Select(i => new Schedule(times[i] - 1f, () => CutsceneController.Next(getTime(i)))),
        ];
    }
    internal static void StartScene()
    {
        new GameObject("Sidequel_EndingScene").AddComponent<Controller>();
    }
    internal static void Prepare()
    {
        schedules.Sort((a, b) => a.time.CompareTo(b.time));
        player = Object.Create().transform;
        CutsceneController.PrepareCutscene(player);
        Spot.SetupAtmospheres();
        Ship.Create();
    }
    private float startTime = 0;
    private static IConversation conversation = null!;
    private static Transform player = null!;
    private void Start()
    {
        new GameObject("Sidequel_Ending_FadeOutscreen").AddComponent<FadeOutScreen>();
        conversation = new TextBoxConversation(player);
        conversation.currentSpeaker = player;
        Save();
        StartCoroutine(Coroutine());
    }
    private IEnumerator Coroutine()
    {
        yield return Wait(3f);
        yield return Talk1();
        StartReplay();
        yield return Wait(1.5f);
        yield return Talk2();
        yield return Wait(0.5f);
        Continue();
        yield return Wait(1.5f);
        yield return Talk3();
        yield return Wait(0.5f);
        Continue();
        yield return Wait(1.5f);
        yield return Talk4();
        conversation.Hide();
        yield return Wait(1f);
        int i = 0;
        Schedule schedule = schedules[0];
        startTime = Time.time;
#if DEBUG && DEBUG_PRINT_TIME
        float countdown = Time.time + 1;
#endif
        while (true)
        {
#if DEBUG && DEBUG_PRINT_TIME
            if (Time.time > countdown)
            {
                countdown = Time.time + 1;
                Debug($"time {Time.time - startTime}");
            }
#endif
            if (Time.time - startTime >= schedule.time)
            {
                schedule.action();
                i++;
                if (i == schedules.Count) break;
                schedule = schedules[i];
            }
            if (SkipKeyWatcher.HasFired && Time.time - SkipKeyWatcher.FiredTime >= 10f) break;
            yield return new WaitForFixedUpdate();
        }
        SceneManager.LoadScene("TitleScene");
    }
    private void Save()
    {
        STags.SetBool(Const.STags.HasCompletedGame, true);
        World.PlayerPosition.OnEndingScene();
        Context.globalData.SaveGameAsync();
    }
    private IEnumerator Talk1()
    {
        yield return Line(1);
        yield return Wait(1f);
        yield return Lines(2, 4);
        yield return Wait(2f);
        yield return Line(5);
        yield return Wait(1f);
        yield return Lines(6, 7);
    }
    private IEnumerator Talk2()
    {
        yield return Line(8);
    }
    private IEnumerator Talk3()
    {
        yield return Lines(9, 10);
    }
    private IEnumerator Talk4()
    {
        yield return Lines(11, 14);
        yield return Wait(1f);
        yield return Lines(15, 19);
        yield return Wait(1f);
        yield return Lines(20, 21);
        yield return Wait(0.5f);
        yield return Lines(22, 25);
    }
    private static void PopUp(int index, float time)
    {
        var text = I18nLocalize($"node.Ending.{index:00}");
        TemporalBox.Add(player, text, time);
    }
    private IEnumerator Lines(int start, int end)
    {
        foreach (var i in Enumerable.Range(start, end - start + 1))
        {
            yield return Line(i);
        }
    }
    private IEnumerator Line(int index)
    {
        var text = I18nLocalize($"node.Ending.{index:00}");
        yield return conversation.ShowLine(text);
    }
    private IEnumerator Wait(float time)
    {
        conversation.Hide();
        yield return new WaitForSeconds(time);
    }
    private static void StartReplay() => Object.ActivePlayerReplay.Play();
    private static void Continue() => Object.ActivePlayerReplay.Continue();
    private class SkipKeyWatcher : MonoBehaviour
    {
        internal static bool IsActive { get; private set; } = false;
        internal static bool HasFired { get; private set; } = false;
        internal static float FiredTime { get; private set; } = 0;
        private GameUserInput input = null!;
        private static SkipKeyWatcher instance = null!;
        private TextMeshProUGUI text = null!;
        internal static void Create()
        {
            instance = new GameObject("Sidequel_EndingSkipKeyWatcher").AddComponent<SkipKeyWatcher>();
        }
        private bool deactivated = false;
        internal static void Deactivate() => instance.deactivated = true;
        private void Awake()
        {
            input = GameUserInput.CreateInput(gameObject);
            text = GameObject.Find("Cutscenes").transform.Find("ClosingCutscene/Canvas/Image/Text (1)").GetComponent<TextMeshProUGUI>();
            var obj = Context.ui.CreateTextBoxContent("");
            text.font = obj.textMesh.font;
            GameObject.Destroy(obj.gameObject);
            text.text = TextReplacer.ReplaceVariables(I18nLocalize("Ending.SkipButtonMessage"));
            text.gameObject.SetActive(true);
            text.color = new(0.5f, 0.5f, 0.5f, 0);
            //text.fontSize = 12;
        }
        private float lastHoldStartTime = -1;
        private const float ActivatingTime = 1f;
        private const float FadeInOutTime = 0.5f;
        private const float ACoeff = 1f / FadeInOutTime;
        private void Update()
        {
            if (!deactivated && input.GetJumpButton().isPressed && input.GetUseItemButton().isPressed)
            {
                if (lastHoldStartTime < 0) lastHoldStartTime = Time.time;
                var time = Time.time - lastHoldStartTime;
                if (time >= ActivatingTime) OnFired();
            }
            else lastHoldStartTime = -1;
            if (deactivated)
            {
                timeout = -1;
                showing = false;
            }
            else if (IsHoldingAnyKey())
            {
                timeout = 3.0f;
                showing = true;
            }
            if (timeout > 0)
            {
                timeout -= Time.deltaTime;
                if (timeout < 0) showing = false;
            }
            if (showingEndMes && text.color.a < 1f)
            {
                var a = Math.Clamp(text.color.a + Time.deltaTime * 0.5f, 0, 1);
                text.color = text.color with { a = a };
            }
            else if (showing && text.color.a < 1f)
            {
                var a = Math.Clamp(text.color.a + Time.deltaTime * ACoeff, 0, 1);
                text.color = text.color with { a = a };
            }
            else if (!showing && text.color.a > 0f)
            {
                var a = Math.Clamp(text.color.a - Time.deltaTime * ACoeff, 0, 1);
                text.color = text.color with { a = a };
            }
        }
        internal static void PrepareEndMessage()
        {
#if ENDING_MESSAGE_ENABLED
            if (instance.showingEndMes) return;
            instance.showingEndMes = true;
            var rect = instance.text.gameObject.GetComponent<RectTransform>();
            rect.sizeDelta = rect.sizeDelta with { x = 350 };
            instance.text.gameObject.transform.localPosition = new(-175.2788f, -30, 0);
            instance.text.alignment = TextAlignmentOptions.Center;
            instance.text.horizontalAlignment = HorizontalAlignmentOptions.Center;
            instance.text.text = I18nLocalize("Ending.ThanksForPlaying");
            instance.text.color = new(0.673f, 0.561f, 0, 0);
#endif
        }
        private bool showingEndMes = false;
        private bool showing = false;
        private float timeout = 0;
        private bool IsHoldingAnyKey()
        {
            ButtonState[] states = [input.button1, input.button2, input.button3, input.button4, input.leftBumper, input.rightBumper];
            if (states.Any(s => s.isPressed)) return true;
            if (UnityEngine.Input.anyKey) return true;
            return false;
        }
        private void OnDestroy() => instance = null!;
        private void OnFired()
        {
            Deactivate();
            HasFired = true;
            FiredTime = Time.time;
            if (FadeOutScreen.IsFadingIn)
            {
                Timer.Register(1, () => FadeOutScreen._FadeOutBody(2f));
            }
            else if (!FadeOutScreen.IsFadingOut)
            {
                FadeOutScreen._FadeOutBody(3f);
            }
            if (FadeOutScreen.IsTextFadingIn)
            {
                Timer.Register(1, () => FadeOutScreen._FadeOutTextBody(0.7f));
            }
            else if (!FadeOutScreen.IsTextFadingOut && FadeOutScreen.IsShowingText)
            {
                FadeOutScreen._FadeOutTextBody(0.7f);
            }
            Timer.Register(2, () =>
            {
                FadeOutScreen.SetTextColor(null);
                FadeOutScreen._FadeInTextBody("the end", 3f);
            });
            Timer.Register(5, PrepareEndMessage);
            Music.FadeOutCurrentMusic2(5f);
        }
    }
    private class FadeOutScreen : MonoBehaviour
    {
        internal static bool IsShowingText => text.color.a > 0;
        internal static bool IsTextFadingIn => instance.isTextFadingIn;
        internal static bool IsTextFadingOut => instance.isTextFadingOut;
        internal static bool IsFadingIn => instance.isFadingIn;
        internal static bool IsFadingOut => instance.isFadingOut;
        private static FadeOutScreen instance = null!;
        private Image image = null!;
        private Color color;
        private Color textColor;
        private bool isFadingOut = false;
        private bool isFadingIn = false;
        private bool isTextFadingOut = false;
        private bool isTextFadingIn = false;
        private float startTime = 0;
        private float textStartTime = 0;
        private float transitionTime;
        private float textTransitionTime;
        private const float DefaultTransitionTime = 1f;
        private static Transform textObj = null!;
        private static Text text = null!;
        private static Color defaultColor;
        private void Awake()
        {
            instance = this;
            var closingCutscene = GameObject.Find("Cutscenes").transform.Find("ClosingCutscene");
            var canvas = closingCutscene!.Find("Canvas");
            var imageObj = canvas!.Find("Image");
            textObj = imageObj.Find("Text");
            text = textObj.GetComponent<Text>();
            textColor = text.color;
            textColor.a = 0;
            text.color = textColor;
            defaultColor = text.color;
            imageObj.Find("Text (1)").gameObject.SetActive(false);
            image = imageObj.GetComponent<Image>();
            color = image.color;
            color.a = 0;
            image.color = color;
            canvas.gameObject.SetActive(true);
            closingCutscene.gameObject.SetActive(true);
        }
        private void FixedUpdate()
        {
            if (isFadingOut)
            {
                var time = Time.time - startTime;
                if (time > transitionTime) isFadingOut = false;
                var t = Mathf.Clamp(time / transitionTime, 0, 1);
                color.a = Mathf.Lerp(0, 1, t);
                image.color = color;
            }
            else if (isFadingIn)
            {
                var time = Time.time - startTime;
                if (time > transitionTime) isFadingIn = false;
                var t = Mathf.Clamp(time / transitionTime, 0, 1);
                color.a = Mathf.Lerp(1, 0, t);
                image.color = color;
            }
            if (isTextFadingOut)
            {
                var time = Time.time - textStartTime;
                if (time > textTransitionTime) isTextFadingOut = false;
                var t = Mathf.Clamp(time / textTransitionTime, 0, 1);
                textColor.a = Mathf.Lerp(1, 0, t);
                text.color = textColor;
            }
            else if (isTextFadingIn)
            {
                var time = Time.time - textStartTime;
                if (time > textTransitionTime) isTextFadingIn = false;
                var t = Mathf.Clamp(time / textTransitionTime, 0, 1);
                textColor.a = Mathf.Lerp(0, 1, t);
                text.color = textColor;
            }
        }
        internal static void SetTextColor(Color? _color = null)
        {
            var color = _color ?? defaultColor;
            var a = text.color.a;
            instance.textColor = text.color = color with { a = a };
        }
        internal static void FadeOutText() => FadeOutText(null);
        internal static void FadeOutText(float? transitionTime)
        {
            if (!SkipKeyWatcher.HasFired) _FadeOutTextBody(transitionTime);
        }
        internal static void _FadeOutTextBody(float? transitionTime)
        {
            instance.textTransitionTime = transitionTime ?? DefaultTransitionTime;
            instance.textStartTime = Time.time;
            instance.isTextFadingOut = true;
        }
        internal static void FadeInText(string _text) => FadeInText(_text, null);
        internal static void FadeInText(string _text, float? transitionTime)
        {
            if (!SkipKeyWatcher.HasFired) _FadeInTextBody(_text, transitionTime);
        }
        internal static void _FadeInTextBody(string _text, float? transitionTime)
        {
            text.text = _text;
            text.color = text.color with { a = 0 };
            instance.textTransitionTime = transitionTime ?? DefaultTransitionTime;
            instance.textStartTime = Time.time;
            instance.isTextFadingIn = true;
        }
        internal static void FadeOut() => FadeOut(null);
        internal static void FadeOut(float? transitionTime)
        {
            if (!SkipKeyWatcher.HasFired) _FadeOutBody(transitionTime);
        }
        internal static void _FadeOutBody(float? transitionTime)
        {
            // Debug($"start fading out");
            instance.transitionTime = transitionTime ?? DefaultTransitionTime;
            instance.startTime = Time.time;
            instance.isFadingOut = true;
        }
        internal static void FadeIn() => FadeIn(null);
        internal static void FadeIn(float? transitionTime)
        {
            if (SkipKeyWatcher.HasFired) return;
            // Debug($"start fading in");
            instance.transitionTime = transitionTime ?? DefaultTransitionTime;
            instance.startTime = Time.time;
            instance.isFadingIn = true;
        }
    }
    private class Ship : MonoBehaviour
    {
        private static bool isActive = false;
        private static GameObject ship = null!;
        private void Awake()
        {
            ship = GameObject.Find("Structures").transform.Find("Boat").gameObject;
        }
        private void FixedUpdate()
        {
            if (!isActive) return;
            ship.transform.position += ship.transform.forward.SetY(0).normalized * -0.5f;
            //Debug($"moving ship: {ship.transform.position.x}, {ship.transform.position.z}");
        }

        internal static void Activate()
        {
            isActive = true;
            ship.transform.forward = new(-0.8794f, 0.007f, -0.4761f);
            ship.transform.position = new(806.6683f, 15.1128f, 789.4117f);
            var ch = ModdingAPI.Character.Get(Characters.ShipWorker1);
            Character.Pose.Set(ch.transform, Poses.Standing);
            ch.transform.localPosition = new(2.3945f, 3.6043f, -11.0234f);
            ch.transform.localRotation = Quaternion.Euler(0, 178.4325f, 0);
        }
        internal static void Create()
        {
            new GameObject("Sidequel_Ending_Ship").AddComponent<Ship>();
        }
    }
}

internal class Schedule(float time, Action action)
{
    internal readonly float time = time;
    internal readonly Action action = action;
}

