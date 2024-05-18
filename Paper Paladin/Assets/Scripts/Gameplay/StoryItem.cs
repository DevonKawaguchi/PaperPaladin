using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryItem : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] Dialogue dialogue;

    public void OnPlayerTriggered(PlayerController player)
    {
        //Debug.Log("Story item is working");
        player.Character.Animator.IsMoving = false; //Stops player from playing animations when in dialogue
        StartCoroutine(DialogueManager.Instance.ShowDialogue(dialogue));
    }

    public bool TriggerRepeatedly => false;
}
