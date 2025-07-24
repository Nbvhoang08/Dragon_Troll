using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SnakePathCreator : MonoBehaviour
{
    [Header("Path Settings")]
    public Transform[] pathPoints;
    public LineRenderer pathLineRenderer;
    public Color pathColor = Color.white;
    public float pathWidth = 0.1f;

    [Header("Path Visualization")]
    public GameObject pathPointPrefab;
    public bool showPathPoints = true;

    private Vector3[] pathPositions;
    private bool isInitialized = false;

    void Awake()
    {
        // Khởi tạo sớm hơn trong Awake
        InitializePath();
    }

    void Start()
    {
        // Đảm bảo đã được khởi tạo
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

            // Tạo path mặc định nếu không có
            CreateDefaultPath();
            return;
        }

        // Kiểm tra xem có pathPoint nào bị null không
        List<Vector3> validPositions = new List<Vector3>();
        for (int i = 0; i < pathPoints.Length; i++)
        {
            if (pathPoints[i] != null)
            {
                validPositions.Add(pathPoints[i].position);
            }
            else
            {
                Debug.LogWarning($"PathPoint tại index {i} bị null! Bỏ qua điểm này.");
            }
        }

        if (validPositions.Count < 2)
        {
            Debug.LogError("Không đủ pathPoints hợp lệ! Tạo path mặc định.");
            CreateDefaultPath();
            return;
        }

        pathPositions = validPositions.ToArray();

        // Tạo LineRenderer để hiển thị đường đi
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
        // Tạo path mặc định từ trái sang phải
        pathPositions = new Vector3[]
        {
            new Vector3(-8, 0, 0),
            new Vector3(-4, 2, 0),
            new Vector3(0, 0, 0),
            new Vector3(4, -2, 0),
            new Vector3(8, 0, 0)
        };

        Debug.Log("Đã tạo path mặc định. Hãy thiết lập pathPoints trong Inspector để tùy chỉnh đường đi.");
    }

    void VisualizePathPoints()
    {
        if (!showPathPoints || pathPointPrefab == null || pathPositions == null) return;

        for (int i = 0; i < pathPositions.Length; i++)
        {
            GameObject point = Instantiate(pathPointPrefab, pathPositions[i], Quaternion.identity);
            point.name = "PathPoint_" + i;
            point.transform.SetParent(transform);
        }
    }

    public Vector3[] GetPathPositions()
    {
        if (!isInitialized)
        {
            InitializePath();
        }

        return pathPositions;
    }

    public float GetPathLength()
    {
        if (!isInitialized)
        {
            InitializePath();
        }

        if (pathPositions == null || pathPositions.Length < 2)
        {
            Debug.LogWarning("PathPositions không hợp lệ trong GetPathLength!");
            return 0f;
        }

        float totalLength = 0f;
        for (int i = 1; i < pathPositions.Length; i++)
        {
            totalLength += Vector3.Distance(pathPositions[i - 1], pathPositions[i]);
        }

        return totalLength;
    }

    // Method để debug thông tin path
    public void DebugPathInfo()
    {
        Debug.Log($"Path initialized: {isInitialized}");
        Debug.Log($"PathPositions count: {(pathPositions != null ? pathPositions.Length : 0)}");
        Debug.Log($"PathPoints count: {(pathPoints != null ? pathPoints.Length : 0)}");
        Debug.Log($"Path length: {GetPathLength()}");
    }
}