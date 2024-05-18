using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class Inventory : MonoBehaviour
{
    [SerializeField] List<ItemSlot> slots;

    public event Action OnUpdated;

    public List<ItemSlot> Slots => slots;

    //public ItemBase GetItem(int itemIndex, int categoryIndex)
    //{

    //}

    public ItemBase UseItem(int itemIndex, Pokemon selectedPokemon)
    {
        var item = slots[itemIndex].Item;
        bool itemUsed = item.Use(selectedPokemon);
        if (itemUsed)
        {
            RemoveItem(item);
            return item;
        }

        return null;
    }

    public void AddItem(ItemBase item, int count = 1) //Incomplete - From #64
    {
        var itemSlot = slots.First(slot => slot.Item == item);
        if (itemSlot != null)
        {
            itemSlot.Count += count;
        }
        //NOTE: Do not add items that the player doesn't already have. Code logic for item types incomplete due to time constraints meaning new types of items will not be added to inventory.
        //Temporary Fix: Have items set to 0 in inventory

        OnUpdated?.Invoke();
    }

    public void RemoveItem(ItemBase item)
    {
        var itemSlot = slots.First(slot => slot.Item == item);
        itemSlot.Count--;
        if (itemSlot.Count == 0)
        {
            slots.Remove(itemSlot);
        }

        OnUpdated?.Invoke();
    }

    //public bool HasItem(ItemBase item)
    //{
    //    var itemSlot = slots.First(slot => slot.Item == item);

    //    //NOTE: Code logic incomplete
    //}

    public static Inventory GetInventory()
    {
        return FindObjectOfType<PlayerController>().GetComponent<Inventory>();
    }
}

[Serializable]
public class ItemSlot
{
    [SerializeField] ItemBase item;
    [SerializeField] int count;

    public ItemBase Item => item;
    public int Count
    {
        get => count;
        set => count = value;
    }

}