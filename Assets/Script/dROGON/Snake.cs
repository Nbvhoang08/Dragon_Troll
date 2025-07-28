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

    private List<SnakeSegment> segments = new List<SnakeSegment>();
    private Vector3[] pathPositions;
    private Vector3[] pathRotations;
    private float pathLength;
    private float currentPathProgress = 0f;
    private bool isMoving = true;
    private bool isReversing = false;
    private List<SegmentType> segmentSequence = new List<SegmentType>();

    // Thêm cache cho smooth rotation - SỬA ĐỔI: Sử dụng SnakeSegment làm key thay vì int
    private Dictionary<SnakeSegment, float> segmentCurrentRotations = new Dictionary<SnakeSegment, float>();
    private Dictionary<SnakeSegment, bool> segmentFlipStates = new Dictionary<SnakeSegment, bool>();

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
        InitializeRotationCache();
    }

    void InitializeRotationCache()
    {
        segmentCurrentRotations.Clear();
        segmentFlipStates.Clear();

        // SỬA ĐỔI: Sử dụng segment object làm key
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

        if (segmentSequence.Count == 2 && index == 1)
        {
            return headToFirstSegmentSpacing;
        }

        float distanceToPrevious = GetTotalDistanceUpToSegment(index - 1);

        if (index == segmentSequence.Count - 1)
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
        // Bỏ qua Head và Tail khi chuyển đổi
        if (segmentType == SegmentType.Head || segmentType == SegmentType.Tail)
            return BusColor.None; // Default

        return (BusColor)((int)segmentType - 1);
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

            // Smooth position update
            segments[i].UpdatePosition(targetPosition, positionSmoothSpeed);

            // Smooth rotation update - SỬA ĐỔI: Truyền segment object thay vì index
            UpdateSegmentRotationSmooth(segments[i], targetRotation);
        }
    }

    // PHƯƠNG THỨC SỬA ĐỔI: Sử dụng segment object thay vì segmentIndex
    void UpdateSegmentRotationSmooth(SnakeSegment segment, Vector3 targetRotation)
    {
        float targetRotationZ = targetRotation.z;

        // Lấy rotation hiện tại từ cache - SỬA ĐỔI: Sử dụng segment làm key
        if (!segmentCurrentRotations.ContainsKey(segment))
        {
            segmentCurrentRotations[segment] = targetRotationZ;
        }

        float currentRotationZ = segmentCurrentRotations[segment];

        // Tính toán shortest angle để tránh quay vòng 360 độ
        float deltaRotation = Mathf.DeltaAngle(currentRotationZ, targetRotationZ);

        // Smooth lerp rotation
        float smoothedRotation = currentRotationZ + deltaRotation * rotationSmoothSpeed * Time.deltaTime;

        // Update cache - SỬA ĐỔI: Sử dụng segment làm key
        segmentCurrentRotations[segment] = smoothedRotation;

        // Normalize rotation để tính flip
        float normalizedRotation = NormalizeAngle(smoothedRotation);

        // Kiểm tra flip state với hysteresis để tránh flickering - SỬA ĐỔI: Sử dụng segment làm key
        bool currentFlipState = segmentFlipStates.ContainsKey(segment) ? segmentFlipStates[segment] : false;
        bool shouldFlipY = ShouldFlipWithHysteresis(normalizedRotation, currentFlipState);

        // Chỉ update flip nếu có thay đổi
        if (shouldFlipY != currentFlipState)
        {
            segmentFlipStates[segment] = shouldFlipY;
            segment.SetFlipY(shouldFlipY);
        }

        // Apply smooth rotation
        Vector3 smoothRotationVector = new Vector3(0, 0, smoothedRotation);
        segment.UpdateRotation(smoothRotationVector);
    }

    // PHƯƠNG THỨC MỚI: Flip với hysteresis để tránh flickering
    bool ShouldFlipWithHysteresis(float rotationZ, bool currentFlipState)
    {
        const float hysteresisMargin = 5f; // 5 độ margin để tránh flickering

        if (currentFlipState)
        {
            // Nếu đang flip, chỉ unflip khi rotation về khoảng an toàn
            return !(rotationZ <= (90f - hysteresisMargin) && rotationZ >= (-90f + hysteresisMargin));
        }
        else
        {
            // Nếu không flip, chỉ flip khi rotation ra khỏi khoảng an toàn
            return rotationZ > (90f + hysteresisMargin) || rotationZ < (-90f - hysteresisMargin);
        }
    }

    float NormalizeAngle(float angle)
    {
        while (angle > 180f)
            angle -= 360f;
        while (angle < -180f)
            angle += 360f;
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

        // IMPROVED: Interpolate rotation giữa các điểm để mượt hơn
        for (int i = 1; i < pathPositions.Length; i++)
        {
            float segmentLength = Vector3.Distance(pathPositions[i - 1], pathPositions[i]);
            if (totalLength + segmentLength >= targetLength)
            {
                // Interpolate rotation giữa 2 điểm
                if (segmentLength > 0)
                {
                    float t = (targetLength - totalLength) / segmentLength;
                    Vector3 rot1 = pathRotations[i - 1];
                    Vector3 rot2 = pathRotations[i];

                    // Sử dụng Mathf.LerpAngle cho Z rotation để tránh jump 360 độ
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
        // SỬA ĐỔI: Tìm segment bị destroy và clean up cache
        SnakeSegment destroyedSegment = segments.Find(s => s != null && s.segmentIndex == segmentIndex);
        if (destroyedSegment != null)
        {
            // Clean up rotation cache for destroyed segment
            if (segmentCurrentRotations.ContainsKey(destroyedSegment))
                segmentCurrentRotations.Remove(destroyedSegment);
            if (segmentFlipStates.ContainsKey(destroyedSegment))
                segmentFlipStates.Remove(destroyedSegment);
        }

        ReconnectSegments(segmentIndex);
    }

    void ReconnectSegments(int destroyedIndex)
    {
        // SỬA ĐỔI: Giữ lại rotation data trước khi rebuild segments list
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

        if (segments.Count == 0)
        {
            return;
        }

        // SỬA ĐỔI: Rebuild cache mà vẫn giữ lại rotation state của các segment còn sống
        segmentCurrentRotations.Clear();
        segmentFlipStates.Clear();

        for (int i = 0; i < segments.Count; i++)
        {
            segments[i].SetSegmentIndex(i);

            // Preserve rotation state từ data đã lưu trước đó
            if (preservedRotations.ContainsKey(segments[i]))
            {
                segmentCurrentRotations[segments[i]] = preservedRotations[segments[i]];
            }
            else
            {
                // Nếu không có data cũ, khởi tạo với rotation hiện tại
                segmentCurrentRotations[segments[i]] = segments[i].transform.eulerAngles.z;
            }

            if (preservedFlipStates.ContainsKey(segments[i]))
            {
                segmentFlipStates[segments[i]] = preservedFlipStates[segments[i]];
            }
            else
            {
                // Nếu không có data cũ, khởi tạo với flip state hiện tại
                segmentFlipStates[segments[i]] = segments[i].IsFlippedY();
            }
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