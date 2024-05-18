using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quest/Create a new quest")]

public class QuestBase : ScriptableObject
{
    //Holds the basic data of a quest
    [SerializeField] new string name;
    [SerializeField] string description;

    [SerializeField] Dialogue startDialogue;
    [SerializeField] Dialogue inProgressDialogue;
    [SerializeField] Dialogue completedDialogue;

    [SerializeField] ItemBase requiredItem;
    [SerializeField] ItemBase rewardItem;

    public string Name => name;
    public string Description => description;

    public Dialogue StartDialogue => startDialogue;
    public Dialogue InProgressDialogue => inProgressDialogue?.Lines?.Count > 0 ? inProgressDialogue : startDialogue; //Returns in-progress dialogue if present, though returns start dialogue if not. "?" are null conditional operators to ensure the line doesn't cause the game to crash if returned value is null
    public Dialogue CompletedDialogue => completedDialogue;

    public ItemBase RequiredItem => requiredItem;
    public ItemBase RewardItem => rewardItem;
}
