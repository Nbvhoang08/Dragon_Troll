using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class PathMovement : MonoBehaviour
{
    [Header("Các điểm tạo quỹ đạo (ít nhất 2 điểm)")]
    public Transform[] pathPoints;

    [Header("Cài đặt chuyển động")]
    public float duration = 3f;
    public PathType pathType = PathType.CatmullRom;
    public bool loop = true;

    [Header("Cài đặt con rắn")]
    public GameObject bodyPrefab; // Prefab cho đốt của rắn
    public int bodyCount = 5; // Số lượng đốt
    public float spacing = 0.5f; // Khoảng cách giữa các đốt

    private Tween moveTween;
    private List<GameObject> bodyParts = new List<GameObject>();
    private List<Vector3> positionHistory = new List<Vector3>();

    void Start()
    {
        if (pathPoints.Length < 2)
        {
            Debug.LogWarning("Cần ít nhất 2 điểm để tạo quỹ đạo.");
            return;
        }

        // Tạo mảng path từ pathPoints với z = 11f
        Vector3[] path = new Vector3[pathPoints.Length];
        for (int i = 0; i < pathPoints.Length; i++)
        {
            Vector3 pos = pathPoints[i].position;
            pos.z = 11f;
            path[i] = pos;
        }

        // ⚠️ Đặt vị trí đầu rắn = điểm bắt đầu của path
        transform.position = path[0];

        // Tạo các đốt rắn sau khi đầu đã ở đúng vị trí
        CreateSnakeBody();

        // Di chuyển theo path
        moveTween = transform.DOPath(path, duration, pathType, PathMode.TopDown2D)
                             .SetEase(Ease.Linear)
                             .SetLookAt(0.01f)
                             .OnUpdate(UpdateBodyParts);

        if (loop)
        {
            moveTween.SetLoops(-1, LoopType.Restart);
        }
    }

    void CreateSnakeBody()
    {
        for (int i = 0; i < bodyCount; i++)
        {
            Vector3 spawnPos = transform.position;
            spawnPos.z = 11f;

            GameObject body = Instantiate(bodyPrefab, spawnPos, Quaternion.identity);
            bodyParts.Add(body);
            positionHistory.Add(spawnPos);
        }
    }

    void UpdateBodyParts()
    {
        // Cập nhật vị trí đầu vào lịch sử (với z = 11f)
        Vector3 headPos = transform.position;
        headPos.z = 11f;
        positionHistory.Insert(0, headPos);

        for (int i = 0; i < bodyParts.Count; i++)
        {
            int index = Mathf.Min(i * Mathf.RoundToInt(spacing * 60), positionHistory.Count - 1);
            Vector3 targetPosition = positionHistory[index];
            targetPosition.z = 11f;

            bodyParts[i].transform.position = targetPosition;

            // Xoay đốt nhìn theo hướng di chuyển
            if (i < positionHistory.Count - 1)
            {
                Vector3 direction = positionHistory[index] - bodyParts[i].transform.position;
                if (direction != Vector3.zero)
                {
                    bodyParts[i].transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
                }
            }
        }

        // Giới hạn lịch sử
        if (positionHistory.Count > bodyCount * spacing * 60)
        {
            positionHistory.RemoveAt(positionHistory.Count - 1);
        }
    }

    void Update()
    {
        // Nhấn Space để tạm dừng / tiếp tục
        if (Input.GetKeyDown(KeyCode.Space) && moveTween != null)
        {
            if (moveTween.IsPlaying()) moveTween.Pause();
            else moveTween.Play();
        }
    }
}
