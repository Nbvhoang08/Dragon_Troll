using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SnakePathCreator : MonoBehaviour
{
    [Header("Path Settings")]
    public Transform[] pathPoints;

    [Header("DOTween Path Settings")]
    [Tooltip("Loại đường cong được tạo bởi DOTween. CatmullRom cho đường cong mượt.")]
    public PathType pathType = PathType.CatmullRom;

    [Tooltip("Độ phân giải của đường đi. Số điểm được lấy mẫu càng cao, đường đi càng mượt.")]
    [Range(10, 500)]
    public int pathResolution = 150;

    [Header("Rotation Smoothing")]
    [Tooltip("Làm mượt góc xoay của rắn. Giá trị cao hơn sẽ nhìn về phía trước xa hơn, làm cho các khúc cua mượt mà hơn nhưng ít phản ứng hơn. Đặt thành 1 để tắt tính năng làm mượt.")]
    [Range(1, 25)]
    public int rotationSmoothingFactor = 10;

    [Header("Path Visualization")]
    public LineRenderer pathLineRenderer;
    public Color pathColor = Color.white;
    public float pathWidth = 0.1f;
    public GameObject pathPointPrefab;
    public bool showPathPoints = true;

    private Vector3[] pathPositions;
    private Vector3[] pathRotations;
    private bool isInitialized = false;

    void Awake()
    {
        InitializePath();
    }

    void Start()
    {
        if (!isInitialized)
        {
            InitializePath();
        }
        VisualizePathPoints();
    }

    public void InitializePath()
    {
        if (isInitialized) return;
        CreatePath();
        isInitialized = true;
    }

    void CreatePath()
    {
        if (pathPoints == null || pathPoints.Length < 2)
        {
            Debug.LogError("Cần ít nhất 2 điểm để tạo đường đi! Vui lòng gán pathPoints trong Inspector.");
            CreateDefaultPath();
            return;
        }

        List<Vector3> waypointsList = new List<Vector3>();
        foreach (var point in pathPoints)
        {
            if (point != null)
            {
                waypointsList.Add(point.position);
            }
        }

        if (waypointsList.Count < 2)
        {
            Debug.LogError("Không đủ pathPoints hợp lệ! Tạo path mặc định.");
            CreateDefaultPath();
            return;
        }

        Vector3[] waypoints = waypointsList.ToArray();

        GameObject dummy = new GameObject("DOTweenPathGenerator_Temp");
        dummy.transform.position = waypoints[0];

        Tweener pathTweener = dummy.transform.DOPath(waypoints, 1f, pathType, PathMode.TopDown2D)
            .SetAutoKill(false)
            .Pause();

        pathTweener.ForceInit();

        pathPositions = new Vector3[pathResolution + 1];
        for (int i = 0; i <= pathResolution; i++)
        {
            float percentage = (float)i / pathResolution;
            pathPositions[i] = pathTweener.PathGetPoint(percentage);
        }

        pathTweener.Kill();
        if (Application.isPlaying) Destroy(dummy);
        else DestroyImmediate(dummy);

        pathRotations = new Vector3[pathPositions.Length];
        if (pathPositions.Length > 1)
        {
            int lookAhead = Mathf.Clamp(rotationSmoothingFactor, 1, pathPositions.Length - 1);

            for (int i = 0; i < pathPositions.Length; i++)
            {
                int lookAheadIndex = Mathf.Min(i + lookAhead, pathPositions.Length - 1);
                Vector3 direction = pathPositions[lookAheadIndex] - pathPositions[i];
                
                if (i > pathPositions.Length - lookAhead)
                {
                    direction = pathPositions[pathPositions.Length - 1] - pathPositions[i];
                }

                if (direction.sqrMagnitude > 0.001f)
                {
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    pathRotations[i] = new Vector3(0, 0, angle);
                }
                else if (i > 0)
                {
                    pathRotations[i] = pathRotations[i - 1];
                }
            }
        }

        if (pathLineRenderer != null)
        {
            pathLineRenderer.positionCount = pathPositions.Length;
            pathLineRenderer.SetPositions(pathPositions);
            pathLineRenderer.startColor = pathColor;
            pathLineRenderer.startWidth = pathWidth;
            pathLineRenderer.endWidth = pathWidth;
            pathLineRenderer.useWorldSpace = true;
        }
    }

    void CreateDefaultPath()
    {
        pathPositions = new Vector3[]
        {
            new Vector3(-8, 0, 0),
            new Vector3(-4, 2, 0),
            new Vector3(0, 0, 0),
            new Vector3(4, -2, 0),
            new Vector3(8, 0, 0)
        };

        pathRotations = new Vector3[pathPositions.Length];
        Debug.Log("Đã tạo path mặc định. Hãy thiết lập pathPoints trong Inspector để tùy chỉnh đường đi.");
    }

    void VisualizePathPoints()
    {
        if (!showPathPoints || pathPointPrefab == null || pathPoints == null) return;
        foreach (var pointTransform in pathPoints)
        {
            if (pointTransform != null)
            {
                GameObject point = Instantiate(pathPointPrefab, pointTransform.position, pointTransform.rotation);
                point.name = "PathPoint_" + pointTransform.GetSiblingIndex();
                point.transform.SetParent(transform);
            }
        }
    }

    public Vector3[] GetPathPositions()
    {
        if (!isInitialized) InitializePath();
        return pathPositions;
    }

    public Vector3[] GetPathRotations()
    {
        if (!isInitialized) InitializePath();
        return pathRotations;
    }

    public float GetPathLength()
    {
        if (!isInitialized) InitializePath();
        if (pathPositions == null || pathPositions.Length < 2) return 0f;

        float totalLength = 0f;
        for (int i = 1; i < pathPositions.Length; i++)
        {
            totalLength += Vector3.Distance(pathPositions[i - 1], pathPositions[i]);
        }
        return totalLength;
    }

    public Vector3 GetPointAtProgress(float progress)
    {
        if (!isInitialized) InitializePath();
        if (pathPositions == null || pathPositions.Length == 0) return Vector3.zero;

        progress = Mathf.Clamp01(progress);

        if (pathPositions.Length == 1) return pathPositions[0];
        if (progress <= 0f) return pathPositions[0];
        if (progress >= 1f) return pathPositions[pathPositions.Length - 1];

        float pathLength = GetPathLength();
        if (pathLength <= 0) return pathPositions[0];

        float totalLength = 0f;
        float targetLength = progress * pathLength;
        for (int i = 1; i < pathPositions.Length; i++)
        {
            float segmentLength = Vector3.Distance(pathPositions[i - 1], pathPositions[i]);
            if (totalLength + segmentLength >= targetLength)
            {
                float p = segmentLength > 0 ? (targetLength - totalLength) / segmentLength : 0;
                return Vector3.Lerp(pathPositions[i - 1], pathPositions[i], p);
            }
            totalLength += segmentLength;
        }
        return pathPositions[pathPositions.Length - 1];
    }
}