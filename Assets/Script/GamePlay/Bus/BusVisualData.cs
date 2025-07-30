using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BusVisualData", menuName = "Game/BusVisualData")]
public class BusVisualData : ScriptableObject
{
    public List<BusVisualSet> busVisualSets;
    public BusColliderData[] colliderDatas; // tương ứng với từng busType

    public BusDirectionSprites GetSprites(BusType type, BusColor color, BusDirection direction)
    {
        var visualSet = busVisualSets.Find(v => v.busType == type && v.busColor == color);
        if (visualSet == null)
        {
            if(color != BusColor.None)
                Debug.LogWarning($"No visual set found for type {type} and color {color}");
            return null;
        }

        var dirSet = visualSet.directionSprites.Find(d => d.direction == direction);
        return dirSet;
    }

   

    public BusColliderData GetCollider(BusType type, BusDirection dir)
    {
        // Lấy collider cơ bản theo busType
        var baseCollider = colliderDatas[(int)type];

        // Tính offset theo hướng
        //Vector2 rotatedOffset = RotateOffset(baseCollider.offset, dir);

        return new BusColliderData
        {
            size = baseCollider.size, // BoxCollider2D size không xoay
            offset = baseCollider.offset // Offset không xoay, chỉ cần điều chỉnh theo hướng
        };
    }

    //private Vector2 RotateOffset(Vector2 offset, BusDirection dir)
    //{
    //    float angle = dir switch
    //    {
    //        BusDirection.Up => 0f,
    //        BusDirection.UpRight => 45f,
    //        BusDirection.Right => 90f,
    //        BusDirection.DownRight => 135f,
    //        BusDirection.Down => 180f,
    //        BusDirection.DownLeft => 225f,
    //        BusDirection.Left => 270f,
    //        BusDirection.UpLeft => 315f,
    //        _ => 0f
    //    };

    //    float rad = angle * Mathf.Deg2Rad;
    //    float cos = Mathf.Cos(rad);
    //    float sin = Mathf.Sin(rad);

    //    return new Vector2(
    //        offset.x * cos - offset.y * sin,
    //        offset.x * sin + offset.y * cos
    //    );
    //}


}
[System.Serializable]
public struct BusColliderData
{
    public Vector2 size;
    public Vector2 offset;
}