using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using System;

public class Snake : MonoBehaviour
{
    [Header("Snake Settings")]
    public float moveSpeed = 2f;
    public float reverseSpeed = 1.5f;
    [Header("Initial Speed Settings")]
    [Tooltip("Tốc độ di chuyển ban đầu (từ đầu đến giữa path)")]
    public float initialSpeed = 5f;
    [Tooltip("Tốc độ di chuyển sau khi đến giữa path")]
    public float normalSpeed = 1f;
    [Tooltip("Khoảng cách chung giữa các đốt thân.")]
    public float segmentSpacing = 1f;
    [Tooltip("Khoảng cách riêng giữa đầu rắn và đốt thân đầu tiên.")]
    public float headToFirstSegmentSpacing = 1.5f;
    [Tooltip("Khoảng cách riêng giữa đốt thân cuối cùng và đuôi rắn.")]
    public float lastSegmentToTailSpacing = 1.5f;

    [Header("Smooth Movement Settings")]
    [Tooltip("Tốc độ làm mượt rotation (càng cao càng mượt)")]
    [Range(1f, 20f)]
    public float rotationSmoothSpeed = 8f;
    [Tooltip("Tốc độ làm mượt position")]
    [Range(0.05f, 0.5f)]
    public float positionSmoothSpeed = 0.1f;

    public DragonSpriteData dragonSpriteData;

    [Header("Path")]
    public SnakePathCreator pathCreator;

    [Header("Spawner Integration")]
    public Spawner spawner;

    [Header("Win/Lose Conditions")]
    [Tooltip("Thời gian chờ tại điểm cuối trước khi thua (giây)")]
    public float endPointWaitTime = 5f;
    [Tooltip("Khoảng cách để coi như rắn đã đến điểm cuối")]
    public float endPointThreshold = 0.1f;

    private List<SnakeSegment> segments = new List<SnakeSegment>();
    private Vector3[] pathPositions;
    private Vector3[] pathRotations;
    private float pathLength;
    private float currentPathProgress = 0f;
    private bool isMoving = true;
    private bool isReversing = false;
    private List<SegmentType> segmentSequence = new List<SegmentType>();

    private Dictionary<SnakeSegment, float> segmentCurrentRotations = new Dictionary<SnakeSegment, float>();
    private Dictionary<SnakeSegment, bool> segmentFlipStates = new Dictionary<SnakeSegment, bool>();

    // Win/Lose variables
    private bool isAtEndPoint = false;
    private float endPointTimer = 0f;
    private bool gameEnded = false;
    private bool hasReachedMiddle = false; // Đã đến giữa path chưa
    private bool canCheckWinLose = false; // Có thể kiểm tra win/lose chưa

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
        
        // Kiểm tra điều kiện thắng/thua
        CheckWinLoseConditions();
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
        InitializeRotationCache();
    }

    void InitializeRotationCache()
    {
        segmentCurrentRotations.Clear();
        segmentFlipStates.Clear();

        for (int i = 0; i < segments.Count; i++)
        {
            segmentCurrentRotations[segments[i]] = 0f;
            segmentFlipStates[segments[i]] = false;
        }
    }

    void GenerateSegmentSequence()
    {
        segmentSequence.Clear();
        segmentSequence.Add(SegmentType.Head);

        if (spawner != null)
        {
            var spawnerSequence = spawner.GetFinalScales();
            foreach (var _busColor in spawnerSequence)
            {
                segmentSequence.Add(_busColor.ToSegmentType());
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
            SnakeSegment segment = Pool.Instance.segment;
            segment.SetSegmentIndex(i);
            segment.SetSegmentType(dragonSpriteData.GetVisualData(segmentType).dragonSegment);
            segment.busColor = ToBusColor(segmentType);
            segment.segmentSpacing = segmentSpacing;

            float initialOffset = GetTotalDistanceUpToSegment(i);
            Vector3 startPos = GetPositionOnPath(-initialOffset / pathLength);
            segment.gameObject.transform.position = startPos;
            segment.gameObject.transform.parent = transform;
            segments.Add(segment);
            segment.gameObject.name = $"Segment_{i}_{segmentType}";
        }

        UpdateSegmentSortingOrders();
        Debug.Log($"Đã tạo rắn với {segments.Count} đốt: {string.Join(", ", segmentSequence)}");
    }

    private float GetTotalDistanceUpToSegment(int index)
    {
        if (index <= 0) return 0f;
        if (segmentSequence.Count == 2 && index == 1) return headToFirstSegmentSpacing;

        float distanceToPrevious = GetTotalDistanceUpToSegment(index - 1);

        if (index == segments.Count - 1)
        {
            return distanceToPrevious + lastSegmentToTailSpacing;
        }
        else if (index == 1)
        {
            return distanceToPrevious + headToFirstSegmentSpacing;
        }
        else
        {
            return distanceToPrevious + segmentSpacing;
        }
    }

    public BusColor ToBusColor(SegmentType segmentType)
    {
        if (segmentType == SegmentType.Head || segmentType == SegmentType.Tail)
            return BusColor.None;
        return (BusColor)((int)segmentType - 1);
    }

    void MoveSnake()
    {
        if (pathPositions == null || pathLength <= 0) return;

        // Kiểm tra xem đã đến giữa path chưa
        if (!hasReachedMiddle && currentPathProgress >= 0.5f)
        {
            hasReachedMiddle = true;
            canCheckWinLose = true;
            Debug.Log("Rắn đã đến giữa path! Chuyển sang tốc độ bình thường và bắt đầu tính win/lose.");
        }

        // Chọn tốc độ dựa trên vị trí hiện tại
        float currentSpeed;
        if (!hasReachedMiddle)
        {
            // Chưa đến giữa path - dùng tốc độ ban đầu
            currentSpeed = isReversing ? initialSpeed : initialSpeed;
        }
        else
        {
            // Đã đến giữa path - dùng tốc độ bình thường
            currentSpeed = isReversing ? normalSpeed : normalSpeed;
        }

        float speedMultiplier = isReversing ? -1f : 1f;
        currentPathProgress += (currentSpeed / pathLength) * Time.deltaTime * speedMultiplier;

        currentPathProgress = Mathf.Clamp01(currentPathProgress);

        UpdateAllSegmentPositions();
    }

    void UpdateAllSegmentPositions()
    {
        for (int i = 0; i < segments.Count; i++)
        {
            if (segments[i] == null || segments[i].IsDestroyed()) continue;

            float totalOffsetDistance = GetTotalDistanceUpToSegment(i);
            float segmentProgress = currentPathProgress - (totalOffsetDistance / pathLength);

            Vector3 targetPosition = GetPositionOnPath(segmentProgress);
            Vector3 targetRotation = GetRotationOnPath(segmentProgress);

            segments[i].UpdatePosition(targetPosition, positionSmoothSpeed);
            UpdateSegmentRotationSmooth(segments[i], targetRotation);
        }
    }

    void UpdateSegmentRotationSmooth(SnakeSegment segment, Vector3 targetRotation)
    {
        float targetRotationZ = targetRotation.z;
        if (!segmentCurrentRotations.ContainsKey(segment))
        {
            segmentCurrentRotations[segment] = targetRotationZ;
        }
        float currentRotationZ = segmentCurrentRotations[segment];
        float deltaRotation = Mathf.DeltaAngle(currentRotationZ, targetRotationZ);
        float smoothedRotation = currentRotationZ + deltaRotation * rotationSmoothSpeed * Time.deltaTime;
        segmentCurrentRotations[segment] = smoothedRotation;

        float normalizedRotation = NormalizeAngle(smoothedRotation);
        bool currentFlipState = segmentFlipStates.ContainsKey(segment) ? segmentFlipStates[segment] : false;
        bool shouldFlipY = ShouldFlipWithHysteresis(normalizedRotation, currentFlipState);

        if (shouldFlipY != currentFlipState)
        {
            segmentFlipStates[segment] = shouldFlipY;
            segment.SetFlipY(shouldFlipY);
        }

        Vector3 smoothRotationVector = new Vector3(0, 0, smoothedRotation);
        segment.UpdateRotation(smoothRotationVector);
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

    public void OnSegmentDestroyed(int segmentIndex)
    {
        SnakeSegment destroyedSegment = segments.Find(s => s != null && s.segmentIndex == segmentIndex);
        if (destroyedSegment != null)
        {
            if (segmentCurrentRotations.ContainsKey(destroyedSegment))
                segmentCurrentRotations.Remove(destroyedSegment);
            if (segmentFlipStates.ContainsKey(destroyedSegment))
                segmentFlipStates.Remove(destroyedSegment);
        }

        ReconnectSegments(segmentIndex);
    }

    void ReconnectSegments(int destroyedIndex)
    {
        Dictionary<SnakeSegment, float> preservedRotations = new Dictionary<SnakeSegment, float>(segmentCurrentRotations);
        Dictionary<SnakeSegment, bool> preservedFlipStates = new Dictionary<SnakeSegment, bool>(segmentFlipStates);

        List<SnakeSegment> newSegments = new List<SnakeSegment>();
        foreach (var segment in segments)
        {
            if (segment != null && !segment.IsDestroyed())
            {
                newSegments.Add(segment);
            }
        }
        segments = newSegments;

        if (segments.Count == 0) return;

        segmentCurrentRotations.Clear();
        segmentFlipStates.Clear();

        for (int i = 0; i < segments.Count; i++)
        {
            segments[i].SetSegmentIndex(i);
            if (preservedRotations.ContainsKey(segments[i]))
            {
                segmentCurrentRotations[segments[i]] = preservedRotations[segments[i]];
            }
            else
            {
                segmentCurrentRotations[segments[i]] = segments[i].transform.eulerAngles.z;
            }

            if (preservedFlipStates.ContainsKey(segments[i]))
            {
                segmentFlipStates[segments[i]] = preservedFlipStates[segments[i]];
            }
            else
            {
                segmentFlipStates[segments[i]] = segments[i].IsFlippedY();
            }
        }

        UpdateSegmentSortingOrders();

        if (pathLength > 0)
        {
            currentPathProgress -= (segmentSpacing / pathLength);
            currentPathProgress = Mathf.Max(0, currentPathProgress);
        }

        UpdateAllSegmentPositions();
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

    // ============ WIN/LOSE FUNCTIONS ============

    // Hàm kiểm tra điều kiện thắng/thua
    void CheckWinLoseConditions()
    {
        // Chỉ kiểm tra win/lose sau khi đã đến giữa path
        if (gameEnded || !canCheckWinLose) return;
        
        CheckWinCondition();
        CheckLoseCondition();
    }

    // Hàm kiểm tra điều kiện thắng - chỉ cần hết các đốt thân (không tính đầu và đuôi)
    void CheckWinCondition()
    {
        // Sử dụng hàm có sẵn để đếm số đốt có thể phá hủy còn lại
        // Hàm này đã loại trừ đầu và đuôi rắn
        int destructibleSegments = GetDestructibleSegmentCount();
        
       
        
        if (destructibleSegments <= 2)
        {
            Debug.Log("WIN");
            gameEnded = true;
            OnWin();
        }
    }

    // Hàm kiểm tra điều kiện thua
    void CheckLoseCondition()
    {
        if (pathPositions == null || pathPositions.Length == 0) return;
        
        // Kiểm tra xem rắn có đang ở điểm cuối không (currentPathProgress >= 1.0f)
        bool currentlyAtEndPoint = currentPathProgress >= 1.0f;
        
        if (currentlyAtEndPoint)
        {
            if (!isAtEndPoint)
            {
                // Vừa mới đến điểm cuối, bắt đầu đếm thời gian
                isAtEndPoint = true;
                endPointTimer = 0f;
                Debug.Log("Rắn đã đến điểm cuối, bắt đầu đếm thời gian...");
            }
            else
            {
                // Đang ở điểm cuối, tăng timer
                endPointTimer += Time.deltaTime;
                Debug.Log($"Đang đếm thời gian tại điểm cuối: {endPointTimer:F1}/{endPointWaitTime}s");
                
                if (endPointTimer >= endPointWaitTime)
                {
                    Debug.Log("THUA");
                    gameEnded = true;
                    OnLose();
                }
            }
        }
        else
        {
            if (isAtEndPoint)
            {
                // Rắn đã rời khỏi điểm cuối, reset timer
                isAtEndPoint = false;
                endPointTimer = 0f;
                Debug.Log("Rắn đã rời khỏi điểm cuối, reset timer");
            }
        }
    }

    // Hàm được gọi khi thắng
    void OnWin()
    {
        StopSnake();
        // Thêm logic xử lý khi thắng ở đây
        // Ví dụ: hiển thị UI thắng, chuyển level, etc.
    }

    // Hàm được gọi khi thua
    void OnLose()
    {
        StopSnake();
        // Thêm logic xử lý khi thua ở đây
        // Ví dụ: hiển thị UI thua, restart game, etc.
    }

    // Hàm reset game state (có thể gọi khi restart)
    public void ResetGameState()
    {
        gameEnded = false;
        isAtEndPoint = false;
        endPointTimer = 0f;
        hasReachedMiddle = false;
        canCheckWinLose = false;
        currentPathProgress = 0f;
    }

    // ============ PUBLIC METHODS ============

    public void StopSnake() => isMoving = false;
    public void StartSnake() => isMoving = true;
    public int GetSegmentCount() => segments.Count;
    public bool IsReversing() => isReversing;
    public void SetReverse(bool reverse) => isReversing = reverse;
    public void ForceForward() => isReversing = false;
    public void ForceReverse() => isReversing = true;
    public bool HasReachedMiddle() => hasReachedMiddle;
    public bool CanCheckWinLose() => canCheckWinLose;

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

    // New public method to get the snake's progress
    public float GetCurrentPathProgress()
    {
        return currentPathProgress;
    }
}