
using HarmonyLib;
using ModdingAPI;
using UnityEngine;

namespace Sidequel.System;

internal class Music
{
    private static PlayMusicOnUpdraft updraftMusic = null!;
    private static bool isMuting = false;
    private static float ambienceVolume;
    private static float effectVolume;
    internal static void Setup(IModHelper helper)
    {
        helper.Events.Gameloop.GameStarted += (_, _) =>
        {
            if (!State.IsActive) return;
            var collider = GameObject.Find("Cutscenes").transform.Find("PeakCutscene/TopUpdraft/Collider");
            Assert(collider != null, "collider of TopUpdraft is null");
            updraftMusic = collider!.GetComponent<PlayMusicOnUpdraft>();
        };
        helper.Events.Gameloop.ReturnedToTitle += (_, _) => UnMute();
        Application.quitting += UnMute;
    }
    internal static void MuteExceptBGM()
    {
        if (isMuting) return;
        isMuting = true;
        ambienceVolume = Volume.GetVolume(Volume.Channel.Ambience);
        effectVolume = Volume.GetVolume(Volume.Channel.SoundEffects);
        Volume.SetVolume(Volume.Channel.Ambience, 0);
        Volume.SetVolume(Volume.Channel.SoundEffects, 0);
    }
    internal static void UnMute()
    {
        if (!isMuting) return;
        isMuting = false;
        Volume.SetVolume(Volume.Channel.Ambience, ambienceVolume);
        Volume.SetVolume(Volume.Channel.SoundEffects, effectVolume);
    }
    internal static void PlayUpdraftMusic()
    {
        var music = new ScriptedMusic(updraftMusic.music, updraftMusic.musicPriority);
        music.PlayOnce(updraftMusic.music.fadeOutTime);
    }
    internal static void FadeOutCurrentMusic(float? time = null)
    {
        var manager = Singleton<MusicManager>.instance;
        if (time != null)
        {
            var currentMusicSet = manager.currentMusicSet;
            if (currentMusicSet == null)
            {
                Debug($"BGM is inactive now");
                return;
            }
            var activeMusicSet = manager.GetActiveMusicSet(currentMusicSet);
            Assert(activeMusicSet != null, "activeMusicSet is null");
            var data = activeMusicSet!.musicSetData;
            Debug($"default fadeOutTime: {data.fadeOutTime} (name: {data.name})");
            var defaultFadeOutTime = data.fadeOutTime;
            data.fadeOutTime = (float)time;
            Timer.Register((float)time + 1f, () =>
            {
                data.fadeOutTime = defaultFadeOutTime;
            });
        }
        Debug("start to fadeout music");
        manager.UnregisterAll();
    }
    internal static void FadeOutCurrentMusic2(float? time = null) => FadeOutController.StartFadeOut(time);
    private class FadeOutController : MonoBehaviour
    {
        private static FadeOutController? instance = null;
        private ActiveMusicSet musicSet = null!;
        private static float? fadeOutTime = null;
        private float startTime;
        private float time;
        private Traverse tr = null!;
        internal static void StartFadeOut(float? time = null)
        {
            if (instance != null) return;
            fadeOutTime = time;
            instance = new GameObject("Sidequel_MusicFadeOutContrller").AddComponent<FadeOutController>();
            Debug("start to fadeout music (type 2)");
        }
        private void Awake()
        {
            var manager = Singleton<MusicManager>.instance;
            musicSet = manager.GetActiveMusicSet(manager.currentMusicSet);
            time = fadeOutTime ?? musicSet.musicSetData.fadeOutTime;
            startTime = Time.time;
            tr = Traverse.Create(musicSet).Field("volume");
        }
        private void Update()
        {
            var volume = Mathf.Clamp((startTime + time - Time.time) / time, 0, 1);
            tr.SetValue(volume);
            if (volume < 0.01f)
            {
                GameObject.Destroy(gameObject);
                instance = null;
                FadeOutCurrentMusic(1f);
            }
        }
    }
}

