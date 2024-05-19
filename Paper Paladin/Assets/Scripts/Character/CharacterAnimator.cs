using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [SerializeField] List<Sprite> walkUpSprites;
    [SerializeField] List<Sprite> walkDownSprites;
    [SerializeField] List<Sprite> walkLeftSprites;
    [SerializeField] List<Sprite> walkRightSprites;
    [SerializeField] FacingDirection defaultDirection = FacingDirection.Down;

    //Parameters
    public float MoveX { get; set; }
    public float MoveY { get; set; }
    public bool IsMoving { get; set; }
    public bool IsJumping { get; set; }

    //States
    SpriteAnimator walkUpAnim;
    SpriteAnimator walkDownAnim;
    SpriteAnimator walkLeftAnim;
    SpriteAnimator walkRightAnim;

    SpriteAnimator currentAnim;
    bool wasPreviouslyMoving;

    //References
    SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        walkUpAnim = new SpriteAnimator(walkUpSprites, spriteRenderer);
        walkDownAnim = new SpriteAnimator(walkDownSprites, spriteRenderer);
        walkLeftAnim = new SpriteAnimator(walkLeftSprites, spriteRenderer);
        walkRightAnim = new SpriteAnimator(walkRightSprites, spriteRenderer);
        SetFacingDirection(defaultDirection);

        currentAnim = walkDownAnim;
    }

    private void Update()
    {
        var prevAnim = currentAnim;

        if (MoveX == 1)
        {
            currentAnim = walkRightAnim; //
        }
        else if (MoveX == -1)
        {
            currentAnim = walkLeftAnim;
        }
        else if (MoveY == 1)
        {
            currentAnim = walkUpAnim;
        }
        else if (MoveY == -1)
        {
            currentAnim = walkDownAnim;
        }

        if (currentAnim != prevAnim || IsMoving != wasPreviouslyMoving) //"IsMoving != wasPreviouslyMoving" makes it so that if the player makes small movements, an short animation will still play. Previously, if this happened, the player sprite would "slide" across the screen without initiating an animation.
        {
            currentAnim.Start();
        }

        if (IsJumping)
        {
            spriteRenderer.sprite = currentAnim.Frames[currentAnim.Frames.Count - 1]; //Jump animation is last frame of walk animation
        }
        else if (IsMoving) //Won't execute if character is jumping
        {
            currentAnim.HandleUpdate();
        }
        else
        {
            spriteRenderer.sprite = currentAnim.Frames[0];
        }

        wasPreviouslyMoving = IsMoving; 
    }

    public void SetFacingDirection(FacingDirection dir)
    {
        //For cutscene animations. Fixes error where character sometimes wouldn't turn in the intended direction. Occurred as MoveX/MoveY values would overlap in else-ifs
        MoveX = 0;
        MoveY = 0;

        if (dir == FacingDirection.Right)
        {
            MoveX = 1;
        }
        else if (dir == FacingDirection.Left)
        {
            MoveX = -1;
        }
        else if (dir == FacingDirection.Down)
        {
            MoveY = -1;
        }
        else if (dir == FacingDirection.Up)
        {
            MoveY = 1;
        }
    }

    public FacingDirection DefaultDirection
    {
        get => defaultDirection;
    }
}

public enum FacingDirection { Up, Down, Left, Right }
