using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum BattleState { Start, PlayerAction, PlayerMove, EnemyMove, Busy }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleHUD playerHUD;

    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHUD enemyHUD;

    [SerializeField] BattleDialogueBox dialogueBox;

    [SerializeField] PartyScreen partyScreen;

    public event Action<bool> OnBattleOver; //Action<bool> used to determine whether the player won or lost the battle

    BattleState state;
    int currentAction;
    int currentMove;

    PokemonParty playerParty;
    Pokemon wildPokemon;

    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        playerUnit.Setup(playerParty.GetHealthyPokemon());
        playerHUD.SetData(playerUnit.Pokemon);

        enemyUnit.Setup(wildPokemon);
        enemyHUD.SetData(enemyUnit.Pokemon);

        partyScreen.Init();

        dialogueBox.SetMoveNames(playerUnit.Pokemon.Moves);

        yield return dialogueBox.TypeDialogue($"A wild {enemyUnit.Pokemon.Base.Name} appeared!"); //$ acts the same as f string in python. "StartCourotine" present as TypeDialogue is a coroutine. Also yield return means the coroutine will wait for this line to complete before continuing to next line

        PlayerAction();
    }

    void PlayerAction()
    {
        state = BattleState.PlayerAction;
        dialogueBox.SetDialogue("Choose an action");
        dialogueBox.EnableActionSelector(true);
    }

    void OpenPartyScreen()
    {
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
    }

    void PlayerMove()
    {
        state = BattleState.PlayerMove;
        dialogueBox.EnableActionSelector(false);
        dialogueBox.EnableDialogueText(false);
        dialogueBox.EnableMoveSelector(true);
    }

    IEnumerator PerformPlayerMove()
    {
        state = BattleState.Busy; //Ensures player won't be able to change the value of currentMove variable

        var move = playerUnit.Pokemon.Moves[currentMove];
        move.PP--; //Decreases PP value by 1
        yield return dialogueBox.TypeDialogue($"{playerUnit.Pokemon.Base.Name} used {move.Base.Name}!");

        playerUnit.PlayAttackAnimation(); //Plays attack animation (from BattleUnit.cs script)
        yield return new WaitForSeconds(1f);

        enemyUnit.PlayHitAnimation(); //Plays hit animation

        var damageDetails = enemyUnit.Pokemon.TakeDamage(move, playerUnit.Pokemon); //Applies damage to enemy pokemon and returns whether they fainted as a result or not
        yield return enemyHUD.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted)
        {
            yield return dialogueBox.TypeDialogue($"{enemyUnit.Pokemon.Base.Name} fainted!");
            enemyUnit.PlayFaintAnimation(); //Plays faint animation

            yield return new WaitForSeconds(2f);
            OnBattleOver(true); //True bool to indicate player won the battle
        }
        else
        {
            StartCoroutine(EnemyMove());
        }
    }

    IEnumerator EnemyMove()
    {
        state = BattleState.EnemyMove;

        var move = enemyUnit.Pokemon.GetRandomMove();
        move.PP--; //Decreases PP value by 1
        yield return dialogueBox.TypeDialogue($"{enemyUnit.Pokemon.Base.Name} used {move.Base.Name}!");

        enemyUnit.PlayAttackAnimation(); //Plays attack animation
        yield return new WaitForSeconds(1f);

        playerUnit.PlayHitAnimation(); //Plays hit animation

        var damageDetails = playerUnit.Pokemon.TakeDamage(move, playerUnit.Pokemon); //Applies damage to enemy pokemon and returns whether they fainted as a result or not
        yield return playerHUD.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted)
        {
            yield return dialogueBox.TypeDialogue($"{playerUnit.Pokemon.Base.Name} fainted!");
            playerUnit.PlayFaintAnimation(); //Plays faint animation

            yield return new WaitForSeconds(2f);

            var nextPokemon = playerParty.GetHealthyPokemon();
            if (nextPokemon != null)
            {
                playerUnit.Setup(nextPokemon);
                playerHUD.SetData(nextPokemon);

                dialogueBox.SetMoveNames(nextPokemon.Moves);

                yield return dialogueBox.TypeDialogue($"Go {nextPokemon.Base.Name}!"); //$ acts the same as f string in python. "StartCourotine" present as TypeDialogue is a coroutine. Also yield return means the coroutine will wait for this line to complete before continuing to next line

                PlayerAction();
            }
            else
            {
                OnBattleOver(false); //False bool to indicate player lost the battle
            }
        }
        else
        {
            PlayerAction();
        }
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
        {
            yield return dialogueBox.TypeDialogue("A critical hit!");
        }

        if (damageDetails.TypeEffectiveness > 1f)
        {
            yield return dialogueBox.TypeDialogue("It's super effective!");
        }
        else if (damageDetails.TypeEffectiveness < 1f)
        {
            yield return dialogueBox.TypeDialogue("It's not very effective!");
        }
    }

    public void HandleUpdate()
    {
        if (state == BattleState.PlayerAction)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.PlayerMove)
        {
            HandleMoveSelection();
        }
    }

    void HandleActionSelection() //As actions are sorted by index values of 0, 1, etc, we can use this convention to be able to track what action the player is selecting
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentAction += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentAction -= 2;
        }

        currentAction = Mathf.Clamp(currentAction, 0, 3); //Ensures currentAction doesn't exceed 3. Modify 3rd value if seeking to increase or decrease action options 

        dialogueBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z)) //MAKE SURE TO CHANGE! - When Z key is pressed during action selection, the dialogue box will show moves available. CHANGE THIS TO ENTER KEY! Z key only present in line for tutorial.
        {
            if (currentAction == 0)
            {
                //Fight
                PlayerMove();
            }
            else if (currentAction == 1)
            {
                //Bag
            }
            else if (currentAction == 2)
            {
                //Pokemon
                OpenPartyScreen();
            }
            else if (currentAction == 3)
            {
                //Run
            }    
        }
    }

    void HandleMoveSelection() //Works the same as how HandleActionSelection() works
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentMove += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentMove -= 2;
        }

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Pokemon.Moves.Count - 1); //Ensures currentAction doesn't exceed 3

        dialogueBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z)) //Change to Enter in future
        {
            dialogueBox.EnableMoveSelector(false);
            dialogueBox.EnableDialogueText(true);
            StartCoroutine(PerformPlayerMove());
        }
        else if (Input.GetKeyDown(KeyCode.X)) //Returns player to PlayerAction() state if press X
        {
            dialogueBox.EnableMoveSelector(false);
            dialogueBox.EnableDialogueText(true);
            PlayerAction();
        }
    }
}
