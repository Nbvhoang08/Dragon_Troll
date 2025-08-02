using UnityEngine;
using DG.Tweening;

public class Cat : MonoBehaviour
{
    [Header("Cat Settings")]
    [Tooltip("Vị trí ban đầu của mèo trên path (0 = đầu path, 1 = cuối path)")]
    [Range(0f, 1f)]
    public float initialPathPosition = 0.5f;

    [Tooltip("Tốc độ di chuyển của mèo")]
    public float moveSpeed = 4f;

    [Tooltip("Tốc độ làm mượt rotation")]
    [Range(1f, 20f)]
    public float rotationSmoothSpeed = 10f;

    [Header("Path Integration")]
    public SnakePathCreator pathCreator;

    [Header("Snake Trigger Settings")]
    public Snake targetSnake;
    [Tooltip("Tiến trình (0-1) trên đường đi của rắn sẽ kích hoạt mèo chạy.")]
    [Range(0f, 1f)]
    public float snakeTriggerProgress = 0.7f;

    [Header("Visual Settings")]
    public SpriteRenderer spriteRenderer;

    [Header("Animation Settings")]
    public Animator anim;
    [Tooltip("Tên animation khi mèo đang idle/chờ")]
    public string idleAnimName = "idle";
    [Tooltip("Tên animation khi mèo chạy")]
    public string runAnimName = "run";
    [Tooltip("Danh sách tên animation joke khi mèo đến đích")]
    public string[] jokeAnimNames = { "joke1", "joke2", "joke3" };
    [Tooltip("Tên animation cry khi rắn đến đích")]
    public string cryAnimName = "cry";
    [Tooltip("Thời gian chờ giữa các lần random joke animation (giây)")]
    public float jokeRandomInterval = 5f;

    // Private variables
    private Vector3[] pathPositions;
    private Vector3[] pathRotations;
    private float pathLength;
    private float currentPathProgress;
    private bool isRunning = false;
    private bool hasReachedEnd = false;
    private bool isCrying = false; // NEW: Trạng thái cry
    private string currentName = "";

    // Joke animation timing
    private float lastJokeTime = 0f;

    // Smooth rotation
    private float currentRotationZ = 0f;
    private bool isFlippedY = false;

    void Start()
    {
        InitializeCat();

        // Set initial idle animation
        if (anim != null)
        {
            ChangeAnim(idleAnimName);
        }
    }

    void Update()
    {
        // NEW: Kiểm tra xem rắn đã đến đích chưa
        CheckSnakeReachedEnd();

        if (isCrying)
        {
            // Nếu đang cry thì không làm gì khác
            return;
        }

        if (hasReachedEnd)
        {
            // Handle joke animation cycling after reaching end
            HandleJokeAnimationCycle();
            return;
        }

        // Check the trigger condition instead of distance
        CheckTriggerCondition();

        if (isRunning)
        {
            MoveCat();
        }
    }

    // NEW: Kiểm tra xem rắn đã đến đích chưa
    void CheckSnakeReachedEnd()
    {
        if (isCrying || targetSnake == null) return;

        // Kiểm tra nếu rắn đã đến cuối đường (progress >= 1.0)
        float snakeProgress = targetSnake.GetCurrentPathProgress();
        if (snakeProgress >= 1f)
        {
            StartCrying();
        }
    }

    // NEW: Bắt đầu cry animation
    void StartCrying()
    {
        if (isCrying) return;

        isCrying = true;
        hasReachedEnd = false; // Ngừng joke animation cycle
        
        // Chuyển sang cry animation
        ChangeAnim(cryAnimName);

        Debug.Log("Snake reached the end! Cat is now crying.");
    }

    void InitializeCat()
    {
        if (pathCreator == null)
        {
            Debug.LogError("Cat: Cần gán SnakePathCreator trong Inspector!", this);
            enabled = false;
            return;
        }

        // Get animator if not assigned
        if (anim == null)
        {
            anim = GetComponent<Animator>();
        }

        pathCreator.InitializePath();
        pathPositions = pathCreator.GetPathPositions();
        pathRotations = pathCreator.GetPathRotations();
        pathLength = pathCreator.GetPathLength();

        if (pathPositions == null || pathPositions.Length < 2)
        {
            Debug.LogError("Cat: Path không hợp lệ!", this);
            enabled = false;
            return;
        }

        currentPathProgress = Mathf.Clamp01(initialPathPosition);
        Vector3 startPosition = GetPositionOnPath(currentPathProgress);
        Vector3 startRotation = GetRotationOnPath(currentPathProgress);

        transform.position = SetZToZero(startPosition);
        currentRotationZ = startRotation.z;
        transform.rotation = Quaternion.Euler(0, 0, currentRotationZ);
    }

    // New logic to check for the trigger condition
    void CheckTriggerCondition()
    {
        if (isRunning || targetSnake == null) return;

        // Get the snake's current progress on its path
        float snakeProgress = targetSnake.GetCurrentPathProgress();

        // If the snake has passed the trigger point, start running
        if (snakeProgress >= snakeTriggerProgress)
        {
            StartRunning();
        }
    }

    void StartRunning()
    {
        if (isRunning) return;

        isRunning = true;

        // Change to run animation
        ChangeAnim(runAnimName);

        Debug.Log($"Cat detected snake progress >= {snakeTriggerProgress}. Starting to run!");
    }

    void MoveCat()
    {
        if (hasReachedEnd) return;

        currentPathProgress += (moveSpeed / pathLength) * Time.deltaTime;
        currentPathProgress = Mathf.Clamp01(currentPathProgress);

        Vector3 targetPosition = GetPositionOnPath(currentPathProgress);
        Vector3 targetRotation = GetRotationOnPath(currentPathProgress);

        transform.position = SetZToZero(targetPosition);
        UpdateRotationSmooth(targetRotation);

        if (currentPathProgress >= 1f)
        {
            hasReachedEnd = true;
            OnReachedEnd();
        }
    }

    // Animation system
    private void ChangeAnim(string animName)
    {
        if (anim == null) return;

        if (currentName != animName)
        {
            // Reset previous trigger if it exists
            if (!string.IsNullOrEmpty(currentName))
            {
                anim.ResetTrigger(currentName);
            }

            currentName = animName;
            anim.SetTrigger(currentName);

            Debug.Log($"Cat animation changed to: {animName}");
        }
    }

    void OnReachedEnd()
    {
        Debug.Log("Cat has reached the end of the path!");

        // Chỉ bắt đầu joke cycle nếu không đang cry
        if (!isCrying)
        {
            StartJokeAnimationCycle();
        }
    }

    void StartJokeAnimationCycle()
    {
        // Play first random joke animation immediately
        PlayRandomJokeAnimation();

        // Set timer for next cycle
        lastJokeTime = Time.time;
    }

    void HandleJokeAnimationCycle()
    {
        // Không chạy joke nếu đang cry
        if (isCrying) return;

        // Check if it's time for next joke animation
        if (Time.time - lastJokeTime >= jokeRandomInterval)
        {
            PlayRandomJokeAnimation();
            lastJokeTime = Time.time;
        }
    }

    void PlayRandomJokeAnimation()
    {
        // Không chạy joke nếu đang cry
        if (isCrying) return;

        if (jokeAnimNames != null && jokeAnimNames.Length > 0)
        {
            int randomIndex = Random.Range(0, jokeAnimNames.Length);
            string selectedJokeAnim = jokeAnimNames[randomIndex];
            ChangeAnim(selectedJokeAnim);

            Debug.Log($"Cat is now playing joke animation: {selectedJokeAnim}");
        }
    }

    #region Helper and Utility Methods
    void UpdateRotationSmooth(Vector3 targetRotation)
    {
        float targetRotationZ = targetRotation.z;
        float deltaRotation = Mathf.DeltaAngle(currentRotationZ, targetRotationZ);
        currentRotationZ += deltaRotation * rotationSmoothSpeed * Time.deltaTime;
        float normalizedRotation = NormalizeAngle(currentRotationZ);
        bool shouldFlipY = ShouldFlipWithHysteresis(normalizedRotation, isFlippedY);
        if (shouldFlipY != isFlippedY)
        {
            isFlippedY = shouldFlipY;
            if (spriteRenderer != null)
            {
                spriteRenderer.flipY = isFlippedY;
            }
        }
        transform.rotation = Quaternion.Euler(0, 0, currentRotationZ);
    }

    bool ShouldFlipWithHysteresis(float rotationZ, bool currentFlipState)
    {
        const float hysteresisMargin = 5f;
        if (currentFlipState)
        {
            return !(rotationZ <= (90f - hysteresisMargin) && rotationZ >= (-90f + hysteresisMargin));
        }
        else
        {
            return rotationZ > (90f + hysteresisMargin) || rotationZ < (-90f - hysteresisMargin);
        }
    }

    float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }

    Vector3 GetPositionOnPath(float progress)
    {
        if (pathPositions == null || pathPositions.Length == 0) return Vector3.zero;
        if (pathPositions.Length == 1) return SetZToZero(pathPositions[0]);
        progress = Mathf.Clamp01(progress);
        if (progress <= 0f) return SetZToZero(pathPositions[0]);
        if (progress >= 1f) return SetZToZero(pathPositions[pathPositions.Length - 1]);
        float totalLength = 0f;
        float targetLength = progress * pathLength;
        for (int i = 1; i < pathPositions.Length; i++)
        {
            float segmentLength = Vector3.Distance(pathPositions[i - 1], pathPositions[i]);
            if (totalLength + segmentLength >= targetLength)
            {
                float t = segmentLength > 0 ? (targetLength - totalLength) / segmentLength : 0;
                return SetZToZero(Vector3.Lerp(pathPositions[i - 1], pathPositions[i], t));
            }
            totalLength += segmentLength;
        }
        return SetZToZero(pathPositions[pathPositions.Length - 1]);
    }

    Vector3 GetRotationOnPath(float progress)
    {
        if (pathRotations == null || pathRotations.Length == 0) return Vector3.zero;
        if (pathRotations.Length == 1) return pathRotations[0];
        progress = Mathf.Clamp01(progress);
        if (progress <= 0f) return pathRotations[0];
        if (progress >= 1f) return pathRotations[pathRotations.Length - 1];
        float totalLength = 0f;
        float targetLength = progress * pathLength;
        for (int i = 1; i < pathPositions.Length; i++)
        {
            float segmentLength = Vector3.Distance(pathPositions[i - 1], pathPositions[i]);
            if (totalLength + segmentLength >= targetLength)
            {
                if (segmentLength > 0)
                {
                    float t = (targetLength - totalLength) / segmentLength;
                    Vector3 rot1 = pathRotations[i - 1];
                    Vector3 rot2 = pathRotations[i];
                    float lerpedZ = Mathf.LerpAngle(rot1.z, rot2.z, t);
                    return new Vector3(rot1.x, rot1.y, lerpedZ);
                }
                return pathRotations[i - 1];
            }
            totalLength += segmentLength;
        }
        return pathRotations[pathRotations.Length - 1];
    }

    Vector3 SetZToZero(Vector3 pos)
    {
        pos.z = 0;
        return pos;
    }
    #endregion

    // New gizmo to visualize the trigger point
    void OnDrawGizmosSelected()
    {
        // Draw the trigger point on the snake's path
        if (targetSnake != null && targetSnake.pathCreator != null)
        {
            Vector3 triggerPosition = targetSnake.pathCreator.GetPointAtProgress(snakeTriggerProgress);

            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(triggerPosition, 0.5f);

            Gizmos.color = Color.gray;
            Gizmos.DrawLine(transform.position, triggerPosition);
        }

        // Draw the end point on the cat's own path
        if (pathPositions != null && pathPositions.Length > 0)
        {
            Gizmos.color = Color.blue;
            Vector3 endPos = GetPositionOnPath(1f);
            Gizmos.DrawWireSphere(endPos, 0.5f);
        }
    }

    #region Public Methods for External Control
    /// <summary>
    /// Manually trigger cat to start running (useful for testing)
    /// </summary>
    public void ManualStartRunning()
    {
        StartRunning();
    }

    /// <summary>
    /// Manually play a specific joke animation
    /// </summary>
    public void PlayJokeAnimation(int jokeIndex)
    {
        if (isCrying) return; // Không cho phép joke khi đang cry

        if (jokeAnimNames != null && jokeIndex >= 0 && jokeIndex < jokeAnimNames.Length)
        {
            ChangeAnim(jokeAnimNames[jokeIndex]);
        }
    }

    /// <summary>
    /// Reset cat to idle state
    /// </summary>
    public void ResetToIdle()
    {
        ChangeAnim(idleAnimName);
        isRunning = false;
        hasReachedEnd = false;
        isCrying = false; // NEW: Reset cry state
        lastJokeTime = 0f;
        currentPathProgress = Mathf.Clamp01(initialPathPosition);

        Vector3 startPosition = GetPositionOnPath(currentPathProgress);
        transform.position = SetZToZero(startPosition);
    }

    /// <summary>
    /// Stop joke animation cycling
    /// </summary>
    public void StopJokeCycle()
    {
        hasReachedEnd = false;
        lastJokeTime = 0f;
    }

    /// <summary>
    /// Manually trigger a new random joke animation
    /// </summary>
    public void TriggerRandomJoke()
    {
        if (!isCrying) // Chỉ cho phép khi không cry
        {
            PlayRandomJokeAnimation();
        }
    }

    /// <summary>
    /// NEW: Manually trigger cry animation
    /// </summary>
    public void ManualStartCrying()
    {
        StartCrying();
    }

    /// <summary>
    /// NEW: Check if cat is currently crying
    /// </summary>
    public bool IsCrying()
    {
        return isCrying;
    }

    /// <summary>
    /// NEW: Stop crying and return to previous state
    /// </summary>
    public void StopCrying()
    {
        isCrying = false;
        
        // Trở về trạng thái phù hợp
        if (hasReachedEnd)
        {
            StartJokeAnimationCycle();
        }
        else if (isRunning)
        {
            ChangeAnim(runAnimName);
        }
        else
        {
            ChangeAnim(idleAnimName);
        }
    }
    #endregion
}