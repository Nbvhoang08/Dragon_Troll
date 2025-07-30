using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level", order = 1)]
public class LevelData : ScriptableObject
{
    public List<Level> Levels;
    public Level CurrentLevel(int PlayerIndex) 
    {
        if (Levels == null || Levels.Count == 0 || PlayerIndex > Levels.Count)
        {
            Debug.LogError("No levels available in LevelData.");
            return null;
        }
        Level level = Levels.Find(x => x.LevelIndex == PlayerIndex);
        return level;
    }
}

[System.Serializable]
public class Level 
{
    public int LevelIndex;
    public GameObject LevelPrefab;
}