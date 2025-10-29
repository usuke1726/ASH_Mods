
using System.Collections;
using ModdingAPI;
using QuickUnityTools.Input;
using Sidequel.Dialogue.Actions;
using UnityEngine;

namespace Sidequel.Dialogue;

internal class DialogueController : MonoBehaviour
{
    internal static DialogueController instance = null!;
    private static bool characterObjectSetupDone = false;
    private static Action? forceKillCurrentDialogue = null;
    internal static void Setup(IModHelper helper)
    {
        helper.Events.Gameloop.GameStarted += (_, _) =>
        {
            ModdingAPI.Character.OnSetupDone(() => characterObjectSetupDone = true);
            instance = new GameObject("SidequelDialogueController").AddComponent<DialogueController>();
            if (State.IsNewGame) instance.StartConversation(null);
        };
        helper.Events.Gameloop.ReturnedToTitle += (_, _) => characterObjectSetupDone = false;
    }

    private TextBoxConversation currentConversation = null!;
    private Node currentNode = null!;
    private Node? nextNode = null;
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
        BaseAction.OnNodeStarted(node);
        currentConversation = new TextBoxConversation(speaker);
        if (node.onConversationFinish != null)
        {
            currentConversation.onConversationFinish += node.onConversationFinish;
        }
        if (NodeData.NewGame.ShouldNewGameNodeStart()) OnNewGameNode();
        StartDialogue();
        return currentConversation;
    }
    internal void SetNext(string nodeId, Characters? character)
    {
        if (currentConversation == null || !currentConversation.isAlive) return;
        nextNode = NodeSelector.Find(nodeId, character);
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
        forceKillCurrentDialogue = () =>
        {
            StopAllCoroutines();
            currentConversation.Kill();
        };
        while (true)
        {
            var action = currentNode.NextAction();
            if (action is NodeCompleteAction end && end.End())
            {
                if (nextNode == null) break;
                if (currentNode.onConversationFinish != null)
                {
                    currentConversation.onConversationFinish -= currentNode.onConversationFinish;
                }
                currentNode = nextNode;
                BaseAction.OnNodeStarted(currentNode);
                if (currentNode.onConversationFinish != null)
                {
                    currentConversation.onConversationFinish += currentNode.onConversationFinish;
                }
                continue;
            }
            yield return action.Invoke(currentConversation);
            if (action is OptionAction option) LastSelected = option.selected;
        }
        forceKillCurrentDialogue = null;
        currentConversation.Kill();
    }

    private void OnNewGameNode()
    {
        var obj = new GameObject("SidequelDialogueControllerInputWatcher");
        obj.AddComponent<InputWatcher>();
        currentConversation.onConversationFinish += () => GameObject.Destroy(obj);
    }
    private class InputWatcher : MonoBehaviour
    {
        private void Update()
        {
            if (Activated())
            {
                Debug($"killing newGameNode");
                forceKillCurrentDialogue?.Invoke();
            }
        }
        private bool Activated()
        {
            var f1 = (
                UnityEngine.Input.GetKey(KeyCode.F1) ||
                UnityEngine.Input.GetKey(KeyCode.Alpha1)
            ) && UnityEngine.Input.GetKey(KeyCode.Backspace)
                && UnityEngine.Input.GetKeyDown(KeyCode.Escape);
            if (f1) return true;
            var control = GameUserInput.sharedActionSet;
            var f2 = (
                control.button1.IsPressed // JumpButton
                && control.button2.IsPressed // UseItemButton
                && control.button3.IsPressed // RunButton
            //&& control.button4.IsPressed // MenuButton
            );
            if (f2) return true;
            return false;
        }
    }
}

