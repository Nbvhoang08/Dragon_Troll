using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CanonData", menuName = "Game/CanonData")]
public class CanonData :ScriptableObject    
{
    public List<CanonVisualData> canonVisuals;
    public CanonVisualData GetVisualData(BusColor color)
    {
        var visualData = canonVisuals.Find(v => v.canonColor == color);
        if (visualData == null)
        {
            return null;
        }
        return visualData;
    }
}
[System.Serializable]
public class CanonVisualData
{
    public BusColor canonColor;
    public Sprite AmmoImage;
    public Sprite bulletSprite;
    public Sprite canonSprite;
    public Sprite bottomSprite;
    public Color Color;

}
