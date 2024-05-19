using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(CutsceneActor))]

public class CutsceneActorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property); //Lets Unity know the property is inside these functions -> allows Unity to provide default functionalities in editor

        position = EditorGUI.PrefixLabel(position, label);

        var togglePos = new Rect(position.x, position.y, 70, position.height); //x, y, width, height
        var fieldPos = new Rect(position.x + 70, position.y, position.width - 70, position.height); //x, y, witdth, height

        var isPlayerProp = property.FindPropertyRelative("isPlayer");

        isPlayerProp.boolValue = GUI.Toggle(togglePos, isPlayerProp.boolValue, "Is Player");
        isPlayerProp.serializedObject.ApplyModifiedProperties();

        if (!isPlayerProp.boolValue) //boolValue as isPlayerProp is a serialised property
        {
            EditorGUI.PropertyField(fieldPos, property.FindPropertyRelative("character"), GUIContent.none);
        }

        EditorGUI.EndProperty();
    }
}
