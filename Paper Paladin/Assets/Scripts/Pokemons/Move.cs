using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Script contains data of move stats changed during battle.
public class Move
{
    public MoveBase Base { get; set; }
    public int PP { get; set; }

    public Move(MoveBase pBase)
    {
        Base = pBase;
        PP = pBase.PP;
    }
}
