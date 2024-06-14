using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using DG.Tweening;
using UnityEngine.UI;

public class LongGrass : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] AudioClip battleBeginMusic; //Changed

    [SerializeField] int xDir; //Where the player moves back in terms of x when colliding with grass
    [SerializeField] int yDir; //Where the player moves back in terms of y when colliding with grass

    public static bool battleDefeat = false;

    public static bool whatever = false;

    [SerializeField] List<Vector2> movementPattern;

    //private void FixedUpdate()
    //{
    //    if (battleDefeat == true)
    //    {
    //        Character grassActor = PlayerController.i.Character;

    //        whatever = true;
    //        StartCoroutine(Jump(grassActor));
    //        battleDefeat = false;

    //        Debug.Log("Player should be able to move now");
    //    }
    //}

    public bool TryToJump(Character character, Vector2 moveDir)
    {
        if (battleDefeat == true) //Checks if character is facing the same direction as the ledge direction
        {
            //Debug.Log($"Initiated because battleDefeat is {battleDefeat} and currentlyStanding is {currentlyStanding}");
            StartCoroutine(Jump(character));
            return true;
        }
        return false;
    }

    IEnumerator Jump(Character character)
    {
        GameController.Instance.PauseGame(true); //Disables player movement  
        character.Animator.IsJumping = true; //Activates jump animation

        var jumpDestination = character.transform.position + new Vector3(xDir, yDir) * -2;
        yield return character.transform.DOJump(jumpDestination, 0.3f, 1, 0.5f).WaitForCompletion(); //Location, Jump Power, Num of Jumps, Length of Jump

        character.Animator.IsJumping = false; //De-activates jump animation
        GameController.Instance.PauseGame(false); //Re-enables player movement
        //whatever = false;

        battleDefeat = false;
    }

    public void OnPlayerTriggered(PlayerController player)
    {
        //currentlyStanding = true;
        //Debug.Log($"ALERT! currentlyStanding is {currentlyStanding}");

        AudioManager.i.PlayMusic(battleBeginMusic, false); //Plays "BattleStart" sound

        player.Character.Animator.IsMoving = false;
        StartCoroutine(WaitForBattle(player)); //Lets sound play before initiating battle
    }

    public bool TriggerRepeatedly => true;

    IEnumerator WaitForBattle(PlayerController player)
    {
        GameController.Instance.PauseGame(true);

        yield return new WaitForSeconds(1.4f);

        player.Character.Animator.IsMoving = false;
        GameController.Instance.PauseGame(false);
        GameController.Instance.StartBattle();
    }

    //IEnumerator MakePlayerMove()
    //{
    //    Character grassActor = PlayerController.i.Character;

    //    yield return grassActor.Move(movementPattern[0]);
    //    Debug.Log("Executed MakePlayerMove() coroutine");
    //}
}