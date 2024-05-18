using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quest
{
    public QuestBase Base { get; private set; }
    public QuestStatus Status { get; private set; }

    public Quest(QuestBase _base) //Constructor (since this script is pure C# script)
    {
        Base = _base;
    }

    public IEnumerator StartQuest()
    {
        Status = QuestStatus.Started;

        yield return DialogueManager.Instance.ShowDialogue(Base.StartDialogue);

    }

    public IEnumerator CompleteQuest(Transform player)
    {
        Status = QuestStatus.Completed;

        yield return DialogueManager.Instance.ShowDialogue(Base.CompletedDialogue);

        var inventory = Inventory.GetInventory();
        if (Base.RequiredItem != null) //Removes required item from player's inventory
        {
            inventory.RemoveItem(Base.RequiredItem);
        }

        if (Base.RewardItem != null)
        {
            inventory.AddItem(Base.RewardItem);

            string playerName = player.GetComponent<PlayerController>().Name; //Gets name of player through PlayerController

            yield return DialogueManager.Instance.ShowDialogueText($"{playerName} received {Base.RewardItem.Name}!");
        }
    }

    //NOTE: Code logic incomplete. Script will not check if prerequisite item is required for quest completion
    //public bool CanBeCompleted() //Checks if a quest can be completed or not
    //{
    //    if (Base.RequiredItem != null) //Checks if quest requires item
    //    {

    //    }
    //}
}

public enum QuestStatus { None, Started, Completed }
