using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] GameObject dialogueBox;
    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] int lettersPerSecond;

    public event Action OnShowDialogue;
    public event Action OnCloseDialogue;

    public static DialogueManager Instance { get; private set; } //"private set" means the instance can only be set from inside this class

    private void Awake()
    {
        Instance = this;
    }

    Dialogue dialogue;
    int currentLine = 0;
    bool isTyping;

    public bool IsShowing { get; private set; }

    public IEnumerator ShowDialogue(Dialogue dialogue)
    {
        yield return new WaitForEndOfFrame();

        OnShowDialogue?.Invoke();

        IsShowing = true;

        this.dialogue = dialogue;
        dialogueBox.SetActive(true);
        StartCoroutine(TypeDialogue(dialogue.Lines[0])); //Shows first line of the dialogue

    }

    public void HandleUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Z) && !isTyping)
        {
            ++currentLine;
            if (currentLine < dialogue.Lines.Count) //Technically a while loop
            {
                StartCoroutine(TypeDialogue(dialogue.Lines[currentLine])); //Displays next line
            }
            else
            {
                currentLine = 0;

                IsShowing = false;

                dialogueBox.SetActive(false); //When no lines left in dialogue, disable dialogue box
                OnCloseDialogue?.Invoke();
            }
        }
    }

    public IEnumerator TypeDialogue(string line) //Coroutine allows text to be shown one character at a time
    {
        isTyping = true;

        dialogueText.text = "";
        foreach (var letter in line.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond); //Determines how fast letters are shown. For this, e.g 1f/30 (in lettersPerSecond variable) will add a character per every 30th of a second.
        }
        isTyping = false;
    }
}
