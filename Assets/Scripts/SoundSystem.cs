using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundSystem : MonoBehaviour
{
    static public SoundSystem Instance;
    static public float songPositionTime => Instance.channels[0] != null ? Instance.channels[0].src.time : 0;
    public AudioClip[] defaultSong = new AudioClip[4];
    public float defaultSongLoopPoint = 0;    // in seconds
    [HideInInspector]public SoundChannel[] channels;

    bool bgmMode = false;
    
    void Awake()
    {
        Instance = this;
        channels = GetComponentsInChildren<SoundChannel>();
        for (int i = 0; i < 4; i++)
            channels[i].gameObject.name = "Channel " + (i + 1);
    }

    void Start()
    {
        /*
        if (defaultSong != null)
            PlayBgm(defaultSong, defaultSongLoopPoint, true);*/
    }

    static public void PlayBgm(AudioClip[] song, float loop, bool firstPlay)
    {
        if (song == null || song.Length < 4)   return;


        for (int i = 0; i < 4; i++)
        {
            Instance.channels[i].PlayBgm(song[i]);
            if (!firstPlay)
                Instance.channels[i].src.time = loop;
        }

        if (loop > 0)
            Instance.StartCoroutine(Instance.LoopMusicToTime(song, loop));
        Instance.bgmMode = true;
    }

    static public void DoFade(float volChange)
    {
        Instance.StartCoroutine(Instance.FadeOut(1.0F - volChange));
    }

    IEnumerator FadeOut(float volChange)
    {
        if (!bgmMode)
            yield break;
        
        while (bgmMode && channels[0].src.volume > .1)
        {
            foreach (SoundChannel channel in Instance.channels) 
            {
                channel.src.volume *= volChange;
            }
            yield return new WaitForSeconds(.8F);
        }

        foreach (SoundChannel channel in Instance.channels) 
        {
            channel.src.Stop();
            channel.src.volume = .5F;
        }

        bgmMode = false;

        yield break;
    }

    
    IEnumerator LoopMusicToTime(AudioClip[] song, float loop)
    {
        yield return new WaitUntil(() => SoundSystem.songPositionTime >= song[0].length - .01F || (!Instance.channels[0].src.isPlaying && bgmMode));
        for (int i = 0; i < 4; i++)
            PlayBgm(song, loop, false);
        yield break;
    }

    static public void Pause()
    {
        foreach (SoundChannel channel in Instance.channels)
            channel.src.Pause();
    }

    static public void Unpause()
    {
        foreach (SoundChannel channel in Instance.channels)
            channel.src.UnPause();
    }

    static public void PlaySfx(AudioClip sound, int channelIndex)
    {
        if (channelIndex <= 1 || channelIndex > 4)
        {
            Debug.LogError("Channel " + channelIndex + "is either not indexed or is not permitted for sound effect usage");
            return;
        }
        Instance.channels[channelIndex - 1].PlaySfx(sound);
    }
}
