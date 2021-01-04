using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CharacterClass
{
    Warrior,
    Hunter,
    Wizard,
    Alchemyst,
    Priest
}

public class CharacterSelect : MonoBehaviour
{
    public Toggle WarriorToggle;
    public Toggle WizardToggle;
    public Toggle AlchemystToggle;
    public Toggle HunterToggle;
    public Toggle PriestToggle;

    public static CharacterClass SelectedClass = CharacterClass.Warrior;

    const string PrefsKey = "selectedClass";

    private void Start()
    {
        if (PlayerPrefs.HasKey(PrefsKey))
        {
            SelectedClass = (CharacterClass)PlayerPrefs.GetInt(PrefsKey);

            switch (SelectedClass)
            {
                case CharacterClass.Warrior:
                    WarriorToggle.isOn = true;
                    break;
                case CharacterClass.Hunter:
                    HunterToggle.isOn = true;
                    break;
                case CharacterClass.Wizard:
                    WizardToggle.isOn = true;
                    break;
                case CharacterClass.Alchemyst:
                    AlchemystToggle.isOn = true;
                    break;
                case CharacterClass.Priest:
                    PriestToggle.isOn = true;
                    break;
                default:
                    WarriorToggle.isOn = true;
                    break;
            }
        }
    }

    public void SelectWarrior()
    {
        SelectClass(CharacterClass.Warrior);
    }
    public void SelectHunter()
    {
        SelectClass(CharacterClass.Hunter);
    }

    public void SelectWizard()
    {
        SelectClass(CharacterClass.Wizard);
    }

    public void SelectAlchemyst ()
    {
        SelectClass(CharacterClass.Alchemyst);
    }

    public void SelectPriest()
    {
        SelectClass(CharacterClass.Priest);
    }

    public void SelectClass(CharacterClass characterClass)
    {
        SelectedClass = characterClass;
        PlayerPrefs.SetInt(PrefsKey, (int)SelectedClass);
    }
}
