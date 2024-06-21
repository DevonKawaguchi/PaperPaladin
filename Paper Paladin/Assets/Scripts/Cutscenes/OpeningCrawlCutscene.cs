using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class OpeningCrawlCutscene : MonoBehaviour
{
    [SerializeField] GameObject dialogueBox;
    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] int lettersPerSecond;

    [SerializeField] AudioSource MusicPlayer;
    [SerializeField] AudioSource SFXPlayer;
    [SerializeField] AudioClip SelectSound;

    AudioClip currMusic;

    [SerializeField] AudioClip OpeningCrawlP1Music;
    [SerializeField] AudioClip OpeningCrawlP2Music;
    [SerializeField] AudioClip OpeningCrawlP3Music;
    [SerializeField] AudioClip OpeningCrawlP4Music;
    [SerializeField] AudioClip OpeningCrawlP5Music;

    [SerializeField] Image blackImage;

    bool shownDialogue = false;
    bool dialogueComplete = false;

    [SerializeField] List<string> dialogueList;

    int dialogueCounter = 0;

    private void Awake()
    {
        StartCoroutine(CutsceneFadeIn());
        PlayMusic(OpeningCrawlP1Music);
    }

    private void Update()
    {
        if (shownDialogue == false)
        {
            StartCoroutine(TypeDialogue(dialogueList[dialogueCounter]));
            shownDialogue = true;
        }

        if (Input.GetKeyDown(KeyCode.Z) && dialogueComplete == true)
        {
            if (dialogueCounter < dialogueList.Count - 1)
            {
                SFXPlayer.PlayOneShot(SelectSound);

                dialogueCounter += 1;
                dialogueComplete = false;
                shownDialogue = false;
            }
            else
            {
                SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1); //Plays next scene by finding scene that's +1 build setting index from the current scene the script is in
            }
        }

        if (dialogueCounter == 2)
        {
            PlayMusic(OpeningCrawlP2Music);
        }
        if (dialogueCounter == 4)
        {
            PlayMusic(OpeningCrawlP3Music);
        }
        if (dialogueCounter == 6)
        {
            PlayMusic(OpeningCrawlP4Music);
        }
        if (dialogueCounter == 8)
        {
            PlayMusic(OpeningCrawlP5Music);
        }

    }

    public void PlayMusic(AudioClip clip) //Plays audio clip passed in the parameters
    {
        if (clip == null || clip == currMusic) return; //Fallback: If clip parameter is null, do not try to make the below code play it. Furthermore, if "clip == currMusic" ensures music that is already playing will continue playing and won't reset if the player switches scenes with the same set music

        currMusic = clip;
        MusicPlayer.clip = clip;
        MusicPlayer.Play();
    }



    //public IEnumerator PlayMusic()
    //{
    //    if (dialogueCounter == 1)
    //    {
    //        yield return MusicPlayer.Play(OpeningCrawlP1Music);
    //    }
    //    if (dialogueCounter == 2)
    //    {
    //        MusicPlayer.PlayOneShot(OpeningCrawlP2Music);
    //    }
    //    if (dialogueCounter == 3)
    //    {
    //        MusicPlayer.PlayOneShot(OpeningCrawlP3Music);
    //    }
    //    if (dialogueCounter == 4)
    //    {
    //        MusicPlayer.PlayOneShot(OpeningCrawlP4Music);
    //    }
    //    if (dialogueCounter == 5)
    //    {
    //        MusicPlayer.PlayOneShot(OpeningCrawlP5Music);
    //    }
    //}

    public IEnumerator TypeDialogue(string line) //Coroutine allows text to be shown one character at a time
    {
        dialogueText.text = "";
        foreach (var letter in line.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond); //Determines how fast letters are shown. For this, e.g 1f/30 (in lettersPerSecond variable) will add a character per every 30th of a second.
        }
        dialogueComplete = true;
    }

    IEnumerator CutsceneFadeIn()
    {
        yield return blackImage.DOFade(0, 2.5f).WaitForCompletion();
    }
}
