using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public enum InventoryUIState { ItemSelection, PartySelection, Busy }

public class InventoryUI : MonoBehaviour
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;

    [SerializeField] Image itemIcon;
    [SerializeField] TextMeshProUGUI itemDescription;

    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;

    [SerializeField] PartyScreen partyScreen;

    int selectedItem = 0;

    InventoryUIState state;

    const int itemsInViewport = 7;

    List<ItemSlotUI> slotUIList;

    Inventory inventory;

    RectTransform itemListRect;

    private void Awake()
    {
        inventory = Inventory.GetInventory();
        itemListRect = itemList.GetComponent<RectTransform>();
    }

    private void Start()
    {
        UpdateItemList();

        inventory.OnUpdated += UpdateItemList; //Updates the item list
    }

    void UpdateItemList()
    {
        //Clear all the existing items
        foreach (Transform child in itemList.transform) //Destroys all ItemUI children game objects when running game
        {
            Destroy(child.gameObject);
        }

        slotUIList = new List<ItemSlotUI>();

        foreach (var itemSlot in inventory.Slots) //For each slot, instantiate a slotUI prefab and attach to itemList game object
        {
            var slotUIObj = Instantiate(itemSlotUI, itemList.transform);
            slotUIObj.SetData(itemSlot);

            slotUIList.Add(slotUIObj);
        }

        UpdateItemSelection();
    }

    public void HandleUpdate(Action onBack)
    {
        if (state == InventoryUIState.ItemSelection)
        {
            int prevSelection = selectedItem;

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                ++selectedItem;
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                --selectedItem;
            }

            selectedItem = Mathf.Clamp(selectedItem, 0, inventory.Slots.Count - 1);

            if (prevSelection != selectedItem)
            {
                UpdateItemSelection();
            }

            if (Input.GetKey(KeyCode.Z))
            {
                OpenPartyScreen();
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                onBack?.Invoke();
            }
        }
        else if (state == InventoryUIState.PartySelection)
        {
            //Handle party selection
            Action onSelected = () =>
            {
                //Use the items on the selected Pokemon
                StartCoroutine(UseItem());
            };

            Action onBackPartyScreen = () =>
            {
                ClosePartyScreen();
            };

            partyScreen.HandleUpdate(onSelected, onBackPartyScreen);
        }
    }

    IEnumerator UseItem()
    {
        state = InventoryUIState.Busy;

        //yield return HandleTMItems();

        var usedItem = inventory.UseItem(selectedItem, partyScreen.SelectedMember);
        if (usedItem != null) //If true, item was used
        {
            yield return DialogueManager.Instance.ShowDialogueText($"The player used {usedItem.Name}");
        }
        else
        {
            yield return DialogueManager.Instance.ShowDialogueText($"It won't have any effect!");
        }

        ClosePartyScreen();
    }

    //IEnumerator HandleTMItems()
    //{
    //    selectedItem 
    //}

    void UpdateItemSelection()
    {
        for (int i = 0; i < slotUIList.Count; i++)
        {
            if (i == selectedItem)
            {
                slotUIList[i].NameText.color = GlobalSettings.i.HighlightedColour;
            }
            else
            {
                slotUIList[i].NameText.color = Color.black;

            }
        }

        //Sets the item's sprite and description
        var item = inventory.Slots[selectedItem].Item; 
        itemIcon.sprite = item.Icon;
        itemDescription.text = item.Description;

        HandleScrolling();
    }

    void HandleScrolling()
    {
        if (slotUIList.Count <= itemsInViewport) return; //Only execute below logic if item count is greater than max items able to be viewed in inventory
        {
            float scrollPos = Mathf.Clamp(selectedItem - itemsInViewport / 2, 0, selectedItem) * (slotUIList[0].Height - 5); //Mathf clamp to ensure scrollPos doesn't go below 0
            //-5 in above line is to account for spacing
            itemListRect.localPosition = new Vector2(itemListRect.localPosition.x, scrollPos);

            bool showUpArrow = selectedItem > itemsInViewport / 2;
            upArrow.gameObject.SetActive(showUpArrow);

            bool showDownArrow = selectedItem + itemsInViewport / 2 < slotUIList.Count;
            downArrow.gameObject.SetActive(showDownArrow);
        }
    }

    void OpenPartyScreen()
    {
        state = InventoryUIState.PartySelection;
        partyScreen.gameObject.SetActive(true);
    }

    void ClosePartyScreen()
    {
        state = InventoryUIState.ItemSelection;
        partyScreen.gameObject.SetActive(false);
    }
}
