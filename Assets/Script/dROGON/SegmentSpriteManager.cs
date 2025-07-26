// SegmentSpriteManager.cs (Không đổi)
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class SegmentSpriteMapping
{
    public SegmentType segmentType;
    public Sprite sprite;
}

public class SegmentSpriteManager : MonoBehaviour
{
    public static SegmentSpriteManager Instance { get; private set; }

    public List<SegmentSpriteMapping> spriteMappings;

    private Dictionary<SegmentType, Sprite> _spriteDictionary;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _spriteDictionary = new Dictionary<SegmentType, Sprite>();
        foreach (var mapping in spriteMappings)
        {
            if (mapping.sprite != null)
            {
                _spriteDictionary[mapping.segmentType] = mapping.sprite;
            }
        }
    }

    public Sprite GetSpriteForType(SegmentType type)
    {
        if (_spriteDictionary.TryGetValue(type, out Sprite sprite))
        {
            return sprite;
        }

        Debug.LogWarning($"Không tìm thấy sprite cho SegmentType: {type}. Vui lòng gán trong SegmentSpriteManager.");
        return null;
    }
}