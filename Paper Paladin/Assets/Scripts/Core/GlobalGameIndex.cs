using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalGameIndex : MonoBehaviour
{
    public static int enemyIndex = 0;
    public int enemyMusicIndex = 0;
    public static int longGrassIndex = 1;

    private void Awake()
    {
        //Indexes reset to 0 in Awake() as index data may persist if the player finishes the game and returns to play again.
        enemyIndex = 0;
        enemyMusicIndex = 0;
        longGrassIndex = 1;


    }
}
