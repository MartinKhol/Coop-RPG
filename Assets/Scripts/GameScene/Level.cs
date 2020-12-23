using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Create Level")]
public class Level : ScriptableObject
{
    public string SceneName;

    [Serializable]
    public struct Wave
    {
        [Serializable]
        public struct Monsters
        {
            public string mob;
            public int count;    
        }
        public Monsters[] mobs;
        [Tooltip("Delay between waves")]
        public float delay;
    }

    public Wave[] waves;
}
