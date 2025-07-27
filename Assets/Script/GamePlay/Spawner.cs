using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Spawner : MonoBehaviour
{
    [System.Serializable]
    public class AmmoEntry
    {
        public BusColor color;
        public int count;
    }

    public List<AmmoEntry> ammoList; // Kéo trong inspector
    public int maxChunkSize = 4;

    private List<BusColor> finalScales = new List<BusColor>();

    void Start()
    {
        GenerateScaleSequence();
        PrintSequence();
    }

    void GenerateScaleSequence()
    {
        List<List<BusColor>> allChunks = new List<List<BusColor>>();

        foreach (var ammo in ammoList)
        {
            int remaining = ammo.count;

            while (remaining > 0)
            {
                int chunkSize = Random.Range(1, Mathf.Min(maxChunkSize, remaining) + 1);
                var chunk = Enumerable.Repeat(ammo.color, chunkSize).ToList();
                allChunks.Add(chunk);
                remaining -= chunkSize;
            }
        }

        // Shuffle chunk list
        Shuffle(allChunks);

        // Flatten into final list
        finalScales = allChunks.SelectMany(chunk => chunk).ToList();
    }

    public GameObject squarePrefab;
    public Transform spawnParent;

    // Mỗi BusColor sẽ có 1 màu riêng
    private Dictionary<BusColor, Color> colorMap = new Dictionary<BusColor, Color>()
    {
        { BusColor.Red, Color.red },
        { BusColor.Blue, Color.blue },
        { BusColor.Green, Color.green },
        { BusColor.Yellow, Color.yellow },
        { BusColor.Purple, new Color(0.5f, 0f, 0.5f) },
        { BusColor.Pink, new Color(1f, 0.4f, 0.7f) },
        { BusColor.Brown, new Color(0.6f, 0.3f, 0f) },
        { BusColor.Cyan, Color.cyan },
    };

    void PrintSequence()
    {
        float offsetX = 1.2f; // khoảng cách giữa các Square
        Vector3 startPos = Vector3.zero;

        for (int i = 0; i < finalScales.Count; i++)
        {
            BusColor color = finalScales[i];
            Vector3 spawnPos = startPos + new Vector3(i * offsetX, 0, 0);
            GameObject square = Instantiate(squarePrefab, spawnPos, Quaternion.identity, spawnParent);

            SpriteRenderer sr = square.GetComponent<SpriteRenderer>();
            if (sr != null && colorMap.ContainsKey(color))
            {
                sr.color = colorMap[color];
            }

            square.name = $"{i + 1}_{color}";
        }
    }

    // Fisher-Yates Shuffle
    void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }

    // *** THÊM METHOD PUBLIC ĐỂ SNAKE CÓ THỂ TRUY CẬP ***
    
    // Trả về danh sách các màu đã được generate
    public List<BusColor> GetFinalScales()
    {
        // Nếu chưa generate thì generate ngay
        if (finalScales == null || finalScales.Count == 0)
        {
            GenerateScaleSequence();
        }
        return new List<BusColor>(finalScales); // Trả về copy để tránh modify từ bên ngoài
    }

    // Trả về tổng số đạn
    public int GetTotalAmmoCount()
    {
        if (ammoList == null) return 0;
        return ammoList.Sum(ammo => ammo.count);
    }

    // Trả về thông tin chi tiết về ammo
    public Dictionary<BusColor, int> GetAmmoBreakdown()
    {
        var breakdown = new Dictionary<BusColor, int>();
        if (ammoList != null)
        {
            foreach (var ammo in ammoList)
            {
                breakdown[ammo.color] = ammo.count;
            }
        }
        return breakdown;
    }

    // Force regenerate sequence (useful cho restart game)
    public void RegenerateSequence()
    {
        GenerateScaleSequence();
        Debug.Log($"Đã regenerate sequence với {finalScales.Count} đạn: {string.Join(", ", finalScales)}");
    }

    // Kiểm tra xem có đủ ammo để tạo rắn không
    public bool HasValidAmmo()
    {
        return ammoList != null && ammoList.Count > 0 && GetTotalAmmoCount() > 0;
    }
}