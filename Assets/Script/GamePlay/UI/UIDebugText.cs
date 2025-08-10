using UnityEngine;
using TMPro;
using DG.Tweening;

public class UIDebugText : MonoBehaviour
{
    private static TextMeshProUGUI debugText;
    private static GameObject debugTextObj;
    private static RectTransform rectTransform;
    private static Vector2 startPosition;

    private void Awake()
    {
        debugText = GetComponent<TextMeshProUGUI>();
        debugTextObj = gameObject;
        rectTransform = GetComponent<RectTransform>();
        startPosition = rectTransform.anchoredPosition;
        gameObject.SetActive(false);
    }

    public static void ShowMessage(string message, float duration = 3f)
    {
        if (debugText == null) return;

        // Reset position and make visible
        debugTextObj.SetActive(true);
        rectTransform.anchoredPosition = startPosition;
        debugText.text = message;
        debugText.alpha = 0f;

        // Kill any existing animations
        DOTween.Kill(debugText);
        DOTween.Kill(rectTransform);

        Sequence sequence = DOTween.Sequence();

        // Fade in while scaling up
        sequence.Append(debugText.DOFade(1f, 0.3f))
                .Join(rectTransform.DOScale(1.1f, 0.3f))
                .Join(rectTransform.DOAnchorPosY(startPosition.y + 50f, 0.3f));

        // Scale back to normal
        sequence.Append(rectTransform.DOScale(1f, 0.2f));

        // Float up slowly while staying visible
        sequence.Append(rectTransform.DOAnchorPosY(startPosition.y + 100f, duration - 0.5f));

        // Fade out at the end
        sequence.Append(debugText.DOFade(0f, 0.5f))
                .Join(rectTransform.DOScale(0.8f, 0.5f));

        // Hide object when done
        sequence.OnComplete(() =>
        {
            debugTextObj.SetActive(false);
            rectTransform.anchoredPosition = startPosition;
            rectTransform.localScale = Vector3.one;
        });
    }
}
