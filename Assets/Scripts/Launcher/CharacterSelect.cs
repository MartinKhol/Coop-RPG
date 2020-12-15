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
    public Toggle warriorToggle;
    public Toggle wizardToggle;
    public Toggle alchemystToggle;
    public Toggle hunterToggle;
    public Toggle priestToggle;

    public static CharacterClass selectedClass = CharacterClass.Warrior;

    const string prefKey = "selectedClass";

    private void Start()
    {
        if (PlayerPrefs.HasKey(prefKey))
        {
            selectedClass = (CharacterClass)PlayerPrefs.GetInt(prefKey);

            switch (selectedClass)
            {
                case CharacterClass.Warrior:
                    warriorToggle.isOn = true;
                    break;
                case CharacterClass.Hunter:
                    hunterToggle.isOn = true;
                    break;
                case CharacterClass.Wizard:
                    wizardToggle.isOn = true;
                    break;
                case CharacterClass.Alchemyst:
                    alchemystToggle.isOn = true;
                    break;
                case CharacterClass.Priest:
                    priestToggle.isOn = true;
                    break;
                default:
                    warriorToggle.isOn = true;
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
        selectedClass = characterClass;
        PlayerPrefs.SetInt(prefKey, (int)selectedClass);
    }
}
