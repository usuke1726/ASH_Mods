
using HarmonyLib;
using ModdingAPI;
using Sidequel.Dialogue;
using Sidequel.System;
using UnityEngine;

namespace Sidequel.NodeData;

internal class KidDeer : NodeEntry
{
    internal const string Start1 = "KidBoatDeer.Start1";
    internal const string Start2 = "KidBoatDeer.Start2";
    internal const string Start3 = "KidBoatDeer.Start3";
    internal const string Start4 = "KidBoatDeer.Start4";
    internal const string KidDeerShowedItemsTag = "KidDeerShowedItems";
    internal const string KidDeerShowedCountTag = "KidDeerShowedCount";
    internal const string ItemFound = "KidBoatDeer.ItemFound";
    internal const string Item_GoldMedal = "KidBoatDeer.GoldMedal";
    internal const string Item_SouvenirMedal = "KidBoatDeer.SouvenirMedal";
    internal const string Item_FishHook = "KidBoatDeer.FishHook";
    internal const string Item_AntiqueFigure = "KidBoatDeer.AntiqueFigure";
    internal const string Item_OldPicture = "KidBoatDeer.OldPicture";
    internal const string Item_CuteEmptyCan = "KidBoatDeer.CuteEmptyCan";
    internal const string Item_TradingCard = "KidBoatDeer.TradingCard";
    internal const string Item_FishScale1 = "KidBoatDeer.FishScale1";
    internal const string Item_FishScale2 = "KidBoatDeer.FishScale2";
    internal const string Item_FishScale3 = "KidBoatDeer.FishScale3";

    internal const int Start4Border = 5;
    private int showedCount;
    private readonly HashSet<string> showed = [];
    private static readonly List<string> showableItems = [
        Items.GoldMedal,
        Items.SouvenirMedal,
        Items.FishHook,
        Items.AntiqueFigure,
        Items.OldPicture,
        Items.CuteEmptyCan,
        Items.TradingCard,
        Items.FishScale1,
        Items.FishScale2,
        Items.FishScale3,
    ];
    protected override Characters? Character => Characters.KidBoatDeer2;
    private string showingItem = null!;
    protected override Node[] Nodes => [
        new(Start1, [
            lines(1, 13, digit2, [5, 9, 12], [
                new(10, emote(Emotes.Surprise, Original)),
                new(11, emote(Emotes.Normal, Original)),
            ]),
            done(),
        ], condition: () => NodeYet(Start1)),

        new(Start2, [
            lines(1, 6, digit2, [3, 4]),
            option(["O1", "O2"]),
            @if(() => LastSelected == 0,
                lines(1, 2, digit2("O1"), []),
                lines(1, 3, digit2("O2"), [1])
            ),
            lines(7, 12, digit2, [8, 9, 11], [
                new(10, emote(Emotes.Surprise, Original)),
                new(12, emote(Emotes.Happy, Original)),
            ]),
            done(),
        ], condition: () => NodeDone(Start1) && NodeYet(Start2)),

        new(Start3, [
            lines(1, 3, digit2, [2]),
        ], condition: () => NodeDone(Start2) && !CanStartAny && showedCount < Start4Border),

        new(Start4, [
            lines(1, 2, digit2, [2], [new(1, emote(Emotes.Happy, Original))]),
            cont(-10, condition: () => NodeYet(Start4)),
            done(),
        ], condition: () => NodeDone(Start2) && !CanStartAny && showedCount >= Start4Border),

        new(ItemFound, [
            command(() => OnStarted(showingItem)),
            next(() => $"KidBoatDeer.{showingItem}"),
            line("KidBoatDeer.ItemFound", Player, useId: false),
        ], condition: () => NodeDone(Start2) && CanStartAny),

        new(Item_GoldMedal, [
            lines(1, 12, digit2, [2, 3, 6, 7, 9, 12], [
                new(5, emote(Emotes.Happy, Original)),
                new(6, emote(Emotes.Normal, Original)),
                new(8, emote(Emotes.Surprise, Original)),
                new(10, emote(Emotes.Normal, Original)),
            ]),
        ], condition: () => false),
        new(Item_SouvenirMedal, [
            lines(1, 10, digit2, [2, 4, 5, 6, 9, 10]),
        ], condition: () => false),
        new(Item_FishHook, [
            lines(1, 3, digit2, [2]),
            @if(() => GetBool(Const.STags.HasBorrowedFishingRodOnce),
                lines(1, 6, digit2("BorrowedRodOnce"), [1, 3, 4, 6], [
                    new(4, emote(Emotes.EyesClosed, Player)),
                    new(6, emote(Emotes.Normal, Player)),
                ]),
                lines(1, 11, digit2("HasNotBorrowed"), [1, 3, 6, 7, 9, 11], [
                    new(9, emote(Emotes.Surprise, Player)),
                    new(11, emote(Emotes.Normal, Player)),
                ])
            ),
        ], condition: () => false),
        new(Item_AntiqueFigure, [
            lines(1, 7, digit2, [2, 3, 6, 7]),
        ], condition: () => false),
        new(Item_OldPicture, [
            lines(1, 5, digit2, [2, 5]),
            line(6, Player, condition: () => GetBool(Const.STags.HasShownOldPictureToJon)),
            lines(7, 10, digit2, [9]),
        ], condition: () => false),
        new(Item_CuteEmptyCan, [
            lines(1, 13, digit2, [2, 4, 6, 7, 10, 11, 13], [
                new(3, emote(Emotes.Happy, Original)),
                new(5, emote(Emotes.Normal, Original)),
                new(13, emote(Emotes.Surprise, Player)),
            ]),
        ], condition: () => false),
        new(Item_TradingCard, [
            lines(1, 12, digit2, [2, 4, 5, 7, 10, 11], [
                new(12, emote(Emotes.Happy, Original)),
            ]),
        ], condition: () => false),
        new(Item_FishScale1, [
            lines(1, 2, digit2, [2]),
            @if(() => !GetBool(Const.STags.HasShownFishScaleToKidDeer), lines(3, 5, digit2, [4])),
            tag(Const.STags.HasShownFishScaleToKidDeer, true),
            lines(6, 9, digit2, [2, 4, 6, 8, 9]),
        ], condition: () => false),
        new(Item_FishScale2, [
            command(() => OnStarted(Items.FishScale1)),
            lines(1, 2, digit2, [2]),
            @if(() => !GetBool(Const.STags.HasShownFishScaleToKidDeer), lines(3, 5, digit2, [4])),
            tag(Const.STags.HasShownFishScaleToKidDeer, true),
            lines(6, 7, digit2, [6]),
            @switch(() => {
                if(NodeDone(Const.Events.ScaleEvent)) return "eventDone";
                if(GetBool(Const.STags.HasShownFishScale2ToJen)) return "showedJen";
                return "other";
            }),
            lines(1, 5, digit2("ShowedJen"), [1, 2, 4, 5], anchor: "showedJen"),
            end(),
            lines(1, 4, digit2("ScaleEventDone"), [1, 2], anchor: "eventDone"),
            @if(() => !Items.Has(Items.FishScale3), "notHasScale3"),
            line("ScaleEventDone.Has3", Player),
            command(() => {
                fromScale2 = true;
                OnStarted(Items.FishScale3);
                SetNext(Item_FishScale3);
            }),
            end(),
            lines(5, 7, digit2("ScaleEventDone"), [5, 6], anchor: "notHasScale3"),
            end(),
            lines(1, 2, digit2("other"), [1, 2], anchor: "other"),
        ], condition: () => false),
        new(Item_FishScale3, [
            command(() => OnStarted(Items.FishScale1)),
            command(() => OnStarted(Items.FishScale2)),
            emote(Emotes.Happy, Original),
            line(1, Original),
            emote(Emotes.Normal, Original),
            line(2, Original, condition: () => !fromScale2),
            line(3, Player),
            @if(() => !GetBool(Const.STags.HasShownFishScaleToKidDeer), lines(4, 6, digit2, [5])),
            tag(Const.STags.HasShownFishScaleToKidDeer, true),
            @if(() => NodeDone(Const.Events.ScaleEvent), "eventDone"),
            lines(1, 3, digit2("other"), [1, 3], [
                new(2, emote(Emotes.Happy, Original)),
            ]),
            end(),
            anchor("eventDone"),
            lines(1, 4, digit2("ScaleEventDone"), [1, 2]),
            option(["ScaleEventDone.O1", "ScaleEventDone.O2"]),
            @if(() => LastSelected == 1, "accept"),
            lines(1, 3, digit2("ScaleEventDone.O1"), [3], [
                new(1, emote(Emotes.Surprise, Original)),
                new(2, emote(Emotes.Normal, Original)),
                new(3, emote(Emotes.Happy, Player)),
            ]),
            end(),
            anchor("accept"),
            lines(1, 4, digit2("ScaleEventDone.O2"), [2, 4], [
                new(1, emote(Emotes.Happy, Original)),
                new(3, item(Items.FishScale3, -1)),
            ]),
        ], condition: () => false),
    ];
    private bool fromScale2 = false;
    internal override void OnGameStarted()
    {
        ModdingAPI.Character.OnSetupDone(() =>
        {
            Ch(Characters.KidBoatDeer1).gameObject.SetActive(false);
            Ch(Characters.KidBoatDeer2).gameObject.SetActive(true);
            Ch(Characters.KidBoatDeer3).gameObject.SetActive(false);
            var ch = Ch(Characters.KidBoatDeer2);
            ch.transform.parent = GameObject.Find("NPCs").transform;
            ch.transform.position = new(143.6824f, 20.059f, 1311.036f);
            ch.transform.localRotation = Quaternion.Euler(342.9369f, 2.4441f, 3.4641f);
            Timer.Register(1f, () =>
            {
                var range = ch.transform.GetComponent<RangedInteractable>();
                range.range = 7f;
                Traverse.Create(range).Field("rangeSqr").SetValue(49f);
            });
            Sidequel.Character.Pose.Set(ch.transform, Poses.Sitting);
        });
        Load();
    }

    private bool CanStartAny => (showingItem = Find()) != null;
    private string Find()
    {
        var s = showableItems.Find(CanStart);
        if (s == Items.FishScale1)
        {
            if (CanStart(Items.FishScale3)) return Items.FishScale3;
            if (CanStart(Items.FishScale2)) return Items.FishScale2;
            return s;
        }
        else if (s == Items.FishScale2)
        {
            if (CanStart(Items.FishScale3) && NodeIP(Const.Events.ScaleEvent)) return Items.FishScale3;
            return s;
        }
        else return s;
    }
    private bool CanStart(string itemId) => Items.Has(itemId) && !showed.Contains(itemId);
    private void OnStarted(string itemId)
    {
        if (showed.Contains(itemId)) return;
        showed.Add(itemId);
        showedCount = showed.Count;
        Save();
    }
    private void Save() => STags.SetString(KidDeerShowedItemsTag, string.Join(",", showed));
    private void Load()
    {
        var items = STags.GetString(KidDeerShowedItemsTag);
        showedCount = 0;
        if (items == null) return;
        foreach (var item in items.Split(",")) showed.Add(item);
        showedCount = showed.Count;
    }
}

