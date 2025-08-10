using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIWIN : UICanvas
{
    [SerializeField] private TextMeshProUGUI currentLevelText;  // Text hiển thị level vừa hoàn thành
         // Text hiển thị level tiếp theo
    
    // THÊM: Biến lưu level đã hoàn thành để hiển thị chính xác
    private int completedLevel;
    
    private void OnEnable()
    {
        // LƯU level hiện tại làm completed level khi mở UI Win
        completedLevel = GameManager.Instance.currentLevel;
        UpdateLevelTexts();
    }
    
    private void UpdateLevelTexts()
    {
        if (GameManager.Instance != null)
        {
            int nextLevel = completedLevel + 1;
            
            // Update text cho level vừa hoàn thành
            if (currentLevelText != null)
            {
                currentLevelText.text = $"Level {completedLevel:00} Complete!";
            }
            
            
            
            Debug.Log($"UIWIN: Completed Level {completedLevel}, Next Level {nextLevel}");
        }
    }
    
    public void HomeBtn()
    {
        // Về Home
        GameManager.Instance.ClearLevel();
        UIManager.Instance.CloseAll();
        UIManager.Instance.OpenUI<UIHome>();
        
        Debug.Log("Returned to Home");
    }
    
    public void NextLevelBtn()
    {
        // SỬA: Tăng level dựa trên completedLevel thay vì currentLevel
        GameManager.Instance.currentLevel = completedLevel + 1;
        
        // Close UI Win
        UIManager.Instance.CloseUIDirectly<UIWIN>();
        
        // Setup level mới
        GameManager.Instance.SetUpLevel();
        
        // Mở UI Gameplay
        UIManager.Instance.OpenUI<UIGamePlay>();
        
        Debug.Log($"Starting Level {GameManager.Instance.currentLevel}");
    }
    
    public void RestartBtn()
    {
        // SỬA: Chơi lại level đã hoàn thành
        GameManager.Instance.currentLevel = completedLevel;
        
        UIManager.Instance.CloseUIDirectly<UIWIN>();
        
        // Setup lại level hiện tại
        GameManager.Instance.SetUpLevel();
        
        // Mở UI Gameplay
        UIManager.Instance.OpenUI<UIGamePlay>();
        
        Debug.Log($"Restarting Level {GameManager.Instance.currentLevel}");
    }
}