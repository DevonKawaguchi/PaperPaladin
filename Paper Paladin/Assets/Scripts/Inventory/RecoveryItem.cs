using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new recovery item")]

public class RecoveryItem : ItemBase
{
    [Header("HP")] 
    [SerializeField] int hpAmount; //Amount of HP that should be restored
    [SerializeField] bool restoreMaxHP;

    [Header("PP")]
    [SerializeField] int ppAmount; //Amount of PP that should be restored
    [SerializeField] bool restoreMaxPP;

    [Header("Status Conditions")]
    [SerializeField] ConditionID status;
    [SerializeField] bool recoverAllStatus;

    [Header("Revive")]
    [SerializeField] bool revive;
    [SerializeField] bool maxRevive;

    public override bool Use(Pokemon pokemon)
    {
        if (hpAmount > 0)
        {
            if (pokemon.HP == pokemon.MaxHp)
            {
                return false;
            }
            //pokemon.IncreaseHP(hpAmount, HP); Error
        }
        return true;
    }
}
