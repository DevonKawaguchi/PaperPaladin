using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MoveActorAction : CutsceneAction
{
    //Responsible for moving characters in cutscene

    [SerializeField] CutsceneActor actor; //Character that should be moved
    [SerializeField] List<Vector2> movePatterns; //Move pattern for character

    public override IEnumerator Play()
    {
        var character = actor.GetCharacter();

        foreach (var moveVec in movePatterns)
        {
            yield return character.Move(moveVec, checkCollisions: false); //yield waits for pattern to be executed one at a time
        }
    }
}

[System.Serializable]
public class CutsceneActor
{
    [SerializeField] bool isPlayer;
    [SerializeField] Character character;

    public Character GetCharacter() => (isPlayer) ? PlayerController.i.Character : character; //Returns character. Otherwise, return character assigned in the field
}
