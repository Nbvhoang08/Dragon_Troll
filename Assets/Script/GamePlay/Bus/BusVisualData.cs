using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BusVisualData", menuName = "Game/BusVisualData")]
public class BusVisualData : ScriptableObject
{
    public List<BusVisualSet> busVisualSets;

    public BusDirectionSprites GetSprites(BusType type, BusColor color, BusDirection direction)
    {
        var visualSet = busVisualSets.Find(v => v.busType == type && v.busColor == color);
        if (visualSet == null)
        {
            Debug.LogWarning($"No visual set found for type {type} and color {color}");
            return null;
        }

        var dirSet = visualSet.directionSprites.Find(d => d.direction == direction);
        return dirSet;
    }
}
