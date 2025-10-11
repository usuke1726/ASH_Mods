
using ModdingAPI;
using UnityEngine;

namespace Sidequel.System;

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

