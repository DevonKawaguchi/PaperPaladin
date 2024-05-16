using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{
    public static void Init()
    {
        foreach (var kvp in Conditions)
        {
            var conditionID = kvp.Key;
            var condition = kvp.Value;

            condition.ID = conditionID;
        }
    }

    //Static dictionary below to avoid instance of the DB class
    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        { ConditionID.PSN,
            new Condition()
            {
                Name = "Poison",
                StartMessage = "has been poisoned!",
                OnAfterTurn = (Pokemon pokemon) => //Lambda function
                {
                    pokemon.DecreaseHP(pokemon.MaxHp / 8); //Reduces pokemon health by 1/8 of Pokemon HP after every turn
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} hurt itself due to poison!");
                }
            }
        },
        { ConditionID.BRN,
            new Condition()
            {
                Name = "Burn",
                StartMessage = "has been burned!",
                OnAfterTurn = (Pokemon pokemon) => //Lambda function
                {
                    pokemon.DecreaseHP(pokemon.MaxHp / 16); //Reduces pokemon health by 1/16 of Pokemon HP after every turn
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} hurt itself due to burn!");
                }
            }
        },
        { ConditionID.PAR, //Pokemon has 1/4 chance of being paralysed and won't be able to perform a move in a turn
            new Condition()
            {
                Name = "Paralysed",
                StartMessage = "has been paralysed!",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (Random.Range(1,5) == 1) //1-4 (5 is exclusive)
                    {
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}'s paralysed and can't move!");
                        return false; //Move can't be performed
                    }

                    return true; //Move can be performed
                }
            }
        },
        { ConditionID.FRZ, //Pokemon is paralysed and can't perform a move, though has a 1/4 chance of curing status
            new Condition()
            {
                Name = "Freeze",
                StartMessage = "has been frozen!",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (Random.Range(1,5) == 1) //1-4 (5 is exclusive)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is not frozen anymore!");
                        return true; //Move can now be performed
                    }

                    return false; //Move can't be performed
                }
            }
        },
        { ConditionID.SLP, //Pokemon paralysed and can't perform a move for 1-3 turns
            new Condition()
            {
                Name = "Sleep",
                StartMessage = "has fallen asleep!",
                OnStart = (Pokemon pokemon) =>
                {
                    //Sleep for 1-3 turns
                    pokemon.StatusTime = Random.Range(1,4); //1-3 (4 is exclusive)
                    Debug.Log($"Will be asleep for {pokemon.StatusTime} moves!");
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (pokemon.StatusTime <= 0) //Wake up Pokemon if StatusTime reaches 0
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} woke up!");
                        return true; //Move can now be performed
                    }

                    pokemon.StatusTime--;
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is sleeping!");
                    return false; //Move can't be performed
                }
            }
        },
        //Volatile Status Conditions
        { ConditionID.Confusion, //Lasts for 1-4 moves and causes Pokemon to potentially hurt itself and lose 1/8 of its health. However, the Pokemon will also have a 50% chance to be able to perform a move during this status
            new Condition()
            {
                Name = "Confusion",
                StartMessage = "has been confused!",
                OnStart = (Pokemon pokemon) =>
                {
                    //Sleep for 1-4 turns
                    pokemon.StatusTime = Random.Range(1,5); //1-4 (5 is exclusive)
                    Debug.Log($"Will be confused for {pokemon.VolatileStatusTime} moves!");
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (pokemon.VolatileStatusTime <= 0) //Wake up Pokemon if StatusTime reaches 0
                    {
                        pokemon.CureVolatileStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} kicked out of confusion!");
                        return true; //Move can now be performed
                    }
                    pokemon.VolatileStatusTime--;

                    //50% chance to do a move
                    if (Random.Range(1,3) == 1)
                    {
                        return true;
                    }

                    //Hurt by confusion
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is confused!");
                    pokemon.DecreaseHP(pokemon.MaxHp / 8);
                    pokemon.StatusChanges.Enqueue($"It hurt itself due to confusion!");
                    return false; //Move can't be performed
                }
            }
        }
    };

    public static float GetStatusBonus(Condition condition)
    {
        if (condition == null)
        {
            return 1f;
        }
        else if (condition.ID == ConditionID.SLP || condition.ID == ConditionID.FRZ)
        {
            return 2f;
        }
        else if (condition.ID == ConditionID.PAR || condition.ID == ConditionID.PSN || condition.ID == ConditionID.BRN)
        {
            return 1.5f;
        }
        return 1f;
    }
}

public enum ConditionID //None, Poison, Burn, Sleep, Paralyse, Freeze
{
    none, PSN, BRN, SLP, PAR, FRZ,
    Confusion
}