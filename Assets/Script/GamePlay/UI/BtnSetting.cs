using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BtnSetting : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] Transform model;
    [SerializeField] TypeSetting typeSetting;
    [SerializeField] float maxX = 46.8f;
    [SerializeField] float minX = 46.8f;
    [SerializeField] Image BG ;
    [SerializeField] Sprite spriteOn;
    [SerializeField] Sprite spriteOff;
    Button button;
    public void Start()
    {
        
        button = GetComponent<Button>();
        if (button == null)
        {
            gameObject.AddComponent<Button>();
            button = GetComponent<Button>();
        }
        button.onClick.AddListener(OnClick);
        bool value = GameManager.Instance.GetStateSetting(typeSetting);

        Vector3 setupPosition = model.localPosition;
        if (value)
            setupPosition.x = maxX;
        else
            setupPosition.x = minX;
        model.localPosition = setupPosition;
        BG.sprite = value ? spriteOn : spriteOff;
    }
    void OnClick()
    {
        float duration = 0.05f;
        bool newValue = GameManager.Instance.ChangeSetting(typeSetting);
        if (newValue)
            model.DOLocalMoveX(maxX, duration);
        else
            model.DOLocalMoveX(minX, duration);
        BG.sprite = newValue ? spriteOn : spriteOff;
    }
}
public enum TypeSetting
{
    Music = 0,
    Sound,
    Haptic
}