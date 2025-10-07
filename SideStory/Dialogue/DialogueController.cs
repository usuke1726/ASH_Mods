
using System.Collections;
using ModdingAPI;
using SideStory.Dialogue.Actions;
using UnityEngine;

namespace SideStory.Dialogue;

internal class DialogueController : MonoBehaviour
{
    internal static DialogueController instance = null!;
    private static bool characterObjectSetupDone = false;
    internal static void Setup(IModHelper helper)
    {
        helper.Events.Gameloop.GameStarted += (_, _) =>
        {
            ModdingAPI.Character.OnSetupDone(() => characterObjectSetupDone = true);
            instance = new GameObject("SideStoryDialogueController").AddComponent<DialogueController>();
            if (State.IsNewGame) instance.StartConversation(null);
        };
        helper.Events.Gameloop.ReturnedToTitle += (_, _) => characterObjectSetupDone = false;
    }

    private TextBoxConversation currentConversation = null!;
    private Node currentNode = null!;
    internal IConversation StartConversation(DialogueInteractable? dialogue)
    {
        var speaker = dialogue?.transform;
        var node = NodeSelector.Find(dialogue);
        if (node == null)
        {
            Monitor.Log($"Empty node! speaker: {speaker?.name}", LL.Warning);
            return null!;
        }
        node.Reset();
        currentNode = node;
        currentConversation = new TextBoxConversation(speaker);
        if (node.onConversationFinish != null)
        {
            currentConversation.onConversationFinish += node.onConversationFinish;
        }
        StartDialogue();
        return currentConversation;
    }
    private void StartDialogue()
    {
        StopAllCoroutines();
        StartCoroutine(MainCoroutine());
    }
    internal int LastSelected { get; private set; } = -1;
    private IEnumerator MainCoroutine()
    {
        yield return new WaitUntil(() => characterObjectSetupDone);
        while (true)
        {
            var action = currentNode.NextAction();
            if (action is NodeCompleteAction end && end.End()) break;
            yield return action.Invoke(currentConversation);
            if (action is OptionAction option) LastSelected = option.selected;
        }
        currentConversation.Kill();
    }
}

