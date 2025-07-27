// SnakeSegment.cs (Không đổi)
using UnityEngine;
using DG.Tweening;

public class SnakeSegment : MonoBehaviour
{
    [Header("Segment Settings")]
    public int segmentIndex;
    public SegmentType segmentType = SegmentType.Red;
    public float segmentSpacing = 1f;

    [Header("Visual Settings")]
    public SpriteRenderer spriteRenderer;
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

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = segmentType.GetSprite();
            spriteRenderer.color = Color.white;
        }

        if (GetComponent<Collider2D>() == null)
        {
            CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
            collider.radius = 0.5f;
        }
    }

    void OnMouseEnter()
    {
        if (!isDestroyed && segmentType.IsDestructible() && spriteRenderer != null)
        {
            spriteRenderer.color = hoverColor;
        }
    }

    void OnMouseExit()
    {
        if (!isDestroyed && segmentType.IsDestructible() && spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }

    void OnMouseDown()
    {
        if (!isDestroyed && segmentType.IsDestructible())
        {
            DestroySegment();
        }
    }

    public void DestroySegment()
    {
        if (isDestroyed || !segmentType.IsDestructible()) return;
        isDestroyed = true;

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

        if (spriteRenderer != null)
        {
    // spriteRenderer.DOColor(Color.red, destroyAnimationDuration * 0.5f);
        }
    }

    public void SetSegmentIndex(int index)
    {
        segmentIndex = index;
    }

    public void SetSegmentType(SegmentType type)
    {
        segmentType = type;
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = segmentType.GetSprite();
            spriteRenderer.color = Color.white;
        }
    }

    public SegmentType GetSegmentType() => segmentType;
    public bool IsHead() => segmentType == SegmentType.Head;
    public bool IsDestroyed() => isDestroyed;

    public void UpdatePosition(Vector3 newPosition, float duration = 0.1f)
    {
        if (!isDestroyed)
        {
            transform.DOMove(newPosition, duration);
        }
    }

    public void UpdateRotation(Vector3 newEulerAngles)
    {
        if (!isDestroyed)
        {
            transform.rotation = Quaternion.Euler(newEulerAngles);
        }
    }

    public void UpdateSortingOrder(int order)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = order;
        }
    }
}