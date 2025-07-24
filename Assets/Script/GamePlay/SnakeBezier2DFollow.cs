using System.Collections.Generic;
using UnityEngine;

public class SnakeBezier2DFollow : MonoBehaviour
{
    [Header("Bezier Routes")]
    public Transform[] routes;

    [Header("Snake Settings")]
    public GameObject segmentPrefab;
    public float segmentSpacing = 0.3f;
    public float speed = 2f;

    [System.Serializable]
    public class ColorSetting
    {
        public string colorName;
        public int count;
    }

    [Header("Segment Colors")]
    public List<ColorSetting> colorSettings = new List<ColorSetting>()
    {
        new ColorSetting { colorName = "Green", count = 3 },
        new ColorSetting { colorName = "Red", count = 3 },
        new ColorSetting { colorName = "Yellow", count = 2 },
        new ColorSetting { colorName = "Orange", count = 2 }
    };

    private List<string> segmentColors = new List<string>();
    private List<Transform> segments = new List<Transform>();
    private List<BezierPoint> trail = new List<BezierPoint>();

    private int routeToGo = 0;
    private int segmentIndex = 0;
    private float tParam = 0f;

    private class BezierPoint
    {
        public Vector2 position;
        public int routeIndex;

        public BezierPoint(Vector2 pos, int idx)
        {
            position = pos;
            routeIndex = idx;
        }
    }

    void Start()
    {
        GenerateSegmentColors();

        // Thêm đầu rắn vào danh sách segments
        segments.Add(transform);

        foreach (string color in segmentColors)
        {
            GameObject seg = Instantiate(segmentPrefab, transform.position, Quaternion.identity);

            // Thêm CircleCollider2D nếu chưa có
            if (seg.GetComponent<CircleCollider2D>() == null)
            {
                seg.AddComponent<CircleCollider2D>();
            }

        

            // 🔄 Nếu muốn thay màu sprite:
            SpriteRenderer sr = seg.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = GetColorFromName(color);

            segments.Add(seg.transform);
        }

        trail.Add(new BezierPoint(transform.position, routeToGo));
    }

    void GenerateSegmentColors()
    {
        segmentColors.Clear();
        foreach (var setting in colorSettings)
        {
            segmentColors.AddRange(CreateColorList(setting.colorName, setting.count));
        }
    }

    List<string> CreateColorList(string color, int count)
    {
        List<string> list = new List<string>();
        for (int i = 0; i < count; i++) list.Add(color);
        return list;
    }

    // (Optional) Nếu bạn muốn dùng màu thay vì layer
    Color GetColorFromName(string color)
    {
        switch (color.ToLower())
        {
            case "green": return Color.green;
            case "red": return Color.red;
            case "yellow": return Color.yellow;
            case "orange": return new Color(1f, 0.5f, 0f); // cam
            default: return Color.white;
        }
    }

    void Update()
    {
        MoveHead();
        MoveBodySegments();
        TrimTrail();

        // Xử lý click chuột
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

            if (hit.collider != null)
            {
                Transform clickedSegment = hit.transform;
                int index = segments.IndexOf(clickedSegment);
                if (index != -1)
                {
                    RemoveSegment(index);
                }
            }
        }
    }

    void MoveBodySegments()
    {
        // Bắt đầu từ index 1 vì index 0 là đầu rắn
        for (int i = 1; i < segments.Count; i++)
        {
            float targetDist = segmentSpacing * i;
            float distSum = 0f;

            for (int j = 0; j < trail.Count - 1; j++)
            {
                float d = Vector2.Distance(trail[j].position, trail[j + 1].position);
                distSum += d;

                if (distSum >= targetDist)
                {
                    segments[i].position = trail[j + 1].position;
                    break;
                }
            }
        }
    }

    void RemoveSegment(int index)
    {
        // Không cho phép xóa đầu rắn
        if (index == 0) return;

        // --- Bước 1: Tìm vị trí mới cho đầu rắn bằng cách lùi lại trên trail ---
        float recoilDistance = segmentSpacing;
        float distanceSum = 0f;
        int trailIndexForNewHead = 0;

        for (int i = 0; i < trail.Count - 1; i++)
        {
            distanceSum += Vector2.Distance(trail[i].position, trail[i + 1].position);
            if (distanceSum >= recoilDistance)
            {
                trailIndexForNewHead = i + 1;
                break;
            }
        }

        // Nếu không đủ trail để lùi, chỉ xóa đốt và không làm gì thêm
        if (trailIndexForNewHead == 0)
        {
            Destroy(segments[index].gameObject);
            segments.RemoveAt(index);
            segmentColors.RemoveAt(index - 1);
            return;
        }

        Vector2 newHeadPos = trail[trailIndexForNewHead].position;

        // --- Bước 2: Tìm trạng thái (route, segment, t) gần nhất với vị trí mới ---
        float minDistance = float.MaxValue;
        int bestRoute = routeToGo;
        int bestSegment = segmentIndex;
        float bestT = tParam;

        for (int r = 0; r < routes.Length; r++)
        {
            Transform route = routes[r];
            int pointCount = route.childCount;
            if (pointCount < 4) continue;
            int segmentCountInRoute = (pointCount - 1) / 3;

            for (int s = 0; s < segmentCountInRoute; s++)
            {
                int p_idx = s * 3;
                Vector2 p0 = route.GetChild(p_idx).position;
                Vector2 p1 = route.GetChild(p_idx + 1).position;
                Vector2 p2 = route.GetChild(p_idx + 2).position;
                Vector2 p3 = route.GetChild(p_idx + 3).position;

                // Kiểm tra các điểm dọc theo đoạn curve này
                for (int k = 0; k <= 10; k++)
                {
                    float t = k / 10.0f;
                    Vector2 pointOnCurve = Mathf.Pow(1 - t, 3) * p0 +
                                          3 * Mathf.Pow(1 - t, 2) * t * p1 +
                                          3 * (1 - t) * Mathf.Pow(t, 2) * p2 +
                                          Mathf.Pow(t, 3) * p3;
                    float dist = Vector2.Distance(newHeadPos, pointOnCurve);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        bestRoute = r;
                        bestSegment = s;
                        bestT = t;
                    }
                }
            }
        }
        
        // --- Bước 3: Cập nhật trạng thái và dọn dẹp ---

        // Xóa đốt khỏi scene và các danh sách
        Destroy(segments[index].gameObject);
        segments.RemoveAt(index);
        segmentColors.RemoveAt(index - 1);

        // Cập nhật trạng thái của đầu rắn để nó tiếp tục di chuyển từ vị trí mới
        routeToGo = bestRoute;
        segmentIndex = bestSegment;
        tParam = bestT;
        transform.position = newHeadPos; // Di chuyển đầu rắn đến vị trí mới ngay lập tức

        // Xóa phần trail đã bị lùi qua
        trail.RemoveRange(0, trailIndexForNewHead);
    }


    float GetTrailLength()
    {
        float length = 0;
        for (int i = 0; i < trail.Count - 1; i++)
        {
            length += Vector2.Distance(trail[i].position, trail[i + 1].position);
        }
        return length;
    }

    void MoveHead()
    {
        if (routes.Length == 0) return;

        while (true)
        {
            Transform route = routes[routeToGo];
            int pointCount = route.childCount;
            if (pointCount < 4) return;

            int segmentCountInRoute = (pointCount - 1) / 3;

            if (segmentIndex >= segmentCountInRoute)
            {
                routeToGo++;
                if (routeToGo >= routes.Length) routeToGo = 0;
                segmentIndex = 0;
                tParam = 0f;
                continue;
            }

            int i = segmentIndex * 3;
            if (i + 3 >= pointCount) return;

            Vector2 p0 = route.GetChild(i).position;
            Vector2 p1 = route.GetChild(i + 1).position;
            Vector2 p2 = route.GetChild(i + 2).position;
            Vector2 p3 = route.GetChild(i + 3).position;

            // Tính độ dài đường cong hiện tại
            float curveLength = GetBezierLength(p0, p1, p2, p3);

            // Điều chỉnh tParam dựa trên khoảng cách thực tế
            float stepSize = (speed * Time.deltaTime) / curveLength;
            tParam += stepSize;

            Vector2 newPos = Mathf.Pow(1 - tParam, 3) * p0 +
                           3 * Mathf.Pow(1 - tParam, 2) * tParam * p1 +
                           3 * (1 - tParam) * Mathf.Pow(tParam, 2) * p2 +
                           Mathf.Pow(tParam, 3) * p3;

            transform.position = newPos;

            if (trail.Count == 0 || Vector2.Distance(newPos, trail[0].position) > 0.01f)
            {
                trail.Insert(0, new BezierPoint(newPos, routeToGo));
            }

            if (tParam >= 1f)
            {
                tParam = 0f;
                segmentIndex++;
            }

            break;
        }
    }

    void TrimTrail()
    {
        float maxLength = segmentSpacing * (segments.Count + 2);
        float distSum = 0f;
        int i = 0;

        for (; i < trail.Count - 1; i++)
        {
            float d = Vector2.Distance(trail[i].position, trail[i + 1].position);
            distSum += d;
            if (distSum > maxLength + 1f) break;
        }

        if (i < trail.Count - 1)
        {
            trail.RemoveRange(i + 1, trail.Count - (i + 1));
        }
    }

    float GetBezierLength(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, int segments = 20)
    {
        float length = 0;
        Vector2 prevPoint = p0;

        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector2 point = Mathf.Pow(1 - t, 3) * p0 +
                           3 * Mathf.Pow(1 - t, 2) * t * p1 +
                           3 * (1 - t) * Mathf.Pow(t, 2) * p2 +
                           Mathf.Pow(t, 3) * p3;

            length += Vector2.Distance(prevPoint, point);
            prevPoint = point;
        }
        return length;
    }
}