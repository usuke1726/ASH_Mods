
using System.Collections;
using System.Reflection;
using HarmonyLib;
using QuickUnityTools.Input;
using UnityEngine;

namespace CustomBoatRace;

internal static class CustomBoatRace
{
    public static bool Enabled { get => course != null; }
    private static BoatRaceCourse? course = null;
    private static bool firstCall = true;
    private static List<BoatRaceCheckpoint> defaultCheckpoints = [];
    private static List<BoatRaceCheckpoint> prevCheckpoints = [];
    internal static void Reset()
    {
        course = null;
        firstCall = true;
        defaultCheckpoints.Clear();
        prevCheckpoints.Clear();
    }
    public static void SetCourse(string id, BoatScripting __instance)
    {
        var TR = Traverse.Create(__instance);
        if (firstCall)
        {
            firstCall = false;
            var checkpoints = TR.Field("checkpoints").GetValue<List<BoatRaceCheckpoint>>();
            defaultCheckpoints = [.. checkpoints];
        }
        if (course != null && course.id == id) return;
        else if (course == null && id == BoatRaceCourse.VanillaCourseId) return;
        for (int i = 0; i < prevCheckpoints.Count; i++)
        {
            GameObject.Destroy(prevCheckpoints[i].transform.parent.gameObject);
        }
        prevCheckpoints.Clear();
        List<List<Renderer>> checkpointRenderers = [];
        if (id == BoatRaceCourse.VanillaCourseId)
        {
            foreach (var checkpoint in defaultCheckpoints)
            {
                checkpoint.transform.parent.gameObject.SetActive(true);
                checkpoint.gameObject.SetActive(true);
                List<Renderer> list = [];
                foreach (Transform child in checkpoint.transform.parent.GetChildren())
                {
                    if (child != checkpoint.transform)
                    {
                        var renderer = child.GetComponent<Renderer>();
                        if ((bool)renderer) list.Add(renderer);
                    }
                }
                checkpointRenderers.Add(list);
            }
            TR.Field("checkpoints").SetValue(defaultCheckpoints);
            TR.Field("checkpointRenderers").SetValue(checkpointRenderers);
            course = null;
            return;
        }
        if (!BoatRaceCourse.TryToGet(id, out var r)) return;
        course = r;
        var parent = __instance.checkpointParent;
        var comp = defaultCheckpoints[0];
        comp.gameObject.SetActive(true);
        for (int i = 0; i < course.data.Count; i++)
        {
            var section = comp.transform.parent.gameObject.Clone();
            var objectName = $"CustomCheckpoint ({i + 1})";
            section.gameObject.name = objectName;
            var transform = section.transform;
            transform.position = transform.position
                .SetX(course.data[i].Item1)
                .SetZ(course.data[i].Item2);
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, course.data[i].Item3, transform.rotation.eulerAngles.z);
            transform.parent = parent;
            var checkpoint = section.GetComponentInChildren<BoatRaceCheckpoint>();
            checkpoint.transform.position = transform.position;
            Traverse.Create(checkpoint).Field("startPos").SetValue(transform.position);
            prevCheckpoints.Add(checkpoint);
            List<Renderer> list = [];
            foreach (Transform child in checkpoint.transform.parent.GetChildren())
            {
                if (child != checkpoint.transform)
                {
                    var renderer = child.GetComponent<Renderer>();
                    if ((bool)renderer) list.Add(renderer);
                }
            }
            checkpointRenderers.Add(list);
        }
        foreach (var checkpoint in defaultCheckpoints) checkpoint.transform.parent.gameObject.SetActive(false);
        TR.Field("checkpoints").SetValue(prevCheckpoints);
        TR.Field("checkpointRenderers").SetValue(checkpointRenderers);
    }

    internal static IEnumerator ChallengeCoroutine(BoatScripting __instance)
    {
        var type = __instance.GetType();
        var TR = Traverse.Create(__instance);
        GameObject inputLock = GameUserInput.CreateInputGameObjectWithPriority(10);
        bool transitionDone = false;
        Singleton<GameServiceLocator>.instance.transitionAnimation.Begin(delegate
        {
            __instance.boat.Interact();
            __instance.boat.Place(__instance.raceStart.position, __instance.raceStart.rotation);
            type.GetMethod("ResetRaceObjects", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, null);
        }, delegate
        {
            transitionDone = true;
        });
        yield return (object)new WaitUntil((Func<bool>)(() => transitionDone));
        yield return (object)new WaitForSeconds(0.5f);
        GameObject.Destroy(inputLock);
        float bestRaceTime = Singleton<GlobalData>.instance.gameData.tags.GetFloat(__instance.boatRaceBestTimeTag);

        bestRaceTime = course!.BestTime;

        Singleton<GlobalData>.instance.gameData.tags.SetString(__instance.boatRaceBestTimeTextTag, ConvertRaceTimeToDialogueString(bestRaceTime));
        DialogueController dialogue = Singleton<GameServiceLocator>.instance.dialogue;
        IConversation conversation = dialogue.StartConversation(__instance.beforeRaceStartYarnNode, ((Component)__instance.boatKidAnimator).transform.parent);
        yield return (object)new WaitUntil((Func<bool>)(() => !conversation.isAlive));
        StackResourceSortingKey silenceKey = Singleton<MusicManager>.instance.RegisterSilenece(50);
        Singleton<MusicManager>.instance.TrimRetiredActiveMusicSets(3f);
        var music = new ScriptedMusic(__instance.raceMusic);
        TR.Field("music").SetValue(music);
        music.Load();
        var timerUI = Singleton<GameServiceLocator>.instance.ui.AddUI(__instance.timerUIPrefab.Clone()).GetComponent<TimerUI>();
        TR.Field("timerUI").SetValue(timerUI);
        TR.Field("penalties").SetValue(0);
        inputLock = GameUserInput.CreateInputGameObjectWithPriority(10);
        yield return new WaitForSeconds(0.5f);
        var countdown = (IEnumerator)(type.GetMethod("Countdown", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, null));
        yield return countdown;
        GameObject.Destroy(inputLock);
        BoatCameraTarget cameraTargetFocuser = __instance.gameObject.GetComponent<BoatCameraTarget>();
        if (cameraTargetFocuser == null)
        {
            cameraTargetFocuser = __instance.gameObject.AddComponent<BoatCameraTarget>().Setup(__instance);
        }
        else
        {
            cameraTargetFocuser.Revive();
        }
        silenceKey.ReleaseResource();
        music.Play();
        timerUI.Begin();
        RaceGoalUI goal = Singleton<GameServiceLocator>.instance.ui.AddUI(__instance.goalUIPrefab.Clone()).GetComponent<RaceGoalUI>();
        goal.disappearOnScreen = false;
        goal.offset = Vector2.up * 25f;
        var boatArrow = Singleton<GameServiceLocator>.instance.ui.AddUI(__instance.boatArrowUIPrefab.Clone()).GetComponent<BoatGoalArrowUI>();
        boatArrow.destinationOffset = Vector2.up * 25f;
        TR.Field("boatArrow").SetValue(boatArrow);
        Singleton<GlobalData>.instance.gameData.tags.SetBool(__instance.boatRaceActiveTag);
        Singleton<GlobalData>.instance.gameData.tags.SetBool(__instance.ranAwayFromRaceTag, value: false);
        var checkpoints = TR.Field("checkpoints").GetValue<List<BoatRaceCheckpoint>>();
        Action cleanUpRace = delegate
        {
            GameObject.Destroy(goal.gameObject);
            GameObject.Destroy(boatArrow.gameObject);
            GameObject.Destroy(timerUI.gameObject);
            music.Stop();
            Singleton<GlobalData>.instance.gameData.tags.SetBool(__instance.boatRaceActiveTag, value: false);
            foreach (BoatRaceCheckpoint checkpoint2 in checkpoints)
            {
                checkpoint2.Finished();
            }
            cameraTargetFocuser.Kill();
        };
        for (int i = 0; i < checkpoints.Count; i++)
        {
            BoatRaceCheckpoint checkpoint = checkpoints[i];
            checkpoint.Awaken();
            cameraTargetFocuser.GetType().GetMethod("SetCheckpoint", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(cameraTargetFocuser, new object?[] { checkpoint.transform, (i < checkpoints.Count - 1) ? ((Component)checkpoints[i + 1]).transform : null });
            goal.destination = checkpoint.transform;
            boatArrow.destination = checkpoint.transform;
            var waitForRaceNode = new BoatScripting.WaitForRaceNode(__instance, checkpoint.transform);
            TR.Field("waitForRaceNode").SetValue(waitForRaceNode);
            yield return waitForRaceNode;
            checkpoint.Finished();
            switch (waitForRaceNode.continueRaceResult)
            {
                case BoatScripting.WaitForRaceNode.WaitResult.Continue:
                    __instance.checkpointSound.Play();
                    break;
                case BoatScripting.WaitForRaceNode.WaitResult.Cancel:
                    cleanUpRace();
                    yield break;
                case BoatScripting.WaitForRaceNode.WaitResult.Reset:
                    cleanUpRace();
                    __instance.StartChallenge();
                    yield break;
            }
        }
        __instance.winnerSound.Play();
        var player = TR.Field("player").GetValue<Player>();
        __instance.confettiPrefab.CloneAt(player.transform.position).transform.parent = __instance.boat.transform;
        float time = timerUI.time;
        bool flag = time < bestRaceTime;
        float? defaultRaceBestTime = null;
        if (flag)
        {
            course.SendTime(time);
            defaultRaceBestTime = Singleton<GlobalData>.instance.gameData.tags.GetFloat(__instance.boatRaceBestTimeTag);
            Singleton<GlobalData>.instance.gameData.tags.SetFloat(__instance.boatRaceBestTimeTag, time);
            Singleton<GameServiceLocator>.instance.achievements.SetLeaderboard("BoatChallenge", Mathf.RoundToInt(time * 1000f));
        }
        Singleton<GlobalData>.instance.gameData.tags.SetBool(__instance.boatRaceWonTag, flag);
        Singleton<GlobalData>.instance.gameData.tags.SetString(__instance.boatRaceTimeTextTag, ConvertRaceTimeToDialogueString(time));
        var penalties = TR.Field("penalties").GetValue<int>();
        Singleton<GlobalData>.instance.gameData.tags.SetFloat(__instance.boatRacePenaltiesTag, penalties);
        cleanUpRace();
        inputLock = GameUserInput.CreateInputGameObjectWithPriority(10);
        float originalDrag = __instance.boat.body.drag;
        __instance.boat.body.drag = 2f;
        yield return (object)new WaitForSeconds(1.5f);
        __instance.boat.body.drag = originalDrag;
        GameObject.Destroy(inputLock);
        __instance.boatKidAnimator.lookAt = ((Component)Singleton<GameServiceLocator>.instance.levelController.player).transform;
        conversation = dialogue.StartConversation(__instance.finishedRaceYarnNode, __instance.boatKidAnimator.transform.parent);
        yield return (object)new WaitUntil((Func<bool>)(() => !conversation.isAlive));
        __instance.boatKidAnimator.lookAt = null;
        if (defaultRaceBestTime != null)
        {
            Singleton<GlobalData>.instance.gameData.tags.SetFloat(__instance.boatRaceBestTimeTag, (float)defaultRaceBestTime);
        }
    }

    public static string ConvertRaceTimeToDialogueString(float seconds)
    {
        return seconds.ToString("0.00");
    }
}

