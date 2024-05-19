using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableObjectAction : CutsceneAction
{
    [SerializeField] GameObject go; //go stands for game object

    public override IEnumerator Play() //Disables specified game object
    {
        go.SetActive(false);
        yield break;
    }
}
