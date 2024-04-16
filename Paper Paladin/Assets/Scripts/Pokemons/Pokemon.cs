using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Script calculates the values specific to the level of a certain pokemon and moves

public class Pokemon
{
    public PokemonBase Base { get; set; }
    public int Level { get; set; }

    public int HP { get; set; }

    public List<Move> Moves { get; set; }

    public Pokemon(PokemonBase pBase, int pLevel)
    {
        Base = pBase;
        Level = pLevel;
        HP = MaxHp;

        //Generate Moves
        Moves = new List<Move>();
        foreach (var move in Base.LearnableMoves)
        {
            if (move.Level <= Level) //Determines whether Pokemon is at the required level to be able to learn a certain move(s)
            {
                Moves.Add(new Move(move.Base));
            }

            if (Moves.Count >= 4) //Ensures Pokemon do not exceed 4 learned moves.
            {
                break;
            }
        }
    }

    public int MaxHp
    {
        get { return Mathf.FloorToInt((Base.MaxHp * Level / 100f) + 10); } //Increases Pokemon health value based on level. FloorToInt present to remove decimal point by converting float to int. +10 instead of normal +5 increase intentional and only present for MaxHp.
    }
    public int Attack
    {
        get { return Mathf.FloorToInt((Base.Attack * Level / 100f) + 5); } //Same as above, repeated for below.
    } 
    public int Defense
    {
        get { return Mathf.FloorToInt((Base.Defense * Level / 100f) + 5); } 
    }
    public int SpAttack
    {
        get { return Mathf.FloorToInt((Base.SpAttack * Level / 100f) + 5); } 
    }
    public int SpDefense
    {
        get { return Mathf.FloorToInt((Base.SpDefense * Level / 100f) + 5); } 
    }
    public int Speed
    {
        get { return Mathf.FloorToInt((Base.Speed * Level / 100f) + 5); } 
    }
}
