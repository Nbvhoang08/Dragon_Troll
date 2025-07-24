using UnityEngine;
using DG.Tweening;

public class SnakeSegment : MonoBehaviour
{
    [Header("Segment Settings")]
    public int segmentIndex;
    public bool isHead = false;
    public float segmentSpacing = 1f;

    [Header("Visual Settings")]
    public SpriteRenderer spriteRenderer;
    public Color normalColor = Color.green;
    public Color headColor = Color.red;
    public Color hoverColor = Color.yellow;

    [Header("Animation Settings")]
    public float destroyAnimationDuration = 0.3f;
    public Ease destroyEase = Ease.OutBack;

    private Snake parentSnake;
    private bool isDestroyed = false;
    private Vector3 originalScale;

    void Start()
    {
        parentSnake = GetComponentInParent<Snake>();
        originalScale = transform.localScale;

        // Thiết lập màu sắc
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isHead ? headColor : normalColor;
        }

        // Thêm Collider2D để có thể click
        if (GetComponent<Collider2D>() == null)
        {
            CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
            collider.radius = 0.5f;
        }
    }

    void OnMouseEnter()
    {
        if (!isDestroyed && !isHead && spriteRenderer != null)
        {
            spriteRenderer.color = hoverColor;
        }
    }

    void OnMouseExit()
    {
        if (!isDestroyed && !isHead && spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }
    }

    void OnMouseDown()
    {
        if (!isDestroyed && !isHead)
        {
            DestroySegment();
        }
    }

    public void DestroySegment()
    {
        if (isDestroyed) return;

        isDestroyed = true;

        // Animation phá hủy
        transform.DOScale(Vector3.zero, destroyAnimationDuration)
            .SetEase(destroyEase)
            .OnComplete(() =>
            {
                if (parentSnake != null)
                {
                    parentSnake.OnSegmentDestroyed(segmentIndex);
                }
                Destroy(gameObject);
            });

        // Hiệu ứng màu sắc
        if (spriteRenderer != null)
        {
            spriteRenderer.DOColor(Color.red, destroyAnimationDuration * 0.5f);
        }
    }

    public void SetSegmentIndex(int index)
    {
        segmentIndex = index;
    }

    public void SetAsHead(bool head)
    {
        isHead = head;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isHead ? headColor : normalColor;
        }
    }

    public bool IsDestroyed()
    {
        return isDestroyed;
    }

    public void UpdatePosition(Vector3 newPosition, float duration = 0.1f)
    {
        if (!isDestroyed)
        {
            transform.DOMove(newPosition, duration);
        }
    }
}