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
    private bool hasLoadedThisLevel = false; // FIX: Flag để tránh load trùng

    private void OnEnable()
    {
        GameEvents.GameStart += OnGameStart;
    }

    private void OnDisable()
    {
        GameEvents.GameStart -= OnGameStart;
    }

    // FIX: Reset khi level mới được load
    private void Start()
    {
        ResetSpawner();
    }

    // FIX: Public method để reset spawner từ bên ngoài
    public void ResetSpawner()
    {
        Debug.Log("Resetting Spawner...");
        hasLoadedThisLevel = false;
        ammoList?.Clear();
        finalScales?.Clear();
        CurrentLevelIndex = GameManager.Instance.currentLevel;
    }

    void OnGameStart()
    {
        Debug.Log($"Spawner OnGameStart - CurrentLevel: {GameManager.Instance.currentLevel}");
        
        // FIX: Luôn load lại ammo cho level mới
        CurrentLevelIndex = GameManager.Instance.currentLevel;
        
        // FIX: Clear data cũ trước khi load mới
        if (ammoList != null)
        {
            ammoList.Clear();
        }
        finalScales?.Clear();
        hasLoadedThisLevel = false;

        // Load ammo data cho level hiện tại
        LoadAmmoByLevelIndex(CurrentLevelIndex);
        
        // Generate sequence
        GenerateScaleSequence();
        
        hasLoadedThisLevel = true;
        
        Debug.Log($"✅ Spawner loaded Level {CurrentLevelIndex} với {GetTotalAmmoCount()} đạn");
    }

    /// <summary>
    /// Load ammo data từ AmmoDatabase.json theo LevelIndex
    /// </summary>
    public void LoadAmmoByLevelIndex(int levelIndex)
    {
        Debug.Log($"Loading ammo for Level {levelIndex}...");
        
        string path = Path.Combine(Application.dataPath, "Export Level", "AmmoDatabase.json");

        if (!File.Exists(path))
        {
            Debug.LogError($"Không tìm thấy file JSON Database: {path}");
            LoadDefaultAmmo(); // FIX: Load default nếu không tìm thấy file
            return;
        }

        try
        {
            string json = File.ReadAllText(path);
            AmmoDatabase database = JsonUtility.FromJson<AmmoDatabase>(json);

            if (database == null || database.levels == null || database.levels.Count == 0)
            {
                Debug.LogError("AmmoDatabase rỗng hoặc không hợp lệ!");
                LoadDefaultAmmo();
                return;
            }

            // Tìm Level trùng index
            AmmoSummaryWrapper levelData = database.levels.Find(l => l.levelIndex == levelIndex);

            if (levelData == null)
            {
                Debug.LogWarning($"Không tìm thấy dữ liệu cho Level {levelIndex}, sử dụng Level 1");
                levelData = database.levels.Find(l => l.levelIndex == 1);
                
                if (levelData == null)
                {
                    Debug.LogError("Không tìm thấy Level 1, load default ammo");
                    LoadDefaultAmmo();
                    return;
                }
            }

            // FIX: Tạo mới ammoList thay vì assign reference
            ammoList = new List<AmmoEntry>();
            foreach (var entry in levelData.ammoEntries)
            {
                ammoList.Add(new AmmoEntry 
                { 
                    color = entry.color, 
                    count = entry.count 
                });
            }
            
            Debug.Log($"✅ Loaded Level {levelIndex} với {ammoList.Sum(a => a.count)} đạn: {string.Join(", ", ammoList.Select(a => $"{a.color}:{a.count}"))}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Lỗi khi load AmmoDatabase: {e.Message}");
            LoadDefaultAmmo();
        }
    }

    // FIX: Load ammo mặc định khi có lỗi
    private void LoadDefaultAmmo()
    {
        Debug.Log("Loading default ammo...");
        ammoList = new List<AmmoEntry>
        {
            new AmmoEntry { color = BusColor.Red, count = 3 },
            new AmmoEntry { color = BusColor.Blue, count = 3 },
            new AmmoEntry { color = BusColor.Green, count = 3 }
        };
    }

    // ======================================
    // Toàn bộ code GenerateSequence giữ nguyên
    // ======================================

    void GenerateScaleSequence()
    {
        if (ammoList == null || ammoList.Count == 0)
        {
            Debug.LogError("AmmoList rỗng, không thể generate sequence!");
            return;
        }

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
        
        Debug.Log($"Generated sequence với {finalScales.Count} segments: {string.Join(", ", finalScales)}");
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