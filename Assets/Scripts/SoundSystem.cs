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

    static public void PlayBgm(AudioClip[] song, float loop, bool firstPlay)
    {
        for (int i = 0; i < 4; i++)
        {
            Instance.channels[i].PlayBgm(song[i]);
            if (!firstPlay)
                Instance.channels[i].src.time = loop;
        }

        if (loop > 0)
            Instance.StartCoroutine(Instance.LoopMusicToTime(song, loop));
    }

    
    IEnumerator LoopMusicToTime(AudioClip[] song, float loop)
    {
        yield return new WaitUntil(() => SoundSystem.songPositionTime >= song[0].length - .01F/* || !Instance.channels[0].src.isPlaying*/);
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
