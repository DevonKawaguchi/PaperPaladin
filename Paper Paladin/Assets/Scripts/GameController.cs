using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { FreeRoam, Battle, Dialogue, Cutscene, Paused }

public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;

    GameState state;
    GameState stateBeforePause;

    public SceneDetails CurrentScene { get; private set; }
    public SceneDetails PrevScene { get; private set; }

    public static GameController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        PokemonDB.Init();
        MoveDB.Init();
        ConditionsDB.Init();
    }

    private void Start()
    {
        battleSystem.OnBattleOver += EndBattle;


        DialogueManager.Instance.OnShowDialogue += () =>
        {
            state = GameState.Dialogue;
        };

        DialogueManager.Instance.OnCloseDialogue += () =>
        {
            if (state == GameState.Dialogue)
            {
                state = GameState.FreeRoam;
            }
        };
    }

    public void PauseGame(bool pause) //Assigns states when pausing or resuming the game
    {
        if (pause)
        {
            stateBeforePause = state;
            state = GameState.Paused;
        }
        else
        {
            state = stateBeforePause;
        }
    }

    public void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<PokemonParty>();
        var wildPokemon = CurrentScene.GetComponent<MapArea>().GetRandomWildPokemon();

        var wildPokemonCopy = new Pokemon(wildPokemon.Base, wildPokemon.Level); //Ensures random enemy is a copy of the map area's

        battleSystem.StartBattle(playerParty, wildPokemonCopy);
    }

    TrainerController trainer;

    public void StartTrainerBattle(TrainerController trainer)
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        this.trainer = trainer;

        var playerParty = playerController.GetComponent<PokemonParty>();
        var trainerParty = trainer.GetComponent<PokemonParty>();

        battleSystem.StartTrainerBattle(playerParty, trainerParty);
    }

    public void OnEnterTrainersView(TrainerController trainer)
    {
        state = GameState.Cutscene;
        StartCoroutine(trainer.TriggerTrainerBattle(playerController));
    }

    void EndBattle(bool won)
    {
        if (trainer != null && won == true)
        {
            trainer.BattleLost(); //Disables Trainer FOV when Player wins against Trainer
            trainer = null;
        }

        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
    }

    //Ensures player can't move in freeroam and battle at the same time by enabling update functions for the respective "FreeRoam" or "Battle" state depending on whether the player is in battle or not
    private void Update()
    {
        if (state == GameState.FreeRoam)
        {
            playerController.HandleUpdate();

            if (Input.GetKeyDown(KeyCode.S))
            {
                SavingSystem.i.Save("saveSlot1");
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                SavingSystem.i.Load("saveSlot1");
            }
        }
        else if (state == GameState.Battle) //If battle is triggered, enable BattleSystem update functions. This thus disables PlayerController update functions
        {
            battleSystem.HandleUpdate();
        }
        else if (state == GameState.Dialogue)
        {
            DialogueManager.Instance.HandleUpdate();
        }
    }

    public void SetCurrentScene(SceneDetails currScene)
    {
        PrevScene = CurrentScene;
        CurrentScene = currScene;
    }
}
