// ========== UPDATED UIPause.cs ==========
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class UIPause : UICanvas
{
    [SerializeField] private GameObject InGameUI;
    [SerializeField] GameObject SettingPopUp;

    [Header("Settings")]
    [SerializeField] private BtnSetting musicToggle;
    [SerializeField] private BtnSetting soundToggle;
    [SerializeField] private BtnSetting hapticToggle;

    void OnEnable()
    {
        if (InGameUI == null)
        {
            Debug.LogError("InGameUI is not assigned in UIPause");
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager instance not ready yet, disabling InGameUI.");
            InGameUI.SetActive(false);
            return;
        }
        OpenUIAnimation();
    }

    void OnDisable()
    {

    }

    private void Start()
    {
        // Kiểm tra và đảm bảo các toggle được set đúng trạng thái
        if (musicToggle != null)
            musicToggle.Init();
        if (soundToggle != null)
            soundToggle.Init();
        if (hapticToggle != null)
            hapticToggle.Init();
    }

    public void Restart()
    {
        Debug.Log("Restart button clicked");

        // FIX: Cleanup toàn diện trước khi restart
        CleanupCurrentSnake();

        // Reset GameManager state
        if (GameManager.Instance != null)
        {
            GameManager.Instance.gameState = GameState.Starting;
            // FIX: Setup lại level để tạo rắn mới với delay đủ lâu
            DOVirtual.DelayedCall(0.2f, () =>
            {
                GameManager.Instance.SetUpLevel();
                GameManager.Instance.gameState = GameState.Playing;

                // FIX: Đảm bảo rắn mới được khởi tạo với delay đủ lâu
                DOVirtual.DelayedCall(0.5f, () =>
                {
                    Snake newSnake = FindObjectOfType<Snake>();
                    if (newSnake != null && !newSnake.IsInitialized())
                    {
                        newSnake.ForceInitialize();
                        Debug.Log("Force initialize rắn mới sau restart");
                    }
                });
            });
        }

        CloseUIAnimation();
    }

    public void Resume()
    {
        GameManager.Instance.gameState = GameState.Playing;
        CloseUIAnimation();
    }

    public void Home()
    {
        // FIX: Cleanup toàn diện trước khi về trang chủ
        CleanupCurrentSnake();

        GameManager.Instance.gameState = GameState.Starting;
        GameManager.Instance.FadeOut(0, () =>
        {
            UIManager.Instance.CloseUI<UIPause>(0.5f);
            UIManager.Instance.OpenUI<UIHome>();
            GameManager.Instance.ClearLevel();
        });
        CloseUIAnimation();
    }

    // FIX: Hàm cleanup toàn diện
    private void CleanupCurrentSnake()
    {
        // Kill tất cả DOTween animations trước
        DOTween.KillAll();

        Snake currentSnake = FindObjectOfType<Snake>();
        if (currentSnake != null)
        {
            currentSnake.DestroySnake();
            Debug.Log("Đã cleanup rắn hiện tại hoàn toàn");
        }

        // Force garbage collection
        System.GC.Collect();
    }

    public void CloseBtn()
    {
        gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
        CloseUIAnimation();
        if (GameManager.Instance.InGame)
        {
            GameManager.Instance.gameState = GameState.Playing;
        }
    }

    public void CloseUIAnimation()
    {
        if (SettingPopUp != null)
        {
            RectTransform rectTransform = SettingPopUp.GetComponent<RectTransform>();
            rectTransform.localScale = Vector3.one;
            InGameUI.SetActive(false);
            rectTransform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                CloseDirectly();
            });
        }
    }

    public void OpenUIAnimation()
    {
        if (SettingPopUp != null)
        {
            RectTransform rectTransform = SettingPopUp.GetComponent<RectTransform>();
            rectTransform.localScale = Vector3.zero;
            rectTransform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutQuad).OnComplete(
                () => InGameUI.SetActive(GameManager.Instance.InGame)
            );
        }
    }
}