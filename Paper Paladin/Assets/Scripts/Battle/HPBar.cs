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

    public IEnumerator SetHPSmooth(float newHP) 
    {
        float curHP = health.transform.localScale.x;
        float changeAmt = curHP - newHP; 

        Debug.Log($"changeAmt is {changeAmt}, curHP is {curHP}, newHP is {newHP}"); 

        if (changeAmt > 0)
        {
            //Code logic for the negative health transition
            Debug.Log("Negative health transition");
            while (curHP - newHP > Mathf.Epsilon) //While current HP subtracted by new HP is greater than the smallest possible value
            {
                curHP -= changeAmt * Time.deltaTime; //Decreases the current HP until the difference between the current HP and the new HP is negligible as a function of time
                health.transform.localScale = new Vector3(curHP, 1f); //Updates HP bar transform with current HP  
                yield return null;
            }

            health.transform.localScale = new Vector3(newHP, 1f);
        }


        else
        {
            //Positive health transition is the reversed code logic of the negative health transition
            Debug.Log("Positive health transition");
            while (newHP - curHP > Mathf.Epsilon) //While new HP subtracted by the current HP is greater than the smallest possible value
            {
                curHP += -(changeAmt) * Time.deltaTime; //Increases the current HP until the difference between the current HP and the new HP is negligible as a function of time
                health.transform.localScale = new Vector3(curHP, 1f); //Updates HP bar transform with current HP  
                yield return null;
            }

            health.transform.localScale = new Vector3(newHP, 1f);
        }

    }
}
