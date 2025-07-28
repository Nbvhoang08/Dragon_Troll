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
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }
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

    public void OnSegmentDestroyed()
    {
        if (gameEnded) return;

        // Kiểm tra chiến thắng bằng cách đếm số đốt có thể phá hủy
        if (snakeController != null && snakeController.GetDestructibleSegmentCount() == 0)
        {
            OnVictory();
        }
    }

   
    void OnVictory()
    {
        if (gameEnded) return;
        gameEnded = true;
        Debug.Log("THẮNG! Đã phá hủy hoàn toàn các đốt của rắn!");

        if (snakeController != null)
        {
            snakeController.StopSnake();
        }
        Debug.Log("Ấn R để chơi lại!");
    }

    void RestartGame()
    {
        Debug.Log("Restarting game...");
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    void TogglePause()
    {
        if (snakeController == null || gameEnded) return;

        if (Time.timeScale > 0)
        {
            Time.timeScale = 0;
            Debug.Log("Game đã tạm dừng. Ấn P để tiếp tục.");
        }
        else
        {
            Time.timeScale = 1;
            Debug.Log("Game tiếp tục.");
        }
    }

    public void NotifySegmentDestroyed()
    {
        OnSegmentDestroyed();
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

