
using UnityEngine;

namespace ModdingAPI;

public class TemporalBox : MonoBehaviour
{
    private FloatingBox? floatingBox = null;
    private TextBoxContent? content = null;
    public bool IsShowing { get; private set; }
    public bool IsDestroyed { get; private set; }
    private string text = null!;
    private bool useFloatTime = false;
    private float time = 0f;
    private float startTime = 0f;
    private int countDown;
    private int countUp;
    private float desiredXOffset;
    private Transform target = null!;
    private void Set(Transform target, string text, int countDown, int countUp, float desiredXOffset)
    {
        this.target = target;
        this.text = text;
        this.countDown = countDown;
        this.countUp = countUp;
        this.desiredXOffset = desiredXOffset;
        IsShowing = false;
        IsDestroyed = false;
        if (countUp <= 0) Show();
    }
    private void Set(Transform target, string text, float time, float desiredXOffset)
    {
        this.target = target;
        this.text = text;
        this.time = time;
        this.desiredXOffset = desiredXOffset;
        IsShowing = false;
        IsDestroyed = false;
        useFloatTime = true;
        startTime = Time.time;
        Show();
    }
    private static int GetId(Transform target) => target.GetInstanceID();
    public static void Add(string text, int countDown = 90, int countUp = 0, float desiredXOffset = 0.35f)
    {
        if (Context.TryToGetPlayer(out var player)) Add(player.transform, text, countDown, countUp, desiredXOffset);
    }
    public static void Add(string text, float time, float desiredXOffset = 0.35f)
    {
        if (Context.TryToGetPlayer(out var player)) Add(player.transform, text, time, desiredXOffset);
    }
    public static void Add(Transform target, string text, int countDown = 90, int countUp = 0, float desiredXOffset = 0.35f)
    {
        if (!Context.GameStarted) return;
        var box = new GameObject("TemporalBox").AddComponent<TemporalBox>();
        box.Set(target, text, countDown, countUp, desiredXOffset);
    }
    public static void Add(Transform target, string text, float time, float desiredXOffset = 0.35f)
    {
        if (!Context.GameStarted) return;
        var box = new GameObject("TemporalBox").AddComponent<TemporalBox>();
        box.Set(target, text, time, desiredXOffset);
    }
    private void Show()
    {
        if (!Context.GameStarted) { Destroy(); return; }
        floatingBox = Context.serviceLocator.Locate<UI>().CreateFloatingBox();
        floatingBox.target = target;
        floatingBox.desiredPositionNormalizedXOffset = desiredXOffset;
        content = Context.serviceLocator.Locate<UI>().CreateTextBoxContent(text);
        floatingBox.SetContent(content);
        IsShowing = true;
    }
    private void Update()
    {
        if (IsDestroyed) return;
        if (useFloatTime)
        {
            if (Time.time - startTime >= time) Destroy();
        }
        else
        {
            if (!IsShowing)
            {
                countUp--;
                if (countUp <= 0) Show();
                return;
            }
            countDown--;
            if (countDown <= 0) Destroy();
        }
    }
    private void Destroy()
    {
        IsShowing = false;
        IsDestroyed = true;
        floatingBox?.Kill();
        floatingBox = null;
        content = null;
        GameObject.Destroy(gameObject);
    }
}

