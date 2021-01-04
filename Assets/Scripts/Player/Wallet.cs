using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public delegate void ValueChanged(int value);

public class Wallet
{
    public ValueChanged OnValueChanged;

    public int Coins
    {
        get;
        private set;
    }

    public bool AddCoins(int amount)
    {
        int newTotal = Coins + amount;
        if (newTotal < 0) return false;
        Coins = newTotal;
        if (OnValueChanged !=null)
            OnValueChanged.Invoke(newTotal);
        Debug.Log("money " + amount);
        return true;
    }

    const string PrefsKey = "coins";

    public Wallet()
    {
        Load();
    }

    public void Save()
    {
        PlayerPrefs.SetInt(string.Concat(PrefsKey, CharacterSelect.SelectedClass), Coins);
    }

    public void Load()
    {
        if (PlayerPrefs.HasKey(string.Concat(PrefsKey, CharacterSelect.SelectedClass)))
        {
            Coins = PlayerPrefs.GetInt(string.Concat(PrefsKey, CharacterSelect.SelectedClass));
        }
    }
}
