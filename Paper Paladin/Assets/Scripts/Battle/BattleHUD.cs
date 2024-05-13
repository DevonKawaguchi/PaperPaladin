using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class BattleHUD : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI statusText;

    [SerializeField] HPBar hpBar;

    [SerializeField] GameObject expBar;

    [SerializeField] Color poisonColour;
    [SerializeField] Color burnColour;
    [SerializeField] Color paralyseColour;
    [SerializeField] Color freezeColour;
    [SerializeField] Color sleepColour;

    Pokemon _pokemon;
    Dictionary<ConditionID, Color> statusColours;

    public void SetData(Pokemon pokemon)
    {
        _pokemon = pokemon;

        nameText.text = pokemon.Base.Name;
        SetLevel();
        hpBar.SetHP((float) pokemon.HP / pokemon.MaxHp);
        SetExp();

        statusColours = new Dictionary<ConditionID, Color>()
        {
            {ConditionID.PSN, poisonColour },
            {ConditionID.BRN, burnColour },
            {ConditionID.PAR, paralyseColour },
            {ConditionID.FRZ, freezeColour },
            {ConditionID.SLP, sleepColour },
        };

        SetStatusText(); //Whenever setting data, the HUD text will also be updated, as well as whenever the status of the pokemon changes
        _pokemon.OnStatusChanged += SetStatusText;
    }

    void SetStatusText()
    {
        if (_pokemon.Status == null)
        {
            statusText.text = "";
        }
        else
        {
            statusText.text = _pokemon.Status.ID.ToString().ToUpper();
            statusText.color = statusColours[_pokemon.Status.ID];
        }
    }

    public void SetLevel()
    {
        levelText.text = "Lvl " + _pokemon.Level;
    }

    public void SetExp()
    {
        if (expBar == null) return; //Ensures enemy Pokemons don't also set XP bars

        float normalisedExp = GetNormalisedExp();
        expBar.transform.localScale = new Vector3(normalisedExp, 1, 1);
    }

    public IEnumerator SetExpSmooth(bool reset = false)
    {
        if (expBar == null) yield break; //Ensures enemy Pokemons don't also set XP bars

        if (reset)
        {
            expBar.transform.localScale = new Vector3(0, 1, 1); //Resets XP bar
        }

        float normalisedExp = GetNormalisedExp();
        yield return expBar.transform.DOScaleX(normalisedExp, 1.5f).WaitForCompletion();
    }

    float GetNormalisedExp()
    {
        int currLevelExp = _pokemon.Base.GetExpForLevel(_pokemon.Level);
        int nextLevelExp = _pokemon.Base.GetExpForLevel(_pokemon.Level + 1);

        float normalisedExp = (float)(_pokemon.Exp - currLevelExp) / (nextLevelExp - currLevelExp);
        return Mathf.Clamp01(normalisedExp); //Mathf.Clamp01 ensures normalisedExp is only between 0-1
    }

    public IEnumerator UpdateHP()
    {
        if (_pokemon.HPChanged)
        {
            yield return hpBar.SetHPSmooth((float)_pokemon.HP / _pokemon.MaxHp);
            _pokemon.HPChanged = false;
        }
    }
}
