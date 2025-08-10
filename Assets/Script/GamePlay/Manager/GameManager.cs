using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.EventSystems;

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

    // THÊM MỚI: Reference đến Snake để thao tác với các segments
    private Snake currentSnake;

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
        playerData = LoadPlayerData();
        InGame = false;
        gameState = GameState.Starting;
    }

    private void OnEnable()
    {
        GameEvents.GameStart += OnGameStart;
    }

    private void OnDisable()
    {
        GameEvents.GameStart -= OnGameStart;
    }

    void Start()
    {
        UIManager.Instance.OpenUI<UIHome>();
        FadeIn(1);
        if (MusicOn)
        {
            DOVirtual.DelayedCall(0.5f, () =>
            {
                SoundManager.Instance.PlayBGMusic();
            });
        }
    }

    public void FadeIn()
    {
        UIManager.Instance.OpenUI<UITransition>();
        UIManager.Instance.GetUI<UITransition>().PlayRevealIn(() => UIManager.Instance.CloseUIDirectly<UITransition>());
    }

    public void FadeIn(float time, System.Action callback = null)
    {
        UIManager.Instance.OpenUI<UITransition>();
        UITransition trans = UIManager.Instance.GetUI<UITransition>();
        trans.SetRadius(0f);
        DOVirtual.DelayedCall(time, () =>
        {
            callback?.Invoke();
            trans.PlayRevealIn(() => UIManager.Instance.CloseUIDirectly<UITransition>());
        });
    }

    public void FadeOut(float time, System.Action callback = null)
    {
        UIManager.Instance.OpenUI<UITransition>();
        UITransition trans = UIManager.Instance.GetUI<UITransition>();
        trans.SetRadius(0f);
        DOVirtual.DelayedCall(time, () =>
        {
            callback?.Invoke();
            trans.PlayRevealIn(() => UIManager.Instance.CloseUIDirectly<UITransition>());
        });
    }

    // FIX: Thêm hàm reset tất cả slots
    public void ResetAllSlots()
    {
        Debug.Log("Resetting all slots...");
        if (slots != null && slots.Length > 0)
        {
            foreach (Slot slot in slots)
            {
                if (slot != null)
                {
                    slot.ResetSlot();
                }
            }
        }
    }

    public void SetUpLevel()
    {
        // FIX: Clear level và snake trước khi setup level mới
        ClearLevel();

        // FIX: Reset tất cả slots trước khi setup level mới
        ResetAllSlots();

        InGame = true;
        Debug.Log("===== SETUP LEVEL - INVOKING GAMESTART =====");

        // FIX: Reset game state trước khi tạo level mới
        isGameOver = false;
        isGameWin = false;
        gameState = GameState.Starting;

        GameObject currentLV = Instantiate(levelDatas.CurrentLevel(currentLevel).LevelPrefab, _levelContainer);

        // FIX: Đợi 1 frame để level được tạo hoàn toàn trước khi invoke GameStart
        DOVirtual.DelayedCall(0.1f, () =>
        {
            // THÊM MỚI: Tìm và lưu reference đến Snake
            currentSnake = FindObjectOfType<Snake>();
            if (currentSnake == null)
            {
                Debug.LogWarning("Không tìm thấy Snake trong scene!");
            }
            else
            {
                Debug.Log($"Tìm thấy Snake: {currentSnake.name}");
            }

            GameEvents.GameStart?.Invoke();
            Debug.Log($"GameStart event invoked. Subscriber count: {GameEvents.GameStart?.GetInvocationList()?.Length ?? 0}");
        });
    }

    public void ClearLevel()
    {
        Debug.Log("===== CLEARING LEVEL =====");

        // FIX: Xóa và reset Snake trước khi xóa level container
        if (currentSnake != null)
        {
            Debug.Log("Destroying current snake...");
            currentSnake.DestroySnake();
            currentSnake = null;
        }
        else
        {
            // Tìm và xóa tất cả Snake có thể còn sót lại
            Snake[] allSnakes = FindObjectsOfType<Snake>();
            foreach (Snake snake in allSnakes)
            {
                Debug.Log($"Found and destroying snake: {snake.name}");
                snake.DestroySnake();
            }
        }

        // Xóa tất cả children của level container
        int childCount = _levelContainer.transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = _levelContainer.transform.GetChild(i);
            if (child != null)
            {
                DestroyImmediate(child.gameObject);
            }
        }

        // FIX: Reset slots khi clear level
        ResetAllSlots();

        // FIX: Reset spawner data
        Spawner[] spawners = FindObjectsOfType<Spawner>();
        foreach (Spawner spawner in spawners)
        {
            if (spawner != null)
            {
                // Clear ammo list để force reload từ JSON
                spawner.ammoList?.Clear();
            }
        }

        InGame = false;

        Debug.Log("===== LEVEL CLEARED =====");
    }

    public void OnGameStart()
    {
        gameState = GameState.Playing;

        // THÊM MỚI: Tìm Snake nếu chưa có
        if (currentSnake == null)
        {
            currentSnake = FindObjectOfType<Snake>();
            if (currentSnake != null)
            {
                Debug.Log($"Found Snake in OnGameStart: {currentSnake.name}");
            }
        }
    }

    public bool ChangeSetting(TypeSetting typeSetting)
    {
        switch (typeSetting)
        {
            case TypeSetting.Music:
                MusicOn = !MusicOn;
                Debug.Log($"Music turned {(MusicOn ? "ON" : "OFF")}");
                if (MusicOn)
                {
                    SoundManager.Instance.PlayBGMusic();
                }
                else
                {
                    SoundManager.Instance.MuteMusic();
                }
                playerData.musicOn = MusicOn;
                SavePlayerData(playerData);
                return MusicOn;

            case TypeSetting.Sound:
                SoundOn = !SoundOn;
                Debug.Log($"Sound turned {(SoundOn ? "ON" : "OFF")}");
                SoundManager.Instance.SetSoundVolume(SoundOn ? 1f : 0f);
                playerData.soundOn = SoundOn;
                SavePlayerData(playerData);
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
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUIObject())
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;

            Effect clickEffect = Pool.Instance.clickedEffect;
            clickEffect.transform.position = mousePos;
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
        DarkBG.transform.position = new Vector3(0, 0, 1);
        sr.sortingOrder = 30;
        if (sr == null) return;
        sr.material.DOFade(1f, 1f)
          .SetEase(Ease.Linear);
    }

    public void SlipDone()
    {
        gameState = GameState.Playing;
        SpriteRenderer sr = DarkBG.GetComponent<SpriteRenderer>();

        if (sr == null) return;
        sr.material.DOFade(0f, 1f)
          .SetEase(Ease.Linear)
          .OnComplete(() =>
          {
              DarkBG.SetActive(false);
          });
    }

    public void RemoveCanon()
    {
        // Kiểm tra xem có canon nào không
        bool hasAnyCanon = false;
        foreach (var slot in slots)
        {
            if (slot.isOccupied && slot.canon != null && slot.canon.gameObject.activeInHierarchy)
            {
                hasAnyCanon = true;
                break;
            }
        }

        if (!hasAnyCanon)
        {
            Debug.Log("Không có canon nào để xóa!");
            return;
        }

        Debug.Log("Vào chế độ chọn canon để xóa...");
        
        gameState = GameState.RemoveCanon;
        DarkBG.SetActive(true);
        DarkBG.transform.position = new Vector3(0, 0, -2);
        SpriteRenderer sr = DarkBG.GetComponent<SpriteRenderer>();
        sr.sortingOrder = 55;
        
        if (sr == null) return;

        sr.material.DOFade(1f, 0.5f)
          .SetEase(Ease.Linear);
        GameEvents.RemoveCanon?.Invoke(true);
        
        // Không tự động xóa nữa - chờ người chơi chọn
        Debug.Log("Hãy click vào canon bạn muốn xóa!");
    }

    // THÊM MỚI: Hàm xử lý khi người chơi chọn canon cụ thể để xóa
    public void OnCanonSelectedForRemoval(Canon selectedCanon)
    {
        if (selectedCanon == null) return;
        
        // Tìm slot chứa canon này
        Slot targetSlot = null;
        foreach (var slot in slots)
        {
            if (slot.canon == selectedCanon)
            {
                targetSlot = slot;
                break;
            }
        }
        
        if (targetSlot == null) return;
        
        var colorToRemove = selectedCanon.GetCanonColor();
        var ammoCount = targetSlot.GetCurrentBulletCount();
        
        Debug.Log($"Người chơi chọn xóa canon màu {colorToRemove} với {ammoCount} viên đạn");
        
        // Xóa các đốt rắn tương ứng trước khi xóa canon
        if (currentSnake != null)
        {
            currentSnake.RemoveSegmentsByColorAndCount(colorToRemove, ammoCount);
        }
        
        // Xóa canon
        selectedCanon.DoneAnimation();
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
              DarkBG.SetActive(false);
          });
        GameEvents.RemoveCanon?.Invoke(false);
    }

    public bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        for (int i = 0; i < results.Count; i++)
        {
            if (results[i].gameObject.layer == 5)
            {
                return true;
            }
        }

        return false;
    }

    //Player Data
    private string FolderPath => Path.Combine(Application.persistentDataPath, "SaveData");
    private string FilePath => Path.Combine(FolderPath, "PlayerData.json");

    public void SavePlayerData(PlayerData data)
    {
        if (!Directory.Exists(FolderPath))
            Directory.CreateDirectory(FolderPath);

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(FilePath, json);

        Debug.Log($"✅ PlayerData saved to {FilePath}");
    }

    public PlayerData LoadPlayerData()
    {
        if (!File.Exists(FilePath))
        {
            PlayerData defaultData = new PlayerData();
            SavePlayerData(defaultData);
            return defaultData;
        }

        string json = File.ReadAllText(FilePath);
        PlayerData data = JsonUtility.FromJson<PlayerData>(json);

        if (data == null)
        {
            data = new PlayerData();
            SavePlayerData(data);
        }

        return data;
    }

    public void DeletePlayerData()
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
    public int currentLevel = 1;
    public int gold = 9999;
    public bool musicOn = true;
    public bool soundOn = true;
}
