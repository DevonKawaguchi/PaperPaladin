using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new TM or HM")]

//Script used to add items that teach the player new moves. Discarded due to time constraints and its irrelevance to the project 

public class TMItem : ItemBase
{
    [SerializeField] MoveBase move;

    public override bool Use(Pokemon pokemon)
    {
        return true;
    }

    public MoveBase Move => move;

}
