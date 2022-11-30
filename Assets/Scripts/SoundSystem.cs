using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundSystem : MonoBehaviour
{
    static public SoundSystem Instance;
    static public int songPositionSamples => Instance.channels[0] != null ? Instance.channels[0].src.timeSamples : 0;
    public AudioClip[] defaultSong = new AudioClip[4];
    public int defaultSongLoopPoint = 0;    // in samples
    [HideInInspector]public SoundChannel[] channels;
    
    void Awake()
    {
        Instance = this;
        channels = GetComponentsInChildren<SoundChannel>();
        for (int i = 0; i < 4; i++)
            channels[i].gameObject.name = "Channel " + (i + 1);
    }

    void Start()
    {
        if (defaultSong[0] != null)
            PlayBgm(defaultSong, defaultSongLoopPoint, true);
    }

    static public void PlayBgm(AudioClip[] song, int loop, bool firstPlay)
    {
        for (int i = 0; i < 4; i++)
        {
            Instance.channels[i].PlayBgm(song[i]);
            if (!firstPlay)
                Instance.channels[i].src.timeSamples = loop;
        }

        if (loop > 0)
            Instance.StartCoroutine(Instance.LoopMusicToSample(song, loop));
    }

    
    IEnumerator LoopMusicToSample(AudioClip[] song, int loop)
    {
        yield return new WaitUntil(() => SoundSystem.songPositionSamples >= song[0].samples - 1000/* || !Instance.channels[0].src.isPlaying*/);
        for (int i = 0; i < 4; i++)
            PlayBgm(song, loop, false);
        yield break;
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
