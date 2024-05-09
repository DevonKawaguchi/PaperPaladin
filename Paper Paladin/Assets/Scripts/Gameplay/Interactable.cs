using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Interactable //interface rather than class as it will allow multiple functions to be shared across multiple classes rather than having to repeat it for every single one. This is especially important as there's many NPCs present in the game and so repeating code for all of them would significantly decrease the application's performance
{
    void Interact();
}
