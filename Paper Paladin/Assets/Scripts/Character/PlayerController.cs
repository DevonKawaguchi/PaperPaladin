using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    private Vector2 input;

    private Character character;

    public event Action OnEnountered;

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
                StartCoroutine(character.Move(input, CheckForEncounters));
            }
        }

        character.HandleUpdate();

        if (Input.GetKeyDown(KeyCode.Z))
        {
            Interact();
        }
    }

    void Interact()
    {
        var facingDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var interactPos = transform.position + facingDir;

        //Debug.DrawLine(transform.position, interactPos, Color.green, 0.5f); //Draws a line in front of the player to troubleshoot interactPos direction (only viewable in Scene view)

        var collider = Physics2D.OverlapCircle(interactPos, 0.3f, GameLayers.i.InteractableLayer);
        if (collider != null)
        {
            collider.GetComponent<Interactable>()?.Interact(); //"?" to ensure line doesn't crash the application in case it returns null
        }
    }

    private void CheckForEncounters()
    {
        if (Physics2D.OverlapCircle(transform.position, 0.2f, GameLayers.i.GrassLayer) != null)
        {
            if (UnityEngine.Random.Range(1, 101) <= 10) //As System and UnityEngine both have their own interpretations of Random, UnityEngine.Random is used to specify
            {
                //Debug.Log("Encountered a wild pokemon"); //Temporary code
                character.Animator.IsMoving = false; //Disables freeroam player moving animations when entering battle
                OnEnountered();
            }
        }
    }
}

//Player Controller Position Note: Make sure x position value is in the format x.5 and y position value in the format x.8. y position value format can also be the default x.5, though x.8 is more visually appealing as it allows the player model to be slightly above the centre of the tiles