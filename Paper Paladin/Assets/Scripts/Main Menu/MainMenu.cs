using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using DG.Tweening;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject Menu;
    List<TextMeshProUGUI> mainMenuItems;

    [SerializeField] GameObject ControlsMenu;
    [SerializeField] private Button DefaultControlsMenuButton;

    [SerializeField] AudioSource MusicPlayer;
    [SerializeField] AudioSource SFXPlayer;
    [SerializeField] AudioClip SelectMoveSound;
    [SerializeField] AudioClip SelectSound;
    [SerializeField] AudioClip BackSound;
    [SerializeField] AudioClip StartSound;

    [SerializeField] Color highlightedColour;

    [SerializeField] Image blackImage;

    int selectedItem = 0;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked; //Disables mouse when opening menu
        mainMenuItems = Menu.GetComponentsInChildren<TextMeshProUGUI>().ToList();
    }

    private void Update()
    {
        int prevSelection = selectedItem;

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ++selectedItem;
            SFXPlayer.PlayOneShot(SelectMoveSound);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            --selectedItem;
            SFXPlayer.PlayOneShot(SelectMoveSound);
        }

        selectedItem = Mathf.Clamp(selectedItem, 0, mainMenuItems.Count - 1);

        if (prevSelection != selectedItem || selectedItem == 0)
        {
            UpdateItemSelection();
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (selectedItem == 0)
            {
                MusicPlayer.Stop();
                SFXPlayer.PlayOneShot(StartSound);
                StartCoroutine(PlayGame());
            }
            else if (selectedItem == 1) //Player selects controls menu
            {
                SFXPlayer.PlayOneShot(SelectSound);
                Menu.gameObject.SetActive(false);
                ControlsMenu.gameObject.SetActive(true);
                DestinationButton(DefaultControlsMenuButton);
            }
            else if (selectedItem == 2)
            {
                QuitGame();
            }
        }
    }

    IEnumerator PlayGame()
    {
        yield return new WaitForSeconds(4f);

        yield return blackImage.DOFade(1, 1f).WaitForCompletion();
        yield return new WaitForSeconds(2f);

        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1); //Plays next scene by finding scene that's +1 build setting index from the current scene the script is in
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void DestinationButton(Button destinationButton)
    {
        destinationButton.Select();
    }

    void UpdateItemSelection()
    {
        for (int i = 0; i < mainMenuItems.Count; i++)
        {
            if (i == selectedItem) //
            {
                mainMenuItems[i].color = highlightedColour;
            }
            else
            {
                mainMenuItems[i].color = Color.black;

            }
        }
    }



}
