using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Snake : MonoBehaviour
{
    [Header("Snake Settings")]
    public int initialSegmentCount = 10;
    public float segmentSpacing = 1f;
    public float moveSpeed = 2f;
    public float reverseSpeed = 1.5f; // Tốc độ đi lùi

    [Header("Prefabs")]
    public GameObject segmentPrefab;
    public GameObject headPrefab;

    [Header("Path")]
    public SnakePathCreator pathCreator;

    private List<SnakeSegment> segments = new List<SnakeSegment>();
    private Vector3[] pathPositions;
    private float pathLength;
    private float currentPathProgress = 0f;
    private bool isMoving = true;
    private bool isReversing = false; // Trạng thái đi lùi

    [Header("Game Events")]
    public UnityEngine.Events.UnityEvent OnReachEnd;

    void Start()
    {
        // Đợi 1 frame để đảm bảo PathCreator đã được khởi tạo
        Invoke(nameof(InitializeSnake), 0.01f);
    }

    void Update()
    {
        // Xử lý input
        HandleInput();

        if (isMoving && segments.Count > 0 && pathPositions != null)
        {
            MoveSnake();
        }
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleReverse();
        }
    }

    public void ToggleReverse()
    {
        isReversing = !isReversing;

        if (isReversing)
        {
            Debug.Log("Rắn đang đi lùi!");
        }
        else
        {
            Debug.Log("Rắn đang đi tiến!");
        }
    }

    void InitializeSnake()
    {
        if (pathCreator == null)
        {
            Debug.LogError("Cần gán SnakePathCreator trong Inspector!");
            return;
        }

        // Đảm bảo PathCreator đã được khởi tạo
        pathCreator.InitializePath();

        pathPositions = pathCreator.GetPathPositions();
        pathLength = pathCreator.GetPathLength();

        // Kiểm tra xem path có hợp lệ không
        if (pathPositions == null || pathPositions.Length < 2)
        {
            Debug.LogError("PathPositions không hợp lệ! Kiểm tra PathCreator setup.");
            return;
        }

        if (pathLength <= 0)
        {
            Debug.LogError("PathLength không hợp lệ! Kiểm tra PathCreator setup.");
            return;
        }

        CreateSnakeSegments();
    }

    void CreateSnakeSegments()
    {
        for (int i = 0; i < initialSegmentCount; i++)
        {
            GameObject segmentObj;

            if (i == 0)
            {
                segmentObj = Instantiate(headPrefab != null ? headPrefab : segmentPrefab, transform);
            }
            else
            {
                segmentObj = Instantiate(segmentPrefab, transform);
            }

            SnakeSegment segment = segmentObj.GetComponent<SnakeSegment>();
            if (segment == null)
            {
                segment = segmentObj.AddComponent<SnakeSegment>();
            }

            segment.SetSegmentIndex(i);
            segment.SetAsHead(i == 0);
            segment.segmentSpacing = segmentSpacing;

            Vector3 startPos = GetPositionOnPath(-i * segmentSpacing / pathLength);
            segmentObj.transform.position = startPos;

            segments.Add(segment);
        }
    }

    void MoveSnake()
    {
        // Kiểm tra null trước khi sử dụng
        if (pathPositions == null || pathLength <= 0)
        {
            Debug.LogWarning("PathPositions hoặc PathLength không hợp lệ!");
            return;
        }

        // Tính toán tốc độ dựa trên hướng di chuyển
        float currentSpeed = isReversing ? reverseSpeed : moveSpeed;
        float speedMultiplier = isReversing ? -1f : 1f;

        currentPathProgress += (currentSpeed / pathLength) * Time.deltaTime * speedMultiplier;

        // Kiểm tra giới hạn
        if (currentPathProgress >= 1f)
        {
            currentPathProgress = 1f;
            if (!isReversing) // Chỉ kết thúc game khi đi tiến và đến cuối path
            {
                isMoving = false;
                OnReachEnd?.Invoke();
                Debug.Log("Rắn đã đến thành! Game Over!");
                return;
            }
        }
        else if (currentPathProgress <= 0f)
        {
            currentPathProgress = 0f;
            // Có thể thêm logic khi rắn về đến điểm bắt đầu
        }

        // Cập nhật vị trí cho từng segment
        for (int i = 0; i < segments.Count; i++)
        {
            if (segments[i] == null || segments[i].IsDestroyed()) continue;

            float segmentProgress = currentPathProgress - (i * segmentSpacing / pathLength);
            Vector3 targetPosition = GetPositionOnPath(segmentProgress);
            segments[i].UpdatePosition(targetPosition, 0.1f);
        }
    }

    Vector3 GetPositionOnPath(float progress)
    {
        // Kiểm tra null và empty array
        if (pathPositions == null || pathPositions.Length == 0)
        {
            Debug.LogError("PathPositions is null or empty!");
            return Vector3.zero;
        }

        if (pathPositions.Length == 1)
        {
            return SetZToZero(pathPositions[0]);
        }

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
                float segmentProgress = segmentLength > 0 ? (targetLength - totalLength) / segmentLength : 0;
                return SetZToZero(Vector3.Lerp(pathPositions[i - 1], pathPositions[i], segmentProgress));
            }

            totalLength += segmentLength;
        }

        return SetZToZero(pathPositions[pathPositions.Length - 1]);
    }

    Vector3 SetZToZero(Vector3 pos)
    {
        pos.z = 0;
        return pos;
    }

    public void OnSegmentDestroyed(int segmentIndex)
    {
        SnakeSegment destroyedSegment = null;
        for (int i = 0; i < segments.Count; i++)
        {
            if (segments[i] != null && segments[i].segmentIndex == segmentIndex)
            {
                destroyedSegment = segments[i];
                break;
            }
        }

        if (destroyedSegment == null) return;

        ReconnectSegments(segmentIndex);

        // Thông báo cho GameManager
        if (GameManagerExtension.Instance != null)
        {
            GameManagerExtension.Instance.NotifySegmentDestroyed();
        }
    }

    void ReconnectSegments(int destroyedIndex)
    {
        List<SnakeSegment> newSegments = new List<SnakeSegment>();

        for (int i = 0; i < segments.Count; i++)
        {
            if (segments[i] != null && !segments[i].IsDestroyed())
            {
                newSegments.Add(segments[i]);
            }
        }

        segments = newSegments;

        if (segments.Count == 0)
        {
            OnReachEnd?.Invoke();
            return;
        }

        for (int i = 0; i < segments.Count; i++)
        {
            segments[i].SetSegmentIndex(i);
            segments[i].SetAsHead(i == 0);
        }

        // Kiểm tra pathLength trước khi sử dụng
        if (pathLength > 0)
        {
            currentPathProgress -= (segmentSpacing * 1f / pathLength);
            currentPathProgress = Mathf.Max(0, currentPathProgress);
        }
    }

    public void StopSnake()
    {
        isMoving = false;
    }

    public void StartSnake()
    {
        isMoving = true;
    }

    public int GetSegmentCount()
    {
        return segments.Count;
    }

    // Các method mới để kiểm soát hướng di chuyển
    public bool IsReversing()
    {
        return isReversing;
    }

    public void SetReverse(bool reverse)
    {
        isReversing = reverse;
    }

    public void ForceForward()
    {
        isReversing = false;
    }

    public void ForceReverse()
    {
        isReversing = true;
    }
}