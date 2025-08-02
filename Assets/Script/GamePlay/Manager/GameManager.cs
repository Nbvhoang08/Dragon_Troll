using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class GameManager : Singleton<GameManager>
{
    public List<Transform> checkpoints;
    public bool MusicOn = true;
    public bool SoundOn = true;
    public bool HapticOn = true;
    public Slot[] slots;
    public GameState gameState = GameState.Playing;
    public GameObject DarkBG;
    public PlayerData playerData;
    public Transform _levelContainer;
    public LevelData levelDatas;
    public bool isGameOver = false;
    public bool isGameWin = false;
    public bool InGame = false;

    public int currentLevel
    {
        get => playerData.currentLevel;
        set
        {
            playerData.currentLevel = value;
            SavePlayerData(playerData);
        }
    }
    public int gold
    {
        get => playerData.gold;
        set
        {
            playerData.gold = value;
            SavePlayerData(playerData);
        }
    }


    public override void Awake()
    {
        base.Awake();
        playerData =LoadPlayerData();
        InGame = false;
    }
    void Start()
    {
       
        SetUpLevel();
    }

    public void SetUpLevel() 
    {
        int childCount = _levelContainer.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform child = _levelContainer.transform.GetChild(i);
            Destroy(child.gameObject);
        }



        GameObject currentLV = Instantiate(levelDatas.CurrentLevel(currentLevel).LevelPrefab, _levelContainer);
    }

    public bool ChangeSetting(TypeSetting typeSetting)
    {
        switch (typeSetting)
        {
            case TypeSetting.Music:
                MusicOn = !MusicOn;
                if (MusicOn)
                    SoundManager.Instance.PlayBGMusic();
                else
                    SoundManager.Instance.MuteMusic();
                return MusicOn;
            case TypeSetting.Sound:
                SoundOn = !SoundOn;
                return SoundOn;
            case TypeSetting.Haptic:
                HapticOn = !HapticOn;
                return HapticOn;
        }
        return false;
    }
    public bool GetStateSetting(TypeSetting typeSetting)
    {
        switch (typeSetting)
        {
            case TypeSetting.Music:
                return MusicOn;
            case TypeSetting.Sound:
                return SoundOn;
            case TypeSetting.Haptic:
                return HapticOn;
        }
        return false;
    }


    public Slot ValidSlot()
    {
        Slot bestSlot = null;
        float highestPriority = float.MinValue;

        foreach (var slot in slots)
        {
            if (!slot.isOccupied && slot.priority > highestPriority)
            {
                highestPriority = slot.priority;
                bestSlot = slot;
            }
        }
        return bestSlot;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 0 = Left click
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f; // Đảm bảo z = 0 nếu là game 2D

            Effect clickEffect = Pool.Instance.clickedEffect;
            clickEffect.transform.position = mousePos;
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            RemoveCanon();
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
           SlipBus();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            Boost();
        }

    }
    public void Boost() 
    {
        GameEvents.BoostFire?.Invoke();
    }
    public void SlipBus()
    {
        gameState = GameState.Slip;
        DarkBG.SetActive(true);
        SpriteRenderer sr = DarkBG.GetComponent<SpriteRenderer>();
        if (sr == null) return;
        // Fade alpha từ 0 -> 1
        sr.material.DOFade(1f, 1f)
          .SetEase(Ease.Linear);
    }
    public void SlipDone()
    {
          gameState = GameState.Playing;
        // Fade alpha từ 1 -> 0
        SpriteRenderer sr = DarkBG.GetComponent<SpriteRenderer>();
        if (sr == null) return;
        // Fade alpha từ 1 -> 0
        sr.material.DOFade(0f, 1f)
          .SetEase(Ease.Linear)
          .OnComplete(() =>
          {
              DarkBG.SetActive(false); // Tắt object sau khi fade
          });

    }
    public void RemoveCanon() 
    {
        gameState = GameState.RemoveCanon;
        DarkBG.SetActive(true);
        DarkBG.GetComponent<SpriteRenderer>().sortingOrder = 55;
        SpriteRenderer sr = DarkBG.GetComponent<SpriteRenderer>();
        if (sr == null) return;
        // Fade alpha từ 0 -> 1
        sr.material.DOFade(1f, 0.5f)
          .SetEase(Ease.Linear);
        GameEvents.RemoveCanon?.Invoke(true);

    }
    public void RemoveCanonDone() 
    {
        gameState = GameState.Playing;
        SpriteRenderer sr = DarkBG.GetComponent<SpriteRenderer>();
        if (sr == null) return;
        sr.material.DOFade(0f, 1f)
          .SetEase(Ease.Linear)
          .OnComplete(() =>
          {
              DarkBG.SetActive(false); // Tắt object sau khi fade
          });
    }



    //Player Data
    private  string FolderPath => Path.Combine(Application.persistentDataPath, "SaveData");
    private string FilePath => Path.Combine(FolderPath, "PlayerData.json");

    /// <summary>
    /// Lưu dữ liệu người chơi ra JSON
    /// </summary>
    public  void SavePlayerData(PlayerData data)
    {
        if (!Directory.Exists(FolderPath))
            Directory.CreateDirectory(FolderPath);

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(FilePath, json);

        Debug.Log($"✅ PlayerData saved to {FilePath}");
    }

    /// <summary>
    /// Load dữ liệu người chơi từ JSON
    /// </summary>
    public  PlayerData LoadPlayerData()
    {
        if (!File.Exists(FilePath))
        {
            // Nếu chưa có file, tạo mới với giá trị mặc định
            PlayerData defaultData = new PlayerData();
            SavePlayerData(defaultData);
            return defaultData;
        }

        string json = File.ReadAllText(FilePath);
        PlayerData data = JsonUtility.FromJson<PlayerData>(json);

        if (data == null)
        {
            // Nếu file hỏng thì reset
            data = new PlayerData();
            SavePlayerData(data);
        }

        return data;
    }

    /// <summary>
    /// Xóa dữ liệu người chơi
    /// </summary>
    public  void DeletePlayerData()
    {
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
            Debug.Log("🗑 PlayerData deleted.");
        }
    }



}


[System.Serializable]
public class PlayerData
{
    public int currentLevel = 1;  // Level hiện tại (mặc định 1)
    public int gold = 9999;       // Vàng hiện tại (mặc định 9999)
}


