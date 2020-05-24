using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {

    public static SoundManager instance = null;          //чтобы остальные скрипты были в порядке
    public AudioSource musicSource;
    public AudioSource generalSoundSource;

    public bool engineIsOn;
    public bool turningOffEngine;
    public bool turningOnEnginge;


    // Инициализация
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            DestroyObject(gameObject);
        }

        PauseOrPlayMusic(false);
    }

    public void PlaySound(AudioSource soundSource, AudioClip sound, bool loop)
    {
        // проиграть звук
        soundSource.loop = loop;
        soundSource.PlayOneShot(sound);
    }

    public void StartOrStopEngine(AudioSource engineSource, bool start)
    {
        if (start)
        {
            engineIsOn = true;
            StartCoroutine(FadeIn(engineSource, 5f));                         
        }
        else
        {
            engineIsOn = false;
            StartCoroutine(FadeOut(engineSource, 2f));                                 
        }
    }

    public IEnumerator FadeIn(AudioSource audioSource, float FadeTime)
    {
        //немножечко увеличения звука движка
        turningOnEnginge = true;
       
        float startVolume = 0.1f;

        while (audioSource.volume < 1 && engineIsOn)
        {
            audioSource.volume += startVolume * Time.deltaTime * FadeTime;

            yield return null;
        }
        turningOnEnginge = false;
    }

    public IEnumerator FadeOut(AudioSource audioSource, float FadeTime)
    {   //немножечко затихания звука движка
        turningOffEngine = true;
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0 && !engineIsOn)
        {
            audioSource.volume -= startVolume * Time.deltaTime / FadeTime;

            yield return null;
        }
        turningOffEngine = false;
        //audioSource.Stop();
    }

    public void PauseOrPlayMusic(bool pause)
    {
        if (pause)
        {
            musicSource.Pause();
        }
        else
        {
            musicSource.Play();
        }

    }
}
