using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundChannel : MonoBehaviour
{
    [HideInInspector]public AudioSource src;
    bool muteBgm = false;
    bool songPlaying = false;

    int bgmResumeSample = 0;
    int sfxSampleTime = 0;

    Coroutine resumeBgm = null;

    AudioClip bgm;


    void Awake()
    {
        src = GetComponent<AudioSource>();
    }

    public void PlayBgm(AudioClip clip)
    {
        return;
        bgmResumeSample = 0;
        sfxSampleTime = 0;

        muteBgm = false;
        bgm = src.clip = clip;
        src.loop = false;
        songPlaying = true;

        src.Play();
    }

    public void PlaySfx(AudioClip sfxClip)
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

            if (resumeBgm != null)
            {
                StopCoroutine(resumeBgm);
            }

            resumeBgm = StartCoroutine(ResumeBgm(sfxClip));
        }
        else
        {
            src.PlayOneShot(sfxClip);
        }
    }

    IEnumerator ResumeBgm(AudioClip sfx)
    {
        muteBgm = true;
        src.loop = false;
        src.clip = sfx;
        src.timeSamples = 0;
        src.Play();

        yield return new WaitUntil(() => !src.isPlaying);

        int resumeAtSamples = SoundSystem.songPositionSamples;    
        src.clip = bgm;
        src.loop = false;
        muteBgm = false;
        src.timeSamples = resumeAtSamples;
        sfxSampleTime = 0;
        src.Play();


        resumeBgm = null;
        yield break;
    }
}
