using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CutsceneAction 
{
    //All cutscene actions are inherited from this class

    [SerializeField] string name;
    [SerializeField] bool waitForCompletion = true;

    public virtual IEnumerator Play() //virtual to allow function to be overrided by other classes
    {
        yield break;
    }

    public string Name //Allows cutscene.cs to grab the name of the action
    {
        get => name;
        set => name = value;
    }

    public bool WaitForCompletion => waitForCompletion;
}
