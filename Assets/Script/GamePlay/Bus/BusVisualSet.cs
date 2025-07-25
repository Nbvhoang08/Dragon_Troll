using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BusVisualSet
{
    public BusType busType;
    public BusColor busColor;
    public List<BusDirectionSprites> directionSprites;
}

[System.Serializable]
public class BusDirectionSprites
{
    public BusDirection direction;
    public Sprite topSprite;
    public Sprite bottomSprite;
}
