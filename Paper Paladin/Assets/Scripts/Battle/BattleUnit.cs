using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; //DOTween

public class BattleUnit : MonoBehaviour
{
    //[SerializeField] PokemonBase _base;
    //[SerializeField] int level;
    [SerializeField] bool isPlayerUnit;
    [SerializeField] BattleHUD hud;

    public bool IsPlayerUnit
    {
        get { return isPlayerUnit; }
    }

    public BattleHUD HUD
    {
        get { return hud; }
    }

    public Pokemon Pokemon { get; set; }

    Image image;
    Vector3 originalPos;
    Color originalColour;

    private void Awake()
    {
        image = GetComponent<Image>(); //Stored in a variable as I use this line a lot in this script
        originalPos = image.transform.localPosition; //localPosition ensures originalPos is image position in the canvas not the world
        originalColour = image.color;
    }

    public void Setup(Pokemon pokemon)
    {
        Pokemon = pokemon;
        if (isPlayerUnit)
        {
            image.sprite = Pokemon.Base.BackSprite;
        }
        else
        {
            image.sprite = Pokemon.Base.FrontSprite;
        }

        hud.gameObject.SetActive(true);
        hud.SetData(pokemon);

        transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
        image.color = originalColour; //Reverts image to original color - this is to revert the faint transition if initiating another battle as originally the respective image would remain transparent

        PlayEnterAnimation();
    }

    public void Clear()
    {
        hud.gameObject.SetActive(false);
    }

    public void PlayEnterAnimation()
    {
        if (isPlayerUnit)
        {
            image.transform.localPosition = new Vector3(-1000f, originalPos.y); //This may be required to be changed following tutorial setup if I decide to inrease resolution  
        }
        else
        {
            image.transform.localPosition = new Vector3(1000f, originalPos.y);
        }

        image.transform.DOLocalMoveX(originalPos.x, 1f); //1st value is where the DOTween should move, and the 2nd is how long the transition should last
    }

    public void PlayAttackAnimation()
    {
        var sequence = DOTween.Sequence(); //DOTween.Sequence() allows multiple animations to be played one by one/sequentially
        //Moves player image right when attacking, moves enemy image left when attacking
        if (isPlayerUnit)
        {
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x + 50f, 0.15f)); //1: Transition location, 2: Transition duration, as stated above
        }
        else 
        {
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x - 50f, 0.15f));
        }

        sequence.Append(image.transform.DOLocalMoveX(originalPos.x, 0.25f)); //Returns respective image to original position
    }

    public void PlayHitAnimation()
    {
        var Sequence = DOTween.Sequence();
        Sequence.Append(image.DOColor(Color.gray, 0.1f));
        Sequence.Append(image.DOColor(originalColour, 0.1f));
    }

    public void PlayFaintAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.transform.DOLocalMoveY(originalPos.y - 150f, 0.25f));
        sequence.Join(image.DOFade(0f, 0.25f)); //Join() instead of Append() as doing Append() would make fade transition occur after the image transition downwards had completed. By  doing Join(), these transitions will instead play concurrently
    }

    public IEnumerator PlayCaptureAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOFade(0, 0.5f));
        sequence.Join(transform.DOLocalMoveY(originalPos.y + 50f, 0.5f));
        sequence.Join(transform.DOScale(new Vector3(0.3f, 0.3f, 1f), 0.5f));
        yield return sequence.WaitForCompletion();
    }

    public IEnumerator PlayBreakOutAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOFade(1, 0.5f));
        sequence.Join(transform.DOLocalMoveY(originalPos.y, 0.5f));
        sequence.Join(transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f));
        yield return sequence.WaitForCompletion();
    }

}
