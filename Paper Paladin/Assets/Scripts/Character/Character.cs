using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;
using System;

public class Character : MonoBehaviour
{
    public float moveSpeed;

    public bool IsMoving { get; private set; }

    public float OffsetY { get; private set; } = 0.3f;

    CharacterAnimator animator;

    private void Awake()
    {
        animator = GetComponent<CharacterAnimator>();
        SetPositionAndSnapToTile(transform.position);
    }

    public void SetPositionAndSnapToTile(Vector2 pos)
    {
        //Ensures object positions are in the centre of the tile
        pos.x = Mathf.Floor(pos.x) + 0.5f; //E.g: 2.3 -> 2 (because of .Floor) -> 2.5f
        pos.y = Mathf.Floor(pos.y) + 0.5f + OffsetY;

        transform.position = pos;
    }

    public IEnumerator Move(Vector2 moveVec, Action OnMoveOver = null)
    {
        animator.MoveX = Mathf.Clamp(moveVec.x, -1f, 1f);
        animator.MoveY = Mathf.Clamp(moveVec.y, -1f, 1f);

        var targetPos = transform.position;
        targetPos.x += moveVec.x;
        targetPos.y += moveVec.y;

        if (!IsPathClear(targetPos)) //If IsPathClear = False, break the path and move onto the next viable path
        {
            yield break; 
        }

        IsMoving = true;

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;

        IsMoving = false;

        OnMoveOver?.Invoke();
    }

    public void HandleUpdate()
    {
        animator.IsMoving = IsMoving;
    }

    private bool IsPathClear(Vector3 targetPos) //Checks if NPC path is walkable
    {
        var diff = targetPos - transform.position; //Calculates distance between target position in path: Target - Current Position 
        var dir = diff.normalized; //Returns a vector which is the same as the direction in a length of 1
        if (Physics2D.BoxCast(transform.position + dir, new Vector2(0.2f, 0.2f), 0f, dir, diff.magnitude - 1, GameLayers.i.SolidLayer | GameLayers.i.InteractableLayer | GameLayers.i.PlayerLayer) == true) //.magnitude returns length of the vector
        //Above boxcast spaced away from the NPC rather than right next to it as if close it will collide with it
        {
            return false;
        }
        return true; //Otherwise return True
    }

    private bool IsWalkable(Vector3 targetPos) //Determines if player is walking over tiles labelled "SolidObjects"
    {
        if (Physics2D.OverlapCircle(targetPos, 0.2f, GameLayers.i.SolidLayer | GameLayers.i.InteractableLayer) != null) //Means player won't be able to walk on top of designated "solidObjectsLayer" objects and NPCs labelled with the "interactableLayer" layer
        {
            return false;
        }

        return true;
    }

    public void LookTowards(Vector3 targetPos) //Makes the character looks towards the target position passed - For NPCs looking towards Player in dialogue
    {
        var xdiff = Mathf.Floor(targetPos.x) - Mathf.Floor(transform.position.x); //Math.Floor ensures returned values are in integers
        var ydiff = Mathf.Floor(targetPos.y) - Mathf.Floor(transform.position.y);

        if (xdiff == 0 || ydiff == 0) //To block diagonal directions - LookTowards function will only work if x or ydiff = 0
        {
            animator.MoveX = Mathf.Clamp(xdiff, -1f, 1f);
            animator.MoveY = Mathf.Clamp(ydiff, -1f, 1f);
        }
        else
        {
            Debug.LogError("Error in LookTowards: You don't ask the character to look diagonally");
        }
    }

    public CharacterAnimator Animator
    {
        get => animator;
    }
}
