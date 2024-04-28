using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Condition
{
    public ConditionID ID { get; set; }

    public string Name { get; set; }
    public string Description { get; set; }
    public string StartMessage { get; set; }

    public Action<Pokemon> OnStart { get; set; }

    public Func<Pokemon, bool> OnBeforeMove { get; set; } //2nd value in Func<> is return type

    public Action<Pokemon> OnAfterTurn { get; set; } 
}
