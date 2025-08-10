using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UILOSE : UICanvas
{
    [SerializeField] private GameObject InGameUI;
    [SerializeField] GameObject SettingPopUp;
    // Start is called before the first frame update
    void Start()
    {
        
    }
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
    public void Home()
    {
        // FIX: Cleanup toàn diện trước khi về trang chủ
        CleanupCurrentSnake();

        GameManager.Instance.gameState = GameState.Starting;
        GameManager.Instance.FadeOut(0, () =>
        {
            UIManager.Instance.CloseUI<UILOSE>(0.5f);
            UIManager.Instance.OpenUI<UIHome>();
            //GameManager.Instance.ClearLevel();
        });
        CloseUIAnimation();
    }
     public void OpenUIAnimation()
    {
        RectTransform rectTransform = SettingPopUp.GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.zero;
        rectTransform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutQuad).OnComplete(
            () => InGameUI.SetActive(GameManager.Instance.InGame)
        );
    }
     public void CloseUIAnimation()
    {
        RectTransform rectTransform = SettingPopUp.GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.one;
        InGameUI.SetActive(false);
        rectTransform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            CloseDirectly();
        });
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
