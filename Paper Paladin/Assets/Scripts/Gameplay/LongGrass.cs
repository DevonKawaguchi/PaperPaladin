using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using DG.Tweening;
using UnityEngine.UI;
using Unity.VisualScripting;
using UnityEngine.U2D;
using UnityEngine.Tilemaps;

public class LongGrass : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] AudioClip battleBeginMusic; //Changed

    [SerializeField] int xDir; //Where the player moves back in terms of x when colliding with grass
    [SerializeField] int yDir; //Where the player moves back in terms of y when colliding with grass
    [SerializeField] List<Vector2> movementPattern;

    public static bool battleDefeat = false;

    [SerializeField] AudioClip bossBattleBeginMusic;
    [SerializeField] Sprite sprite;
    [SerializeField] new string name;

    private int bossIndexRequirement = 7; //Set to 7

    public void Start()
    {
        GetComponent<TilemapRenderer>().enabled = false;
    }

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

        if (GlobalGameIndex.enemyIndex == bossIndexRequirement) //To be 5
        {
            AudioManager.i.PlayMusic(bossBattleBeginMusic, false);

            player.Character.Animator.IsMoving = false;
            StartCoroutine(WaitForBattle(player));
        }
        else
        {
            AudioManager.i.PlayMusic(battleBeginMusic, false); //Plays "BattleStart" sound

            player.Character.Animator.IsMoving = false;
            StartCoroutine(WaitForBattle(player)); //Lets sound play before initiating battle
        }
    }

    public bool TriggerRepeatedly => true;

    IEnumerator WaitForBattle(PlayerController player)
    {
        GlobalGameIndex.longGrassMovementxDir = xDir;
        GlobalGameIndex.longGrassMovementyDir = yDir;

        Debug.Log($"GlobalGameIndex.longGrassMovementxDir is {GlobalGameIndex.longGrassMovementxDir}");
        Debug.Log($"GlobalGameIndex.longGrassMovementyDir is {GlobalGameIndex.longGrassMovementyDir}");

        GameController.Instance.PauseGame(true);

        yield return new WaitForSeconds(1.4f);

        player.Character.Animator.IsMoving = false;
        GameController.Instance.PauseGame(false);

        if (GlobalGameIndex.enemyIndex == bossIndexRequirement)
        {
            GameController.Instance.StartTrainerBattle(this);
        }
        else
        {
            GameController.Instance.StartBattle();
        }
    }

    public void BattleLost()
    {
        battleDefeat = true;
    }

    public string Name
    {
        get => name;
    }

    public Sprite Sprite
    {
        get => sprite;
    }

    //IEnumerator MakePlayerMove()
    //{
    //    Character grassActor = PlayerController.i.Character;

    //    yield return grassActor.Move(movementPattern[0]);
    //    Debug.Log("Executed MakePlayerMove() coroutine");
    //}
}