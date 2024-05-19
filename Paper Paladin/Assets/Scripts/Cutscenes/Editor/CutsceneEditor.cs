using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Cutscene))] 
public class CutsceneEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var cutscene = target as Cutscene;

        using (var scope = new GUILayout.HorizontalScope()) //Aligns below editor buttons to be in a grid
        {
            if (GUILayout.Button("Dialogue Action")) //Returns true if button clicked
            {
                cutscene.AddAction(new DialogueAction());
            }
            else if (GUILayout.Button("Move Actor Action"))
            {
                cutscene.AddAction(new MoveActorAction());
            }
        }

        using (var scope = new GUILayout.HorizontalScope()) //Aligns below editor buttons to be in a grid
        {
            if (GUILayout.Button("Turn Actor Action"))
            {
                cutscene.AddAction(new TurnActorAction());
            }
            else if (GUILayout.Button("Teleport Object Action"))
            {
                cutscene.AddAction(new TeleportObjectAction());
            }
        }

        using (var scope = new GUILayout.HorizontalScope()) //Aligns below editor buttons to be in a grid
        {
            if (GUILayout.Button("Enable Object"))
            {
                cutscene.AddAction(new EnableObjectAction());
            }
            else if (GUILayout.Button("Disable Object"))
            {
                cutscene.AddAction(new DisableObjectAction());
            }
        }

        using (var scope = new GUILayout.HorizontalScope()) //Aligns below editor buttons to be in a grid
        {
            if (GUILayout.Button("NPC Interact"))
            {
                cutscene.AddAction(new NPCInteractAction());
            }
        }

        using (var scope = new GUILayout.HorizontalScope()) //Aligns below editor buttons to be in a grid
        {
            if (GUILayout.Button("Fade In"))
            {
                cutscene.AddAction(new FadeInAction());
            }
            else if (GUILayout.Button("Fade Out"))
            {
                cutscene.AddAction(new FadeOutAction());
            }
        }

        base.OnInspectorGUI();
    }
}
