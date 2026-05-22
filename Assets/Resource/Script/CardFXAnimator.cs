using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CardFXAnimator : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private Image flyCardPrefab; // FlyCardPrefab（Image）

    [Header("Move")]
    [SerializeField] private float moveDuration = 0.25f;
    [SerializeField] private float popScale = 1.08f;

    [Header("Flip")]
    [SerializeField] private float flipHalfDuration = 0.10f;

    private void Awake()
    {
        if (rootCanvas == null)
        {
            rootCanvas = FindObjectOfType<Canvas>();
        }
    }

    public IEnumerator FlyCard(RectTransform fromRect, RectTransform toRect, Sprite sprite, bool doFlip)
    {
        if (rootCanvas == null || flyCardPrefab == null || fromRect == null || toRect == null)
        {
            Debug.LogError("[CardFXAnimator] Missing references.");
            yield break;
        }

        // 生成（Canvas直下）
        Image fx = Instantiate(flyCardPrefab, rootCanvas.transform);
        RectTransform fxRect = fx.rectTransform;

        fx.sprite = sprite;
        fx.preserveAspect = true;

        // 開始位置
        fxRect.position = fromRect.position;
        fxRect.localScale = Vector3.one;

        // ちょいポップしてから移動
        yield return PopAndMove(fxRect, toRect.position, moveDuration, popScale);

        // 到着後フリップ（任意）
        if (doFlip)
        {
            yield return FlipOnce(fxRect, flipHalfDuration);
        }

        Destroy(fx.gameObject);
    }

    private IEnumerator PopAndMove(RectTransform rect, Vector3 targetWorldPos, float duration, float scaleUp)
    {
        float half = Mathf.Max(0.01f, duration * 0.5f);

        Vector3 startPos = rect.position;
        Vector3 endPos = targetWorldPos;

        // 中間地点（少し浮かす）
        Vector3 midPos = Vector3.Lerp(startPos, endPos, 0.35f) + new Vector3(0f, 40f, 0f);

        // 前半：ポップ＋中間へ
        float t = 0f;
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float k = Smooth01(t / half);

            rect.position = Vector3.Lerp(startPos, midPos, k);
            rect.localScale = Vector3.Lerp(Vector3.one, Vector3.one * scaleUp, k);
            yield return null;
        }

        // 後半：目的地へ＋スケール戻す
        t = 0f;
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float k = Smooth01(t / half);

            rect.position = Vector3.Lerp(midPos, endPos, k);
            rect.localScale = Vector3.Lerp(Vector3.one * scaleUp, Vector3.one, k);
            yield return null;
        }

        rect.position = endPos;
        rect.localScale = Vector3.one;
    }

    private IEnumerator FlipOnce(RectTransform rect, float halfDuration)
    {
        // 横につぶす→戻す（画像差し替え無しの“フリップ感”）
        yield return ScaleX(rect, 1f, 0f, halfDuration);
        yield return ScaleX(rect, 0f, 1f, halfDuration);
    }

    private IEnumerator ScaleX(RectTransform rect, float from, float to, float duration)
    {
        float t = 0f;
        Vector3 s = rect.localScale;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Smooth01(t / duration);
            float x = Mathf.Lerp(from, to, k);
            rect.localScale = new Vector3(x, s.y, s.z);
            yield return null;
        }

        rect.localScale = new Vector3(to, s.y, s.z);
    }

    private float Smooth01(float x)
    {
        x = Mathf.Clamp01(x);
        return x * x * (3f - 2f * x);
    }
}
