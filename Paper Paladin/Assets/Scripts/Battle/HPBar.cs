using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBar : MonoBehaviour
{
    [SerializeField] GameObject health;

    public void SetHP(float hpNormalised) //Changes scale of "Health" image based on Pokemon health value
    {
        health.transform.localScale = new Vector3(hpNormalised, 1f); //
    }

    public IEnumerator SetHPSmooth(float newHP) //1
    {
        float curHP = health.transform.localScale.x;
        float changeAmt = curHP - newHP; //-value

        Debug.Log($"changeAmt is {changeAmt}, curHP is {curHP}, newHP is {newHP}");

        //changeAmt = 1.6667
        //curHP = 1
        //newHP = 0.8333
        if (changeAmt > 0)
        {
            Debug.Log("Steve");
            while (curHP - newHP > Mathf.Epsilon) //1 - 0.8333
            {
                curHP -= changeAmt * Time.deltaTime; //1 - 0.16667
                health.transform.localScale = new Vector3(curHP, 1f); //1
                yield return null;
            }

            health.transform.localScale = new Vector3(newHP, 1f); //0.8333
        }

        //changeAmt = -0.16667
        //curHP = 0.8333
        //newHP = 1
        else
        {
            Debug.Log("Bob");
            while (newHP - curHP > Mathf.Epsilon) //1 - 0.8333
            {
                curHP += -(changeAmt) * Time.deltaTime; //0.83 - 0.167
                health.transform.localScale = new Vector3(curHP, 1f); //1
                yield return null;
            }

            health.transform.localScale = new Vector3(newHP, 1f); //0.8333
        }

    }
}
