using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardDealAnimator : MonoBehaviour
{
    // ----------------------------
    // Events / Public API (互換)
    // ----------------------------
    public event Action OnDealFinished;

    public IReadOnlyList<Button> PlayerButtons => playerButtons;
    public IReadOnlyList<Button> EnemyButtons => enemyButtons;

    // ----------------------------
    // Inspector
    // ----------------------------
    [Header("Refs")]
    [SerializeField] private RectTransform deckRect;
    [SerializeField] private RectTransform playerHandAreaRect;
    [SerializeField] private RectTransform enemyHandAreaRect;
    [SerializeField] private Image dealBackPrefab;

    [Header("Card Prefabs")]
    [Tooltip("旧フィールド互換：プレイヤー用Prefab（これまでここに入れていたならそのままでOK）")]
    [SerializeField] private Button cardButtonPrefab; // = Player用(互換)

    [Tooltip("プレイヤー手札に生成するカードPrefab（未設定なら Card Button Prefab を使う）")]
    [SerializeField] private Button playerCardButtonPrefab;

    [Tooltip("敵手札に生成するカードPrefab（Hover無し推奨）")]
    [SerializeField] private Button enemyCardButtonPrefab;

    [Header("Deal")]
    [SerializeField] private int dealCount = 5;
    [SerializeField] private float moveDuration = 0.25f;
    [SerializeField] private float interval = 0.06f;

    [Header("Flip (Player Only)")]
    [SerializeField] private bool flipPlayerCards = true;
    [SerializeField] private float flipDelayAfterArrive = 0.05f;

    [Header("Order")]
    [SerializeField] private bool dealPlayerFirst = true;

    // ----------------------------
    // Internal
    // ----------------------------
    private Canvas rootCanvas;

    private readonly List<Button> playerButtons = new List<Button>();
    private readonly List<Button> enemyButtons = new List<Button>();

    private List<CardType> queuedPlayerHand;
    private List<CardType> queuedEnemyHand;

    private bool isDealing;

    private void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null) rootCanvas = FindObjectOfType<Canvas>();

        // 互換：playerCardButtonPrefab が未設定なら旧 cardButtonPrefab を使う
        if (playerCardButtonPrefab == null) playerCardButtonPrefab = cardButtonPrefab;

        // enemy 未設定なら player をフォールバック（ただしHover付くので非推奨）
        if (enemyCardButtonPrefab == null) enemyCardButtonPrefab = playerCardButtonPrefab;
    }

    private void Start()
    {
        // 既存の流れで Start から配っていた場合に備えて残す
        // もし「ゲームマネージャ側から必ず呼ぶ」なら消してもOK
        // StartDealWithHands(null, null);
    }

    // ----------------------------
    // Public Methods (互換)
    // ----------------------------

    /// <summary>
    /// 互換：手札を指定して配る（nullならランダムで作る）
    /// </summary>
    public void StartDealWithHands(List<CardType> playerHand, List<CardType> enemyHand)
    {
        if (isDealing) return;

        if (!IsRefsValid())
        {
            Debug.LogError("[CardDealAnimator] References are not set.");
            return;
        }

        // 既存カードを掃除してから配り直す（引き分け再配布対策にもなる）
        ClearHands();

        queuedPlayerHand = playerHand != null ? new List<CardType>(playerHand) : BuildRandomHand(dealCount);
        queuedEnemyHand = enemyHand != null ? new List<CardType>(enemyHand) : BuildRandomHand(dealCount);

        StartCoroutine(DealRoutine(queuedPlayerHand, queuedEnemyHand));
    }

    /// <summary>
    /// 互換：ランダムで配る（GameManager等から呼ぶ用）
    /// </summary>
    public void StartDealWithHands()
    {
        StartDealWithHands(null, null);
    }

    public void RemovePlayerButtonAt(int index)
    {
        if (index < 0 || index >= playerButtons.Count) return;

        var b = playerButtons[index];
        playerButtons.RemoveAt(index);
        if (b != null) Destroy(b.gameObject);
    }

    public void RemoveEnemyButtonAt(int index)
    {
        if (index < 0 || index >= enemyButtons.Count) return;

        var b = enemyButtons[index];
        enemyButtons.RemoveAt(index);
        if (b != null) Destroy(b.gameObject);
    }

    public void ClearHands()
    {
        for (int i = playerButtons.Count - 1; i >= 0; i--)
        {
            if (playerButtons[i] != null) Destroy(playerButtons[i].gameObject);
        }
        playerButtons.Clear();

        for (int i = enemyButtons.Count - 1; i >= 0; i--)
        {
            if (enemyButtons[i] != null) Destroy(enemyButtons[i].gameObject);
        }
        enemyButtons.Clear();
    }

    // ----------------------------
    // Dealing
    // ----------------------------
    private IEnumerator DealRoutine(List<CardType> playerHand, List<CardType> enemyHand)
    {
        isDealing = true;

        RectTransform firstArea = dealPlayerFirst ? playerHandAreaRect : enemyHandAreaRect;
        RectTransform secondArea = dealPlayerFirst ? enemyHandAreaRect : playerHandAreaRect;

        bool firstIsPlayer = dealPlayerFirst;
        bool secondIsPlayer = !dealPlayerFirst;

        // 交互に配る（見た目が気持ちいい）
        int max = Mathf.Max(playerHand.Count, enemyHand.Count);
        for (int i = 0; i < max; i++)
        {
            if (firstIsPlayer)
            {
                if (i < playerHand.Count) yield return DealOne(firstArea, true, playerHand[i]);
                if (i < enemyHand.Count) yield return DealOne(secondArea, false, enemyHand[i]);
            }
            else
            {
                if (i < enemyHand.Count) yield return DealOne(firstArea, false, enemyHand[i]);
                if (i < playerHand.Count) yield return DealOne(secondArea, true, playerHand[i]);
            }

            yield return new WaitForSeconds(interval);
        }

        isDealing = false;
        OnDealFinished?.Invoke();
    }

    private IEnumerator DealOne(RectTransform handArea, bool isPlayer, CardType type)
    {
        // ① 演出用の裏面カードを生成（Canvas直下）
        Image fx = Instantiate(dealBackPrefab, rootCanvas.transform);
        RectTransform fxRect = fx.rectTransform;

        fxRect.position = deckRect.position;
        fxRect.localScale = Vector3.one;

        Vector3 targetPos = handArea.position;

        // ② 移動
        float t = 0f;
        Vector3 startPos = fxRect.position;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.0001f, moveDuration);
            fxRect.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
        fxRect.position = targetPos;

        yield return new WaitForSeconds(flipDelayAfterArrive);

        // ③ 本物カード生成（プレイヤー/敵でPrefab分け）
        Button prefab = isPlayer ? playerCardButtonPrefab : enemyCardButtonPrefab;
        Button real = Instantiate(prefab, handArea);
        real.name = prefab.name + "(Clone)";

        // ④ CardViewへ型を設定
        var view = real.GetComponent<CardView>();
        if (view != null)
        {
            view.SetCardType(type);

            if (isPlayer)
            {
                // プレイヤーは表にするか、裏で保持するかは flipPlayerCards で制御
                if (flipPlayerCards) view.FlipToFront();
                else view.ShowFrontImmediate();
            }
            else
            {
                // 敵は必ず裏（見えない）
                view.ShowBack();
            }
        }

        // ⑤ 生成したボタンを管理リストへ
        if (isPlayer) playerButtons.Add(real);
        else enemyButtons.Add(real);

        // ⑥ 敵だけ Hover/Pulse を強制OFF（Prefabが間違ってても止める保険）
        if (!isPlayer)
        {
            DisableHoverComponents(real.gameObject);
        }

        // ⑦ 演出用裏面を消す
        Destroy(fx.gameObject);
    }

    private void DisableHoverComponents(GameObject go)
    {
        var behaviours = go.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (var mb in behaviours)
        {
            if (mb == null) continue;
            string n = mb.GetType().Name;
            if (n.Contains("Hover") || n.Contains("Pulse"))
            {
                mb.enabled = false;
            }
        }
    }

    // ----------------------------
    // Helpers
    // ----------------------------
    private bool IsRefsValid()
    {
        return deckRect != null
            && playerHandAreaRect != null
            && enemyHandAreaRect != null
            && dealBackPrefab != null
            && (playerCardButtonPrefab != null || cardButtonPrefab != null)
            && enemyCardButtonPrefab != null
            && rootCanvas != null;
    }

    private List<CardType> BuildRandomHand(int count)
    {
        var list = new List<CardType>(count);
        for (int i = 0; i < count; i++)
        {
            list.Add((CardType)UnityEngine.Random.Range(0, 4));
        }
        return list;
    }
}