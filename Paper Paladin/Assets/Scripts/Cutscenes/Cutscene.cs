using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Cutscene : MonoBehaviour, IPlayerTriggerable
{
    //Serialises field as a reference rather than value -> Serialises fields in subclasses
    [SerializeReference] 
    [SerializeField] List<CutsceneAction> actions;

    public bool TriggerRepeatedly => false;

    public IEnumerator Play() //Plays actions one by one
    {
        GameController.Instance.StartCutsceneState();

        foreach (var action in actions)
        {
            if (action.WaitForCompletion) //Allows specified move paths to be performed simultaneously
            {
                yield return action.Play();
            }
            else
            {
                StartCoroutine(action.Play());
            }
        }

        GameController.Instance.StartFreeRoamState();
    }

    public void AddAction(CutsceneAction action)
    {
#if UNITY_EDITOR //Will only compile line if in editor. Avoids "not exist" CS0103 error when building game
        Undo.RegisterCompleteObjectUndo(this, "Add action to cutscene"); //Allows cutscene changes in editor to be saved and hence reversed or brought back
        //Above also marks scene as dirty when changes made. This better indicates changes have been made to the editor and that it should be saved
#endif

        action.Name = action.GetType().ToString(); //Automatically sets name of action in editor
        actions.Add(action);
    }

    public void OnPlayerTriggered(PlayerController player)
    {
        player.Character.Animator.IsMoving = false; //Disables player animation when in cutscene
        StartCoroutine(Play());

    }
}
