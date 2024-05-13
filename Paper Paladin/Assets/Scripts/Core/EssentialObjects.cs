using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EssentialObjects : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject); //Allows all children of EssentialObjects gameobject to remain even when switching scenes
    }
}
