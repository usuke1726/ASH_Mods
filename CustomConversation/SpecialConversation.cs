
using System.Collections;
using ICustomConversation;
using ModdingAPI;
using UnityEngine;

namespace CustomConversation;

public abstract class ConversationDataCore : IConversationData
{
    public abstract bool TransitionStart { get; }
    public abstract bool TransitionEnd { get; }
    public abstract bool Finished { get; }
    public abstract void Update();
    public virtual void Setup() { }
    public virtual void Cleanup() { }
    public virtual void SetupWithinTransition() { }
    public virtual void CleanupWithinTransition() { }
}

public class SpecialConversation : MonoBehaviour
{
    public static bool IsInConversation { get => activeConversation != null; }
    private static SpecialConversation? activeConversation = null;
    private IConversationData data = null!;
    public static void StartConversation(IConversationData data)
    {
        if (!CharacterObject.setupDone)
        {
            Monitor.Log("Setup has not done! Please call me later!", LL.Error);
            return;
        }
        if (activeConversation != null) return;
        if (InputInterceptor.enabledAll) return;
        var conversation = new GameObject("SpecialConversation").AddComponent<SpecialConversation>();
        activeConversation = conversation;
        conversation.data = data;
        conversation.StartCoroutine(conversation.ConversationCoroutine());
    }
    private IEnumerator ConversationCoroutine()
    {
        if (data == null) yield break;
        Monitor.Log($"== Conversation START ==", LL.Warning, onlyMonitor: true);
        InputInterceptor.EnableAll(new(), handleEscapeKey: true);
        data.Setup();
        if (data.TransitionStart)
        {
            var transitionDone = false;
            Context.gameServiceLocator.transitionAnimation.Begin(delegate
            {
                data.SetupWithinTransition();
            }, delegate
            {
                transitionDone = true;
            });
            yield return new WaitUntil(() => transitionDone);
        }
        yield return new WaitForSeconds(0.5f);
        while (true)
        {
            data.Update();
            if (data.Finished) break;
            yield return new WaitForFixedUpdate();
        }
        if (data.TransitionEnd)
        {
            var transitionDone = false;
            Monitor.Log($"Conversation ending...", LL.Warning, onlyMonitor: true);
            Context.gameServiceLocator.transitionAnimation.Begin(delegate
            {
                data.CleanupWithinTransition();
            }, delegate
            {
                transitionDone = true;
            });
            yield return new WaitUntil(() => transitionDone);
        }
        data.Cleanup();
        Monitor.Log($"== Conversation END ==", LL.Warning, onlyMonitor: true);
        InputInterceptor.DisableAll();
        activeConversation = null;
        GameObject.Destroy(gameObject);
    }
}

