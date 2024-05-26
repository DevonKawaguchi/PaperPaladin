using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OpeningCrawlCutscene : MonoBehaviour
{
    [SerializeField] GameObject dialogueBox;
    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] int lettersPerSecond;

    [SerializeField] AudioSource MusicPlayer;
    [SerializeField] AudioSource SFXPlayer;
    [SerializeField] AudioClip SelectSound;

    bool shownDialogue = false;
    bool dialogueComplete = false;

    [SerializeField] List<string> dialogueList;

    int dialogueCounter = 0;

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
    }

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
}
