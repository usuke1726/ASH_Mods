
#if DEBUG
using System.Reflection;
using HarmonyLib;
using ModdingAPI;
using Sidequel.System;
using UnityEngine;

namespace Sidequel.Dialogue;

internal class Debug : MonoBehaviour
{
    private string textCache = null!;
    private float lastShownTime = 0;
    private const float TimeInterval = 1.0f;
    public string _Text
    {
        get
        {
            if (Time.time - lastShownTime < TimeInterval) return textCache;
            lastShownTime = Time.time;
            return textCache = __CurrentText();
        }
        set => __UpdateText(value);
    }
    public bool JADone
    {
        get => System.STags.GetBool(Const.STags.JADone);
        set => System.STags.SetBool(Const.STags.JADone, value);
    }
    public int Cont
    {
        get => Sidequel.Cont.Value;
        set
        {
            System.STags.SetInt(Const.STags.Cont, value);
            typeof(Cont).GetField("value", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, value);
            if (value <= Const.Cont.EndingBorderValue) Flags.SetNodeState(NodeData.LeadEndingCont.Id, NodeEntryBase.NodeStates.Done);
        }
    }
    public int Money
    {
        get => Items.CoinsNum;
        set => Items.Add(Items.Coin, value - Items.CoinsNum);
    }
    public bool MoneySavedUp
    {
        get => System.STags.GetBool(Const.STags.CoinsSavedUp);
        set
        {
            System.STags.SetBool(Const.STags.CoinsSavedUp, value);
            typeof(NodeData.CoinReached).GetProperty("IsActive", BindingFlags.Static | BindingFlags.SetProperty | BindingFlags.NonPublic).SetValue(null, false);
        }
    }
    public GameObject z_Cutscenes { get; private set; } = null!;
    public GameObject z_NPCs { get; private set; } = null!;
    public GameObject z_Player { get; private set; } = null!;
    public bool z_DialogueCancel
    {
        get => false; set
        {
            var action = typeof(DialogueController).GetField("forceKillCurrentDialogue", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            if (action is Action a) a();
        }
    }
    public bool z_DialogueReset
    {
        get => false; set
        {
            var node = Traverse.Create(DialogueController.instance).Field("currentNode").GetValue<Node>();
            if (node == null) return;
            Traverse.Create(node).Field("index").SetValue(-1);
        }
    }
    public enum WarpPoints
    {
        Claire = 1, Charlie, MeteorLaKe, Jon, Avery, Beachstick, Boat, Ship,
        Sunhat, Home, Jim, Peak, PatsPoint
    }
    private static Dictionary<WarpPoints, Vector3> positions = new()
    {
        [WarpPoints.Claire] = new(653, 20, 353),
        [WarpPoints.Charlie] = new(305, 399, 629),
        [WarpPoints.MeteorLaKe] = new(616, 129, 421),
        [WarpPoints.Jon] = new(161, 32, 120),
        [WarpPoints.Avery] = new(48, 58, 349),
        [WarpPoints.Beachstick] = new(107, 11, 986),
        [WarpPoints.Boat] = new(149, 16, 1266),
        [WarpPoints.Ship] = new(744, 28, 743),
        [WarpPoints.Sunhat] = new(953, 11, 954),
        [WarpPoints.Home] = new(258, 268, 559),
        [WarpPoints.Peak] = new(399.0902f, 601.9924f, 804.1075f),
        [WarpPoints.PatsPoint] = new(872.8265f, 218.9995f, 84.9175f),
    };
    public WarpPoints z_WarpTo
    {
        get => 0; set
        {
            var tr = Context.Player?.transform;
            if (tr == null) return;
            switch (value)
            {
                case WarpPoints.Jim:
                    tr.position = ModdingAPI.Character.Get(Characters.OutlookPointGuy).gameObject.transform.position + Vector3.up * 2;
                    break;
                default:
                    if (positions.TryGetValue(value, out var pos)) tr.position = pos;
                    break;
            }
        }
    }
    internal static void Setup(IModHelper helper)
    {
        helper.Events.Gameloop.GameStarted += (_, _) =>
        {
            new GameObject("SSSS_DebugSidequel_Dialogue").AddComponent<Debug>();
            Util.Unregister();
        };
        Util.Setup(helper);
    }
    private void Awake()
    {
        z_Cutscenes = GameObject.Find("Cutscenes");
        z_NPCs = GameObject.Find("NPCs");
        z_Player = GameObject.Find("Player");
        Util.instance = this;
    }
    public string __CurrentText()
    {
        var content = GetContent();
        if (content == null) return null!;
        else return content.textMesh.text.Replace("\n", "\\n");
    }
    public void __UpdateText(string text)
    {
        text = text.Replace("\\n", "\n");
        var content = GetContent();
        if (content == null) return;
        content.Reset(text);
        textCache = text;
    }
    public bool __GetBool(string id) => System.STags.GetBool(id);
    public int __GetInt(string id) => System.STags.GetInt(id, 0);
    public float __GetFloat(string id) => System.STags.GetFloat(id, 0);
    public string __GetString(string id) => System.STags.GetString(id);
    public void __SetBool(string id, bool value) => System.STags.SetBool(id, value);
    public void __SetInt(string id, int value) => System.STags.SetInt(id, value);
    public void __SetFloat(string id, float value) => System.STags.SetFloat(id, value);
    public void __SetString(string id, string value) => System.STags.SetString(id, value);
    public bool __NodeYet(string id) => Flags.NodeYet(id);
    public bool __NodeIP(string id) => Flags.NodeIP(id);
    public bool __NodeRefused(string id) => Flags.NodeRefused(id);
    public bool __NodeDone(string id) => Flags.NodeDone(id);
    private TextBoxContent? GetContent()
    {
        try
        {
            var conv = Traverse.Create(DialogueController.instance).Field("currentConversation").GetValue<TextBoxConversation>();
            if (conv == null || !conv.isAlive) return null;
            var box = Traverse.Create(conv).Field("floatingBox").GetValue<FloatingBox>();
            if (box == null) return null;
            var content = Traverse.Create(box).Field("currentContent").GetValue<IFloatingBoxContent>() as TextBoxContent;
            if (content != null) return content;
            content = Traverse.Create(box).Field("upcomingContent").GetValue<IFloatingBoxContent>() as TextBoxContent;
            if (content != null) return content;
            throw new("content is not TextBoxContent");
        }
        catch (Exception e)
        {
            Monitor.Log($"error on get content (debug): {e}", LL.Warning, true);
            return null;
        }
    }

    private static class Util
    {
        internal static Debug instance = null!;
        private static Action? unregister1 = null!;
        private static Action? unregister2 = null!;
        internal static void Setup(IModHelper helper)
        {
            ModdingAPI.KeyBind.KeyBind.RegisterKeyBind("P(LeftShift)", NewGame, out var parseResult1, name: "StartNewSidequel");
            ModdingAPI.KeyBind.KeyBind.RegisterKeyBind("I(LeftShift)", LoadGame, out var parseResult2, name: "StartSidequel");
            ModdingAPI.KeyBind.KeyBind? keybind1 = null;
            ModdingAPI.KeyBind.KeyBind? keybind2 = null;
            if (parseResult1.Success)
            {
                unregister1 = parseResult1.Unregister;
                keybind1 = parseResult1.KeyBinds[0];
            }
            if (parseResult2.Success)
            {
                unregister2 = parseResult2.Unregister;
                keybind2 = parseResult2.KeyBinds[0];
            }
            helper.Events.Gameloop.ReturnedToTitle += (_, _) =>
            {
                if (keybind1 != null) ModdingAPI.KeyBind.KeyBind.RegisterKeyBind(keybind1, out unregister1);
                if (keybind2 != null) ModdingAPI.KeyBind.KeyBind.RegisterKeyBind(keybind2, out unregister2);
            };
            helper.Events.Gameloop.GameStarted += (_, _) => Timer.Register(1f, () =>
            {
                if (!State.IsActive) return;
                var values = DebugInitialValuesBase.instance;
                var jadone = values.JADone;
                if (jadone != null) instance.JADone = (bool)jadone;
                var cont = values.Cont;
                if (cont != null) instance.Cont = (int)cont;
                var money = values.Money;
                if (money != null) instance.Money = (int)money;
                var moneySavedUp = values.MoneySavedUp;
                if (moneySavedUp != null) instance.MoneySavedUp = (bool)moneySavedUp;
                var p = values.Point;
                if (p != null) instance.z_WarpTo = (WarpPoints)p;
                values.OnGameStarted();
            });
        }
        internal static void Unregister()
        {
            unregister1?.Invoke();
            unregister1 = null!;
            unregister2?.Invoke();
            unregister2 = null!;
        }
        private static void NewGame()
        {
            if (!CanStart(true)) return;
            Unregister();
            Monitor.Log($"===== Sidequel NewGame starting... =====", LL.Warning, true);
            State.SetNewGame(true);
            NewGameController.StartGame();
        }
        private static void LoadGame()
        {
            if (!CanStart(false)) return;
            Unregister();
            Monitor.Log($"===== Sidequel Continue starting... =====", LL.Warning, true);
            State.SetNewGame(false);
            NewGameController.StartGame();
        }
        private static bool CanStart(bool isNewGame)
        {
            if (!Context.OnTitle)
            {
                Debug($"Not on titlescreen");
                return false;
            }
            if (!CrossPlatform.DoesSaveExist())
            {
                Debug($"Savefile does not exist");
                return false;
            }
            if (!isNewGame && !SaveData.DoesSaveDataExists())
            {
                Debug($"Sidequel savedata does not exist");
                return false;
            }
            return true;
        }
    }
}
#endif

