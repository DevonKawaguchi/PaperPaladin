using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;

    [SerializeField] Image itemIcon;
    [SerializeField] TextMeshProUGUI itemDescription;

    int selectedItem = 0;

    List<ItemSlotUI> slotUIList;

    Inventory inventory;

    private void Awake()
    {
        inventory = Inventory.GetInventory();

    }

    private void Start()
    {
        UpdateItemList();
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

        if (Input.GetKeyDown(KeyCode.X))
        {
            onBack?.Invoke();
        }
    }

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
    }
}
