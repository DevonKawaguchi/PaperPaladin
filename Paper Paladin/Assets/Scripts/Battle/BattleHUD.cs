using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleHUD : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI statusText;
    [SerializeField] HPBar hpBar;

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
        levelText.text = "Lvl " + pokemon.Level;
        hpBar.SetHP((float) pokemon.HP / pokemon.MaxHp);

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

    public IEnumerator UpdateHP()
    {
        if (_pokemon.HPChanged)
        {
            yield return hpBar.SetHPSmooth((float)_pokemon.HP / _pokemon.MaxHp);
            _pokemon.HPChanged = false;
        }
    }
}
