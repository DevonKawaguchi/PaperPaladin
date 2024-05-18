using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour, Interactable //", Interactable" states that the class implements the Interactable interface
{
    [SerializeField] Dialogue dialogue;
    [SerializeField] QuestBase questToStart;
    [SerializeField] List<Vector2> movementPattern;
    [SerializeField] float timeBetweenPattern;

    NPCState state;
    float idleTimer = 0f;
    int currentPattern = 0;
    Quest activeQuest;

    Character character;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    public IEnumerator Interact(Transform initiator) //Recieves transform of the game object that initiated the action
    {
        if (state == NPCState.Idle)
        {
            state = NPCState.Dialogue;
            character.LookTowards(initiator.position);

            if (questToStart != null) //Checks if player can initiate quest
            {
                activeQuest = new Quest(questToStart);
                yield return activeQuest.StartQuest();
                questToStart = null; //Quest is complete
            }
            //else if (activeQuest != null) //NOTE: Code logic incomplete. CanBeCompleted function not able to complete
            //{
            //    if (activeQuest.CanBeCompleted)
            //}
            else
            {
                yield return DialogueManager.Instance.ShowDialogue(dialogue);
            }

            idleTimer = 0f; //Added so script doesn't use any previous value
            state = NPCState.Idle; //Returns the NPC to Idle state after dialogue - Allows other NPCs to move around while Player is talking to another NPC
        }

        //StartCoroutine(character.Move(new Vector2(-2, 0))); //Test: If player interacts with NPC, the NPC will walk upwards by 2 tiles

        //Debug.Log("Interacting with NPC...");
    }

    private void Update()
    {
        if (state == NPCState.Idle)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer > timeBetweenPattern)
            {
                idleTimer = 0f;
                if (movementPattern.Count > 0)
                {
                    StartCoroutine(Walk());
                }
            }
        }
        character.HandleUpdate();
    }

    IEnumerator Walk()
    {
        state = NPCState.Walking;

        var oldPos = transform.position;

        yield return character.Move(movementPattern[currentPattern]);

        if (transform.position != oldPos) //Makes it so that the NPC will wait for Player to move if they're in the NPC's way by repeating idleTimer
        {
            currentPattern = (currentPattern + 1) % movementPattern.Count; //Add 1 to currentPattern, divide by the amount of items in the movementPattern array, and then define currentPattern as its remainder. E.g: currentPattern = 1 becomes 1. This is designed so that if currentPattern = 4, it repeats by becoming 0 since the remainder of 4 / 4 is 0.
        }

        state = NPCState.Idle;
    }
}

public enum NPCState { Idle, Walking, Dialogue }
