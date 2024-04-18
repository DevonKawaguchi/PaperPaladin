using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] //Classes will only be shown in inspector if this attribute is applied 

//Script calculates the values specific to the level of a certain pokemon and moves

public class Pokemon
{
    [SerializeField] PokemonBase _base;
    [SerializeField] int level;

    public PokemonBase Base {
        get
        {
            return _base;
        }

    }
    public int Level {
        get
        {
            return level;
        }
    }

    public int HP { get; set; }

    public List<Move> Moves { get; set; }

    public void Init()
    {
        HP = MaxHp;

        //Generate moves
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

    public DamageDetails TakeDamage(Move move, Pokemon attacker)
    {
        //Determines if hit is a critical hit - works wherein if Random.value generates a value (between 1-100) <= 6.25, double the damage
        float critical = 1f;
        if (Random.value * 100f <= 6.25f)
        {
            critical = 2f;
        }

        //Pokemon type strengths and weaknesses modifier
        float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);

        //
        var damageDetails = new DamageDetails()
        {
            TypeEffectiveness = type,
            Critical = critical,
            Fainted = false
        };

        float attack = (move.Base.IsSpecial) ? attacker.SpAttack : attacker.Attack; //Determines if attack is special and should thus utilise a spAttack, or physical and utilise a normal attack. The conventions in this line are also simply a different way of if/else
        float defense = (move.Base.IsSpecial) ? SpDefense : Defense; //Same as above line


        //Following formula used in Pokemon games to calculate damage, referenced from Damage page in Bulbapedia
        //Damage is calculated by determining base damage based off attacker's level, of which is then multiplied by the value of the power of the move and the attacker's stats. Attacker's stats multiplier is mitigated by current player's pokemon defense stats. After multipliers are applied to damage, modifiers applies damage to target within an 85%-100% range.
        float modifiers = Random.Range(0.85f, 1f) * type * critical; 
        float a = (2 * attacker.Level + 10) / 250f; //Level of the attacker
        float d = a * move.Base.Power * ((float)attack / defense) + 2; //Power of the move, attacker's attack stats, and current player pokemon defense stats
        int damage = Mathf.FloorToInt(d * modifiers);

        HP -= damage;
        if (HP <= 0)
        {
            HP = 0; //Ensures negative HP is not displayed in UI
            damageDetails.Fainted = true;
        }

        return damageDetails;
    }

    public Move GetRandomMove()
    {
        int r = Random.Range(0, Moves.Count);
        return Moves[r];
    }
}

public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float TypeEffectiveness { get; set; }
}