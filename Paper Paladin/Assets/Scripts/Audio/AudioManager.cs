using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

public class AudioManager : MonoBehaviour
{
    [SerializeField] List<AudioData> sfxList;

    [SerializeField] AudioSource musicPlayer;
    [SerializeField] AudioSource sfxPlayer;

    [SerializeField] float fadeDuration = 0.75f;

    AudioClip currMusic;

    float originalMusicVol;

    Dictionary<AudioID, AudioData> sfxLookup; //Data dictionary

    public static AudioManager i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    private void Start()
    {
        originalMusicVol = musicPlayer.volume;

        sfxLookup = sfxList.ToDictionary(x => x.id);
    }

    public void PlaySFX(AudioClip clip, bool pauseMusic = false)
    {
        if (clip == null) return;

        if (pauseMusic) //Pauses current audio when true
        {
            musicPlayer.Pause();
            StartCoroutine(UnPauseMusic(clip.length));
        }

        sfxPlayer.PlayOneShot(clip); //PlayOneShot means played audio won't cancel out any other audio being played

    }

    public void PlaySFX(AudioID audioID, bool pauseMusic = false)
    {
        if (!sfxLookup.ContainsKey(audioID)) return; //Fallback: Returns if no audioID is present in sfxLookup dictionary

        var audioData = sfxLookup[audioID];
        PlaySFX(audioData.clip, pauseMusic);
    }

    public void PlayMusic(AudioClip clip, bool loop = true, bool fade = false) //Plays audio clip passed in the parameters
    {
        if (clip == null || clip == currMusic) return; //Fallback: If clip parameter is null, do not try to make the below code play it. Furthermore, if "clip == currMusic" ensures music that is already playing will continue playing and won't reset if the player switches scenes with the same set music

        currMusic = clip;
        StartCoroutine(PlayMusicAsync(clip, loop, fade));
    }

    IEnumerator PlayMusicAsync(AudioClip clip, bool loop, bool fade)
    {
        if (fade) //Fades music between scenes
        {
            yield return musicPlayer.DOFade(0, fadeDuration).WaitForCompletion(); 
        }

        musicPlayer.clip = clip;
        musicPlayer.loop = loop;
        musicPlayer.Play();

        if (fade) //Fades music between scenes
        {
            yield return musicPlayer.DOFade(originalMusicVol, fadeDuration).WaitForCompletion();
        }
    }

    IEnumerator UnPauseMusic(float delay)
    {
        yield return new WaitForSeconds(delay);

        musicPlayer.volume = 0;
        musicPlayer.UnPause();
        musicPlayer.DOFade(originalMusicVol, fadeDuration);
    }
}


public enum AudioID { UISelect, Hit, Faint, ExpGain, ItemObtained, PokemonObtained, UISelectionMove, UIExit, UIError, Victory, BattleStart }

[System.Serializable]
public class AudioData
{
    public AudioID id;
    public AudioClip clip;
}

//Note: Use "AudioManager.i.PlaySFX(AudioID.ItemObtained, pauseMusic: true);" when want to play audio that decreases volume of background music when playing