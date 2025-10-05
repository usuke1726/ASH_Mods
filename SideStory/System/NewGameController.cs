
using ModdingAPI;
using UnityEngine;

namespace SideStory.System;

internal class NewGameController
{
    public static void StartGame()
    {
        if (!Context.OnTitle) return;
        if (State.IsActive) return;
        if (!CrossPlatform.DoesSaveExist()) return;
        State.Activate();
        GameObject.FindObjectOfType<TitleScreen>().ContinueGame();
    }
}

