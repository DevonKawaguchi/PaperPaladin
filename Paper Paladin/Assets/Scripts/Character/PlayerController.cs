using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class PlayerController : MonoBehaviour, ISavable
{
    [SerializeField] new string name;
    [SerializeField] Sprite sprite;

    private Vector2 input;

    private Character character;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    public void HandleUpdate()
    {
        if(!character.IsMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            if (input.x != 0) input.y = 0; //Removes diagonal movement

            if (input != Vector2.zero) //Allows player to remain in previous animation, e.g: will remain in "idle_left" animation state if last input was left - this is because the if statement only changes "MoveX"/"MoveY" float parameters if input is not zero.
            {
                StartCoroutine(character.Move(input, OnMoveOver));
            }
        }

        character.HandleUpdate();

        if (Input.GetKeyDown(KeyCode.Z))
        {
            StartCoroutine(Interact());
        }
    }

    IEnumerator Interact()
    {
        var facingDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var interactPos = transform.position + facingDir;

        //Debug.DrawLine(transform.position, interactPos, Color.green, 0.5f); //Draws a line in front of the player to troubleshoot interactPos direction (only viewable in Scene view)

        var collider = Physics2D.OverlapCircle(interactPos, 0.3f, GameLayers.i.InteractableLayer);
        if (collider != null)
        {
            yield return collider.GetComponent<Interactable>()?.Interact(transform); //"?" to ensure line doesn't crash the application in case it returns null
        }
    }

    IPlayerTriggerable currentlyInTrigger;

    private void OnMoveOver() //Wrapper (not actual name) function that allows multiple other functions to be run in "StartCoroutine(character.Move(input, OnMoveOver));" line
    {
        var colliders = Physics2D.OverlapCircleAll(transform.position - new Vector3(0, character.OffsetY), 0.2f, GameLayers.i.TriggerableLayers);

        IPlayerTriggerable triggerable = null;
        foreach (var collider in colliders)
        {
            triggerable = collider.GetComponent<IPlayerTriggerable>();
            if (triggerable != null)
            {
                if (triggerable == currentlyInTrigger && !triggerable.TriggerRepeatedly) //If move over same trigger and TriggerRepeatedly set to false, don't trigger it
                {
                    break;
                }

                //character.Animator.IsMoving = false;
                triggerable.OnPlayerTriggered(this);
                currentlyInTrigger = triggerable;
                break;
            }
        }

        if (colliders.Count() == 0 || triggerable != currentlyInTrigger) //
        {
            currentlyInTrigger = null;
        }
    }

    public object CaptureState() //Used to save data
    {
        var saveData = new PlayerSaveData()
        {
            position = new float[] { transform.position.x, transform.position.y },
            pokemons = GetComponent<PokemonParty>().Pokemons.Select(p => p.GetSaveData()).ToList() //Converts list of Pokemons to list of Pokemon's save data

        };

        return saveData; 
    }

    public void RestoreState(object state) //Used to restore the data while the game is loading
    {
        var saveData = (PlayerSaveData)state;

        //Restore Position
        var pos = saveData.position;
        transform.position = new Vector3(pos[0], pos[1]); //Restores Player's saved position

        //Restore Party
        GetComponent<PokemonParty>().Pokemons = saveData.pokemons.Select(s => new Pokemon(s)).ToList(); //Converts list of Pokemons to list of Pokemon's save data
    }

    public string Name
    {
        get => name;
    }

    public Sprite Sprite
    {
        get => sprite;
    }

    public Character Character => character;
}

[Serializable]
public class PlayerSaveData
{
    public float[] position;
    public List<PokemonSaveData> pokemons;
}

//Player Controller Position Note: Make sure x position value is in the format x.5 and y position value in the format x.8. y position value format can also be the default x.5, though x.8 is more visually appealing as it allows the player model to be slightly above the centre of the tiles