using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class Portal : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] int sceneToLoad = -1;
    [SerializeField] DestinationIdentifier destinationPortal;
    [SerializeField] Transform spawnPoint;

    PlayerController player;

    public void OnPlayerTriggered(PlayerController player)
    {
        //Debug.Log("Player entered the portal");
        player.Character.Animator.IsMoving = false;
        this.player = player;
        StartCoroutine(SwitchScene());
    }

    public bool TriggerRepeatedly => false;

    Fader fader;

    private void Start()
    {
        fader = FindObjectOfType<Fader>();
    }

    IEnumerator SwitchScene()
    {
        DontDestroyOnLoad(gameObject); //Ensures portal isn't destroyed when switching scenes

        GameController.Instance.PauseGame(true);
        yield return fader.FadeIn(0.5f);

        yield return SceneManager.LoadSceneAsync(sceneToLoad); //LoadSceneAsync similar to coroutine: will wait until a scene has fully loaded
        //Debug.Log("Logging from portal after scene switch");

        var destPortal = FindObjectsOfType<Portal>().First(x => x != this && x.destinationPortal == this.destinationPortal); //Returns first next portal in scene other than the portal itself that also has the same DestinationIdentifier
        player.Character.SetPositionAndSnapToTile(destPortal.spawnPoint.position);

        yield return fader.FadeOut(0.5f);
        GameController.Instance.PauseGame(false);

        Destroy(gameObject); //Portal won't be destroyed until all game logic has been completed
    }

    public Transform SpawnPoint => spawnPoint;
}

public enum DestinationIdentifier { A, B, C, D, E } //Specifically determines which portal to teleport to
