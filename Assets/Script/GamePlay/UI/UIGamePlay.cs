using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIGamePlay : UICanvas
{
    [SerializeField] private TextMeshProUGUI levelText;

    private void Start()
    {
        UpdateLevelText();
    }

    private void OnEnable()
    {
        UpdateLevelText();
        
        // THÊM: Subscribe event để cập nhật khi level thay đổi
        GameEvents.GameStart += OnGameStart;
    }
    
    private void OnDisable()
    {
        // THÊM: Unsubscribe event
        GameEvents.GameStart -= OnGameStart;
    }
    
    // THÊM: Cập nhật level text khi game start
    private void OnGameStart()
    {
        UpdateLevelText();
    }

    private void UpdateLevelText()
    {
        if (levelText != null && GameManager.Instance != null)
        {
            levelText.text = $"Level {GameManager.Instance.currentLevel:00}";
            Debug.Log($"UIGamePlay: Displaying Level {GameManager.Instance.currentLevel}");
        }
    }
    
    // THÊM: Public method để force update level text từ bên ngoài nếu cần
    public void ForceUpdateLevelText()
    {
        UpdateLevelText();
    }

    public void PauseBtn()
    {
        if (GameManager.Instance.gameState != GameState.Paused)
        {
            GameManager.Instance.gameState = GameState.Paused;
        }

        UIManager.Instance.OpenUI<UIPause>();
    }

    public void UpdateLVName()
    {

    }
    public void UpdateSpeed()
    {

    }
    public void RemoveCanon()
    {
        GameManager.Instance.RemoveCanon();
    }

    public void SlipBox()
    {
        GameManager.Instance.SlipBus();
    }

    public void InCreaseRange()
    {
        GameManager.Instance.Boost();
    }

    public void PushBack()
    {

    }
}