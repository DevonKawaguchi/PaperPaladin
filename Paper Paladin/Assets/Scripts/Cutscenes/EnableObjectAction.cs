using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableObjectAction : CutsceneAction
{
    [SerializeField] GameObject go; //go stands for game object

    public override IEnumerator Play() //Enables specified game object
    {
        go.SetActive(true);
        yield break;
    }
}
