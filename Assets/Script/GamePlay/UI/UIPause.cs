using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Analytics;
public class UIPause : UICanvas
{
    [SerializeField] private GameObject InGameUI;
    [SerializeField] GameObject SettingPopUp;
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

    public void Restart() 
    {
        
    }
    public void Resume()
    {
        GameManager.Instance.gameState = GameState.Playing;
        CloseUIAnimation();
    }
    public void Home()
    {
        GameManager.Instance.gameState = GameState.Starting;
        GameManager.Instance.FadeOut(0, () => 
        {
            UIManager.Instance.OpenUI<UIHome>();
            //GameManager.Instance.ClearLevel();
        });
        CloseUIAnimation();
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
        RectTransform rectTransform = SettingPopUp.GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.one;
        InGameUI.SetActive(false);
        rectTransform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            CloseDirectly();
        });

    }
    public void OpenUIAnimation()
    {
        RectTransform rectTransform = SettingPopUp.GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.zero;
        rectTransform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutQuad).OnComplete(
            ()=> InGameUI.SetActive(GameManager.Instance.InGame) 
        );

    }


}
