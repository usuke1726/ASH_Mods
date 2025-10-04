
using System.Collections;
using ModdingAPI;
using SideStory.Dialogue.Actions;
using UnityEngine;

namespace SideStory.Dialogue;

internal class DialogueController : MonoBehaviour
{
    internal static DialogueController instance = null!;
    internal static void Setup(IModHelper helper)
    {
        helper.Events.Gameloop.GameStarted += (_, _) =>
        {
            instance = new GameObject("SideStoryDialogueController").AddComponent<DialogueController>();
        };
    }

    private TextBoxConversation currentConversation = null!;
    private Node currentNode = null!;
    internal IConversation StartConversation(Transform speaker)
    {
        var node = NodeSelector.Find(speaker);
        if (node == null)
        {
            Monitor.Log($"Empty node! speaker: {speaker.name}", LL.Warning);
            return null!;
        }
        node.Reset();
        currentNode = node;
        currentConversation = new TextBoxConversation(speaker);
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
        while (true)
        {
            var action = currentNode.NextAction();
            if (action is NodeCompleteAction) break;
            yield return action.Invoke(currentConversation);
            if (action is OptionAction option) LastSelected = option.selected;
        }
        currentConversation.Kill();
    }
}

