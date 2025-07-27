using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

public class Snake : MonoBehaviour
{
    [Header("Snake Settings")]
    public float moveSpeed = 2f;
    public float reverseSpeed = 1.5f;
    [Tooltip("Khoảng cách chung giữa các đốt thân.")]
    public float segmentSpacing = 1f;
    [Tooltip("Khoảng cách riêng giữa đầu rắn và đốt thân đầu tiên.")]
    public float headToFirstSegmentSpacing = 1.5f;
    [Tooltip("Khoảng cách riêng giữa đốt thân cuối cùng và đuôi rắn.")]
    public float lastSegmentToTailSpacing = 1.5f; // THÊM MỚI

    [Header("Prefabs")]
    public GameObject segmentPrefab;
    public GameObject headPrefab;
    public GameObject tailPrefab;

    [Header("Path")]
    public SnakePathCreator pathCreator;

    [Header("Spawner Integration")]
    public Spawner spawner;

    private List<SnakeSegment> segments = new List<SnakeSegment>();
    private Vector3[] pathPositions;
    private Vector3[] pathRotations;
    private float pathLength;
    private float currentPathProgress = 0f;
    private bool isMoving = true;
    private bool isReversing = false;
    private List<SegmentType> segmentSequence = new List<SegmentType>();

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
            Debug.LogError("PathRotations không được thiết lập hoặc kích thước không khớp với pathPositions.");
            enabled = false;
            return;
        }

        if (pathPositions == null || pathPositions.Length < 2 || pathLength <= 0)
        {
            Debug.LogError("Path không hợp lệ! Kiểm tra PathCreator setup.");
            enabled = false;
            return;
        }

        GenerateSegmentSequence();
        CreateSnakeSegments();
    }

    void GenerateSegmentSequence()
    {
        segmentSequence.Clear();

        segmentSequence.Add(SegmentType.Head);

        if (spawner != null)
        {
            var spawnerSequence = spawner.GetFinalScales();
            foreach (var busColor in spawnerSequence)
            {
                segmentSequence.Add(busColor.ToSegmentType());
            }
        }
        else
        {
            Debug.LogWarning("Không tìm thấy Spawner! Tạo rắn mặc định.");
            for (int i = 1; i <= 10; i++)
            {
                segmentSequence.Add(SegmentType.Red);
            }
        }

        segmentSequence.Add(SegmentType.Tail);
    }

    void CreateSnakeSegments()
    {
        for (int i = 0; i < segmentSequence.Count; i++)
        {
            SegmentType segmentType = segmentSequence[i];

            GameObject prefabToUse = segmentPrefab;
            if (segmentType == SegmentType.Head && headPrefab != null)
            {
                prefabToUse = headPrefab;
            }
            else if (segmentType == SegmentType.Tail && tailPrefab != null)
            {
                prefabToUse = tailPrefab;
            }

            GameObject segmentObj = Instantiate(prefabToUse, transform);

            SnakeSegment segment = segmentObj.GetComponent<SnakeSegment>() ?? segmentObj.AddComponent<SnakeSegment>();

            segment.SetSegmentIndex(i);
            segment.SetSegmentType(segmentType);
            segment.segmentSpacing = segmentSpacing;

            float initialOffset = GetTotalDistanceUpToSegment(i);
            Vector3 startPos = GetPositionOnPath(-initialOffset / pathLength);
            segmentObj.transform.position = startPos;

            segments.Add(segment);
            segmentObj.name = $"Segment_{i}_{segmentType}";
        }

        UpdateSegmentSortingOrders();
        Debug.Log($"Đã tạo rắn với {segments.Count} đốt: {string.Join(", ", segmentSequence)}");
    }

    // CẬP NHẬT: Hàm tính toán khoảng cách được viết lại cho rõ ràng và chính xác
    private float GetTotalDistanceUpToSegment(int index)
    {
        // Đốt đầu (index 0) luôn ở gốc
        if (index <= 0) return 0f;

        // Nếu chỉ có 2 đốt (đầu và đuôi), chỉ dùng khoảng cách đầu
        if (segmentSequence.Count == 2 && index == 1)
        {
            return headToFirstSegmentSpacing;
        }

        // Tính khoảng cách tới đốt ngay phía trước nó
        float distanceToPrevious = GetTotalDistanceUpToSegment(index - 1);

        // Xác định khoảng cách cần thêm vào dựa trên vị trí của đốt *phía trước*
        // Nếu đốt hiện tại là đuôi (index cuối cùng)
        if (index == segmentSequence.Count - 1)
        {
            // Khoảng cách là giữa đốt thân cuối và đuôi
            return distanceToPrevious + lastSegmentToTailSpacing;
        }
        // Nếu đốt hiện tại là đốt thân đầu tiên (index = 1)
        else if (index == 1)
        {
            // Khoảng cách là giữa đầu và đốt thân đầu tiên
            return distanceToPrevious + headToFirstSegmentSpacing;
        }
        // Các trường hợp còn lại là các đốt thân
        else
        {
            // Khoảng cách là khoảng cách tiêu chuẩn
            return distanceToPrevious + segmentSpacing;
        }
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

            float totalOffsetDistance = GetTotalDistanceUpToSegment(i);
            float segmentProgress = currentPathProgress - (totalOffsetDistance / pathLength);

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
        }

        UpdateSegmentSortingOrders();

        if (pathLength > 0)
        {
            currentPathProgress -= (segmentSpacing * 1f / pathLength);
            currentPathProgress = Mathf.Max(0, currentPathProgress);
        }
    }

    void UpdateSegmentSortingOrders()
    {
        if (segments == null) return;

        int totalSegments = segments.Count;
        for (int i = 0; i < totalSegments; i++)
        {
            if (segments[i] != null)
            {
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

    public List<SegmentType> GetSegmentTypes()
    {
        return segments.Where(s => s != null && !s.IsDestroyed())
                      .Select(s => s.GetSegmentType())
                      .ToList();
    }

    public int GetDestructibleSegmentCount()
    {
        return segments.Count(s => s != null && !s.IsDestroyed() && s.GetSegmentType().IsDestructible());
    }
}