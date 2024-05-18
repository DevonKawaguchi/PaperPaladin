using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour, Interactable
{
    [SerializeField] ItemBase item;


    public IEnumerator Interact(Transform initiator)
    {
        Debug.Log("Pickup is working");
        yield break;
    }

}
