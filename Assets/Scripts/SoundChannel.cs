using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundChannel : MonoBehaviour
{
    [HideInInspector]public AudioSource src;
    bool muteBgm = false;
    bool songPlaying = false;

    float bgmResumeTime = 0;
    float sfxTime = 0;

    Coroutine resumeBgm = null;

    AudioClip bgm;


    void Awake()
    {
        src = GetComponent<AudioSource>();
    }

    public void PlayBgm(AudioClip clip)
    {
        bgmResumeTime = 0;
        sfxTime = 0;

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
                sfxTime += src.time;
            else
            {
                sfxTime = 0;
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
        src.time = 0;
        src.Play();

        yield return new WaitUntil(() => !src.isPlaying);

        float resumeAt = SoundSystem.songPositionTime;    
        src.clip = bgm;
        src.loop = false;
        muteBgm = false;
        src.time = resumeAt;
        sfxTime = 0;
        src.Play();


        resumeBgm = null;
        yield break;
    }
}
