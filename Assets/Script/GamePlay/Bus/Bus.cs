using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using  DG.Tweening;
using System.Linq;
public class Bus : MonoBehaviour
{
    [Header("State Setting")]

    private bool isMoving = false;
    public float shakeDuration = 0.2f;
    public float shakeStrength = 0.2f;
    private bool isShaking = false;
    [Header("Path Points")]
    public Vector2 intersection; 
    public Slot targetSlot;

    [Header("Move Settings")]
    public float raycastDistance = 20f;
    public float moveSpeed = 5f;
    [SerializeField]private List<Vector3> _finalPath;
    private int _currentIndex = 0;
    private Vector3 startPosition;
    private Collider2D _collider;
    private BusVisual _busVisual;   

    void Start()
    {
        _collider = GetComponent<Collider2D>();
        startPosition = transform.position;
        _busVisual = GetComponent<BusVisual>();
    }

    private void OnMouseDown()
    {
        if (isMoving) return;
        targetSlot = GameManager.Instance.ValidSlot();
        if (targetSlot == null)
        {
            Debug.LogWarning("Không tìm thấy slot hợp lệ!");
            return;
        }
        SoundManager.Instance.Play(Constants.BoxClickSound);
        targetSlot.SetOccupied(true);
        Vector2 origin = transform.position;
        Vector2 direction = transform.up.normalized;
        Vector2 end = origin + direction * raycastDistance;

        RaycastHit2D hit = Physics2D.Linecast(origin, end, LayerMask.GetMask("Road"));

        if (hit.collider != null)
        {
            Bounds bounds = hit.collider.bounds;
            intersection = GetIntersectionWithCenterLine(origin, direction, bounds);
            MoveToPoint(intersection);
            Debug.DrawLine(origin, intersection, Color.green, 1f);
        }
        else
        {
            Debug.DrawLine(origin, end, Color.red, 1f);
        }
    }

    private void MoveToPoint(Vector2 target)
    {
        isMoving = true;
        Vector3 target3D = new Vector3(target.x, target.y, transform.position.z);
        float distance = Vector3.Distance(transform.position, target3D);
        float duration = distance / moveSpeed;
        transform.DOMove(target3D, duration)
            .SetEase(Ease.Linear)
            .OnComplete(
            () => startMovePath());
    }

    private Vector2 GetIntersectionWithCenterLine(Vector2 rayOrigin, Vector2 rayDir, Bounds bounds)
    {
        Vector2 center = bounds.center;
        Vector2 size = bounds.size;
        Vector2 centerLineStart, centerLineEnd;
        if (size.x >= size.y)
        {
            centerLineStart = new Vector2(center.x - 100f, center.y);
            centerLineEnd = new Vector2(center.x + 100f, center.y);
        }
        else
        {
            centerLineStart = new Vector2(center.x, center.y - 100f);
            centerLineEnd = new Vector2(center.x, center.y + 100f);
        }

        Vector2 rayEnd = rayOrigin + rayDir * raycastDistance;
        Vector2? intersection = LineIntersection(rayOrigin, rayEnd, centerLineStart, centerLineEnd);
        if (intersection.HasValue)
            return intersection.Value;

        Debug.LogWarning("Không tìm được giao điểm với center line!");
        return rayOrigin; 
    }

    private Vector2? LineIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        float A1 = p2.y - p1.y;
        float B1 = p1.x - p2.x;
        float C1 = A1 * p1.x + B1 * p1.y;

        float A2 = p4.y - p3.y;
        float B2 = p3.x - p4.x;
        float C2 = A2 * p3.x + B2 * p3.y;

        float denominator = A1 * B2 - A2 * B1;

        if (Mathf.Approximately(denominator, 0f))
            return null; 

        float x = (B2 * C1 - B1 * C2) / denominator;
        float y = (A1 * C2 - A2 * C1) / denominator;

        return new Vector2(x, y);
    }

    void startMovePath() 
    {
        _finalPath = CalculateShortestPath();
        if (_finalPath == null || _finalPath.Count == 0)
        { 
            isMoving = false;
            DOTween.Kill(transform);
            return;
        }
        _collider.enabled = false; // Vô hiệu hóa collider trong quá trình di chuyển
   
        MoveAlongPath();
    }


    List<Vector3> CalculateShortestPath()
    {
        var allPaths = new List<List<Vector3>>();
        Vector3 start = intersection;
        Vector3 endLower = targetSlot.lowerPoint;

        Vector3 end = targetSlot.transform.position;
        allPaths.Add(new List<Vector3> { start, endLower, end });
        foreach (Transform cp in GameManager.Instance.checkpoints)
        {
            Vector3 cpWorld = ToWorldPosition(cp);
            allPaths.Add(new List<Vector3> { start, cpWorld , endLower, end });
        }

        for (int i = 0; i < GameManager.Instance.checkpoints.Count; i++)
        {
            for (int j = i + 1; j < GameManager.Instance.checkpoints.Count; j++)
            {
                allPaths.Add(new List<Vector3> {
                start,
                GameManager.Instance.checkpoints[i].position,
                GameManager.Instance.checkpoints[j].position,
                endLower,
                end
            });
            }
        }
        var validPaths = allPaths.Where(path =>
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                if (!IsAxisAligned(path[i], path[i + 1])) 
                    return false;
                
            }
            return true;
        }).ToList();

    
        float minDistance = float.MaxValue;
        List<Vector3> shortest = null;

        foreach (var path in validPaths)
        {
            float dist = 0f;
            for (int i = 0; i < path.Count - 1; i++)
                dist += Vector3.Distance(path[i], path[i + 1]);

            if (dist < minDistance)
            {
                minDistance = dist;
                shortest = path;
            }
        }

        return shortest;
    }
    bool IsAxisAligned(Vector3 a, Vector3 b)
    {
        return Mathf.Approximately(a.x, b.x) || Mathf.Approximately(a.y, b.y);
    }

    Vector3 ToWorldPosition(Transform t)
    {
        if (t.parent == null) return t.position;
        return t.parent.TransformPoint(t.localPosition);
    }
    void MoveAlongPath()
    {
        if (_finalPath == null || _finalPath.Count == 0) return;
        isMoving = true;
        _currentIndex = 0;
        MoveToNextPoint();
    }

    void MoveToNextPoint()
    {
        if (_currentIndex >= _finalPath.Count - 1)
        {
            isMoving = false;
            _busVisual.AnimationFadeOut(() =>
            {
                targetSlot.OnOccupied(_busVisual.busColor, _busVisual.busType);
                transform.position = startPosition; // Trả về vị trí ban đầu
            });
            
            return;
        }

        Vector3 from = _finalPath[_currentIndex];
        Vector3 to = _finalPath[_currentIndex + 1];
        float distance = Vector3.Distance(from, to);
        float duration = distance / moveSpeed;

        Vector3 direction = (to - from).normalized;
        Quaternion targetRot = Quaternion.FromToRotation(Vector3.up, direction); // từ up -> direction

        transform.DORotateQuaternion(targetRot, 0.01f).OnComplete(()=>_busVisual.VisualConfig());

        transform.DOMove(to, duration).SetEase(Ease.Linear).OnComplete(() =>
        {
            _currentIndex++;
            MoveToNextPoint();
        });
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
       
        if (isShaking) return;
        DOTween.Kill(transform);
        
        if (!isMoving) 
        {
            isShaking = true;
            transform.DOShakePosition(shakeDuration, shakeStrength)
            .OnComplete(() =>
            {
                isShaking = false;
               
            });
        }
        else 
        {
            targetSlot?.SetOccupied(false); 
            targetSlot = null;
            if (isMoving)
            {
                Vector2 contactPoint = collision.GetContact(0).point;
                SoundManager.Instance.Play(Constants.BoxCrashSound);
                Effect collisionEffect = Pool.Instance.collisionEffect;
                collisionEffect.transform.position = contactPoint;

                transform.DOMove(startPosition, 0.3f)
                    .SetEase(Ease.InOutQuad)
                    .OnComplete(() => isMoving = false);
            }
        }
    }
}
