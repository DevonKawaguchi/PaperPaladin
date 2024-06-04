using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    public GlobalGameIndex globalGameIndex;

    [SerializeField] List<Pokemon> wildPokemons;

    //public GameObject[] longGrassObjectsArray;
    //public BattleSystem battleSystem;
    //[SerializeField] GameObject longGrassObject;

    //public int areaEnemyIndex = 0; //Sets which enemy the player has to face in an area

    public Pokemon GetRandomWildPokemon()
    {
        //longGrassObject.gameObject.SetActive(false);
        var wildPokemon = wildPokemons[globalGameIndex.enemyIndex];
        wildPokemon.Init();
        Debug.Log($"areaEnemyIndex is {globalGameIndex.enemyIndex}");


        return wildPokemon;
    }
}
