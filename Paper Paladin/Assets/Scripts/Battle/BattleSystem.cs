using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, PartyScreen, AboutToUse, MoveToForget, BattleOver }
public enum BattleAction { Move, SwitchPokemon, UseItem, Run }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;

    [SerializeField] BattleDialogueBox dialogueBox;

    [SerializeField] PartyScreen partyScreen;

    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;
    [SerializeField] GameObject pokeballSprite;

    [SerializeField] MoveSelectionUI moveSelectionUI;

    [Header("Audio")]
    [SerializeField] AudioClip[] enemyMusic; //Changed
    [SerializeField] AudioClip trainerBattleMusic;
    [SerializeField] AudioClip battleVictoryMusic;

    public event Action<bool> OnBattleOver; //Action<bool> used to determine whether the player won or lost the battle

    //[SerializeField] BattleActor battleActor; //Character that should be moved

    BattleState state;

    int currentAction;
    int currentMove;
    int escapeAttempts;

    MoveBase moveToLearn;

    bool aboutToUseChoice = true;

    PokemonParty playerParty;
    PokemonParty trainerParty;
    Pokemon wildPokemon;

    bool isTrainerBattle = false;
    PlayerController player;
    TrainerController trainer;

    public MapArea mapArea1;

    public GlobalGameIndex globalGameIndex;
    //public GameObject[] longGrassList;

    public LongGrass longGrassScript;

    //[SerializeField] List<Vector2> movementPattern;

    //public int enemyMusicIndex = -1;

    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;

        player = playerParty.GetComponent<PlayerController>();
        isTrainerBattle = false;

        AudioManager.i.PlayMusic(enemyMusic[globalGameIndex.enemyMusicIndex], true, false);

        StartCoroutine(SetupBattle());
    }

    public void StartTrainerBattle(PokemonParty playerParty, PokemonParty trainerParty)
    {
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;

        isTrainerBattle = true;
        player = playerParty.GetComponent<PlayerController>();
        trainer = trainerParty.GetComponent<TrainerController>();

        AudioManager.i.PlayMusic(trainerBattleMusic);

        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        playerUnit.Clear(); //Makes HUD not shown at start of battle
        enemyUnit.Clear();

        if (!isTrainerBattle)
        {
            //Wild Pokemon Battle
            playerUnit.Setup(playerParty.GetHealthyPokemon());
            enemyUnit.Setup(wildPokemon);

            dialogueBox.SetMoveNames(playerUnit.Pokemon.Moves);
            //AudioManager.i.PlaySFX(AudioID.UISelectionMove);
            yield return dialogueBox.TypeDialogue($"{enemyUnit.Pokemon.Base.Name} appeared!"); //$ acts the same as f string in python. "StartCourotine" present as TypeDialogue is a coroutine. Also yield return means the coroutine will wait for this line to complete before continuing to next line

            for (int i = 0; i < (playerUnit.Pokemon.Moves.Count - 1); ++i) //Returns all player moves to original values
            {
                Debug.Log($"Original {playerUnit.Pokemon.Moves[i]} PP is {playerUnit.Pokemon.Moves[i].PP} though is now {playerUnit.Pokemon.Moves[i].Base.PP}");
                playerUnit.Pokemon.Moves[i].PP = playerUnit.Pokemon.Moves[i].Base.PP;
            }

            for (int i = 0; i < (enemyUnit.Pokemon.Moves.Count - 1); ++i) //Returns all enemy moves to original values
            {
                Debug.Log($"Original {enemyUnit.Pokemon.Moves[i]} PP is {enemyUnit.Pokemon.Moves[i].PP} though is now {playerUnit.Pokemon.Moves[i].Base.PP}");
                enemyUnit.Pokemon.Moves[i].PP = enemyUnit.Pokemon.Moves[i].Base.PP;
            }
        }
        else
        {
            //Trainer Battle

            //Show Trainer and Player sprites
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);

            playerImage.sprite = player.Sprite;
            trainerImage.sprite = trainer.Sprite;

            yield return dialogueBox.TypeDialogue($"WARNING! Approaching {trainer.Name}!");
            yield return dialogueBox.TypeDialogue($"Activating S-ARM shields...");
            yield return dialogueBox.TypeDialogue($"Impact in 3...");
            yield return dialogueBox.TypeDialogue($"Impact in 2...");
            yield return dialogueBox.TypeDialogue($"Impact in 1...");

            //Send out the Trainer's first Pokemon
            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var enemyPokemon = trainerParty.GetHealthyPokemon();
            enemyUnit.Setup(enemyPokemon);
            yield return dialogueBox.TypeDialogue($"{enemyPokemon.Base.Name} appeared!");

            //Send out the Player's first Pokemon
            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerPokemon = playerParty.GetHealthyPokemon();
            playerUnit.Setup(playerPokemon);
            //yield return dialogueBox.TypeDialogue($"Go {playerPokemon.Base.Name}!");
            dialogueBox.SetMoveNames(playerUnit.Pokemon.Moves);
        }

        escapeAttempts = 0;
        partyScreen.Init();
        ActionSelection();
    }

    void BattleOver(bool won)
    {
        Debug.Log("Battle finished");
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
        partyScreen.CalledFrom = state;
        state = BattleState.PartyScreen;
        partyScreen.gameObject.SetActive(true);
    }

    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogueBox.EnableActionSelector(false);
        dialogueBox.EnableDialogueText(false);
        dialogueBox.EnableMoveSelector(true);
    }

    IEnumerator AboutToUse(Pokemon newPokemon)
    {
        state = BattleState.Busy;
        //yield return dialogueBox.TypeDialogue($"{trainer.Name} is about to use {newPokemon.Base.Name}. Do you want to change Pokemon?");
        yield return dialogueBox.TypeDialogue($"Objective located. Would you like to align the destroyer towards the superstorm?");

        state = BattleState.AboutToUse;
        dialogueBox.EnableChoiceBox(true);
    }

    IEnumerator ChooseMoveToForget(Pokemon pokemon, MoveBase newMove)
    {
        state = BattleState.Busy;
        yield return dialogueBox.TypeDialogue($"Choose a move you want to forget");
        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(pokemon.Moves.Select(x => x.Base).ToList(), newMove); //Sets all the Move names in the MoveSelection UI
        moveToLearn = newMove;

        state = BattleState.MoveToForget;
    }

    IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleState.RunningTurn;

        if (playerAction == BattleAction.Move)
        {
            playerUnit.Pokemon.CurrentMove = playerUnit.Pokemon.Moves[currentMove];
            enemyUnit.Pokemon.CurrentMove = enemyUnit.Pokemon.GetRandomMove();

            int playerMovePriority = playerUnit.Pokemon.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.Pokemon.CurrentMove.Base.Priority;

            //Check which Pokemon gets the first move
            bool playerGoesFirst = true;

            if (enemyMovePriority > playerMovePriority)
            {
                playerGoesFirst = false;
            }
            else if (enemyMovePriority == playerMovePriority)
            {
                playerGoesFirst = playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed;
            }

            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;

            var secondPokemon = secondUnit.Pokemon;

            //First Turn
            yield return RunMove(firstUnit, secondUnit, firstUnit.Pokemon.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if (state == BattleState.BattleOver) yield break;

            if (secondPokemon.HP > 0)
            {
                //Second Turn - Will only execute if the Pokemon isn't fainted
                yield return RunMove(secondUnit, firstUnit, secondUnit.Pokemon.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (state == BattleState.BattleOver) yield break;
            }
        }
        else
        {
            if (playerAction == BattleAction.SwitchPokemon)
            {
                var selectedPokemon = partyScreen.SelectedMember;
                state = BattleState.Busy;
                yield return SwitchPokemon(selectedPokemon);
            }
            else if (playerAction == BattleAction.UseItem)
            {
                dialogueBox.EnableActionSelector(false);
                yield return ThrowPokeball();
            }
            else if (playerAction == BattleAction.Run)
            {
                yield return TryToEscape();
            }

            //Enemy Turn
            var enemyMove = enemyUnit.Pokemon.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);
            if (state == BattleState.BattleOver) yield break;
        }

        if (state != BattleState.BattleOver)
        {
            ActionSelection();
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
            AudioManager.i.PlaySFX(move.Base.Sound); //Plays move sound

            yield return new WaitForSeconds(1f);

            targetUnit.PlayHitAnimation(); //Plays hit animation
            AudioManager.i.PlaySFX(AudioID.Hit); //Plays hit sound

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
                yield return HandlePokemonFainted(targetUnit);
            }
        }
        else
        {
            AudioManager.i.PlaySFX(AudioID.UISelectionMove);
            yield return dialogueBox.TypeDialogue($"{sourceUnit.Pokemon.Base.Name}'s attack missed!");
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

    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        //Ensures player doesn't recieve any damage after the battle has concluded
        if (state == BattleState.BattleOver) yield break;
        yield return new WaitUntil(() => state == BattleState.RunningTurn);

        //Following code allows statuses like burn or poison to hurt the pokemon after the turn
        sourceUnit.Pokemon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Pokemon);
        yield return sourceUnit.HUD.UpdateHP();
        if (sourceUnit.Pokemon.HP <= 0)
        {
            yield return HandlePokemonFainted(sourceUnit);

            yield return new WaitUntil(() => state == BattleState.RunningTurn);
        }
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

    IEnumerator HandlePokemonFainted(BattleUnit faintedUnit)
    {
        AudioManager.i.PlaySFX(AudioID.UISelectionMove);
        yield return dialogueBox.TypeDialogue($"{faintedUnit.Pokemon.Base.Name} fainted!");
        faintedUnit.PlayFaintAnimation(); //Plays faint animation
        yield return new WaitForSeconds(2f);

        if (!faintedUnit.IsPlayerUnit) //If is Enemy unit
        {
            bool battleWon = true;
            if (isTrainerBattle) //Checks if all trainer Pokemon has fainted before playing victory music
            {
                battleWon = trainerParty.GetHealthyPokemon() == null; //If null, battleWon is true
            }

            if (battleWon)
            {
                globalGameIndex.enemyIndex += 1;
                Debug.Log("Increased areaEnemyNumber by 1");
                globalGameIndex.enemyMusicIndex += 1;
                Debug.Log($"areaEnemyMusicIndex is {globalGameIndex.enemyMusicIndex}");
                //globalGameIndex.longGrassIndex += 1;
                //longGrassList[globalGameIndex.longGrassIndex].SetActive(false);

                //Debug.Log($"longGrass {globalGameIndex.longGrassIndex} was destroyed");

                AudioManager.i.PlayMusic(battleVictoryMusic, false);
                //AudioManager.i.PlaySFX(AudioID.Victory);
            }

            //Exp Gain
            int expYield = faintedUnit.Pokemon.Base.ExpYield;
            int enemyLevel = faintedUnit.Pokemon.Level;
            float trainerBonus = (isTrainerBattle) ? 2f : 1f; //Multiplies XP gained by 2x if Trainer Pokemon

            int expGain = Mathf.FloorToInt(expYield * enemyLevel * trainerBonus);
            playerUnit.Pokemon.Exp += expGain; //Adds XP to Pokemon
            yield return dialogueBox.TypeDialogue($"{playerUnit.Pokemon.Base.Name} gained {expGain} EXP!");
            //AudioManager.i.PlaySFX(AudioID.ExpGain);
            yield return playerUnit.HUD.SetExpSmooth();

            //Check Level Up
            while (playerUnit.Pokemon.CheckForLevelUp()) //While loop ensures multiple level ups are taken into account
            {
                playerUnit.HUD.SetLevel();
                yield return dialogueBox.TypeDialogue($"{playerUnit.Pokemon.Base.Name} grew to Level {playerUnit.Pokemon.Level}!");

                //Try to learn a new Move
                var newMove = playerUnit.Pokemon.GetLearnableMoveAtCurrentLevel();
                if (newMove != null)
                {
                    if (playerUnit.Pokemon.Moves.Count < PokemonBase.MaxNumberOfMoves) //Will only learn a new move if current move count is less than 4
                    {
                        playerUnit.Pokemon.LearnMove(newMove);
                        yield return dialogueBox.TypeDialogue($"{playerUnit.Pokemon.Base.Name} learned {newMove.Base.Name}!");
                        dialogueBox.SetMoveNames(playerUnit.Pokemon.Moves);
                    }
                    else //Player must forget a move before adding a new one if current move count is 4
                    {
                        yield return dialogueBox.TypeDialogue($"{playerUnit.Pokemon.Base.Name} is trying to learn {newMove.Base.Name}");
                        yield return dialogueBox.TypeDialogue($"But, it cannot learn more than {PokemonBase.MaxNumberOfMoves} moves!");
                        yield return ChooseMoveToForget(playerUnit.Pokemon, newMove.Base);
                        yield return new WaitUntil(() => state != BattleState.MoveToForget);
                        yield return new WaitForSeconds(2f);
                    }
                }

                yield return playerUnit.HUD.SetExpSmooth(true); //If Player accrues more XP than the level cap
            }

            yield return new WaitForSeconds(1f);
        }

        CheckForBattleOver(faintedUnit);
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
                LongGrass.battleDefeat = true;
                BattleOver(false); //False bool to indicate player lost the battle. Initiates when player has no pokemon left in party
            }
        }
        else
        {
            if (!isTrainerBattle)
            {
                Destroy(GameObject.Find($"LongGrass - {GlobalGameIndex.longGrassIndex}").transform.gameObject);

                Debug.Log($"LongGrass - {GlobalGameIndex.longGrassIndex} destroyed");

                GlobalGameIndex.longGrassIndex += 1;

                Debug.Log($"LongGrassIndex increased to {GlobalGameIndex.longGrassIndex}");


                BattleOver(true);
            }
            else
            {
                var nextPokemon = trainerParty.GetHealthyPokemon();
                if (nextPokemon != null)
                {
                    //Send out next Pokemon
                    StartCoroutine(AboutToUse(nextPokemon));
                }
                else
                {
                    BattleOver(true);
                }
            }
        }
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
        {
            AudioManager.i.PlaySFX(AudioID.UISelectionMove);
            yield return dialogueBox.TypeDialogue(">A critical hit!");
        }

        if (damageDetails.TypeEffectiveness > 1f)
        {
            AudioManager.i.PlaySFX(AudioID.UISelectionMove);
            yield return dialogueBox.TypeDialogue(">It's super effective!");
        }
        else if (damageDetails.TypeEffectiveness < 1f)
        {
            AudioManager.i.PlaySFX(AudioID.UIExit);
            yield return dialogueBox.TypeDialogue(">It's not very effective!");
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
        else if (state == BattleState.AboutToUse)
        {
            HandleAboutToUse();
        }
        else if (state == BattleState.MoveToForget)
        {
            Action<int> onMoveSelected = (moveIndex) =>
            {
                moveSelectionUI.gameObject.SetActive(false);
                if (moveIndex == PokemonBase.MaxNumberOfMoves)
                {
                    //Don't learn the new move
                    StartCoroutine(dialogueBox.TypeDialogue($"{playerUnit.Pokemon.Base.Name} did not learn {moveToLearn.Name}"));
                }
                else
                {
                    //Forget the selected move and learn the new move
                    var selectedMove = playerUnit.Pokemon.Moves[moveIndex].Base;
                    StartCoroutine(dialogueBox.TypeDialogue($"{playerUnit.Pokemon.Base.Name} forgot {selectedMove.Name} and learned {moveToLearn.Name}!"));

                    playerUnit.Pokemon.Moves[moveIndex] = new Move(moveToLearn);
                }

                moveToLearn = null;
                state = BattleState.RunningTurn;
            };

            moveSelectionUI.HandleMoveSelection(onMoveSelected); //Allows Player to select different moves from the moves list (when choosing to forget a move)

        }

        //if (Input.GetKeyDown(KeyCode.T))
        //{
        //    StartCoroutine(ThrowPokeball());
        //}
    }

    void HandleActionSelection() //As actions are sorted by index values of 0, 1, etc, we can use this convention to be able to track what action the player is selecting
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {

            ++currentAction;
            AudioManager.i.PlaySFX(AudioID.UISelectionMove);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentAction;
            AudioManager.i.PlaySFX(AudioID.UISelectionMove);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentAction += 2;
            AudioManager.i.PlaySFX(AudioID.UISelectionMove);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentAction -= 2;
            AudioManager.i.PlaySFX(AudioID.UISelectionMove);
        }

        currentAction = Mathf.Clamp(currentAction, 0, 3); //Ensures currentAction doesn't exceed 3. Modify 3rd value if seeking to increase or decrease action options 

        dialogueBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z)) //MAKE SURE TO CHANGE! - When Z key is pressed during action selection, the dialogue box will show moves available. CHANGE THIS TO ENTER KEY! Z key only present in line for tutorial.
        {
            if (currentAction == 0)
            {
                //Fight
                AudioManager.i.PlaySFX(AudioID.UISelect);
                MoveSelection();
            }
            else if (currentAction == 1)
            {
                //Bag
                AudioManager.i.PlaySFX(AudioID.UISelect);
                StartCoroutine(RunTurns(BattleAction.UseItem));
            }
            else if (currentAction == 2)
            {
                //Pokemon
                AudioManager.i.PlaySFX(AudioID.UISelect);
                OpenPartyScreen();
            }
            else if (currentAction == 3)
            {
                //Run
                AudioManager.i.PlaySFX(AudioID.UISelect);
                StartCoroutine(RunTurns(BattleAction.Run));
            }
        }
    }

    void HandleMoveSelection() //Works the same as how HandleActionSelection() works
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentMove;
            AudioManager.i.PlaySFX(AudioID.UISelectionMove);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentMove;
            AudioManager.i.PlaySFX(AudioID.UISelectionMove);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentMove += 2;
            AudioManager.i.PlaySFX(AudioID.UISelectionMove);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentMove -= 2;
            AudioManager.i.PlaySFX(AudioID.UISelectionMove);
        }

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Pokemon.Moves.Count - 1); //Ensures currentAction doesn't exceed 3

        dialogueBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z)) //Change to Enter in future
        {
            var move = playerUnit.Pokemon.Moves[currentMove];
            AudioManager.i.PlaySFX(AudioID.UISelect);
            if (move.PP == 0) return; //If true, the function won't run the below code.

            dialogueBox.EnableMoveSelector(false);
            dialogueBox.EnableDialogueText(true);
            StartCoroutine(RunTurns(BattleAction.Move));
        }
        else if (Input.GetKeyDown(KeyCode.X)) //Returns player to PlayerAction() state if press X
        {
            AudioManager.i.PlaySFX(AudioID.UIExit);
            dialogueBox.EnableMoveSelector(false);
            dialogueBox.EnableDialogueText(true);
            ActionSelection();
        }
    }

    void HandlePartySelection()
    {
        Action onSelected = () =>
        {
            var selectedMember = partyScreen.SelectedMember;
            if (selectedMember.HP <= 0)
            {
                AudioManager.i.PlaySFX(AudioID.UIError);
                partyScreen.SetMessageText("You can't send out a fainted party member!"); //Displays message
                return; //Does not allow fainted pokemons to be called into battle
            }

            if (selectedMember == playerUnit.Pokemon)
            {
                AudioManager.i.PlaySFX(AudioID.UIError);
                partyScreen.SetMessageText("You can't switch with the same move!"); //Displays message
                return;
            }

            partyScreen.gameObject.SetActive(false);

            if (partyScreen.CalledFrom == BattleState.ActionSelection)
            {
                StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
            }
            else
            {
                state = BattleState.Busy;
                bool isTrainerAboutToUse = partyScreen.CalledFrom == BattleState.AboutToUse;
                StartCoroutine(SwitchPokemon(selectedMember, isTrainerAboutToUse));
            }
            partyScreen.CalledFrom = null; //Whenever the player presses Z, partyScreen.CalledFrom will be reset
        };

        Action onBack = () =>
        {
            if (playerUnit.Pokemon.HP <= 0)
            {
                AudioManager.i.PlaySFX(AudioID.UIError);
                partyScreen.SetMessageText("You have to choose a move to continue!");
                return;
            }

            partyScreen.gameObject.SetActive(false);

            if (partyScreen.CalledFrom == BattleState.AboutToUse)
            {
                StartCoroutine(SendNextTrainerPokemon());
            }
            else
            {
                ActionSelection();
            }

            partyScreen.CalledFrom = null;
        };

        partyScreen.HandleUpdate(onSelected, onBack);
    }

    void HandleAboutToUse()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            aboutToUseChoice = !aboutToUseChoice;
        }

        dialogueBox.UpdateChoiceBox(aboutToUseChoice);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogueBox.EnableChoiceBox(false);
            if (aboutToUseChoice == true)
            {
                //Yes option
                AudioManager.i.PlaySFX(AudioID.UISelectionMove);
                OpenPartyScreen();
            }
            else
            {
                //No option
                AudioManager.i.PlaySFX(AudioID.UISelectionMove);
                StartCoroutine(SendNextTrainerPokemon());
            }
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogueBox.EnableChoiceBox(false);
            StartCoroutine(SendNextTrainerPokemon());
        }
    }

    IEnumerator SwitchPokemon(Pokemon newPokemon, bool isTrainerAboutToUse = false)
    {
        if (playerUnit.Pokemon.HP > 0)
        {
            yield return dialogueBox.TypeDialogue($"Come back {playerUnit.Pokemon.Base.Name}!");
            playerUnit.PlayFaintAnimation(); //Temporary transition for switching pokemon
            yield return new WaitForSeconds(2f);
        }

        //Setup new Pokemon selected
        playerUnit.Setup(newPokemon);
        dialogueBox.SetMoveNames(newPokemon.Moves);
        yield return dialogueBox.TypeDialogue($"Go {newPokemon.Base.Name}!"); //$ acts the same as f string in python. "StartCourotine" present as TypeDialogue is a coroutine. Also yield return means the coroutine will wait for this line to complete before continuing to next line

        if (isTrainerAboutToUse)
        {
            StartCoroutine(SendNextTrainerPokemon());
        }
        else
        {
            state = BattleState.RunningTurn;
        }
    }


    IEnumerator SendNextTrainerPokemon()
    {
        state = BattleState.Busy;

        var nextPokemon = trainerParty.GetHealthyPokemon();

        enemyUnit.Setup(nextPokemon);
        AudioManager.i.PlaySFX(AudioID.UIError);
        yield return dialogueBox.TypeDialogue($"{trainer.Name} sent out {nextPokemon.Base.Name}!");

        state = BattleState.RunningTurn;
    }

    IEnumerator ThrowPokeball()
    {
        state = BattleState.Busy;

        if (isTrainerBattle)
        {
            yield return dialogueBox.TypeDialogue($"You can't steal the trainer's Pokemon!");
            state = BattleState.RunningTurn;
            yield break; //Break coroutine
        }

        yield return dialogueBox.TypeDialogue($"{player.Name} used POKEBALL!");

        var pokeballObj = Instantiate(pokeballSprite, playerUnit.transform.position - new Vector3(2, 0), Quaternion.identity);
        var pokeball = pokeballObj.GetComponent<SpriteRenderer>();

        //Animations
        yield return pokeball.transform.DOJump(enemyUnit.transform.position + new Vector3(0, 2), 2f, 1, 1f).WaitForCompletion();
        yield return enemyUnit.PlayCaptureAnimation();
        yield return pokeball.transform.DOMoveY(enemyUnit.transform.position.y - 1.3f, 0.5f).WaitForCompletion();

        int shakeCount = TryToCatchPokemon(enemyUnit.Pokemon);
        
        for (int i = 0; i < MathF.Min(shakeCount, 3); ++i) //MathF.Min ensures shakeCount is 0-3
        {
            yield return new WaitForSeconds(0.5f);
            yield return pokeball.transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.8f).WaitForCompletion();
        }

        if (shakeCount == 4)
        {
            //Pokemon is caught
            yield return dialogueBox.TypeDialogue($"{enemyUnit.Pokemon.Base.Name} was caught!");
            yield return pokeball.DOFade(0, 1.5f).WaitForCompletion();

            playerParty.AddPokemon(enemyUnit.Pokemon);
            yield return dialogueBox.TypeDialogue($"{enemyUnit.Pokemon.Base.Name} has been added to your party!");

            Destroy(pokeball);
            BattleOver(true);
        }
        else
        {
            //Pokemon broke out
            yield return new WaitForSeconds(1f);
            pokeball.DOFade(0, 0.2f);
            yield return enemyUnit.PlayBreakOutAnimation();

            if (shakeCount < 2)
            {
                yield return dialogueBox.TypeDialogue($"{enemyUnit.Pokemon.Base.Name} broke free!");
            }
            else
            {
                yield return dialogueBox.TypeDialogue($"You almost caught it!");
            }
            Destroy(pokeball);
            state = BattleState.RunningTurn; //Continues the battle
        }
    }

    int TryToCatchPokemon(Pokemon pokemon)
    {
        float a = (3 * pokemon.MaxHp - 2 * pokemon.HP) * pokemon.Base.CatchRate * ConditionsDB.GetStatusBonus(pokemon.Status) / (3 * pokemon.MaxHp);

        if (a >= 255) //If Pokemon caught (above/= to 255, the Pokeball will shake 4 times
        {
            return 4;
        }

        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

        int shakeCount = 0;
        while (shakeCount < 4)
        {
            if (UnityEngine.Random.Range(0, 65535) >= b)
            {
                break;
            }

            ++shakeCount;
        }

        return shakeCount;
    }

    IEnumerator TryToEscape()
    {
        state = BattleState.Busy;

        if (isTrainerBattle)
        {
            AudioManager.i.PlaySFX(AudioID.UIError);
            yield return dialogueBox.TypeDialogue($"You can't run from trainer battles!");
            state = BattleState.RunningTurn;
            yield break;
        }

        ++escapeAttempts;

        int playerSpeed = playerUnit.Pokemon.Speed;
        int enemySpeed = enemyUnit.Pokemon.Speed;

        if (enemySpeed < playerSpeed)
        {
            AudioManager.i.PlaySFX(AudioID.UISelectionMove);
            yield return dialogueBox.TypeDialogue($"Ran away safely!");
            LongGrass.battleDefeat = true;
            Debug.Log($"battleDefeat is {LongGrass.battleDefeat}");
            //StartCoroutine(MakePlayerMove());

            BattleOver(true);

            //Debug.Log($"Player should move now");
        }
        else
        {
            float f = (playerSpeed * 128) / enemySpeed + 30 * escapeAttempts;
            f = f % 256;

            if (UnityEngine.Random.Range(0, 256) < f)
            {
                AudioManager.i.PlaySFX(AudioID.UISelectionMove);
                yield return dialogueBox.TypeDialogue($"Ran away safely!");
                LongGrass.battleDefeat = true;
                Debug.Log($"battleDefeat is {LongGrass.battleDefeat} 2");
                BattleOver(true);

            }
            else
            {
                AudioManager.i.PlaySFX(AudioID.UIError);
                yield return dialogueBox.TypeDialogue($"Can't escape!");
                state = BattleState.RunningTurn;
            }
        }
    }

    //IEnumerator MakePlayerMove()
    //{
    //    GameController.Instance.StartCutsceneState();

    //    var character = battleActor.GetCharacter();

    //    Debug.Log("Executed MakePlayerMove() coroutine");
    //    yield return character.Move(movementPattern[0]);

    //    GameController.Instance.StartFreeRoamState();

    //    yield break;
    //}
}

//[System.Serializable]
//public class BattleActor
//{
//    [SerializeField] bool isPlayer;
//    [SerializeField] Character character;

//    public Character GetCharacter() => (isPlayer) ? PlayerController.i.Character : character; //Returns character. Otherwise, return character assigned in the field
//}