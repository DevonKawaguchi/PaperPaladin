using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBar : MonoBehaviour
{
    [SerializeField] GameObject health;

    public void SetHP(float hpNormalised) //Changes scale of "Health" image based on Pokemon health value
    {
        health.transform.localScale = new Vector3(hpNormalised, 1f);
    }
}
