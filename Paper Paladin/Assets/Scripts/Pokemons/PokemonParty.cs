using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PokemonParty : MonoBehaviour
{
    [SerializeField] List<Pokemon> pokemons;

    public List<Pokemon> Pokemons
    {
        get
        {
            return pokemons;
        }
    }

    private void Start()
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
}
