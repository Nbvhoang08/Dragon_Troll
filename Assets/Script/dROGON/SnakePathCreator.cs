using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SnakePathCreator : MonoBehaviour
{
    [Header("Path Settings")]
    public Transform[] pathPoints;

    [Tooltip("Rotation (Euler angles) for each corresponding path point.")]
    public Vector3[] pathRotations;

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

        if (pathRotations == null || pathRotations.Length != pathPoints.Length)
        {
            Debug.LogWarning($"pathRotations is not set up correctly. Resizing to match pathPoints. Please set the rotation values in the Inspector.");
            System.Array.Resize(ref pathRotations, pathPoints.Length);
        }

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
        if (!showPathPoints || pathPointPrefab == null || pathPositions == null) return;
        for (int i = 0; i < pathPositions.Length; i++)
        {
            GameObject point = Instantiate(pathPointPrefab, pathPositions[i], Quaternion.Euler(pathRotations[i]));
            point.name = "PathPoint_" + i;
            point.transform.SetParent(transform);
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
}