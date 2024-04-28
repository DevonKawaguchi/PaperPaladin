using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum BattleState { Start, ActionSelection, MoveSelection, PerformMove, Busy, PartyScreen, BattleOver }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;

    [SerializeField] BattleDialogueBox dialogueBox;

    [SerializeField] PartyScreen partyScreen;

    public event Action<bool> OnBattleOver; //Action<bool> used to determine whether the player won or lost the battle

    BattleState state;
    int currentAction;
    int currentMove;
    int currentMember;

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
        enemyUnit.Setup(wildPokemon);

        partyScreen.Init();

        dialogueBox.SetMoveNames(playerUnit.Pokemon.Moves);

        yield return dialogueBox.TypeDialogue($"A wild {enemyUnit.Pokemon.Base.Name} appeared!"); //$ acts the same as f string in python. "StartCourotine" present as TypeDialogue is a coroutine. Also yield return means the coroutine will wait for this line to complete before continuing to next line

        ChooseFirstTurn();
    }

    void ChooseFirstTurn()
    {
        //If player speed greater than or equal to enemy speed stat, player attacks first. If lower, enemy attacks first
        if (playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed)
        {
            ActionSelection();
        }
        else
        {
            StartCoroutine(EnemyMove());
        }
    }

    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        playerParty.Pokemons.ForEach(p => p.OnBattleOver()); //Shorter way of writing foreach using Linq. Resets each stat boosters applied to all pokemons in the party
        OnBattleOver(won);
    }

    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogueBox.SetDialogue("Choose an action");
        dialogueBox.EnableActionSelector(true);
    }

    void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
    }

    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogueBox.EnableActionSelector(false);
        dialogueBox.EnableDialogueText(false);
        dialogueBox.EnableMoveSelector(true);
    }

    IEnumerator PlayerMove()
    {
        state = BattleState.PerformMove; //Ensures player won't be able to change the value of currentMove variable

        var move = playerUnit.Pokemon.Moves[currentMove];
        yield return RunMove(playerUnit, enemyUnit, move);

        //If the battle was not changed by RunMove, then go to next step
        if (state == BattleState.PerformMove)
        {
            StartCoroutine(EnemyMove()); //Begin enemy move after player performs their move
        }
    }

    IEnumerator EnemyMove()
    {
        state = BattleState.PerformMove;

        var move = enemyUnit.Pokemon.GetRandomMove();
        yield return RunMove(enemyUnit, playerUnit, move);

        //If the battle was not changed by RunMove, then go to next step
        if (state == BattleState.PerformMove)
        {
            ActionSelection(); //Return to action selection screen after enemy has performed their move
        }
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        bool canRunMove = sourceUnit.Pokemon.OnBeforeMove();
        if (!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Pokemon);
            yield return sourceUnit.HUD.UpdateHP();
            yield break; //If true, ignore rest of code in this function. This is particularly for paralyse status, and skips the respective pokemon's move
        }
        yield return ShowStatusChanges(sourceUnit.Pokemon);

        move.PP--; //Decreases PP value by 1
        yield return dialogueBox.TypeDialogue($"{sourceUnit.Pokemon.Base.Name} used {move.Base.Name}!");

        if (CheckIfMoveHits(move, sourceUnit.Pokemon, targetUnit.Pokemon))
        {

            sourceUnit.PlayAttackAnimation(); //Plays attack animation (from BattleUnit.cs script)
            yield return new WaitForSeconds(1f);

            targetUnit.PlayHitAnimation(); //Plays hit animation

            //Applies stat boosters to Pokemon
            if (move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(move.Base.Effects, sourceUnit.Pokemon, targetUnit.Pokemon, move.Base.Target);
            }
            else
            {
                var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon); //Applies damage to enemy pokemon and returns whether they fainted as a result or not
                yield return targetUnit.HUD.UpdateHP();
                yield return ShowDamageDetails(damageDetails);
            }

            if (move.Base.Secondaries != null && move.Base.Secondaries.Count > 0 && targetUnit.Pokemon.HP > 0)
            {
                foreach (var secondary in move.Base.Secondaries)
                {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if (rnd <= secondary.Chance)
                    {
                        yield return RunMoveEffects(secondary, sourceUnit.Pokemon, targetUnit.Pokemon, secondary.Target);
                    }
                }
            }

            if (targetUnit.Pokemon.HP <= 0)
            {
                yield return dialogueBox.TypeDialogue($"{targetUnit.Pokemon.Base.Name} fainted!");
                targetUnit.PlayFaintAnimation(); //Plays faint animation
                yield return new WaitForSeconds(2f);

                CheckForBattleOver(targetUnit); //targetUnit becomes faintedUnit in CheckForBattleOver()
            }
        }
        else
        {
            yield return dialogueBox.TypeDialogue($"{sourceUnit.Pokemon.Base.Name}'s attack missed!");
        }

        //Following code allows statuses like burn or poison to hurt the pokemon after the turn
        sourceUnit.Pokemon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Pokemon);
        yield return sourceUnit.HUD.UpdateHP();
        if (sourceUnit.Pokemon.HP <= 0)
        {
            yield return dialogueBox.TypeDialogue($"{sourceUnit.Pokemon.Base.Name} fainted!");
            sourceUnit.PlayFaintAnimation(); //Plays faint animation
            yield return new WaitForSeconds(2f);

            CheckForBattleOver(sourceUnit); //targetUnit becomes faintedUnit in CheckForBattleOver()

        }
    }

    IEnumerator RunMoveEffects(MoveEffects effects, Pokemon source, Pokemon target, MoveTarget moveTarget)
    {
        //Stat boosting
        if (effects.Boosts != null)
        {
            if (moveTarget == MoveTarget.Self)
            {
                source.ApplyBoosts(effects.Boosts);
            }
            else
            {
                target.ApplyBoosts(effects.Boosts);
            }
        }

        //Status condition
        if (effects.Status != ConditionID.none)
        {
            target.SetStatus(effects.Status);
        }

        //Volatile status condition
        if (effects.VolatileStatus != ConditionID.none)
        {
            target.SetVolatileStatus(effects.VolatileStatus);
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    bool CheckIfMoveHits(Move move, Pokemon source, Pokemon target)
    {
        if (move.Base.AlwaysHits)
        {
            return true;
        }

        float moveAccuracy = move.Base.Accuracy;

        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = target.StatBoosts[Stat.Evasion];

        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (accuracy > 0)
        {
            moveAccuracy *= boostValues[accuracy];
        }
        else
        {
            moveAccuracy /= boostValues[-accuracy];
        }

        if (evasion > 0)
        {
            moveAccuracy /= boostValues[evasion];
        }
        else
        {
            moveAccuracy *= boostValues[-evasion];
        }

        return UnityEngine.Random.Range(1, 101) <= moveAccuracy; //If Pokemon accuracy stats greater or equal to random value between 1-100, attack will hit. If lower, attack will miss
    }

    IEnumerator ShowStatusChanges(Pokemon pokemon)
    {
        while (pokemon.StatusChanges.Count > 0)
        {
            var message = pokemon.StatusChanges.Dequeue();
            yield return dialogueBox.TypeDialogue(message);
        }
    }

    void CheckForBattleOver(BattleUnit faintedUnit) //If player unit fainted, check if player has remaining pokemon in party and end battle with defeat if they have none, or end battle with victory if the faintedUnit is an enemy
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextPokemon = playerParty.GetHealthyPokemon();
            if (nextPokemon != null)
            {
                OpenPartyScreen(); //Open party screen when current Pokemon faints. This will allow the player to directly choose which Pokemon they'd like to choose in their party
            }
            else
            {
                BattleOver(false); //False bool to indicate player lost the battle. Initiates when player has no pokemon left in party
            }
        }
        else
        {
            BattleOver(true);
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
        if (state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if (state == BattleState.PartyScreen)
        {
            HandlePartySelection();
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
                MoveSelection();
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
            StartCoroutine(PlayerMove());
        }
        else if (Input.GetKeyDown(KeyCode.X)) //Returns player to PlayerAction() state if press X
        {
            dialogueBox.EnableMoveSelector(false);
            dialogueBox.EnableDialogueText(true);
            ActionSelection();
        }
    }

    void HandlePartySelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentMember;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentMember;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentMember += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentMember -= 2;
        }

        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Pokemons.Count - 1); 

        partyScreen.UpdateMemberSelection(currentMember);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            var selectedMember = playerParty.Pokemons[currentMember];
            if (selectedMember.HP <= 0)
            {
                partyScreen.SetMessageText("You can't send out a fainted pokemon!"); //Displays message
                return; //Does not allow fainted pokemons to be called into battle
            }

            if (selectedMember == playerUnit.Pokemon)
            {
                partyScreen.SetMessageText("You can't switch with the same pokemon!"); //Displays message
                return;
            }

            partyScreen.gameObject.SetActive(false);
            state = BattleState.Busy;
            StartCoroutine(SwitchPokemon(selectedMember));
        }

        else if (Input.GetKeyDown(KeyCode.X))
        {
            partyScreen.gameObject.SetActive(false);
            ActionSelection();
        }
    }

    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        bool currentPokemonFainted = true;

        if (playerUnit.Pokemon.HP > 0)
        {
            currentPokemonFainted = false;
            yield return dialogueBox.TypeDialogue($"Come back {playerUnit.Pokemon.Base.Name}!");
            playerUnit.PlayFaintAnimation(); //Temporary transition for switching pokemon
            yield return new WaitForSeconds(2f);
        }

        //Setup new Pokemon selected
        playerUnit.Setup(newPokemon);
        dialogueBox.SetMoveNames(newPokemon.Moves);
        yield return dialogueBox.TypeDialogue($"Go {newPokemon.Base.Name}!"); //$ acts the same as f string in python. "StartCourotine" present as TypeDialogue is a coroutine. Also yield return means the coroutine will wait for this line to complete before continuing to next line

        //If pokemon switch occurs because of player pokemon fainting, initiate ChooseFirstTurn() (logic explained previously. If switching when current player pokemon is not fainted, enemy allowed to attack after switch by default
        if (currentPokemonFainted)
        {
            ChooseFirstTurn();
        }
        else
        {
            StartCoroutine(EnemyMove());
        }

        StartCoroutine(EnemyMove()); //Start enemy move
    }
}
