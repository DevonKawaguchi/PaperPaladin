using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportObjectAction : CutsceneAction
{
    //Used to change position of any game object in cutscene

    [SerializeField] GameObject go; //go short for GameObject
    [SerializeField] Vector2 position;

    public override IEnumerator Play()
    {
        go.transform.position = position; //Sets character position to given position
        yield break;
    }
}
