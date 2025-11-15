
using ModdingAPI;
using Sidequel.Dialogue;
using Sidequel.System;
using UnityEngine;

namespace Sidequel.NodeData;

internal class CoinReached : NodeEntry
{
    protected override Characters? Character => null;
    protected override Node[] Nodes => [];
    private static Characters? talkingCharacter = null;
    internal static bool IsActive { get; private set; } = false;
    internal const string Id = "MoneySavedUp";
    internal static Node node = new(Id, [
        command(() => IsActive = false),
        wait(1f),
        cont(-10),
        lines(1, 3, digit2, Player),
        wait(1f),
        line(4, Player),
        item(Items.Coin, -400),
        lines(5, 6, digit2, Player),
        @if(() => Cont.IsEndingCont, "ending"),
        lines(1, 3, digit2("NotEndingCont"), Player),
        end(),
        anchor("ending"),
        lines(1, 3, digit2("EndingCont"), Player),
        lineif(() => GetBool(Const.STags.HasClimbedPeakOnce), "EndingCont.HasClimbedOnce.04", "EndingCont.HasNotClimbed.04", Player),
    ]);
    internal static void OnReached()
    {
        if (Items.CoinsSavedUp) return;
        STags.SetBool(Const.STags.CoinsSavedUp, true);
        IsActive = true;
    }
    internal static void SetSpeaker(Transform? speaker)
    {
        if (speaker != null && ModdingAPI.Character.TryGet(speaker, out var character))
        {
            talkingCharacter = character.character;
        }
    }
}

