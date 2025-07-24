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

        // Đăng ký sự kiện
        if (snakeController != null)
        {
            snakeController.OnReachEnd.AddListener(OnGameOver);
        }

        // Hiển thị hướng dẫn
        if (showInstructions)
        {
            ShowInstructions();
        }
    }

    void Update()
    {
        // Có thể thêm các phím tắt khác ở đây
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
        Debug.Log("- Click vào các đốt rắn (không phải đầu) để phá hủy");
        Debug.Log("- Mục tiêu: Phá hủy hết rắn trước khi nó đến thành");
        Debug.Log("- Ấn R để restart game");
        Debug.Log("- Ấn P để tạm dừng/tiếp tục");
        Debug.Log("====================");
    }

    public void OnSegmentDestroyed()
    {
        if (gameEnded) return;

        // Kiểm tra nếu đã phá hủy hết rắn
        if (snakeController != null && snakeController.GetSegmentCount() <= 1)
        {
            OnVictory();
        }
    }

    void OnGameOver()
    {
        if (gameEnded) return;

        gameEnded = true;
        Debug.Log("GAME OVER! Rắn đã đến thành!");

        if (snakeController != null)
        {
            snakeController.StopSnake();
        }

        // Hiển thị thông tin restart
        Debug.Log("Ấn R để chơi lại!");
    }

    void OnVictory()
    {
        if (gameEnded) return;

        gameEnded = true;
        Debug.Log("THẮNG! Đã phá hủy hoàn toàn con rắn!");

        if (snakeController != null)
        {
            snakeController.StopSnake();
        }

        // Hiển thị thông tin restart
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

    // Hàm gọi từ SnakeSegment khi bị phá hủy
    public void NotifySegmentDestroyed()
    {
        OnSegmentDestroyed();
    }

    // Các method để điều khiển rắn từ bên ngoài
    public void ForceSnakeForward()
    {
        if (snakeController != null)
        {
            snakeController.ForceForward();
        }
    }

    public void ForceSnakeReverse()
    {
        if (snakeController != null)
        {
            snakeController.ForceReverse();
        }
    }

    public void ToggleSnakeDirection()
    {
        if (snakeController != null)
        {
            snakeController.ToggleReverse();
        }
    }

    // Getter để kiểm tra trạng thái
    public bool IsGameEnded()
    {
        return gameEnded;
    }

    public bool IsSnakeReversing()
    {
        return snakeController != null ? snakeController.IsReversing() : false;
    }
}

// Extension để có thể gọi GameManager từ SnakeSegment
public static class GameManagerExtension
{
    public static GameManager1 Instance
    {
        get
        {
            return GameObject.FindObjectOfType<GameManager1>();
        }
    }
}