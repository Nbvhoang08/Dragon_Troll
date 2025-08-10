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
    private bool hasReachedMiddle = false;
    private bool canCheckWinLose = false;
    private bool isInitialized = false;

    private void OnEnable()
    {
        GameEvents.GameStart += OnGameStart;
    }

    private void OnDisable()
    {
        GameEvents.GameStart -= OnGameStart;
    }

    void OnGameStart()
    {
        if (!isInitialized)
        {
            StartCoroutine(WaitForPathCreatorAndInitialize());
        }
    }

    private System.Collections.IEnumerator WaitForPathCreatorAndInitialize()
    {
        Debug.Log("Đang đợi SnakePathCreator...");

        while (pathCreator == null)
        {
            pathCreator = GetComponent<SnakePathCreator>();

            if (pathCreator == null && transform.parent != null)
            {
                pathCreator = GetComponentInParent<SnakePathCreator>();
            }

            if (pathCreator == null)
            {
                pathCreator = GetComponentInChildren<SnakePathCreator>();
            }

            if (pathCreator == null)
            {
                pathCreator = FindObjectOfType<SnakePathCreator>();
            }

            yield return null;
        }

        Debug.Log($"Tìm thấy SnakePathCreator: {pathCreator.name}, đang đợi khởi tạo...");

        yield return null;

        int maxRetries = 200;
        int retryCount = 0;

        while (retryCount < maxRetries)
        {
            if (!pathCreator.IsInitialized())
            {
                pathCreator.InitializePath();
            }

            if (pathCreator.GetPathPositions() != null && pathCreator.GetPathPositions().Length > 1)
            {
                Debug.Log("SnakePathCreator đã sẵn sàng, bắt đầu khởi tạo rắn!");
                InitializeSnake();
                yield break;
            }

            retryCount++;
            yield return null;
        }

        Debug.LogError("Timeout: SnakePathCreator không khởi tạo được sau 200 frames!");
    }

    public void ForceInitialize()
    {
        if (!isInitialized && pathCreator != null)
        {
            InitializeSnake();
        }
        else if (pathCreator == null)
        {
            StartCoroutine(WaitForPathCreatorAndInitialize());
        }
    }

    void Update()
    {
        if (GameManager.Instance.gameState != GameState.Playing) return;
        HandleInput();
        if (isMoving && segments.Count > 0 && pathPositions != null)
        {
            MoveSnake();
        }

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
        if (isInitialized)
        {
            Debug.Log("Rắn đã được khởi tạo, bỏ qua việc khởi tạo lại.");
            return;
        }

        Debug.Log("Bắt đầu khởi tạo rắn...");

        if (pathCreator == null)
        {
            Debug.LogWarning("PathCreator is null! Trying to find it...");

            pathCreator = GetComponent<SnakePathCreator>();

            if (pathCreator == null && transform.parent != null)
            {
                pathCreator = GetComponentInParent<SnakePathCreator>();
            }

            if (pathCreator == null)
            {
                pathCreator = GetComponentInChildren<SnakePathCreator>();
            }

            if (pathCreator == null)
            {
                pathCreator = FindObjectOfType<SnakePathCreator>();
            }

            if (pathCreator != null)
            {
                Debug.Log($"Đã tìm thấy SnakePathCreator: {pathCreator.name}");
            }
        }

        if (pathCreator == null)
        {
            Debug.LogError("Không tìm thấy SnakePathCreator trong scene!");
            return;
        }

        if (!pathCreator.IsInitialized())
        {
            Debug.Log("PathCreator chưa khởi tạo, đang khởi tạo...");
            pathCreator.InitializePath();
        }

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

        isInitialized = true;
        Debug.Log($"Rắn đã được khởi tạo thành công với {segments.Count} đốt!");
    }

    // THÊM MỚI: Hàm xóa các đốt rắn theo màu và số lượng
    public void RemoveSegmentsByColorAndCount(BusColor color, int count)
    {
        Debug.Log($"Xóa {count} đốt rắn màu {color}");

        if (segments == null || segments.Count == 0)
        {
            Debug.LogWarning("Không có segments để xóa!");
            return;
        }

        // Tìm tất cả segments có màu tương ứng (loại trừ đầu và đuôi)
        var targetSegments = segments.Where(s => s != null &&
                                                !s.IsDestroyed() &&
                                                s.busColor == color &&
                                                s.GetSegmentType().IsDestructible())
                                   .OrderBy(s => s.segmentIndex) // Xóa từ đầu về cuối
                                   .Take(count) // Lấy đúng số lượng cần xóa
                                   .ToList();

        Debug.Log($"Tìm thấy {targetSegments.Count} đốt màu {color} để xóa");

        // Xóa từng segment
        foreach (var segment in targetSegments)
        {
            if (segment != null && !segment.IsDestroyed())
            {
                Debug.Log($"Xóa segment index {segment.segmentIndex} màu {segment.busColor}");
                segment.DestroySegment();
            }
        }

        // Cập nhật lại vị trí các segments còn lại
        DOVirtual.DelayedCall(0.1f, () =>
        {
            UpdateAllSegmentPositions();
        });
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

        Debug.Log($"Generated segment sequence: {string.Join(", ", segmentSequence)}");
    }

    void CreateSnakeSegments()
    {
        Debug.Log($"Tạo {segmentSequence.Count} đốt rắn...");

        for (int i = 0; i < segmentSequence.Count; i++)
        {
            SegmentType segmentType = segmentSequence[i];
            SnakeSegment segment = Pool.Instance.segment;

            if (segment == null)
            {
                Debug.LogError($"Không thể lấy segment từ pool cho index {i}!");
                continue;
            }

            segment.ReturnToPool();
            segment = Pool.Instance.segment;

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

            Debug.Log($"Tạo segment {i}: {segmentType} tại vị trí {startPos}");
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

        if (!hasReachedMiddle && currentPathProgress >= 0.5f)
        {
            hasReachedMiddle = true;
            canCheckWinLose = true;
            Debug.Log("Rắn đã đến giữa path! Chuyển sang tốc độ bình thường và bắt đầu tính win/lose.");
        }

        float currentSpeed;
        if (!hasReachedMiddle)
        {
            currentSpeed = isReversing ? initialSpeed : initialSpeed;
        }
        else
        {
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

    void CheckWinLoseConditions()
    {
        if (gameEnded || !canCheckWinLose) return;

        CheckWinCondition();
        CheckLoseCondition();
    }

    void CheckWinCondition()
    {
        int destructibleSegments = GetDestructibleSegmentCount();

        if (destructibleSegments <= 2)
        {
            UIManager.Instance.OpenUI<UIWIN>();
            Debug.Log("WIN");
            gameEnded = true;
            OnWin();
        }
    }

    void CheckLoseCondition()
    {
        if (pathPositions == null || pathPositions.Length == 0) return;

        bool currentlyAtEndPoint = currentPathProgress >= 1.0f;

        if (currentlyAtEndPoint)
        {
            if (!isAtEndPoint)
            {
                isAtEndPoint = true;
                endPointTimer = 0f;
                Debug.Log("Rắn đã đến điểm cuối, bắt đầu đếm thời gian...");
            }
            else
            {
                endPointTimer += Time.deltaTime;
                Debug.Log($"Đang đếm thời gian tại điểm cuối: {endPointTimer:F1}/{endPointWaitTime}s");

                if (endPointTimer >= endPointWaitTime)
                {
                    UIManager.Instance.OpenUI<UILOSE>();
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
                isAtEndPoint = false;
                endPointTimer = 0f;
                Debug.Log("Rắn đã rời khỏi điểm cuối, reset timer");
            }
        }
    }

    void OnWin()
    {
        StopSnake();
    }

    void OnLose()
    {
        StopSnake();
    }

    public void DestroySnake()
    {
        Debug.Log("Đang xóa rắn...");

        DOTween.Kill(transform);

        foreach (var segment in segments)
        {
            if (segment != null)
            {
                segment.ReturnToPool();
            }
        }

        segments.Clear();
        segmentCurrentRotations.Clear();
        segmentFlipStates.Clear();
        segmentSequence.Clear();

        ResetGameState();
        isInitialized = false;

        Debug.Log("Đã xóa rắn hoàn toàn!");
    }

    public void ResetGameState()
    {
        gameEnded = false;
        isAtEndPoint = false;
        endPointTimer = 0f;
        hasReachedMiddle = false;
        canCheckWinLose = false;
        currentPathProgress = 0f;
        isMoving = true;
        isReversing = false;
        Debug.Log("Đã reset game state - currentPathProgress reset về 0!");
    }

    public void StopSnake() => isMoving = false;
    public void StartSnake() => isMoving = true;
    public int GetSegmentCount() => segments.Count;
    public bool IsReversing() => isReversing;
    public void SetReverse(bool reverse) => isReversing = reverse;
    public void ForceForward() => isReversing = false;
    public void ForceReverse() => isReversing = true;
    public bool HasReachedMiddle() => hasReachedMiddle;
    public bool CanCheckWinLose() => canCheckWinLose;
    public bool IsInitialized() => isInitialized;

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

    public float GetCurrentPathProgress()
    {
        return currentPathProgress;
    }
}