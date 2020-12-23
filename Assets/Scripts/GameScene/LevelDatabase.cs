using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Create Level Database")]
public class LevelDatabase : ScriptableObject
{
    [SerializeField]
    private List<Level> levels;

    //returns Level SO of given id
    public Level GetLevel(int id)
    {
        //when over max current implemented level start over
        id %= levels.Count;

        return levels[id];
    }
}
