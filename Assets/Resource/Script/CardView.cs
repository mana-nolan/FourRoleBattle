using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// カード種類
public enum CardType
{
    King = 0,
    Soldier = 1,
    Commoner = 2,
    Assassin = 3
}

// 手札カード（Button）1枚を管理：表/裏画像の切替とフリップ演出
public class CardView : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Image cardImage; // 表示に使うImage（通常はこのButton自身のImage）

    [Header("Sprites (UI)")]
    [SerializeField] private Sprite backSprite;
    [SerializeField] private Sprite kingSprite;
    [SerializeField] private Sprite soldierSprite;
    [SerializeField] private Sprite commonerSprite;
    [SerializeField] private Sprite assassinSprite;

    [Header("Flip Settings")]
    [SerializeField] private float flipHalfDuration = 0.12f; // 片側の時間（0.1?0.15推奨）

    private CardType cardType = CardType.King;

    private void Reset()
    {
        // 同じオブジェクトにImageがある想定
        cardImage = GetComponent<Image>();
    }

    private void Awake()
    {
        if (cardImage == null)
        {
            cardImage = GetComponent<Image>();
        }
    }

    // カード種類を設定
    public void SetCardType(CardType type)
    {
        cardType = type;
    }

    // 裏面を表示
    public void ShowBack()
    {
        if (cardImage == null) return;
        cardImage.sprite = backSprite;
        // 念のため：スケールを戻す
        var s = transform.localScale;
        transform.localScale = new Vector3(1f, s.y, s.z);
    }

    // 表面を表示（演出なし）
    public void ShowFrontImmediate()
    {
        if (cardImage == null) return;
        cardImage.sprite = GetFrontSprite(cardType);
        var s = transform.localScale;
        transform.localScale = new Vector3(1f, s.y, s.z);
    }

    // 裏→表にフリップ
    public void FlipToFront()
    {
        StopAllCoroutines();
        StartCoroutine(FlipRoutine(toFront: true));
    }

    // 表→裏にフリップ（必要なら）
    public void FlipToBack()
    {
        StopAllCoroutines();
        StartCoroutine(FlipRoutine(toFront: false));
    }

    private IEnumerator FlipRoutine(bool toFront)
    {
        // 1) 1 → 0（横につぶす）
        yield return ScaleX(1f, 0f, flipHalfDuration);

        // 2) つぶれてる瞬間に画像を差し替える
        if (cardImage != null)
        {
            cardImage.sprite = toFront ? GetFrontSprite(cardType) : backSprite;
        }

        // 3) 0 → 1
        yield return ScaleX(0f, 1f, flipHalfDuration);
    }

    private IEnumerator ScaleX(float from, float to, float duration)
    {
        float t = 0f;
        Vector3 s = transform.localScale;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            float x = Mathf.Lerp(from, to, Smooth01(k));
            transform.localScale = new Vector3(x, s.y, s.z);
            yield return null;
        }

        transform.localScale = new Vector3(to, s.y, s.z);
    }

    private float Smooth01(float x)
    {
        // SmoothStep
        x = Mathf.Clamp01(x);
        return x * x * (3f - 2f * x);
    }

    private Sprite GetFrontSprite(CardType type)
    {
        switch (type)
        {
            case CardType.King: return kingSprite;
            case CardType.Soldier: return soldierSprite;
            case CardType.Commoner: return commonerSprite;
            case CardType.Assassin: return assassinSprite;
            default: return kingSprite;
        }
    }
}
