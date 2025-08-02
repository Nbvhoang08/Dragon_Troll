using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;

public class  UITransition : UICanvas
{
    [SerializeField] private Image targetImage;
    private Material _mat;

    void Awake()
    {
        _mat = Instantiate(targetImage.material);
        targetImage.material = _mat;

        // Tính aspect giữ vòng tròn
        var r = targetImage.rectTransform.rect;
        float aspect = r.width / r.height;
        _mat.SetVector("_Aspect", new Vector4(aspect, 1, 0, 0));
    }
    public void PlayRevealIn(System.Action callback = null)
    {
        DOTween.To(() => _mat.GetFloat("_Radius"),
                   x => _mat.SetFloat("_Radius", x),
                   0.7f, 2f)
            .SetEase(Ease.Linear)
            .OnComplete(() => callback?.Invoke());
    }
    public void SetRadius(float x) 
    {
        _mat.SetFloat("_Radius", x);
    }
    public void PlayRevealOut(System.Action callback = null)
    {
        DOTween.To(() => _mat.GetFloat("_Radius"),
                   x => _mat.SetFloat("_Radius", x),
                   0f, 2f)
            .SetEase(Ease.Linear)
            .OnComplete(() => callback?.Invoke());

    }
}
