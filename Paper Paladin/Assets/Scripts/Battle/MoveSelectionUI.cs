using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class MoveSelectionUI : MonoBehaviour
{
    [SerializeField] List<TextMeshProUGUI> moveTexts;
    [SerializeField] Color highlightedColour;

    int currentSelection = 0;

    public void SetMoveData(List<MoveBase> currentMoves, MoveBase newMove)
    {
        for (int i = 0; i < currentMoves.Count; ++i) //Sets the names of the current moves to the MoveSelectionUI
        {
            moveTexts[i].text = currentMoves[i].Name; 
        }

        moveTexts[currentMoves.Count].text = newMove.Name;
    }

    public void HandleMoveSelection(Action<int> onSelected) //Determines what move is selected (up/down)
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ++currentSelection;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            --currentSelection;
        }

        currentSelection = Mathf.Clamp(currentSelection, 0, PokemonBase.MaxNumberOfMoves);

        UpdateMoveSelection(currentSelection);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            onSelected?.Invoke(currentSelection);
        }
    }

    public void UpdateMoveSelection(int selection)
    {
        for (int i = 0; i < PokemonBase.MaxNumberOfMoves + 1; i++)
        {
            if (i == selection)
            {
                moveTexts[i].color = highlightedColour;
            }
            else
            {
                moveTexts[i].color = Color.black;
            }
        }
    }
}
