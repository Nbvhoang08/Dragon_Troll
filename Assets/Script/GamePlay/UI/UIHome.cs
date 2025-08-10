using DG.Tweening;
using TMPro;
using UnityEngine;

public class UIHome : UICanvas
{
    [SerializeField] private HomeTab[] homeTabs;
    [HideInInspector] public int currentTabIndex = 0;
    [SerializeField] TextMeshProUGUI[] levelText;
    [SerializeField] TextMeshProUGUI coinText;
    [SerializeField] TextMeshProUGUI resetText;

    public void PlayBtn()
    {
        // FIX: Cleanup toàn diện trước khi bắt đầu game mới
        CleanupAllSnakes();

        DOVirtual.DelayedCall(0.25f, () =>
        {
            GameManager.Instance.FadeIn(0.25f, () =>
            {
                GameManager.Instance.SetUpLevel();

                // FIX: Đợi lâu hơn để đảm bảo SnakePathCreator được tạo và khởi tạo hoàn tất
                DOVirtual.DelayedCall(1f, () =>
                {
                    Snake newSnake = FindObjectOfType<Snake>();
                    if (newSnake != null)
                    {
                        if (!newSnake.IsInitialized())
                        {
                            Debug.Log("Snake chưa được khởi tạo, đang force initialize...");
                            newSnake.ForceInitialize();
                        }
                        else
                        {
                            Debug.Log("Snake đã được khởi tạo sẵn.");
                        }
                    }
                    else
                    {
                        Debug.LogError("Không tìm thấy Snake trong scene sau khi setup level!");
                    }
                });
            });
        });
        UIManager.Instance.OpenUI<UIGamePlay>();
        UIManager.Instance.CloseUI<UIHome>(0.5f);
    }

    private void Start()
    {
        OnTabClick(0); // Default to the first tab (home)
    }

    private void OnEnable()
    {
        UpdateLevelText();

        // FIX: Cleanup toàn diện khi vào trang chủ
        CleanupAllSnakes();
    }

    // FIX: Hàm cleanup toàn diện
    private void CleanupAllSnakes()
    {
        // Kill tất cả DOTween animations trước
        DOTween.KillAll();

        Snake[] existingSnakes = FindObjectsOfType<Snake>();
        if (existingSnakes.Length > 0)
        {
            Debug.Log($"Tìm thấy {existingSnakes.Length} rắn còn sót lại, đang xóa...");
            foreach (Snake snake in existingSnakes)
            {
                if (snake != null)
                {
                    snake.DestroySnake();
                }
            }

            // Đảm bảo cleanup hoàn toàn bằng cách gọi Resources.UnloadUnusedAssets
            System.GC.Collect();
        }
    }

    public void OnTabClick(int index)
    {
        foreach (var tab in homeTabs)
        {
            if (tab.tabIndex == index)
            {
                tab.AnimationOn();
            }
            else
            {
                tab.AnimationOff();
            }
        }
    }

    public void SettingBtn()
    {
        UIManager.Instance.OpenUI<UIPause>();
    }

    private void UpdateLevelText()
    {
        for (int i = 0; i < levelText.Length; i++)
        {
            if (i == 0)
            {
                levelText[i].text = $"Level {GameManager.Instance.currentLevel:00}";
            }
            else
            {
                levelText[i].text = (i + GameManager.Instance.currentLevel).ToString("00");
            }
        }
        coinText.text = GameManager.Instance.gold.ToString();
    }

    public void ResetLevelBtn()
    {
        // Reset current level to 1
        GameManager.Instance.currentLevel = 1;

        // Update UI
        UpdateLevelText();

        // Optional: Show confirmation message
        Debug.Log("Level progress reset to 1");
    }
}