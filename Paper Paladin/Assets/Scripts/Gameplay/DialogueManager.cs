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

    public bool IsShowing { get; private set; }

    public IEnumerator ShowDialogueText(string text, bool waitForInput = true)
    {
        IsShowing = true;
        dialogueBox.SetActive(true);

        yield return TypeDialogue(text);
        if (waitForInput)
        {
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Z));
        }

        dialogueBox.SetActive(false);
        IsShowing = false;
    }

    public IEnumerator ShowDialogue(Dialogue dialogue)
    {
        yield return new WaitForEndOfFrame();

        OnShowDialogue?.Invoke();
        IsShowing = true;
        dialogueBox.SetActive(true);

        foreach (var line in dialogue.Lines)
        {
            yield return TypeDialogue(line); //Shows first line of the dialogue
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Z));

        }

        dialogueBox.SetActive(false);
        IsShowing = false;
        OnCloseDialogue?.Invoke();

    }

    public void HandleUpdate()
    {

    }

    public IEnumerator TypeDialogue(string line) //Coroutine allows text to be shown one character at a time
    {
        dialogueText.text = "";
        foreach (var letter in line.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond); //Determines how fast letters are shown. For this, e.g 1f/30 (in lettersPerSecond variable) will add a character per every 30th of a second.
        }
    }
}
