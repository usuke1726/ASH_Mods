
using System.Collections;
using HarmonyLib;
using ModdingAPI;
using ModdingAPI.KeyBind;
using QuickUnityTools.Input;
using UnityEngine;

namespace CoinChallenge;

[HarmonyPatch(typeof(TimerUI))]
internal static class TimerUIPatch
{
    internal static readonly string SpecialTimerName = "SPECIAL TIMER";
    [HarmonyPrefix()]
    [HarmonyPatch("SecondsToCharArray")]
    internal static bool SecondsToCharArray(float timeInSeconds, char[] array, TimerUI __instance)
    {
        if (__instance.gameObject.name != SpecialTimerName) return true;
        var totalCoins = GameController.TotalCoins.ToString();
        var totalCoinsLen = totalCoins.Length;
        var currentCoins = GameController.CurrentCoins.ToString().PadLeft(totalCoinsLen, '0');
        var coinsStr = $"{currentCoins}/{totalCoins} ";
        var coinsStrLen = coinsStr.Length;
        if (array.Length < coinsStrLen + 7) return true;
        for (int i = 0; i < coinsStrLen; i++)
        {
            array[i] = coinsStr[i];
        }

        int num = (int)(timeInSeconds / 60f);
        array[coinsStrLen + 0] = (char)(48 + (num % 100) / 10);
        array[coinsStrLen + 1] = (char)(48 + num % 10);
        array[coinsStrLen + 2] = ':';
        int num2 = (int)(timeInSeconds - (float)(num * 60));
        array[coinsStrLen + 3] = (char)(48 + num2 / 10);
        array[coinsStrLen + 4] = (char)(48 + num2 % 10);
        array[coinsStrLen + 5] = '.';
        int num3 = (int)(timeInSeconds % 1f * 1000f);
        array[coinsStrLen + 6] = (char)(48 + num3 / 100);
        return false;
    }
}

internal class GameController : MonoBehaviour
{
    private static GameController? currentController = null;
    public static bool IsRaceActive => currentController != null;
    private static GameObject controllerObj = null!;
    private static AudioClip countdownSound = null!;
    private static AudioClip countdownDone = null!;
    private static AudioClip startSound = null!;
    private static AudioClip winnerSound = null!;
    private static GameObject winnerParticles = null!;
    private static MusicSet musicSet = null!;
    private static GameObject timerUIPrefab = null!;
    private static GameObject boatArrowUIPrefab = null!;
    internal static void Setup(IModHelper helper)
    {
        helper.KeyBindingsData.SetDefault(new Dictionary<string, string>()
        {
            ["AbandonCoinRace"] = "Delete(F12)",
        });
        KeyBind.RegisterKeyBind(helper.KeyBindingsData, "AbandonCoinRace", ConfirmToAbandon, "Abandon Coin Race");
        helper.Events.Gameloop.GameStarted += (_, _) => Setup();
        helper.Events.Gameloop.ReturnedToTitle += (_, _) => Reset();
    }
    internal static void ConfirmToAbandon()
    {
        if (currentController != null)
        {
            MainMenu.ShowConfirm("Give up?", "Yes", "No", () =>
            {
                ForceAbandon();
                //Context.player.transform.position = new(658, 21, 358); // back to home
            });
        }
    }
    private static void Setup()
    {
        var boatScripting = GameObject.Find("BoatScripting").GetComponent<BoatScripting>();
        timerUIPrefab = boatScripting.timerUIPrefab;
        boatArrowUIPrefab = boatScripting.boatArrowUIPrefab;
        var raceController = GameObject.Find("PlayerRace").transform.Find("RaceOpponent").GetComponent<RaceController>();
        countdownSound = raceController.countdownSound;
        countdownDone = raceController.countdownDone;
        startSound = raceController.startSound;
        musicSet = raceController.raceMusic;
        winnerSound = raceController.winnerSound;
        winnerParticles = raceController.winnerParticles;
    }
    private static void Reset()
    {
        currentController = null;
        controllerObj = null!;
        countdownSound = null!;
        countdownDone = null!;
        startSound = null!;
        winnerSound = null!;
        winnerParticles = null!;
        musicSet = null!;
        timerUIPrefab = null!;
        boatArrowUIPrefab = null!;
    }
    internal static void Start(ICoinChallenge challenge)
    {
        if (currentController != null)
        {
            return;
        }
        controllerObj = new("CoinChallengeController");
        var controller = controllerObj.AddComponent<GameController>();
        currentController = controller;
        controller.challenge = challenge;
        controller.StartCoroutine(controller.Coroutine());
    }
    internal static void ForceAbandon() => abandonRace = true;
    private static bool abandonRace = false;

    private ICoinChallenge challenge = null!;
    private ScriptedMusic music = null!;
    private bool finished = false;
    private TimerUI timerUI = null!;
    private BoatGoalArrowUI boatArrow = null!;
    private IEnumerator Coroutine()
    {
        var inputLock = GameUserInput.CreateInputGameObjectWithPriority(10);
        Monitor.Log($"Start Transition", LL.Warning, true);
        yield return TryTransition(isSetup: true);
        Monitor.Log($"End Transition", LL.Warning, true);
        if (!SetupCoins())
        {
            GameObject.Destroy(inputLock);
            GameObject.Destroy(gameObject);
            yield break;
        }
        Monitor.Log($"Start Countdown", LL.Warning, true);
        yield return Countdown(() => GameObject.Destroy(inputLock));
        timerUI = Context.gameServiceLocator.ui.AddUI(timerUIPrefab.Clone()).GetComponent<TimerUI>();
        timerUI.gameObject.name = TimerUIPatch.SpecialTimerName;
        SetupArrow();
        var coinsStrNum = totalCoins.ToString().Length * 2 + 2;
        Traverse.Create(timerUI).Field("timeString").SetValue(new char["00:00.0".Length + coinsStrNum]);
        timerUI.Begin();
        var rect = timerUI.GetComponent<RectTransform>();
        rect.pivot = rect.pivot.SetX(1.2f);
        abandonRace = false;
        finished = false;
        lastPlayedTime = Time.time;
        int finishCountdown = 2;
        bool finishing = false;
        while (true)
        {
            if (finished || challenge.ForceFinished || abandonRace) finishing = true;
            if (finishing)
            {
                finishCountdown--;
                if (finishCountdown <= 0) break;
            }
            else
            {
                UpdateArrow();
                UpdateCoinDetectorSound();
                UpdateCameraMotion();
            }
            yield return new WaitForFixedUpdate();
        }
        timerUI.Stop();
        music.Stop();
        GameObject.Destroy(boatArrow.gameObject);
        float time = timerUI.time;
        Monitor.Log($"== CoinChallenge time: {time}", LL.Warning);
        yield return TryTransition(isSetup: false);
        currentController = null;
        challenge = null!;
        Timer timer = null!;
        timer = Timer.Register(3.0f, () =>
        {
            Timer.ReleaseFromManager(timer);
            GameObject.Destroy(timerUI.gameObject);
        });
        GameObject.Destroy(gameObject);
    }
    private IEnumerator Countdown(Action onStart)
    {
        var silenceKey = Singleton<MusicManager>.instance.RegisterSilenece(50);
        Singleton<MusicManager>.instance.TrimRetiredActiveMusicSets(3f);
        music = new(musicSet, 200);
        music.Load();
        countdownSound.Play();
        yield return new WaitForSeconds(1f);
        countdownSound.Play();
        yield return new WaitForSeconds(1f);
        countdownSound.Play();
        yield return new WaitForSeconds(1f);
        countdownDone.Play();
        startSound.Play();
        onStart();
        yield return new WaitForSeconds(0.1f);
        silenceKey.ReleaseResource();
        music.Play();
    }
    private IEnumerator TryTransition(bool isSetup)
    {
        int? allowedFeathers = isSetup ? challenge.AllowedFeathers : null;
        Action task = isSetup ? () => challenge.Setup() : () => challenge.Cleanup();
        bool useTransition = isSetup ? challenge.UseTransitionStart : challenge.UseTransitionEnd;
        Monitor.Log($"useTransition: {useTransition}", LL.Warning, true);
        if (useTransition)
        {
            var transisionDone = false;
            Context.gameServiceLocator.transitionAnimation.Begin(delegate
            {
                Context.player.allowedFeathers = allowedFeathers;
                task();
            }, delegate
            {
                transisionDone = true;
            });
            yield return new WaitUntil(() => transisionDone);
        }
        else
        {
            Context.player.allowedFeathers = allowedFeathers;
            task();
        }
    }
    internal static int TotalCoins { get => totalCoins; }
    internal static int CurrentCoins { get => currentCoins; }
    private static int totalCoins = 0;
    private static int currentCoins = 0;
    List<GameObject> coinObjects = [];
    private bool SetupCoins()
    {
        var coins = challenge.Coins;
        totalCoins = coins.Count;
        currentCoins = 0;
        Monitor.Log($"== TOTAL COINS: {totalCoins} ==", LL.Warning, true);
        if (totalCoins <= 0) return false;
        foreach (var coin in coins)
        {
            coinObjects.Add(coin.gameObject);
            coin.onCollect += OnCollect;
        }
        return true;
    }
    private void OnCollect()
    {
        currentCoins++;
        Monitor.Log($"coin {currentCoins} / {totalCoins}", LL.Debug, true);
        coinDetectorCooltime = 30;
        if (currentCoins >= totalCoins)
        {
            Monitor.Log($"== GOT ALL COINS!! ==", LL.Warning, true);
            finished = true;
            OnWin();
        }
    }
    public static bool KeyboardCameraMotionEnabled = true;
    private void UpdateCameraMotion()
    {
        if (!KeyboardCameraMotionEnabled) return;
        var camera = Context.gameServiceLocator.levelController.cinemaCamera;
        if (camera == null || !camera.activeSelf) return;
        var speed = 1.0f;
        Vector3 p = Vector3.zero;

        if (UnityEngine.Input.GetKey(KeyCode.End)) p = new(0, -speed, 0);
        else if (UnityEngine.Input.GetKey(KeyCode.Home)) p = new(0, speed, 0);

        if (UnityEngine.Input.GetKey(KeyCode.PageUp)) p += new Vector3(speed, 0, 0);
        else if (UnityEngine.Input.GetKey(KeyCode.PageDown)) p += new Vector3(-speed, 0, 0);

        if (p != Vector3.zero)
        {
            camera.transform.rotation = Quaternion.Euler(camera.transform.rotation.eulerAngles + p);
        }
    }
    private bool arrowVisible = false;
    private bool arrowEnabled = true;
    private float lastPlayedTime = 0;
    private float showArrowTime;
    private void SetupArrow()
    {
        boatArrow = Context.gameServiceLocator.ui.AddUI(boatArrowUIPrefab.Clone()).GetComponent<BoatGoalArrowUI>();
        boatArrow.destinationOffset = Vector2.up * 25f;
        boatArrow.destination = coinObjects[0].transform;
        boatArrow.gameObject.SetActive(false);
        boatArrow.gameObject.name = "CoinChallengeHintArrow";
        var time = challenge.ArrowHintTime;
        if (time == null) arrowEnabled = false;
        else showArrowTime = (float)time;
    }
    private void UpdateArrow()
    {
        if (!arrowEnabled) return;
        var dest = boatArrow.destination;
        if (dest == null || dest.gameObject == null || !dest.gameObject.activeSelf)
        {
            Monitor.Log($"remapping destination...", LL.Debug, true);
            UpdateArrowDestination();
        }
        if (arrowVisible)
        {
            if (!boatArrow.gameObject.activeSelf) boatArrow.gameObject.SetActive(true);
            return;
        }
        if (Time.time - lastPlayedTime > showArrowTime)
        {
            arrowVisible = true;
            boatArrow.gameObject.SetActive(true);
            Monitor.Log($"ArrowHint on", LL.Debug, true);
        }
    }
    private void UpdateArrowDestination()
    {
        arrowVisible = false;
        boatArrow.gameObject.SetActive(false);
        var obj = coinObjects.Find(obj => obj != null && obj.activeSelf);
        if (obj == null || !obj.activeSelf) return;
        boatArrow.destination = obj.transform;
    }
    private void OnWin()
    {
        winnerParticles.CloneAt(Context.player.transform.position);
        winnerSound.Play();
    }

    private int coinDetectorCooltime = 0;
    private int coinDetectorCount = 60;
    private int coinDetectorPrevMode = 0;
    private void UpdateCoinDetectorSound()
    {
        var dist = MinSqrDistance();
        var mode = CoinDetectorModeFromDist(dist);
        if (coinDetectorPrevMode != mode)
        {
            coinDetectorPrevMode = mode;
            coinDetectorCount = Math.Min(coinDetectorCount, MaxCountFromCoinDetectorMode(mode));
        }
        if (coinDetectorCooltime > 0)
        {
            coinDetectorCooltime--;
            return;
        }
        coinDetectorCount--;
        if (coinDetectorCount <= 0)
        {
            coinDetectorCount = MaxCountFromCoinDetectorMode(coinDetectorPrevMode);
            if (coinDetectorPrevMode > 0)
            {
                lastPlayedTime = Time.time;
                var source = countdownSound.Play();
                source.volume = CoinDetectorVolume(mode, dist);
                source.pitch = CoinDetectorPitch(mode, dist);
            }
        }
    }
    private const float dist1 = 100f;
    private const float dist2 = 500f;
    private const float dist3 = 1000f;
    private const float dist4 = 2000f;
    private float CoinDetectorPitch(int mode, float dist)
    {
        return mode switch
        {
            4 => 2.0f,
            //1 => 0.8f,
            _ => 1.0f,
        };
    }
    private float CoinDetectorVolume(int mode, float dist)
    {
        return mode switch
        {
            1 => 0.2f,
            _ => 0.3f,
        };
    }
    private int CoinDetectorModeFromDist(float dist)
    {
        return dist switch
        {
            <= dist1 => 4,
            <= dist2 => 3,
            <= dist3 => 2,
            <= dist4 => 1,
            _ => 0
        };
    }
    private int MaxCountFromCoinDetectorMode(int mode)
    {
        return mode switch
        {
            4 => 15,
            3 => 30,
            2 => 60,
            1 => 90,
            _ => 15,
        };
    }
    private float MinSqrDistance()
    {
        var player = Context.player;
        var pos = player.transform.position.SetY(0);
        float ret = float.MaxValue;
        foreach (var coin in coinObjects)
        {
            if (coin == null || !coin.activeSelf) continue;
            var p = coin.transform.position.SetY(0);
            ret = Mathf.Min(ret, (pos - p).sqrMagnitude);
        }
        return ret;
    }
}

