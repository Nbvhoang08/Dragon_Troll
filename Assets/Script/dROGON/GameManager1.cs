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

        if (snakeController != null)
        {
            snakeController.OnReachEnd.AddListener(OnGameOver);
        }

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
        Debug.Log("- Click vào các đốt rắn (không phải đầu) để phá hủy");
        Debug.Log("- Mục tiêu: Phá hủy hết rắn trước khi nó đến thành");
        Debug.Log("- Ấn R để restart game");
        Debug.Log("- Ấn P để tạm dừng/tiếp tục");
        Debug.Log("====================");
    }

    public void OnSegmentDestroyed()
    {
        if (gameEnded) return;

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

    public bool IsGameEnded() => gameEnded;
    public bool IsSnakeReversing() => snakeController != null ? snakeController.IsReversing() : false;
}

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