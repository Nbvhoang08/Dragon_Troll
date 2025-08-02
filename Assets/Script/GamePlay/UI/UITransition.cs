using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class NewBehaviourScript : MonoBehaviour
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

    public void PlayReveal()
    {
        _mat.SetFloat("_Radius", 0f);
        DOTween.To(() => _mat.GetFloat("_Radius"),
                   x => _mat.SetFloat("_Radius", x),
                   1.5f, 1f);
    }
}
