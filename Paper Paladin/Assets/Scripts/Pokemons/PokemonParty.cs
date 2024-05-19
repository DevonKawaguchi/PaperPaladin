using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class PokemonParty : MonoBehaviour
{
    [SerializeField] List<Pokemon> pokemons;

    public event Action OnUpdated;

    public List<Pokemon> Pokemons
    {
        get
        {
            return pokemons;
        }
        set
        {
            pokemons = value;
            OnUpdated?.Invoke();
        }
    }

    private void Awake() //Critical Error Fix (18/5/24): Executes initialisation of Pokemon party before initialising Pokemon party in other scripts by executing the initialisation in Awake() as Awake() is executed before Start()
    {
        foreach (var pokemon in pokemons)
        {
            pokemon.Init();
        }
    }

    public Pokemon GetHealthyPokemon() //Puts healthiest pokemon on the battle - this is done as not all pokemon in a party can all be put forward in a battle all at once
    {
        return pokemons.Where(x => x.HP > 0).FirstOrDefault(); //Where() loops through list of pokemons to return list of pokemons which satisfies the condition, which is in this case is having a health greater than 0
        //FirstOrDefault returns the first pokemon in the party that hasn't fainted, and if all pokemon in party are fainted, it makes the line return null
    }

    public void AddPokemon(Pokemon newPokemon)
    {
        if (pokemons.Count < 6)
        {
            pokemons.Add(newPokemon);
            OnUpdated?.Invoke();
        }
        else
        {
            //TO DO: Add to the PC once that's implemented
        }
    }
    public static PokemonParty GetPlayerParty()
    {
        return FindObjectOfType<PlayerController>().GetComponent<PokemonParty>();
    }
}
