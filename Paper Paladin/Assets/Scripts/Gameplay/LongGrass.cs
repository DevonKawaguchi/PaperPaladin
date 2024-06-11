using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class LongGrass : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] AudioClip battleBeginMusic; //Changed

    public void OnPlayerTriggered(PlayerController player)
    {
        AudioManager.i.PlayMusic(battleBeginMusic, false); //Plays "BattleStart" sound

        player.Character.Animator.IsMoving = false;
        StartCoroutine(WaitForBattle(player)); //Lets sound play before initiating battle

        //OLD LOGIC:
        //Destroy(this.transform.gameObject);
        //Destroy(GameObject.Find($"LongGrass - {GlobalGameIndex.longGrassIndex}"));

        //Debug.Log($"LongGrass - {GlobalGameIndex.longGrassIndex} destroyed");

        //GlobalGameIndex.longGrassIndex += 1;

        //player.Character.Animator.IsMoving = false;

        //GameController.Instance.StartBattle();


        //ORIGINAL LOGIC:
        //LongGrassObject.gameObject.SetActive(false); //Deactivates enemy encounter collider after initiating the battle

        //Destroy(gameObject); //Destroys enemy encounter long grass after initiating battle

        //Removed below logic to ensure the player always initiates an enemy encounter when colliding with a LongGrass object.

        //if (UnityEngine.Random.Range(1, 101) <= 10) //As System and UnityEngine both have their own interpretations of Random, UnityEngine.Random is used to specify
        //{
        //    player.Character.Animator.IsMoving = false;
        //    GameController.Instance.StartBattle();
        //}
    }

    public bool TriggerRepeatedly => true;

    IEnumerator WaitForBattle(PlayerController player)
    {
        GameController.Instance.PauseGame(true);

        yield return new WaitForSeconds(1.4f);

        Destroy(this.transform.gameObject);
        Destroy(GameObject.Find($"LongGrass - {GlobalGameIndex.longGrassIndex}"));

        Debug.Log($"LongGrass - {GlobalGameIndex.longGrassIndex} destroyed");

        GlobalGameIndex.longGrassIndex += 1;

        //player.Character.Animator.IsMoving = false;
        GameController.Instance.PauseGame(false);

        GameController.Instance.StartBattle();
    }
}
