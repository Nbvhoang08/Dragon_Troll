// GameManager1.cs (Đã cập nhật)
using UnityEngine;

public class GameManager1 : MonoBehaviour
{
    [Header("Game Settings")]
    public Snake snakeController;
    public bool showInstructions = true;
    private bool gameEnded = false;

    void Start()
    {
        InitializeGame();


        if (showInstructions)
        {
            ShowInstructions();
        }
    }

    void Update()
    {
        
        
        
    }

    void InitializeGame()
    {
        gameEnded = false;
    }

    void ShowInstructions()
    {
        Debug.Log("=== HƯỚNG DẪN CHƠI ===");
        Debug.Log("- Ấn E để đổi hướng di chuyển (tiến/lùi)");
        Debug.Log("- Click vào các đốt rắn (không phải đầu/đuôi) để phá hủy");
        Debug.Log("- Mục tiêu: Phá hủy hết các đốt màu trước khi rắn đến thành");
        Debug.Log("- Ấn R để restart game");
        Debug.Log("- Ấn P để tạm dừng/tiếp tục");
        Debug.Log("====================");
    }


    public void ForceSnakeForward()
    {
        if (snakeController != null) snakeController.ForceForward();
    }

    public void ForceSnakeReverse()
    {
        if (snakeController != null) snakeController.ForceReverse();
    }

    public void ToggleSnakeDirection()
    {
        if (snakeController != null) snakeController.ToggleReverse();
    }

    public bool IsSnakeReversing() => snakeController != null ? snakeController.IsReversing() : false;
}

