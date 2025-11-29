
using System.Reflection;
using HarmonyLib;
using ModdingAPI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Sidequel.System;

internal class Title
{
    private static string[] disableNames = [
        "BackgroundMountain",
        "CarAnimator",
        "StarParticles",
        "Sky",
        "Grass",
        "Trees",
        "Trees (1)",
        "ScrollingPropsAnimator",
    ];
    private static bool setupDone = false;
    private static Transform world = null!;
    private static List<Transform> worldObjects = [];
    private static Camera camera = null!;
    private static TextMeshProUGUI text = null!;
    private static Transform player = null!;
    private static Light light = null!;
    private static float defaultFarClipPlane;
    private static float defaultLightIntensity;
    private static Color defaultTitleColor;
    private static string defaultTitleText = null!;
    private static Quaternion defaultLightRotation;
    private static Color? defaultAmbientLight = null;
    private static void Activate()
    {
        if (!Context.OnTitle) return;
        if (!CrossPlatform.DoesSaveExist()) return;
        SetupObjects();
        foreach (var obj in worldObjects) obj.gameObject.SetActive(false);
        player.gameObject.SetActive(true);
        camera.farClipPlane = 100;
        text.text = "a short hike - sidequel";
        text.color = new(0.3762f, 0.3322f, 0.7536f, 1);
        light.transform.localRotation = Quaternion.Euler(9.2238f, 68.4014f, 185.2988f);
        light.intensity = 0.5f;
        defaultAmbientLight ??= RenderSettings.ambientLight;
        RenderSettings.ambientLight = new(0.1f, 0.1f, 0.1f, 1);
    }
    private static void Deactivate()
    {
        if (!Context.OnTitle) return;
        SetupObjects();
        player.gameObject.SetActive(false);
        foreach (var obj in worldObjects) obj.gameObject.SetActive(true);
        camera.farClipPlane = defaultFarClipPlane;
        text.text = defaultTitleText;
        text.color = defaultTitleColor;
        light.transform.localRotation = defaultLightRotation;
        light.intensity = defaultLightIntensity;
        if (defaultAmbientLight != null) RenderSettings.ambientLight = (Color)defaultAmbientLight;
    }
    private static void SetupObjects()
    {
        if (setupDone) return;
        setupDone = true;
        var worldObj = GameObject.Find("World");
        NonNull(worldObj, "world");
        world = worldObj.transform;
        foreach (var name in disableNames)
        {
            var obj = world.transform.Find(name);
            NonNull(obj, name);
            worldObjects.Add(obj);
        }

        player = world.Find("Character");
        player.position = new(-22.9041f, -17.2087f, 17.8336f);
        player.localRotation = Quaternion.Euler(0, 83.7236f, 0);
        player.localScale = new(10.5f, 10.5f, 10.5f);
        player.Find("Body").GetComponent<SkinnedMeshRenderer>().material.color = new(0.0898f, 0.1321f, 0.0729f, 1); // shirt
        player.Find("HeadMesh").GetComponent<SkinnedMeshRenderer>().materials[0].color = new(0.229f, 0.3108f, 0.5849f, 1); // skin
        player.Find("HeadMesh").GetComponent<SkinnedMeshRenderer>().materials[1].color = new(1, 0.6135f, 0, 1); // beak
        player.Find("Arms").GetComponent<SkinnedMeshRenderer>().material.color = new(0.229f, 0.3108f, 0.5849f, 1); // skin
        var eye = player.Find("Armature/root/Base/Chest/Head/EyeR");
        var eyeL = player.Find("Armature/root/Base/Chest/Head/EyeL");
        eyeL.gameObject.SetActive(false);
        var pupil = eye.Find("Pupil");
        var eyeRenderer = eye.GetComponent<SpriteRenderer>();
        var pupRenderer = pupil.GetComponent<SpriteRenderer>();
        var eyeSp = eyeRenderer.sprite;
        var pupSp = pupRenderer.sprite;
        var eyeTex = Util.EditableTexture(eyeSp.texture);
        var pupTex = Util.EditableTexture(pupSp.texture);
        for (int y = 0; y < eyeTex.height; y++)
        {
            for (int x = 0; x < eyeTex.width; x++)
            {
                if (eyeTex.GetPixel(x, y).a < 0.5f) continue;
                Color p = x < 100 ? new(0, 0, 0, 0) : new(1, 1, 1, 1);
                eyeTex.SetPixel(x, y, p);
            }
        }
        eyeTex.Apply();
        eyeRenderer.sprite = Sprite.Create(eyeTex, eyeSp.rect, new Vector2(0.5f, 0.5f), eyeSp.pixelsPerUnit);
        for (int y = 0; y < pupTex.height; y++)
        {
            for (int x = 0; x < pupTex.width; x++)
            {
                if (pupTex.GetPixel(x, y).a < 0.5f) continue;
                Color p = x < 73 ? new(0, 0, 0, 0) : new(1, 1, 1, 1);
                pupTex.SetPixel(x, y, p);
            }
        }
        pupTex.Apply();
        pupRenderer.sprite = Sprite.Create(pupTex, pupSp.rect, new Vector2(0.5f, 0.5f), pupSp.pixelsPerUnit);

        var cameraObj = GameObject.Find("Cameras/GameCamera");
        NonNull(cameraObj, "camera");
        camera = cameraObj.GetComponent<Camera>();
        defaultFarClipPlane = camera.farClipPlane;

        var title = GameObject.Find("TitleScreenCanvas/Title");
        NonNull(title, "title");
        text = title.GetComponent<TextMeshProUGUI>();
        defaultTitleColor = text.color;
        defaultTitleText = text.text;

        var lightObj = GameObject.Find("Lighting").transform.Find("Directional Light");
        NonNull(lightObj, "light");
        light = lightObj.GetComponent<Light>();
        defaultLightRotation = light.transform.localRotation;
        defaultLightIntensity = light.intensity;

        AmbienceSoundManager.Create();
    }
    private static void CleanUp()
    {
        setupDone = false;
        IsSidequelMode = false;
        world = null!;
        worldObjects.Clear();
        camera = null!;
        text = null!;
        player = null!;
        defaultTitleText = null!;
        light = null!;
    }
    private static void NonNull(GameObject obj, string name) => Assert(obj != null, $"{name} is null");
    private static void NonNull(Transform obj, string name) => Assert(obj != null, $"{name} is null");
    internal static void Setup(IModHelper helper)
    {
        helper.Events.Gameloop.GameStarted += (_, _) => CleanUp();
    }
    internal static bool IsSidequelMode { get; private set; } = false;
    internal static void Toggle()
    {
        if (!Context.OnTitle) return;
        IsSidequelMode = !IsSidequelMode;
        if (IsSidequelMode) Activate();
        else Deactivate();
    }
    internal static void TryToStart(bool isNewGame)
    {
        if (!Context.OnTitle)
        {
            MainMenu.ShowDialog(I18nLocalize("ModConfigMenu.triedToStartOutsideMainMenu"));
            return;
        }
        if (!CrossPlatform.DoesSaveExist())
        {
            MainMenu.ShowDialog(I18nLocalize("ModConfigMenu.notCreatedSaveData"));
            return;
        }
        var saveDataExists = SaveData.DoesSaveDataExists();
        if (!isNewGame && !saveDataExists)
        {
            MainMenu.ShowDialog(I18nLocalize("ModConfigMenu.notCreatedSaveData"));
            return;
        }
        State.SetNewGame(isNewGame);
        if (isNewGame && saveDataExists)
        {
            MainMenu.ShowConfirm(
                I18nLocalize("ModConfigMenu.confirmOverwritingSaveData"),
                I18nLocalize("ModConfigMenu.confirmOverwritingSaveData.y"),
                I18nLocalize("ModConfigMenu.confirmOverwritingSaveData.n"),
                NewGameController.StartGame);
        }
        else
        {
            NewGameController.StartGame();
        }
    }
    private class AmbienceSoundManager : MonoBehaviour
    {
        private AudioSource audio = null!;
        private float defaultVolume;
        private const float FadingTime = 2f;
        private const float FadingCoeff = 1.0f / FadingTime;
        internal static void Create() => new GameObject("Sidequel_AmbienceSoundManager").AddComponent<AmbienceSoundManager>();
        private void Awake()
        {
            audio = GameObject.Find("World/AmbienceSounds").GetComponent<AudioSource>();
            defaultVolume = audio.volume;
        }
        private void Update()
        {
            var d = Time.deltaTime * FadingCoeff;
            if (IsSidequelMode && audio.volume > 0)
            {
                audio.volume = Mathf.MoveTowards(audio.volume, 0, d);
            }
            else if (!IsSidequelMode && audio.volume < defaultVolume)
            {
                audio.volume = Mathf.MoveTowards(audio.volume, defaultVolume, d);
            }
        }
    }
}

[HarmonyPatch(typeof(TitleScreen))]
internal class TitleScreenPatch
{
    [HarmonyPrefix()]
    [HarmonyPatch("UpdateTitleScreenMenuItems")]
    internal static void OnTitleScreenUpdating()
    {
        if (!Context.OnTitle) return;
        if (!CrossPlatform.DoesSaveExist() && Title.IsSidequelMode) Title.Toggle();
    }
    [HarmonyPrefix()]
    [HarmonyPatch("StartNewGame")]
    internal static bool OnTryingToStartNewGame(TitleScreen __instance)
    {
        if (Title.IsSidequelMode || !CrossPlatform.DoesSaveExist()) return true;
        UI ui = Context.serviceLocator.Locate<UI>();
        LinearMenu submenu = null!;
        submenu = ui.CreateSimpleMenu([I18n.STRINGS.continueText, I18n.STRINGS.dontContinue], [
            () => {
                submenu.Kill();
                BeginLoadingNewGame(__instance);
            },
            () => {
                submenu.Kill();
            }
        ]);
        var obj = ui.CreateTextMenuItem(I18n.STRINGS.overwriteSaveFile);
        var t = I18nLocalize("ModConfigMenu.additionalConfirmMessage");
        if (!string.IsNullOrWhiteSpace(t))
        {
            obj.GetComponentInChildren<TextMeshProUGUI>().text += $"\n{t}";
        }
        obj.transform.SetParent(submenu.transform, false);
        obj.transform.SetAsFirstSibling();
        if (submenu.transform is RectTransform r)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(r);
            r.CenterWithinParent();
        }
        return false;
    }
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(TitleScreen), "BeginLoadingNewGame")]
    internal static void BeginLoadingNewGame(object instance) => throw new NotImplementedException();
}

[HarmonyPatch(typeof(GridMenu))]
internal class GridMenuPatch
{
    [HarmonyPrefix()]
    [HarmonyPatch("Update")]
    internal static bool Prefix(GridMenu __instance)
    {
        if (!Context.OnTitle) return true;
        if (__instance.name != "MainMenu") return true;
        var tr = Traverse.Create(__instance);
        var menuItemGrid = tr.Field("menuItemGrid").GetValue<List<List<GameObject>>>();
        if (menuItemGrid[0].Count == 1)
        {
            Debug($"No Original SaveData");
            return true;
        }
        if (!__instance.userInput.hasFocus) return false;
        if (__instance.isConfirmPressed(__instance.userInput) && __instance.indexedMenuItem != null)
        {
            var name = __instance.indexedMenuItem.gameObject.name;
            if (name == "continue" && Title.IsSidequelMode) Title.TryToStart(false);
            else if (name == "new" && Title.IsSidequelMode) Title.TryToStart(true);
            else __instance.indexedMenuItem.Confirm();
            if (__instance.confirmSound != null)
            {
                __instance.confirmSound.Play();
            }
        }
        selectedIndex ??= typeof(GridMenu).GetProperty("selectedIndex", BindingFlags.NonPublic | BindingFlags.Instance);
        __instance.EnsureValidIndex();
        var mainDirections = Vector2Int.MAIN_DIRECTIONS;
        var s = (Vector2Int)selectedIndex.GetValue(__instance);
        for (int i = 0; i < mainDirections.Length; i++)
        {
            var dir = mainDirections[i];
            if (__instance.userInput.leftStick.WasDirectionTapped(dir.ToV2()))
            {
                int newX = (s.x - dir.x).Mod(2);
                int newY = (s.y - dir.y).Mod(2);
                if ((s.x == 1 && dir.x == 1) || (s.x == 0 && dir.x == -1)) Title.Toggle();
                selectedIndex.SetValue(__instance, new Vector2Int(newX, newY));
                __instance.EnsureValidIndex();
                __instance.moveSound.Play();
                break;
            }
        }
        return false;
    }
    private static PropertyInfo selectedIndex = null!;
}

