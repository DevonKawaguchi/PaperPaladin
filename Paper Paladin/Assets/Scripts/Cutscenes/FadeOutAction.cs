using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeOutAction : CutsceneAction
{
    [SerializeField] float duration = 0.5f; //0.5f = default duration

    public override IEnumerator Play()
    {
        yield return Fader.i.FadeOut(duration);
    }
}
