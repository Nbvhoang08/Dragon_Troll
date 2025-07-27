using UnityEngine;
public static class SegmentTypeExtensions
{
    // Chuyển đổi từ BusColor sang SegmentType
    public static SegmentType ToSegmentType(this BusColor busColor)
    {
        return (SegmentType)((int)busColor + 1);
    }

    // Chuyển đổi từ SegmentType sang BusColor
    public static BusColor ToBusColor(this SegmentType segmentType)
    {
        // Bỏ qua Head và Tail khi chuyển đổi
        if (segmentType == SegmentType.Head || segmentType == SegmentType.Tail)
            return BusColor.Red; // Default

        return (BusColor)((int)segmentType - 1);
    }
    
    // Lấy sprite cho từng loại đốt
    public static Sprite GetSprite(this SegmentType segmentType)
    {
        if (SegmentSpriteManager.Instance != null)
        {
            return SegmentSpriteManager.Instance.GetSpriteForType(segmentType);
        }

        Debug.LogError("SegmentSpriteManager.Instance is null! Hãy chắc chắn có một đối tượng với script SegmentSpriteManager trong scene.");
        return null;
    }

    // Kiểm tra xem đốt có thể bị phá hủy không
    public static bool IsDestructible(this SegmentType segmentType)
    {
        // Cả Head và Tail đều không thể bị phá hủy
        return segmentType != SegmentType.Head && segmentType != SegmentType.Tail;
    }
}