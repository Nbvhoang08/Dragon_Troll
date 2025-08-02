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
        SoundManager.Instance.PlayBGMusic();
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



    public void SetUpLevel()
    {
        int childCount = _levelContainer.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform child = _levelContainer.transform.GetChild(i);
            Destroy(child.gameObject);
        }

        InGame = true;
        GameEvents.GameStart?.Invoke();
        GameObject currentLV = Instantiate(levelDatas.CurrentLevel(currentLevel).LevelPrefab, _levelContainer);
    }
    public void ClearLevel() 
    {
        int childCount = _levelContainer.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform child = _levelContainer.transform.GetChild(i);
            Destroy(child.gameObject);
        }
        InGame = false;
    }
    public void OnGameStart() 
    {
        gameState = GameState.Playing;
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
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUIObject()) // 0 = Left click
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
        sr.sortingOrder = 30; // Đặt z để nó nằm dưới các đối tượng khác
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
        DarkBG.transform.position = new Vector3(0,0,-2); 
        SpriteRenderer sr = DarkBG.GetComponent<SpriteRenderer>();
        sr.sortingOrder = 55; // Đặt z để nó nằm dưới các đối tượng khác
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


