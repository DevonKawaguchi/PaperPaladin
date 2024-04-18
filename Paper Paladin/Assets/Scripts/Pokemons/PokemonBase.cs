using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemon/Create new pokemon")]

public class PokemonBase : ScriptableObject
{
    [SerializeField] string name;

    [TextArea] 
    [SerializeField] string description;

    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;

    [SerializeField] PokemonType type1;
    [SerializeField] PokemonType type2;

    //Base Stats
    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;

    [SerializeField] List<LearnableMove> learnableMoves; 

    //Allows Pokemon stats to be called by other scripts by making values public
    public string Name
    {
        get { return name; }
    }
    public string Description
    {
        get { return description; }
    }

    public Sprite FrontSprite
    {
        get { return frontSprite; }
    }
    public Sprite BackSprite
    {
        get { return backSprite; }
    }

    public PokemonType Type1
    {
        get { return type1; }
    }
    public PokemonType Type2
    {
        get { return type2; }
    }

    public int MaxHp
    {
        get { return maxHp; }
    }
    public int Attack
    {
        get { return attack; }
    }
    public int SpAttack
    {
        get { return spAttack; }
    }
    public int Defense
    {
        get { return defense; }
    }
    public int SpDefense
    {
        get { return defense; }
    }
    public int Speed
    {
        get { return speed; }
    }
    
    public List<LearnableMove> LearnableMoves
    {
        get { return learnableMoves; }
    }
}

[System.Serializable]
public class LearnableMove
{
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;

    public MoveBase Base
    {
        get { return moveBase; }
    }
    public int Level //Determines what level the Pokemon must reach to learn a certain move
    {
        get { return level; }
    }
}

public enum PokemonType
{
    None,
    Normal,
    Fire,
    Water,
    Electric,
    Grass,
    Ice,
    Fighting,
    Poison,
    Ground,
    Flying,
    Pyschic,
    Bug,
    Rock,
    Ghost,
    Dragon
}

public class TypeChart
{
    //2D array to store type weaknesses and strengths in attack and defense

    static float[][] chart =
    {
        //Important: Order must be in the same order as specified in PokemonType above as modifiers are assigned to each respective index value of PokemonType
        //Columns in Order: Normal, Fire, Water, Electric, Grass, Ice, Fighting, Poison
        /*Normal*/  new float [] { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f },
        /*Fire*/  new float [] { 1f, 0.5f, 0.5f, 1f, 2f, 2f, 1f, 1f },
        /*Water*/  new float [] { 1f, 2f, 0.5f, 2f, 0.5f, 1f, 1f, 1f },
        /*Electric*/  new float [] { 1f, 1f, 2f, 0.5f, 0.5f, 2f, 1f, 1f },
        /*Grass*/  new float [] { 1f, 0.5f, 2f, 2f, 0.5f, 1f, 1f, 0.5f },
        /*Poison*/  new float [] { 1f, 1f, 1f, 1f, 2f, 1f, 1f, 1f } //Make sure to add full list of types
    };

    public static float GetEffectiveness(PokemonType attackType, PokemonType defenseType)
    {
        if (attackType == PokemonType.None || defenseType == PokemonType.None)
        {
            return 1;
        }

        int row = (int)attackType - 1;
        int col = (int)defenseType - 1;

        return chart[row][col];
    }
}