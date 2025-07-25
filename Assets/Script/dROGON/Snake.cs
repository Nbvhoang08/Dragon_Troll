using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Snake : MonoBehaviour
{
    [Header("Snake Settings")]
    public int initialSegmentCount = 11; // ĐÃ CẬP NHẬT: Thay đổi giá trị mặc định để khớp với ví dụ
    public float segmentSpacing = 1f;
    public float moveSpeed = 2f;
    public float reverseSpeed = 1.5f;

    [Header("Prefabs")]
    public GameObject segmentPrefab;
    public GameObject headPrefab;

    [Header("Path")]
    public SnakePathCreator pathCreator;

    private List<SnakeSegment> segments = new List<SnakeSegment>();
    private Vector3[] pathPositions;
    private Vector3[] pathRotations;
    private float pathLength;
    private float currentPathProgress = 0f;
    private bool isMoving = true;
    private bool isReversing = false;

    [Header("Game Events")]
    public UnityEngine.Events.UnityEvent OnReachEnd;

    void Start()
    {
        Invoke(nameof(InitializeSnake), 0.01f);
    }

    void Update()
    {
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
        Debug.Log(isReversing ? "Rắn đang đi lùi!" : "Rắn đang đi tiến!");
    }

    void InitializeSnake()
    {
        if (pathCreator == null)
        {
            Debug.LogError("Cần gán SnakePathCreator trong Inspector!");
            return;
        }

        pathCreator.InitializePath();
        pathPositions = pathCreator.GetPathPositions();
        pathRotations = pathCreator.GetPathRotations();
        pathLength = pathCreator.GetPathLength();

        if (pathRotations == null || pathPositions.Length != pathRotations.Length)
        {
            Debug.LogError("PathRotations không được thiết lập hoặc kích thước không khớp với pathPositions. Kiểm tra lại PathCreator.");
            enabled = false;
            return;
        }

        if (pathPositions == null || pathPositions.Length < 2 || pathLength <= 0)
        {
            Debug.LogError("Path không hợp lệ! Kiểm tra PathCreator setup.");
            enabled = false;
            return;
        }

        CreateSnakeSegments();
    }

    void CreateSnakeSegments()
    {
        for (int i = 0; i < initialSegmentCount; i++)
        {
            GameObject segmentObj = Instantiate(i == 0 && headPrefab != null ? headPrefab : segmentPrefab, transform);
            SnakeSegment segment = segmentObj.GetComponent<SnakeSegment>() ?? segmentObj.AddComponent<SnakeSegment>();

            segment.SetSegmentIndex(i);
            segment.SetAsHead(i == 0);
            segment.segmentSpacing = segmentSpacing;

            Vector3 startPos = GetPositionOnPath(-i * segmentSpacing / pathLength);
            segmentObj.transform.position = startPos;
            segments.Add(segment);
        }

        // ĐÃ THÊM: Cập nhật sorting order sau khi tạo rắn
        UpdateSegmentSortingOrders();
    }

    void MoveSnake()
    {
        if (pathPositions == null || pathLength <= 0) return;

        float currentSpeed = isReversing ? reverseSpeed : moveSpeed;
        float speedMultiplier = isReversing ? -1f : 1f;
        currentPathProgress += (currentSpeed / pathLength) * Time.deltaTime * speedMultiplier;

        if (currentPathProgress >= 1f)
        {
            currentPathProgress = 1f;
            if (!isReversing)
            {
                isMoving = false;
                OnReachEnd?.Invoke();
                return;
            }
        }
        else if (currentPathProgress <= 0f)
        {
            currentPathProgress = 0f;
        }

        for (int i = 0; i < segments.Count; i++)
        {
            if (segments[i] == null || segments[i].IsDestroyed()) continue;

            float segmentProgress = currentPathProgress - (i * segmentSpacing / pathLength);

            Vector3 targetPosition = GetPositionOnPath(segmentProgress);
            Vector3 targetRotation = GetRotationOnPath(segmentProgress);

            segments[i].UpdatePosition(targetPosition, 0.1f);
            segments[i].UpdateRotation(targetRotation);
        }
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
                float p = segmentLength > 0 ? (targetLength - totalLength) / segmentLength : 0;
                return SetZToZero(Vector3.Lerp(pathPositions[i - 1], pathPositions[i], p));
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

    public void OnSegmentDestroyed(int segmentIndex)
    {
        ReconnectSegments(segmentIndex);

        if (GameManagerExtension.Instance != null)
        {
            GameManagerExtension.Instance.NotifySegmentDestroyed();
        }
    }

    void ReconnectSegments(int destroyedIndex)
    {
        List<SnakeSegment> newSegments = new List<SnakeSegment>();
        foreach (var segment in segments)
        {
            if (segment != null && !segment.IsDestroyed())
            {
                newSegments.Add(segment);
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

        // ĐÃ THÊM: Cập nhật sorting order sau khi kết nối lại
        UpdateSegmentSortingOrders();

        if (pathLength > 0)
        {
            currentPathProgress -= (segmentSpacing * 1f / pathLength);
            currentPathProgress = Mathf.Max(0, currentPathProgress);
        }
    }

    // ĐÃ THÊM: Phương thức mới để cập nhật sorting order
    void UpdateSegmentSortingOrders()
    {
        if (segments == null) return;

        int totalSegments = segments.Count;
        for (int i = 0; i < totalSegments; i++)
        {
            if (segments[i] != null)
            {
                // Công thức: (Tổng số đốt + 1) - chỉ số hiện tại (0-based)
                // Ví dụ: 11 đốt -> (11+1)-0=12 cho đầu, (11+1)-10=2 cho đuôi
                int sortingOrder = (totalSegments + 1) - i;
                segments[i].UpdateSortingOrder(sortingOrder);
            }
        }
    }

    public void StopSnake() => isMoving = false;
    public void StartSnake() => isMoving = true;
    public int GetSegmentCount() => segments.Count;
    public bool IsReversing() => isReversing;
    public void SetReverse(bool reverse) => isReversing = reverse;
    public void ForceForward() => isReversing = false;
    public void ForceReverse() => isReversing = true;
}