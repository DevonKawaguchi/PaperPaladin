using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

//Teleports the player to a different position without switching scenes

public class LocationPortal : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] DestinationIdentifier destinationPortal;
    [SerializeField] Transform spawnPoint;

    PlayerController player;

    public void OnPlayerTriggered(PlayerController player)
    {
        //Debug.Log("Player entered the portal");
        player.Character.Animator.IsMoving = false;
        this.player = player;
        StartCoroutine(Teleport());
    }

    public bool TriggerRepeatedly => false;

    Fader fader;

    private void Start()
    {
        fader = FindObjectOfType<Fader>();
    }

    IEnumerator Teleport()
    {
        GameController.Instance.PauseGame(true);
        yield return fader.FadeIn(0.5f);

        var destPortal = FindObjectsOfType<LocationPortal>().First(x => x != this && x.destinationPortal == this.destinationPortal);
        player.Character.SetPositionAndSnapToTile(destPortal.spawnPoint.position);

        yield return fader.FadeOut(0.5f);
        GameController.Instance.PauseGame(false);
    }

    public Transform SpawnPoint => spawnPoint;
}
