using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEditor;
using System.Security.Cryptography;

public class SnakeController : MonoBehaviour
{
    [Header("Path Settings")]
    public Transform[] pathPoints;
    private float moveDuration = 5f;
    public PathType pathType = PathType.CatmullRom;

    [Header("Body Settings")]
    public GameObject bodyPrefab;
    public int bodyCount = 5;
    public float spacing = 0.5f; // khoảng cách giữa các đốt
    public float followSpeed = 10f;

    private List<GameObject> bodyParts = new List<GameObject>();
    private List<Vector3> pathHistory = new List<Vector3>();

    private float recordSpacing = 0.05f;
    [SerializeField] private float moveSpeed;
    Vector3[] path;
    private Tween headTween;

    void Start()
    {
        moveDuration = CalculateTotalPathLength(pathPoints) / moveSpeed;

        // Lấy path và set z = 11
        path = new Vector3[pathPoints.Length];
        for (int i = 0; i < pathPoints.Length; i++)
        {
            Vector3 pos = pathPoints[i].position;
            pos.z = 11f;
            path[i] = pos;
        }

        transform.position = path[0];

        headTween = transform.DOPath(path, moveDuration, pathType, PathMode.TopDown2D)
                             .SetEase(Ease.Linear)
                             .SetLookAt(0.01f);

        StartCoroutine(SpawnBodyGradually());
    }

    void Update()
    {
        TrackHead();
        UpdateBodyParts();
    }

    void TrackHead()
    {
        // Lưu vị trí đầu rắn vào history nếu đủ khoảng cách với vị trí trước đó
        if (pathHistory.Count == 0 || Vector3.Distance(transform.position, pathHistory[0]) >= recordSpacing)
        {
            Vector3 headPos = transform.position;
            headPos.z = 11f;
            pathHistory.Insert(0, headPos);
        }

        // Giới hạn độ dài của history (đủ để các đốt dùng)
        int maxLength = Mathf.CeilToInt((bodyCount + 1) * spacing / recordSpacing);
        if (pathHistory.Count > maxLength)
        {
            pathHistory.RemoveAt(pathHistory.Count - 1);
        }
    }

    void UpdateBodyParts()
    {
        for (int i = 0; i < bodyParts.Count; i++)
        {
            int historyIndex = Mathf.Min(Mathf.RoundToInt((i + 1) * spacing / recordSpacing), pathHistory.Count - 1);

            if (historyIndex < 0 || historyIndex >= pathHistory.Count)
                continue;

            Vector3 targetPos = pathHistory[historyIndex];
            GameObject part = bodyParts[i];

            part.transform.position = Vector3.Lerp(part.transform.position, targetPos, Time.deltaTime * followSpeed);

            Vector3 dir = targetPos - part.transform.position;
            if (dir.sqrMagnitude > 0.001f)
                part.transform.up = dir;
        }
    }

    IEnumerator SpawnBodyGradually()
    {
        for (int i = 0; i < bodyCount; i++)
        {
            Vector3 spawnPos;
            Quaternion spawnRot;

            if (bodyParts.Count == 0)
            {
                // Đốt đầu tiên -> spawn ngay sau đầu rắn
                spawnPos = transform.position - transform.up * spacing;
                spawnRot = transform.rotation;
            }
            else
            {
                // Các đốt sau -> spawn phía sau đốt trước đó
                Transform prev = bodyParts[bodyParts.Count - 1].transform;
                spawnPos = prev.position - prev.up * spacing;
                spawnRot = prev.rotation;
            }

            GameObject body = Instantiate(bodyPrefab, spawnPos, spawnRot);

            var seg = body.AddComponent<SnakeSegment>();
            seg.controller = this;
            seg.index = i;

            bodyParts.Add(body);

            yield return new WaitForSeconds(1f);
        }
    }




    float CalculateTotalPathLength(Transform[] points)
    {
        float dist = 0f;
        for (int i = 1; i < points.Length; i++)
        {
            dist += Vector3.Distance(points[i - 1].position, points[i].position);
        }
        return dist;
    }



    public void RemoveSegment(int index)
    {
        if (index < 0 || index >= bodyParts.Count)
            return;

        GameObject removed = bodyParts[index];
        removed.GetComponent<SnakeSegment>().RemoveSegment();
        bodyParts.RemoveAt(index);
        Destroy(removed);

        // Cập nhật lại index
        for (int i = 0; i < bodyParts.Count; i++)
        {
            bodyParts[i].GetComponent<SnakeSegment>().index = i;
        }

        // Co đuôi lại tự nhiên
        StartCoroutine(ShrinkSpacingTemporarily());
    }


    IEnumerator ShrinkSpacingTemporarily()
    {
        float originalSpacing = spacing;
        float shrinkAmount = originalSpacing * 0.6f; // co ngắn spacing lại tạm thời
        float duration = 0.3f;

        float timer = 0f;
        while (timer < duration)
        {
            spacing = Mathf.Lerp(shrinkAmount, originalSpacing, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }

        spacing = originalSpacing;
    }



}
