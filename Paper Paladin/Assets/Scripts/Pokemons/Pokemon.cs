using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable] //Classes will only be shown in inspector if this attribute is applied 

//Script calculates the values specific to the level of a certain pokemon and moves

public class Pokemon
{
    [SerializeField] PokemonBase _base;
    [SerializeField] int level;

    public Pokemon(PokemonBase pBase, int pLevel)
    {
        _base = pBase;
        level = pLevel;

        Init();
    }

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
    public int Exp { get; set; }
    public int StatusTime { get; set; }
    public int VolatileStatusTime { get; set; }

    public bool HPChanged { get; set; }

    public List<Move> Moves { get; set; }

    public Move CurrentMove { get; set; }

    public Dictionary<Stat, int> Stats { get; private set; } //Dictionary instead of list as it'll be easier to find values through keys - better storage of Pokemon stat values
    public Dictionary<Stat, int> StatBoosts { get; private set; }

    public Condition Status { get; private set; }
    public Condition VolatileStatus { get; private set; }

    public Queue<string> StatusChanges { get; private set; } //Queue<> used to store list of elements such as a list, and allows elements to be taken out of the queue while retaining order of elements added in the queue. Queue also simplifies code in comparison to List<>

    public event System.Action OnStatusChanged; //Could use "using System", though just put "System." instead - same thing, different form
    public event System.Action OnHPChanged; 

    public void Init()
    {
        //Generate moves
        Moves = new List<Move>();
        foreach (var move in Base.LearnableMoves)
        {
            if (move.Level <= Level) //Determines whether Pokemon is at the required level to be able to learn a certain move(s)
            {
                Moves.Add(new Move(move.Base));
            }

            if (Moves.Count >= PokemonBase.MaxNumberOfMoves) //Ensures Pokemon do not exceed 4 learned moves.
            {
                break;
            }
        }

        Exp = Base.GetExpForLevel(Level);

        CalculateStats();

        HP = MaxHp; //HP calculation moved to after CalculateStats() as it hadn't been defined previously (past error)

        StatusChanges = new Queue<string>();
        ResetStatBoost();
        Status = null;
        VolatileStatus = null;
    }

    public Pokemon(PokemonSaveData saveData) //Restores Pokemon using its saveable data when loading save file
    {
        _base = PokemonDB.GetPokemonByName(saveData.name);
        HP = saveData.hp;
        level = saveData.level;
        Exp = saveData.exp;

        if (saveData.statusID != null)
        {
            Status = ConditionsDB.Conditions[saveData.statusID.Value];
        }
        else
        {
            Status = null;
        }

        Moves = saveData.moves.Select(s => new Move(s)).ToList();

        CalculateStats();
        StatusChanges = new Queue<string>();
        ResetStatBoost();
        VolatileStatus = null;
    }

    public PokemonSaveData GetSaveData() //Converts Pokemon class to its save data class
    {
        var saveData = new PokemonSaveData()
        {
            name = Base.Name,
            hp = HP,
            level = Level,
            exp = Exp,
            statusID = Status?.ID,
            moves = Moves.Select(m => m.GetSaveData()).ToList()
        };

        return saveData;
    }

    void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt((Base.Attack * Level / 100f) + 5));
        Stats.Add(Stat.Defense, Mathf.FloorToInt((Base.Defense * Level / 100f) + 5));
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt((Base.SpAttack * Level / 100f) + 5));
        Stats.Add(Stat.SpDefense, Mathf.FloorToInt((Base.SpDefense * Level / 100f) + 5));
        Stats.Add(Stat.Speed, Mathf.FloorToInt((Base.Speed * Level / 100f) + 5));

        MaxHp = Mathf.FloorToInt((Base.MaxHp * Level / 100f) + 10 + Level);
    }

    void ResetStatBoost()
    {
        StatBoosts = new Dictionary<Stat, int>()
        {
            {Stat.Attack, 0 },
            {Stat.Defense, 0 },
            {Stat.SpAttack, 0 },
            {Stat.SpDefense, 0 },
            {Stat.Speed, 0 },

            {Stat.Accuracy, 0 },
            {Stat.Evasion, 0 },
        };
    }

    int GetStat(Stat stat) //By returning stat values through this function, stat boosters aren't required to be applied to each stat one by one, but rather all in at once 
    {
        int statVal = Stats[stat];

        //Apply stat boost
        int boost = StatBoosts[stat]; //Retrieves stat boost dictionary values
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (boost >= 0)
        {
            statVal = Mathf.FloorToInt(statVal * boostValues[boost]);
        }
        else
        {
            statVal = Mathf.FloorToInt(statVal / boostValues[-boost]);

        }

        return statVal;
    }

    public void ApplyBoosts(List<StatBoost> statBoosts)
    {
        foreach (var statBoost in statBoosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);

            if (boost > 0)
            {
                StatusChanges.Enqueue($"{Base.Name}'s {stat} rose!"); //Enqueue() same as a List<>'s Add() function
            }
            else
            {
                StatusChanges.Enqueue($"{Base.Name}'s {stat} fell!");
            }

            Debug.Log($"{stat} has been boosted to {StatBoosts[stat]}");
        }
    }

    public bool CheckForLevelUp() //Increases Pokemon level if XP greater than XP cap
    {
        if (Exp > Base.GetExpForLevel(level + 1)) 
        {
            ++level;
            return true;
        }
        return false;
    }
    
    public LearnableMove GetLearnableMoveAtCurrentLevel() //Checks if the Pokemon has any moves they can learn based on their level
    {
        return Base.LearnableMoves.Where(x => x.Level == level).FirstOrDefault(); //FirstOrDefault gets first item from list - present to return null if the list is empty
    }

    public void LearnMove(LearnableMove moveToLearn)
    {
        if (Moves.Count > PokemonBase.MaxNumberOfMoves)
        {
            return;
        }

        Moves.Add(new Move(moveToLearn.Base));
    }

    public int MaxHp { get; private set; } //Get;Set; as MaxHp only calculated once
    public int Attack
    {
        get { return GetStat(Stat.Attack); } //Same as above, repeated for below.
    } 
    public int Defense
    {
        get { return GetStat(Stat.Defense); } 
    }
    public int SpAttack
    {
        get { return GetStat(Stat.SpAttack); } 
    }
    public int SpDefense
    {
        get { return GetStat(Stat.SpDefense); } 
    }
    public int Speed
    {
        get { return GetStat(Stat.Speed); } 
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

        float attack = (move.Base.Category == MoveCategory.Special) ? attacker.SpAttack : attacker.Attack; //Determines if attack is special and should thus utilise a spAttack, or physical and utilise a normal attack. The conventions in this line are also simply a different way of if/else
        float defense = (move.Base.Category == MoveCategory.Special) ? SpDefense : Defense; //Same as above line


        //Following formula used in Pokemon games to calculate damage, referenced from Damage page in Bulbapedia
        //Damage is calculated by determining base damage based off attacker's level, of which is then multiplied by the value of the power of the move and the attacker's stats. Attacker's stats multiplier is mitigated by current player's pokemon defense stats. After multipliers are applied to damage, modifiers applies damage to target within an 85%-100% range.
        float modifiers = Random.Range(0.85f, 1f) * type * critical; 
        float a = (2 * attacker.Level + 10) / 250f; //Level of the attacker
        float d = a * move.Base.Power * ((float)attack / defense) + 2; //Power of the move, attacker's attack stats, and current player pokemon defense stats
        int damage = Mathf.FloorToInt(d * modifiers);

        DecreaseHP(damage);

        return damageDetails;
    }

    public void IncreaseHP(int amount)
    {
        HP = Mathf.Clamp(HP + amount, 0, MaxHp);
        OnHPChanged?.Invoke();
        HPChanged = true;
    }

    public void DecreaseHP(int damage)
    {
        HP = Mathf.Clamp(HP - damage, 0, MaxHp);
        OnHPChanged?.Invoke();
        HPChanged = true;
    }

    public void SetStatus(ConditionID conditionID)
    {
        if (Status != null) return;

        Status = ConditionsDB.Conditions[conditionID];
        Status?.OnStart?.Invoke(this); //Null conditional operators (?) to ensure project doesn't crash if object doesn't have an OnStart action
        StatusChanges.Enqueue($"{Base.Name} {Status.StartMessage}");
        OnStatusChanged?.Invoke();
    }

    public void CureStatus()
    {
        Status = null;
        OnStatusChanged?.Invoke();
    }

    public void SetVolatileStatus(ConditionID conditionID) //FYI: Volatile statuses will only last until battle is over
    {
        if (VolatileStatus != null) return;

        VolatileStatus = ConditionsDB.Conditions[conditionID];
        VolatileStatus?.OnStart?.Invoke(this); //Null conditional operators (?) to ensure project doesn't crash if object doesn't have an OnStart action
        StatusChanges.Enqueue($"{Base.Name} {VolatileStatus.StartMessage}");
    }

    public void CureVolatileStatus()
    {
        VolatileStatus = null;
    }


    public Move GetRandomMove()
    {
        var movesWithPP = Moves.Where(x => x.PP > 0).ToList();

        int r = Random.Range(0, movesWithPP.Count); //Makes the enemy unable to do moves if they run out of stamina
        return movesWithPP[r];
    }

    public bool OnBeforeMove()
    {
        bool canPerformMove = true;

        if (Status?.OnBeforeMove != null) //Will return null in case status doesn't have a OnBeforeMove function
        {
            if (!Status.OnBeforeMove(this))
            {
                canPerformMove = false;
            }
        }

        if (VolatileStatus?.OnBeforeMove != null) 
        {
            if (!VolatileStatus.OnBeforeMove(this))
            {
                canPerformMove = false;
            }
        }

        return canPerformMove;
    }

    public void OnAfterTurn()
    {
        Status?.OnAfterTurn?.Invoke(this); //"?.Invoke" allows line to only invoke OnAfterTurn action if not null
        VolatileStatus?.OnAfterTurn?.Invoke(this); 

    }

    public void OnBattleOver() //Resets applied stat boosts on Pokemon following battle end
    {
        VolatileStatus = null;
        ResetStatBoost();
    }
}

public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float TypeEffectiveness { get; set; }
}

[System.Serializable]
public class PokemonSaveData //Contains all the Pokemon save data that has to be saved
{
    public string name; //Allows to get all the base data of a Pokemon by searching for the Pokemon's name
    public int hp;
    public int level;
    public int exp;
    public ConditionID? statusID; //Nullable as Pokemon may not have a status
    public List<MoveSaveData> moves;
}