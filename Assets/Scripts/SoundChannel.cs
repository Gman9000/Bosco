using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundChannel : MonoBehaviour
{
    AudioSource src;
    bool muteBgm = false;
    bool loopBgm = false;
    bool songPlaying = false;

    int bgmResumeSample = 0;
    int sfxSampleTime = 0;


    void Awake()
    {
        src = GetComponent<AudioSource>();
    }

    public void PlayBgm(AudioClip clip, bool loop)
    {
        bgmResumeSample = 0;
        sfxSampleTime = 0;

        muteBgm = false;
        src.clip = clip;
        src.loop = loopBgm = loop;
        songPlaying = true;
        src.Play();
    }

    public void PlaySfx(AudioClip clip)
    {
        if (songPlaying)
        {
            if (muteBgm)
                sfxSampleTime += src.timeSamples;
            else
            {
                bgmResumeSample = src.timeSamples;
                sfxSampleTime = 0;
            }
            muteBgm = true;
            StartCoroutine(ResumeBgm(src.clip));
        }

        src.loop = false;
        src.clip = clip;
        src.Play();
    }

    IEnumerator ResumeBgm(AudioClip bgm)
    {
        yield return new WaitUntil(() => !src.isPlaying);

        int resumeAtSamples = bgmResumeSample + sfxSampleTime + src.clip.samples;
        src.clip = bgm;
        src.timeSamples = resumeAtSamples;
        src.loop = loopBgm;
        muteBgm = false;
        src.Play();

        sfxSampleTime = 0;
        yield break;
    }
}
