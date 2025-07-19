using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using  DG.Tweening;
public class Bus : MonoBehaviour
{
    public float raycastDistance = 20f;
    public float moveSpeed = 5f;

    private bool isMoving = false;
    public float shakeDuration = 0.2f;
    public float shakeStrength = 0.2f;
    public float duration = 0.5f; // Thời gian di chuyển
    private bool isShaking = false;
    private Vector3 startPosition;
    void Start()
    {
        startPosition = transform.position;
        GameEvents.GameStart?.Invoke();
    }
    private void OnEnable()
    {
        GameEvents.GameStart += OnGameStart;
    }
    private void OnDisable()
    {
        GameEvents.GameStart -= OnGameStart;
    }

    public void OnGameStart()
    {
        Debug.Log("Nhankun.");
    }




    private void OnMouseDown()
    {
        if (isMoving) return;

        Vector2 origin = transform.position;
        Vector2 direction = transform.up.normalized;
        Vector2 end = origin + direction * raycastDistance;

        RaycastHit2D hit = Physics2D.Linecast(origin, end, LayerMask.GetMask("Road"));

        if (hit.collider != null)
        {
            Bounds bounds = hit.collider.bounds;
            Vector2 intersection = GetIntersectionWithCenterLine(origin, direction, bounds);

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

        transform.DOMove(target3D, duration)
            .SetEase(Ease.Linear)
            .OnComplete(() => isMoving = false);
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
            return null; // Song song

        float x = (B2 * C1 - B1 * C2) / denominator;
        float y = (A1 * C2 - A2 * C1) / denominator;

        return new Vector2(x, y);
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
            if (isMoving)
            {
                transform.DOMove(startPosition, 0.3f)
                    .SetEase(Ease.InOutQuad)
                    .OnComplete(() => isMoving = false);
            }
        }
    }
}
