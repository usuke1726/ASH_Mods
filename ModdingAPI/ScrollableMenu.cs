
using UnityEngine;
using UnityEngine.UI;

namespace ModdingAPI;
public class ScrollableMenu : AbstractMenu
{
    public bool Loop = true;
    public float FastScrollActivateTime = 0.5f;
    public float FastScrollStepTime = 0.1f;

    private int absoluteIndex;
    private int relativeIndex;
    private List<GameObject> menuObjects = [];
    private string[] options = [];
    private Action<BasicMenuItem>[] events = [];
    private RectTransform rect = null!;
    private LayoutElement layout = null!;
    private float currentMaxWidth = 0;

    public override IMenuItem indexedMenuItem => GetMenuItem(relativeIndex);
    private IMenuItem GetMenuItem(int index)
    {
        if (index >= menuObjects.Count) return null!;
        return menuObjects[index].GetComponent<IMenuItem>();
    }
    protected override void Awake()
    {
        var lm = gameObject.GetComponent<LinearMenu>();
        userInput = lm.userInput;
        GameObject.Destroy(lm);
        base.Awake();
        rect = gameObject.GetComponent<RectTransform>();
        layout = gameObject.GetComponent<LayoutElement>();
    }

    private int count;
    private readonly int maxCount = 7;
    private readonly int sideNum = 3;
    private bool ready = false;
    private float fastScrollHeldTime;

    private int prevFastScrollDirection;
    protected override void Update()
    {
        if (!ready) return;
        base.Update();
        if (isFocused && menuObjects.Count > 0)
        {
            float dot = Vector2.Dot(Vector2.down, userInput.leftStick.vector);
            int dir = Mathf.Abs(dot) > 0.5f ? (int)Mathf.Sign(dot) : 0;
            if (dir == prevFastScrollDirection && dir != 0)
            {
                fastScrollHeldTime += Time.deltaTime;
            }
            else
            {
                fastScrollHeldTime = 0;
            }
            prevFastScrollDirection = dir;
            int dir2 = 0;
            if (fastScrollHeldTime > FastScrollActivateTime)
            {
                dir2 = dir;
                fastScrollHeldTime = FastScrollActivateTime - FastScrollStepTime;
            }
            IMenuItem previous = indexedMenuItem;
            if (userInput.leftStick.WasDirectionTapped(Vector2.down) || dir2 == 1)
            {
                if (absoluteIndex == count - 1 && !Loop) return;
                absoluteIndex = (absoluteIndex + 1).Mod(count);
                UpdateView(previous);
                moveSound.Play();
            }
            else if (userInput.leftStick.WasDirectionTapped(Vector2.up) || dir2 == -1)
            {
                if (absoluteIndex == 0 && !Loop) return;
                absoluteIndex = (absoluteIndex - 1).Mod(count);
                UpdateView(previous);
                moveSound.Play();
            }
        }
    }
    private int prevMinIdx = -1;
    private int prevMaxIdx = -1;
    private void UpdateView(IMenuItem? previous)
    {
        previous ??= GetMenuItem(0);
        int minIdx, maxIdx;
        if (absoluteIndex < sideNum)
        {
            relativeIndex = absoluteIndex;
            minIdx = 0;
            maxIdx = maxCount - 1;
        }
        else if (absoluteIndex >= count - sideNum)
        {
            relativeIndex = maxCount - (count - absoluteIndex);
            minIdx = count - maxCount;
            maxIdx = count - 1;
        }
        else
        {
            relativeIndex = sideNum;
            minIdx = absoluteIndex - sideNum;
            maxIdx = absoluteIndex + sideNum;
        }
        if (prevMinIdx != minIdx || prevMaxIdx != maxIdx)
        {
            prevMinIdx = minIdx;
            prevMaxIdx = maxIdx;
            UpdateText();
        }
        UpdateIndexSelectionApperance(previous, indexedMenuItem);
        var width = rect.rect.width;
        if (width > currentMaxWidth)
        {
            currentMaxWidth = width;
            layout.minWidth = width;
        }
    }
    private void UpdateText()
    {
        if (prevMinIdx < 0 || prevMaxIdx < 0) return;
        for (int i = prevMinIdx; i <= prevMaxIdx; i++)
        {
            var idx = i - prevMinIdx;
            UI.SetGenericText(menuObjects[idx], options[i]);
        }
    }
    private void OnConfirm(BasicMenuItem item)
    {
        events[absoluteIndex].Invoke(item);
    }
    internal void Refresh(string[] newOptions)
    {
        if (newOptions.Length != count) return;
        options = newOptions;
        UpdateText();
    }
    private static bool IsUsingJapanese(string[] options) => options.Any(o => Util.IsUsingJapanese(o));

    public static AbstractMenu CreateScrollableMenu(string[] options, Action<BasicMenuItem>[] events, int initialIndex = 0, bool? useEnglishFont = null)
    {
        var ui = Context.gameServiceLocator.ui;
        var obj = ui.simpleMenuPrefab.Clone();
        ScrollableMenu menu = obj.AddComponent<ScrollableMenu>();
        ui.AddUI(obj);
        menu.absoluteIndex = initialIndex;
        var useEnglish = useEnglishFont ?? !IsUsingJapanese(options);
        if (options.Length != events.Length)
        {
            Monitor.SLog($"different length (options: {options.Length}, events: {events.Length})", LogLevel.Warning);
        }
        int length = Math.Min(options.Length, events.Length);
        if (length <= menu.maxCount) throw new Exception("too few items");
        for (int i = 0; i < menu.maxCount; i++)
        {
            var itemObj = ui.simpleMenuItemPrefab.Clone();
            UI.SetGenericText(itemObj, "");
            BasicMenuItem item = itemObj.GetComponentInChildren<BasicMenuItem>();
            item.onConfirm.AddListener(delegate
            {
                menu.OnConfirm(item);
            });
            itemObj.transform.SetParent(menu.transform, false);
            if (useEnglish)
            {
                var translator = itemObj.GetComponentInChildren<TextTranslator>();
                translator.enabled = false;
                translator.DisableCustomizedFont();
            }
            menu.menuObjects.Add(itemObj);
        }
        menu.options = options[0..length];
        menu.events = events[0..length];
        menu.count = length;
        menu.ready = true;
        menu.UpdateView(null);
        return menu;
    }
}

