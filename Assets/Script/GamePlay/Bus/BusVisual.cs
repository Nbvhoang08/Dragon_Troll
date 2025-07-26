using UnityEngine;
using DG.Tweening;


#if UNITY_EDITOR
using UnityEditor;
#endif


[ExecuteAlways]
public class BusVisual : MonoBehaviour
{
   
    public BusColor busColor;
    public BusType busType;
    public BusVisualData visualData;

    [HideInInspector][SerializeField] private float _lastAngle = -999f;

    public SpriteRenderer BottomSprite;
    public SpriteRenderer TopSprite;


    public void VisualConfig()
    {
        if (visualData == null) return;

        float angle = transform.eulerAngles.z;
        BusDirection dir = GetBusDirection(angle);
        
        var spriteData = visualData.GetSprites(busType, busColor, dir);
        if (spriteData != null)
        {
            if (TopSprite)
            {
                TopSprite.sprite = spriteData.topSprite;
                if (dir == BusDirection.UpLeft || dir == BusDirection.DownRight || dir == BusDirection.Left)
                {
                    TopSprite.flipX = true;
                }
                else 
                {
                    TopSprite.flipX = false;
                }
                if (BottomSprite)
                {
                    BottomSprite.sprite = spriteData.bottomSprite;
                    BottomSprite.transform.localRotation = Quaternion.Euler(0f, 0f, -transform.eulerAngles.z);

                }

            }
        }
    }
    public void AnimationFadeOut(System.Action onComplete = null)
    {
        Sequence seq = DOTween.Sequence();

        Vector3 localOffset = new Vector3(0.7f, 0f, 0f);
        Vector3 targetLocalPos = TopSprite.transform.localPosition + localOffset;

        seq.Append(TopSprite.transform.DOLocalMove(targetLocalPos, 0.3f).SetEase(Ease.Linear));

        seq.AppendCallback(() =>
        {
            TopSprite.DOFade(0f, 0.25f).SetEase(Ease.OutQuad);
            BottomSprite.DOFade(0f, 0.25f).SetEase(Ease.OutQuad);
        });

        seq.AppendInterval(0.2f);
        seq.OnComplete(() => onComplete?.Invoke());
    }


    private BusDirection GetBusDirection(float angle)
    {
        angle %= 360f;
        if (angle < 0f) angle += 360f;

        if ((angle >= 337.5f && angle < 360f) || (angle >= 0f && angle < 22.5f)) return BusDirection.Up;
        if (angle >= 22.5f && angle < 67.5f) return BusDirection.UpRight;
        if (angle >= 67.5f && angle < 112.5f) return BusDirection.Right;
        if (angle >= 112.5f && angle < 157.5f) return BusDirection.DownRight;
        if (angle >= 157.5f && angle < 202.5f) return BusDirection.Down;
        if (angle >= 202.5f && angle < 247.5f) return BusDirection.DownLeft;
        if (angle >= 247.5f && angle < 292.5f) return BusDirection.Left;
        return BusDirection.UpLeft;

    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        EditorApplication.delayCall += () =>
        {
            if (this == null || visualData == null)
                return;

            float currentAngle = transform.eulerAngles.z;

            if (Mathf.Abs(currentAngle - _lastAngle) > 0.1f || _lastAngle == -999f)
            {
                _lastAngle = currentAngle;
                VisualConfig();

                if (!Application.isPlaying)
                    EditorUtility.SetDirty(this);
            }
            else 
            {
                VisualConfig();
            }

        };
    }
#endif
}



