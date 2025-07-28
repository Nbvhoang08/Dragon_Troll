using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DragonSprite", menuName = "Game/DragonSprite")]
public class DragonSpriteData : ScriptableObject
{
    public List<DragonSprite> DragonVisual;
    public DragonSprite GetVisualData(SegmentType color)
    {
        var visualData = DragonVisual.Find(v => v.SegmentColor == color);
        if (visualData == null)
        {
            return null;
        }
        return visualData;
    }
}
[System.Serializable]
public class DragonSprite
{
    public SegmentType SegmentColor;
    public Sprite dragonSegment;

}