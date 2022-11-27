using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundSystem : MonoBehaviour
{
    static public SoundSystem Instance;
    static public int songPositionSamples => Instance.channels[0] != null ? Instance.channels[0].src.timeSamples : 0;
    public AudioClip[] defaultSong = new AudioClip[4];
    public SoundChannel[] channels;
    
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
            PlayBgm(defaultSong, true);
    }

    static public void PlayBgm(AudioClip[] song, bool loop)
    {
        for (int i = 0; i < 4; i++)
            Instance.channels[i].PlayBgm(song[i], loop);
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
