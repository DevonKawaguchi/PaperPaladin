using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class LongGrass : MonoBehaviour, IPlayerTriggerable
{
    public void OnPlayerTriggered(PlayerController player)
    {
        if (UnityEngine.Random.Range(1, 101) <= 10) //As System and UnityEngine both have their own interpretations of Random, UnityEngine.Random is used to specify
        {
            player.Character.Animator.IsMoving = false;
            GameController.Instance.StartBattle();
        }
    }
}
