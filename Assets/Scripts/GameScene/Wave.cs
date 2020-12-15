using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Creat Wave")]
public class Wave : ScriptableObject
{


    [Serializable]
    public struct Group
    {
        public string[] mob;
        public int count;
        public float delay;
    }

    public Group[] groups;
}
