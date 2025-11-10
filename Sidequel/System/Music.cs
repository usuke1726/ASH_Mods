
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
            data.fadeOutTime = (float)time;
        }
        Debug("start to fadeout music");
        manager.UnregisterAll();
    }
}

