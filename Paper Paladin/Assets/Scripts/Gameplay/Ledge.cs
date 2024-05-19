using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Ledge : MonoBehaviour
{
    [SerializeField] int xDir; //Where the player jumps in terms of x when colliding with ledge
    [SerializeField] int yDir; //Where the player jumps in terms of y when colliding with ledge

    private void Awake()
    {
        GetComponent<SpriteRenderer>().enabled = false; //Hides ledge collider sprites when game scene active
    }

    public bool TryToJump(Character character, Vector2 moveDir) 
    {
        if (moveDir.x == xDir && moveDir.y == yDir) //Checks if character is facing the same direction as the ledge direction
        {
            StartCoroutine(Jump(character));
            return true;
        }
        return false;
    }

    IEnumerator Jump(Character character)
    {
        GameController.Instance.PauseGame(true); //Disables player movement  
        character.Animator.IsJumping = true; //Activates jump animation

        var jumpDestination = character.transform.position + new Vector3(xDir, yDir) * 2;
        yield return character.transform.DOJump(jumpDestination, 0.3f, 1, 0.5f).WaitForCompletion(); //Location, Jump Power, Num of Jumps, Length of Jump

        character.Animator.IsJumping = false; //De-activates jump animation
        GameController.Instance.PauseGame(false); //Re-enables player movement  
    }
}
