using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inventory;

[CreateAssetMenu(fileName = "New Class", menuName = "Create Character Class")]
public class PlayerClassObject : ScriptableObject
{
    public CharacterClass characterClass;
    public RuntimeAnimatorController animatorController;
    public ItemType[] weaponTypes;
    public GameObject visualPrefab;
    public Attribute[] baseAttributes;
    public ItemObject defaultWeapon;
    public Ability ability1;
    public Ability ability2;
}
