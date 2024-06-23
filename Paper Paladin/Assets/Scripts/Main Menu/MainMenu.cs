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
    [SerializeField] private Button PlayMenuButton;
    [SerializeField] private Button ControlsMenuButton;
    [SerializeField] private Button QuitMenuButton;

    [SerializeField] AudioSource MusicPlayer;
    [SerializeField] AudioSource SFXPlayer;
    [SerializeField] AudioClip SelectMoveSound;
    [SerializeField] AudioClip SelectSound;
    [SerializeField] AudioClip BackSound;
    [SerializeField] AudioClip StartSound;

    [SerializeField] Color highlightedColour;
    [SerializeField] Color playSelectedColour;

    [SerializeField] Image blackImage;
    [SerializeField] Image introBlackImage;
    [SerializeField] Image gameLogo;

    int selectedItem = 0;
    bool playButtonPressed = false;

    int interval = 0;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked; //Disables mouse when opening menu
        mainMenuItems = Menu.GetComponentsInChildren<TextMeshProUGUI>().ToList();
        StartCoroutine(MenuAwakeFade());
    }

    private void FixedUpdate()
    {
        if (playButtonPressed == true)
        {
            interval += 1;
            PlayButtonSelectedAnimation();
        }
    }

    private void Update()
    {
        if (playButtonPressed == false)
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
                    playButtonPressed = true;
                    mainMenuItems[0].color = Color.black;
                    MusicPlayer.Stop();
                    SFXPlayer.PlayOneShot(StartSound);
                    StartCoroutine(PlayGame());
                }
                else if (selectedItem == 1) //Player selects controls menu
                {
                    SFXPlayer.PlayOneShot(SelectSound);
                    Menu.gameObject.SetActive(false);
                    gameLogo.gameObject.SetActive(false);
                    ControlsMenu.gameObject.SetActive(true);
                    DestinationButton(DefaultControlsMenuButton);
                }
                else if (selectedItem == 2)
                {
                    QuitGame();
                }
            }
        }
        else
        {
            DestinationButton(PlayMenuButton);
        }
    }

    IEnumerator PlayGame()
    {
        yield return new WaitForSeconds(4f);

        yield return blackImage.DOFade(1, 0.5f).WaitForCompletion();
        yield return new WaitForSeconds(2f);

        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1); //Plays next scene by finding scene that's +1 build setting index from the current scene the script is in
    }

    IEnumerator MenuAwakeFade()
    {
        yield return introBlackImage.DOFade(0, 1f);
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
        //Debug.Log("Ran UpdateItemSelection");

        if (selectedItem == 0)
        {
            DestinationButton(PlayMenuButton);
        }
        else if (selectedItem == 1)
        {
            DestinationButton(ControlsMenuButton);
        }
        else if (selectedItem == 2)
        {
            DestinationButton(QuitMenuButton);
        }

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

    void PlayButtonSelectedAnimation()
    {
        if (interval == 6)
        {
            mainMenuItems[0].color = highlightedColour;
            Debug.Log("Changed play colour to playSelectedColour");
        }
        else if (interval == 12)
        {
            mainMenuItems[0].color = playSelectedColour;
            Debug.Log("Changed play colour to highlightedColour");
            interval = 0;
            Debug.Log("Reset interval to 0");
        }
    }
    //Original blue colour: 1E89AD

}
