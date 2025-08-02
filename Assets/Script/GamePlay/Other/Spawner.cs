using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class Spawner : MonoBehaviour
{
    public List<AmmoEntry> ammoList; // Kéo trong inspector hoặc load từ JSON
    public int maxChunkSize = 4;
    public int CurrentLevelIndex = 1; // Level cần load từ JSON

    private List<BusColor> finalScales = new List<BusColor>();

    private void OnEnable()
    {
        GameEvents.GameStart += OnGameStart;
    }

    private void OnDisable()
    {
        GameEvents.GameStart -= OnGameStart;
    }


    void OnGameStart()
    {
        // Nếu ammoList rỗng thì load từ JSON database theo LevelIndex
        if (ammoList == null || ammoList.Count == 0)
        {
            LoadAmmoByLevelIndex(GameManager.Instance.currentLevel);
        }

        GenerateScaleSequence();
    }

    /// <summary>
    /// Load ammo data từ AmmoDatabase.json theo LevelIndex
    /// </summary>
    



    public void LoadAmmoByLevelIndex(int levelIndex)
    {
        string path = Path.Combine(Application.dataPath, "Export Level", "AmmoDatabase.json");

        if (!File.Exists(path))
        {
            Debug.LogError($"Không tìm thấy file JSON Database: {path}");
            return;
        }

        string json = File.ReadAllText(path);

        AmmoDatabase database = JsonUtility.FromJson<AmmoDatabase>(json);

        if (database == null || database.levels == null || database.levels.Count == 0)
        {
            Debug.LogError("AmmoDatabase rỗng hoặc không hợp lệ!");
            return;
        }

        // Tìm Level trùng index
        AmmoSummaryWrapper levelData = database.levels.Find(l => l.levelIndex == levelIndex);

        if (levelData == null)
        {
            Debug.LogError($"Không tìm thấy dữ liệu cho Level {levelIndex} trong AmmoDatabase!");
            return;
        }

        ammoList = new List<AmmoEntry>(levelData.ammoEntries);
        Debug.Log($"✅ Loaded Level {levelIndex} với {ammoList.Sum(a => a.count)} đạn.");
    }

    // ======================================
    // Toàn bộ code GenerateSequence giữ nguyên
    // ======================================

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

        Shuffle(allChunks);

        finalScales = allChunks.SelectMany(chunk => chunk).ToList();
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }

    public List<BusColor> GetFinalScales() =>
        finalScales == null || finalScales.Count == 0 ? new List<BusColor>() : new List<BusColor>(finalScales);

    public int GetTotalAmmoCount() => ammoList?.Sum(a => a.count) ?? 0;

    public Dictionary<BusColor, int> GetAmmoBreakdown() =>
        ammoList?.ToDictionary(a => a.color, a => a.count) ?? new Dictionary<BusColor, int>();

    public void RegenerateSequence()
    {
        GenerateScaleSequence();
        Debug.Log($"Đã regenerate sequence với {finalScales.Count} đạn: {string.Join(", ", finalScales)}");
    }

    public bool HasValidAmmo() => ammoList != null && ammoList.Count > 0 && GetTotalAmmoCount() > 0;
}





[System.Serializable]
public class AmmoSummaryWrapper
{
    public int levelIndex;
    public List<AmmoEntry> ammoEntries;
}

[System.Serializable]
public class AmmoEntry
{
    public BusColor color;
    public int count;
}
[System.Serializable]
public class AmmoDatabase
{
    public List<AmmoSummaryWrapper> levels = new List<AmmoSummaryWrapper>();
}