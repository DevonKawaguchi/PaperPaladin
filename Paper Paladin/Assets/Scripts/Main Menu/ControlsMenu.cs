using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ControlsMenu : MonoBehaviour
{
    [SerializeField] GameObject controlsMenu;
    List<TextMeshProUGUI> controlsMenuItems;

    [SerializeField] GameObject mainMenu;
    [SerializeField] private Button DefaultMainMenuButton;

    [SerializeField] AudioSource SFXPlayer;
    [SerializeField] AudioClip SelectMoveSound;
    [SerializeField] AudioClip SelectSound;
    [SerializeField] AudioClip BackSound;

    int selectedItem = 0;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked; //Disables mouse when opening menu
        controlsMenuItems = controlsMenu.GetComponentsInChildren<TextMeshProUGUI>().ToList();
    }

    private void Update()
    {
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

        selectedItem = Mathf.Clamp(selectedItem, 0, controlsMenuItems.Count - 1);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (selectedItem == 0)
            {
                controlsMenu.gameObject.SetActive(false);
                mainMenu.gameObject.SetActive(true);
                DestinationButton(DefaultMainMenuButton);
                SFXPlayer.PlayOneShot(BackSound);
            }
        }
        else if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Return))
        {
            controlsMenu.gameObject.SetActive(false);
            mainMenu.gameObject.SetActive(true);
            DestinationButton(DefaultMainMenuButton);
            SFXPlayer.PlayOneShot(BackSound);
        }
    }

    public void DestinationButton(Button destinationButton)
    {
        destinationButton.Select();
    }
}
