using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class FourRoleGameManager : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private CardDealAnimator dealAnimator;
    [SerializeField] private CardArtDatabase artDatabase;
    [SerializeField] private CardFXAnimator fxAnimator;

    [Header("UI - Played Cards")]
    [SerializeField] private Image playerPlayedCardImage;
    [SerializeField] private Image enemyPlayedCardImage;

    [Header("UI - Frames")]
    [SerializeField] private Image playerFrameImage;
    [SerializeField] private Image enemyFrameImage;

    [Header("UI - Text")]
    [SerializeField] private TextMeshProUGUI battleText;
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("UI - End Button")]
    [SerializeField] private Button backToMainButton;
    [SerializeField] private string mainSceneName = "MainScene";

    [Header("Skill Controller")]
    [SerializeField] private SkillController skillController;

    [Header("Enemy Skill Controller")]
    [SerializeField] private EnemySkillController enemySkillController;

    [Header("Skill Reveal UI")]
    [SerializeField] private GameObject skillRevealPanel;
    [SerializeField] private Image revealCardImage;

    [Header("Hand Rule")]
    [SerializeField] private int handSize = 5;

    [Header("Flow")]
    [SerializeField] private bool autoStartDealOnPlay = true;

    [Header("演出テンポ")]
    [SerializeField] private float afterFlyWait = 0.15f;
    [SerializeField] private float afterResultWait = 0.55f;

    [Header("勝者ポップ演出")]
    [SerializeField] private float winPopScale = 1.2f;
    [SerializeField] private float winPopDuration = 0.15f;

    [Header("延長戦（同点）")]
    [SerializeField] private float overtimeMessageWait = 0.9f;

    [Header("LOCK 見た目")]
    [SerializeField] private Color lockedCardTint = new Color(0.45f, 0.45f, 0.45f, 1f);

    [Header("Audio")]
    [SerializeField] private AudioClip cardPlaySE;

    private List<CardType> playerHand = new List<CardType>();
    private List<CardType> enemyHand = new List<CardType>();

    private int playerScore = 0;
    private int enemyScore = 0;

    private bool isInputLocked = true;
    private bool isGameFinished = false;

    // プレイヤーLOCK
    private int playerLockedIndex = -1;
    private readonly Dictionary<Graphic, Color> playerLockedGraphicOriginalColors = new Dictionary<Graphic, Color>();

    // 敵LOCK
    private int enemyLockedIndex = -1;
    private readonly Dictionary<Graphic, Color> enemyLockedGraphicOriginalColors = new Dictionary<Graphic, Color>();

    // ラウンド結果追加メッセージ
    private string roundExtraMessage = "";

    public bool IsInputLocked => isInputLocked;
    public bool IsGameFinished => isGameFinished;
    public int PlayerHandCount => playerHand != null ? playerHand.Count : 0;
    public int EnemyHandCount => enemyHand != null ? enemyHand.Count : 0;
    public bool HasEnemyLockActive => enemyLockedIndex >= 0;
    public bool HasPlayerLockActive => playerLockedIndex >= 0;

    private void Awake()
    {
        if (dealAnimator != null)
            dealAnimator.OnDealFinished += OnDealFinished;

        if (backToMainButton != null)
        {
            backToMainButton.onClick.RemoveAllListeners();
            backToMainButton.onClick.AddListener(OnClickBackToMain);
        }

        if (skillController == null)
            skillController = FindObjectOfType<SkillController>();

        if (enemySkillController == null)
            enemySkillController = FindObjectOfType<EnemySkillController>();

        if (skillRevealPanel != null)
            skillRevealPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (dealAnimator != null)
            dealAnimator.OnDealFinished -= OnDealFinished;

        if (backToMainButton != null)
            backToMainButton.onClick.RemoveListener(OnClickBackToMain);
    }

    private void Start()
    {
        InitUI();

        if (autoStartDealOnPlay)
            StartNewGame();
    }

    public void StartNewGame()
    {
        isGameFinished = false;
        SetInputLocked(true);

        playerScore = 0;
        enemyScore = 0;
        UpdateScoreUI();

        HideEndButton();
        HidePlayedCards();
        SetBattleTextExternal("カードを配るよ！");

        ClearEnemyLockOnTurnEnd();
        ClearPlayerLockOnTurnEnd();

        if (skillController != null)
            skillController.ResetForNewGame();

        if (enemySkillController != null)
            enemySkillController.ResetForNewGame();

        StartNewRound();
    }

    private void StartNewRound()
    {
        ClearEnemyLockOnTurnEnd();
        ClearPlayerLockOnTurnEnd();

        playerHand = BuildHand(handSize);
        enemyHand = BuildHand(handSize);

        HidePlayedCards();
        SetBattleTextExternal("カードを配るよ！");
        SetInputLocked(true);

        dealAnimator.StartDealWithHands(playerHand, enemyHand);
    }

    private void OnDealFinished()
    {
        if (isGameFinished) return;

        HookPlayerButtons();

        string enemySkillMessage = "";
        if (enemySkillController != null)
            enemySkillMessage = enemySkillController.TryPrepareEnemyTurn();

        SetInputLocked(false);

        if (skillController != null)
            skillController.OnDealFinished();

        SetBattleTextExternal(BuildTurnPrompt(enemySkillMessage));
    }

    private void SetInputLocked(bool locked)
    {
        isInputLocked = locked;

        if (skillController != null)
            skillController.OnInputLockChanged(locked);
    }

    private List<CardType> BuildHand(int size)
    {
        List<CardType> hand = new List<CardType>();

        hand.Add(CardType.King);
        hand.Add(CardType.Soldier);
        hand.Add(CardType.Commoner);
        hand.Add(CardType.Assassin);

        while (hand.Count < size)
            hand.Add((CardType)Random.Range(0, 4));

        Shuffle(hand);
        return hand;
    }

    private void Shuffle(List<CardType> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            CardType tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
    }

    private void HookPlayerButtons()
    {
        var buttons = dealAnimator.PlayerButtons;

        for (int i = 0; i < buttons.Count; i++)
        {
            int index = i;
            Button b = buttons[i];
            if (b == null) continue;

            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(() =>
            {
                if (isInputLocked || isGameFinished) return;

                if (IsPlayerCardLocked(index))
                {
                    SetBattleTextExternal("Enemy LOCK！このカードは封印されていて出せない！");
                    return;
                }

                if (skillController != null && skillController.TryConsumeCardClick(index))
                    return;

                if (skillController != null && skillController.IsBlockingCardPlay())
                    return;

                StartCoroutine(ResolveTurnRoutine(index));
            });
        }
    }

    private IEnumerator ResolveTurnRoutine(int playerIndex)
    {
        if (isInputLocked) yield break;
        if (isGameFinished) yield break;
        if (playerIndex < 0 || playerIndex >= playerHand.Count) yield break;
        if (enemyHand.Count <= 0) yield break;

        SetInputLocked(true);

        CardType playerCard = playerHand[playerIndex];

        int enemyIndex = GetRandomPlayableEnemyIndex();
        if (enemyIndex < 0)
        {
            Debug.LogWarning("[FourRoleGameManager] No playable enemy card found.");
            yield break;
        }

        CardType enemyCard = enemyHand[enemyIndex];

        RectTransform playerFrom = dealAnimator.PlayerButtons[playerIndex].GetComponent<RectTransform>();
        RectTransform enemyFrom = dealAnimator.EnemyButtons[enemyIndex].GetComponent<RectTransform>();

        RectTransform playerTo = playerPlayedCardImage.GetComponent<RectTransform>();
        RectTransform enemyTo = enemyPlayedCardImage.GetComponent<RectTransform>();

        dealAnimator.PlayerButtons[playerIndex].gameObject.SetActive(false);
        dealAnimator.EnemyButtons[enemyIndex].gameObject.SetActive(false);

        yield return fxAnimator.FlyCard(playerFrom, playerTo, artDatabase.GetFront(playerCard), true);

        // ★ここで鳴らす（プレイヤー）
        AudioManager.Instance?.PlaySE(cardPlaySE);

        yield return fxAnimator.FlyCard(enemyFrom, enemyTo, artDatabase.GetFront(enemyCard), true);

        // ★ここで鳴らす（敵）
        AudioManager.Instance?.PlaySE(cardPlaySE);

        ShowPlayedCards(playerCard, enemyCard);

        yield return new WaitForSecondsRealtime(afterFlyWait);

        Outcome result = Judge(playerCard, enemyCard);

        if (result == Outcome.PlayerWin)
            yield return StartCoroutine(Pop(playerPlayedCardImage.rectTransform));
        else if (result == Outcome.EnemyWin)
            yield return StartCoroutine(Pop(enemyPlayedCardImage.rectTransform));

        roundExtraMessage = "";
        ApplyScore(result, playerCard, enemyCard);
        UpdateScoreUI();
        SetBattleTextExternal(BuildRoundResultText(result, playerCard, enemyCard));

        if (skillController != null)
            skillController.ClearTurnSkills();

        if (enemySkillController != null)
            enemySkillController.ClearTurnSkills();

        ClearEnemyLockOnTurnEnd();
        ClearPlayerLockOnTurnEnd();

        yield return new WaitForSecondsRealtime(afterResultWait);

        playerHand.RemoveAt(playerIndex);
        enemyHand.RemoveAt(enemyIndex);

        dealAnimator.RemovePlayerButtonAt(playerIndex);
        dealAnimator.RemoveEnemyButtonAt(enemyIndex);

        if (playerHand.Count == 0 || enemyHand.Count == 0)
        {
            yield return StartCoroutine(HandleRoundEndRoutine());
            yield break;
        }

        HookPlayerButtons();

        string enemySkillMessage = "";
        if (enemySkillController != null)
            enemySkillMessage = enemySkillController.TryPrepareEnemyTurn();

        SetInputLocked(false);
        SetBattleTextExternal(BuildTurnPrompt(enemySkillMessage));
    }

    private IEnumerator HandleRoundEndRoutine()
    {
        SetInputLocked(true);

        if (playerScore == enemyScore)
        {
            SetBattleTextExternal("引き分け！延長戦へ…");
            yield return new WaitForSecondsRealtime(overtimeMessageWait);

            ClearEnemyLockOnTurnEnd();
            ClearPlayerLockOnTurnEnd();

            if (skillController != null)
                skillController.ResetForOvertime();

            if (enemySkillController != null)
                enemySkillController.ResetForOvertime();

            StartNewRound();
            yield break;
        }

        isGameFinished = true;

        if (playerScore > enemyScore)
            SetBattleTextExternal($"勝利！ Player {playerScore} - Enemy {enemyScore}");
        else
            SetBattleTextExternal($"敗北… Player {playerScore} - Enemy {enemyScore}");

        ShowEndButton();
        SetInputLocked(true);
    }

    // ----------------------------
    // Public APIs for Skills
    // ----------------------------
    public bool TryChangePlayerCard(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= playerHand.Count) return false;
        if (enemyHand == null || enemyHand.Count <= 0) return false;

        int enemyIndex = Random.Range(0, enemyHand.Count);

        CardType p = playerHand[playerIndex];
        CardType e = enemyHand[enemyIndex];

        playerHand[playerIndex] = e;
        enemyHand[enemyIndex] = p;

        RefreshPlayerCardView(playerIndex);
        RefreshEnemyCardView(enemyIndex);

        return true;
    }

    public bool TryActivateLockForThisTurn()
    {
        if (isGameFinished) return false;
        if (isInputLocked) return false;
        if (enemyHand == null) return false;
        if (enemyHand.Count <= 1) return false;
        if (enemyLockedIndex >= 0) return false;

        enemyLockedIndex = Random.Range(0, enemyHand.Count);
        ApplyEnemyLockVisual(enemyLockedIndex);

        SetBattleTextExternal($"LOCK発動！ {enemyHand[enemyLockedIndex]} を封印！");
        Debug.Log($"[FourRoleGameManager] PLAYER LOCK activated. enemyLockedIndex = {enemyLockedIndex}, card = {enemyHand[enemyLockedIndex]}");

        return true;
    }

    public bool TryActivatePlayerLockForThisTurn()
    {
        if (isGameFinished) return false;
        if (playerHand == null) return false;
        if (playerHand.Count <= 1) return false;
        if (playerLockedIndex >= 0) return false;

        playerLockedIndex = Random.Range(0, playerHand.Count);
        ApplyPlayerLockVisual(playerLockedIndex);

        Debug.Log($"[FourRoleGameManager] ENEMY LOCK activated. playerLockedIndex = {playerLockedIndex}, card = {playerHand[playerLockedIndex]}");
        return true;
    }

    public void ApplyEvenSkill()
    {
        int diff = Mathf.Abs(playerScore - enemyScore);

        if (diff == 0)
        {
            SetBattleTextExternal("EVEN！ でも今は同点だよ。");
            return;
        }

        int reduce = Mathf.Min(2, diff);

        if (playerScore > enemyScore)
        {
            playerScore -= reduce;
            SetBattleTextExternal($"EVEN！ リードを {reduce} 縮めた！");
        }
        else
        {
            enemyScore -= reduce;
            SetBattleTextExternal($"EVEN！ 点差を {reduce} 縮めた！");
        }

        UpdateScoreUI();
    }

    public void ClearEnemyLockOnTurnEnd()
    {
        if (enemyLockedIndex < 0) return;

        ClearEnemyLockVisual(enemyLockedIndex);
        enemyLockedIndex = -1;
        enemyLockedGraphicOriginalColors.Clear();

        Debug.Log("[FourRoleGameManager] ENEMY-HAND LOCK cleared.");
    }

    public void ClearPlayerLockOnTurnEnd()
    {
        if (playerLockedIndex < 0) return;

        ClearPlayerLockVisual(playerLockedIndex);
        playerLockedIndex = -1;
        playerLockedGraphicOriginalColors.Clear();

        Debug.Log("[FourRoleGameManager] PLAYER-HAND LOCK cleared.");
    }

    public void RefreshPlayerCardView(int index)
    {
        if (index < 0 || index >= dealAnimator.PlayerButtons.Count) return;

        Button b = dealAnimator.PlayerButtons[index];
        if (b == null) return;

        CardView view = b.GetComponent<CardView>();
        if (view == null) return;

        view.SetCardType(playerHand[index]);
        view.FlipToFront();
    }

    public void RefreshEnemyCardView(int index)
    {
        if (index < 0 || index >= dealAnimator.EnemyButtons.Count) return;

        Button b = dealAnimator.EnemyButtons[index];
        if (b == null) return;

        CardView view = b.GetComponent<CardView>();
        if (view == null) return;

        view.SetCardType(enemyHand[index]);
        view.ShowBack();
    }

    public CardType GetEnemyCardType(int index)
    {
        if (index < 0 || index >= enemyHand.Count) return CardType.Commoner;
        return enemyHand[index];
    }

    public Sprite GetCardFrontSprite(CardType type)
    {
        if (artDatabase == null) return null;
        return artDatabase.GetFront(type);
    }

    public void RevealEnemyCardSprite(Sprite sprite, float duration)
    {
        if (sprite == null) return;
        if (skillRevealPanel == null || revealCardImage == null) return;

        StartCoroutine(RevealRoutine(sprite, duration));
    }

    private IEnumerator RevealRoutine(Sprite sprite, float duration)
    {
        SetInputLocked(true);

        revealCardImage.sprite = sprite;
        skillRevealPanel.SetActive(true);

        yield return new WaitForSecondsRealtime(duration);

        skillRevealPanel.SetActive(false);

        if (!isGameFinished)
            SetInputLocked(false);
    }

    public void SetBattleTextExternal(string s)
    {
        if (battleText != null)
            battleText.text = s;
    }

    private void ShowEndButton()
    {
        if (backToMainButton == null) return;
        backToMainButton.gameObject.SetActive(true);
        backToMainButton.interactable = true;
    }

    private void HideEndButton()
    {
        if (backToMainButton == null) return;
        backToMainButton.gameObject.SetActive(false);
    }

    private void OnClickBackToMain()
    {
        if (backToMainButton != null)
            backToMainButton.interactable = false;

        if (string.IsNullOrEmpty(mainSceneName))
        {
            Debug.LogError("[FourRoleGameManager] mainSceneName is empty.");
            return;
        }

        SceneManager.LoadScene(mainSceneName);
    }

    private IEnumerator Pop(RectTransform rect)
    {
        Vector3 original = rect.localScale;
        Vector3 target = original * winPopScale;

        float t = 0f;
        while (t < winPopDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / winPopDuration);
            rect.localScale = Vector3.Lerp(original, target, k);
            yield return null;
        }

        t = 0f;
        while (t < winPopDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / winPopDuration);
            rect.localScale = Vector3.Lerp(target, original, k);
            yield return null;
        }

        rect.localScale = original;
    }

    private void InitUI()
    {
        UpdateScoreUI();

        if (battleText != null)
            battleText.text = "";

        HidePlayedCards();
        HideEndButton();

        if (skillRevealPanel != null)
            skillRevealPanel.SetActive(false);
    }

    private void HidePlayedCards()
    {
        if (playerPlayedCardImage != null) playerPlayedCardImage.enabled = false;
        if (enemyPlayedCardImage != null) enemyPlayedCardImage.enabled = false;

        if (playerFrameImage != null) playerFrameImage.enabled = false;
        if (enemyFrameImage != null) enemyFrameImage.enabled = false;
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = $"Player:{playerScore}\nEnemy:{enemyScore}";
    }

    private void ShowPlayedCards(CardType playerCard, CardType enemyCard)
    {
        if (playerPlayedCardImage != null)
        {
            playerPlayedCardImage.sprite = artDatabase.GetFront(playerCard);
            playerPlayedCardImage.enabled = true;
        }

        if (enemyPlayedCardImage != null)
        {
            enemyPlayedCardImage.sprite = artDatabase.GetFront(enemyCard);
            enemyPlayedCardImage.enabled = true;
        }

        if (playerFrameImage != null) playerFrameImage.enabled = true;
        if (enemyFrameImage != null) enemyFrameImage.enabled = true;
    }

    private enum Outcome
    {
        PlayerWin,
        EnemyWin,
        Draw
    }

    private Outcome Judge(CardType player, CardType enemy)
    {
        if (player == enemy) return Outcome.Draw;

        if (player == CardType.King)
        {
            if (enemy == CardType.Soldier || enemy == CardType.Commoner) return Outcome.PlayerWin;
            if (enemy == CardType.Assassin) return Outcome.EnemyWin;
        }

        if (player == CardType.Soldier)
        {
            if (enemy == CardType.Assassin) return Outcome.PlayerWin;
            if (enemy == CardType.King || enemy == CardType.Commoner) return Outcome.EnemyWin;
        }

        if (player == CardType.Commoner)
        {
            if (enemy == CardType.Soldier) return Outcome.PlayerWin;
            if (enemy == CardType.King) return Outcome.EnemyWin;
            if (enemy == CardType.Assassin) return Outcome.Draw;
        }

        if (player == CardType.Assassin)
        {
            if (enemy == CardType.King) return Outcome.PlayerWin;
            if (enemy == CardType.Soldier) return Outcome.EnemyWin;
            if (enemy == CardType.Commoner) return Outcome.Draw;
        }

        return Outcome.Draw;
    }

    private void ApplyScore(Outcome result, CardType playerCard, CardType enemyCard)
    {
        if (result == Outcome.Draw)
        {
            if (skillController != null && skillController.IsDrawPlusActive())
            {
                playerScore += 1;
                roundExtraMessage = " / DRAW+で+1点！";
            }

            if (skillController != null && skillController.IsCallActive())
            {
                skillController.ConsumeCall();
            }

            return;
        }

        bool playerLost = (result == Outcome.EnemyWin);
        bool enemyLost = (result == Outcome.PlayerWin);

        if (playerLost)
        {
            int gain = (playerCard == CardType.King) ? 3 : 1;

            if (skillController != null && skillController.IsWeakActive())
            {
                gain = Mathf.Max(0, gain - 1);
                roundExtraMessage += " / WEAKで相手得点-1！";
            }

            if (enemySkillController != null && enemySkillController.WasDoubleActive())
            {
                gain = enemySkillController.ModifyEnemyGain(gain);
                roundExtraMessage += " / Enemy DOUBLE!";
            }

            if (skillController != null && skillController.IsShieldActive())
            {
                roundExtraMessage += " / SHIELDで防御！";
            }
            else
            {
                enemyScore += gain;
            }

            if (skillController != null && skillController.IsCallActive())
            {
                skillController.ConsumeCall();
            }
        }
        else if (enemyLost)
        {
            int gain = (enemyCard == CardType.King) ? 3 : 1;

            if (skillController != null && skillController.IsCallActive())
            {
                if (enemyCard == skillController.GetCalledType())
                {
                    gain += 2;
                    roundExtraMessage += $" / GUESS成功！ {enemyCard} を当てて+2点！";
                }

                skillController.ConsumeCall();
            }

            if (enemySkillController != null && enemySkillController.IsWeakActive())
            {
                gain = Mathf.Max(0, gain - 1);
                roundExtraMessage += " / Enemy WEAK!";
            }

            if (skillController != null && skillController.WasDoubleActive())
            {
                gain = skillController.ModifyPlayerGain(gain);
                roundExtraMessage += " / DOUBLE発動！";
            }

            if (enemySkillController != null && enemySkillController.IsShieldActive())
            {
                roundExtraMessage += " / Enemy SHIELD!";
            }
            else
            {
                playerScore += gain;
            }
        }
    }

    private string BuildRoundResultText(Outcome result, CardType p, CardType e)
    {
        string baseText = ResultToText(result, p, e);

        if (string.IsNullOrEmpty(roundExtraMessage))
            return baseText;

        return baseText + roundExtraMessage;
    }

    private string ResultToText(Outcome result, CardType p, CardType e)
    {
        if (result == Outcome.PlayerWin) return $"あなたの勝ち！ ({p} > {e})";
        if (result == Outcome.EnemyWin) return $"相手の勝ち… ({p} < {e})";
        return $"引き分け！ ({p} = {e})";
    }

    private string BuildTurnPrompt(string enemySkillMessage)
    {
        string basePrompt = "手札を選択してください！";

        if (string.IsNullOrEmpty(enemySkillMessage))
            return basePrompt;

        return enemySkillMessage + "\n" + basePrompt;
    }

    // ----------------------------
    // LOCK internals
    // ----------------------------
    private int GetRandomPlayableEnemyIndex()
    {
        if (enemyHand == null || enemyHand.Count <= 0) return -1;

        List<int> candidates = new List<int>();

        for (int i = 0; i < enemyHand.Count; i++)
        {
            if (i == enemyLockedIndex) continue;
            candidates.Add(i);
        }

        if (candidates.Count <= 0) return -1;

        int pick = Random.Range(0, candidates.Count);
        return candidates[pick];
    }

    private bool IsPlayerCardLocked(int index)
    {
        return index == playerLockedIndex;
    }

    private void ApplyEnemyLockVisual(int index)
    {
        if (index < 0 || index >= dealAnimator.EnemyButtons.Count) return;

        Button b = dealAnimator.EnemyButtons[index];
        if (b == null) return;

        CardView view = b.GetComponent<CardView>();
        if (view != null)
        {
            view.SetCardType(enemyHand[index]);
            view.FlipToFront();
        }

        Graphic[] graphics = b.GetComponentsInChildren<Graphic>(true);
        foreach (Graphic g in graphics)
        {
            if (g == null) continue;

            if (!enemyLockedGraphicOriginalColors.ContainsKey(g))
                enemyLockedGraphicOriginalColors.Add(g, g.color);

            Color src = enemyLockedGraphicOriginalColors[g];
            g.color = new Color(
                src.r * lockedCardTint.r,
                src.g * lockedCardTint.g,
                src.b * lockedCardTint.b,
                src.a * lockedCardTint.a
            );
        }
    }

    private void ClearEnemyLockVisual(int index)
    {
        if (index < 0 || index >= dealAnimator.EnemyButtons.Count) return;

        Button b = dealAnimator.EnemyButtons[index];
        if (b == null) return;

        foreach (KeyValuePair<Graphic, Color> pair in enemyLockedGraphicOriginalColors)
        {
            if (pair.Key != null)
                pair.Key.color = pair.Value;
        }

        CardView view = b.GetComponent<CardView>();
        if (view != null && index >= 0 && index < enemyHand.Count)
        {
            view.SetCardType(enemyHand[index]);
            view.ShowBack();
        }
    }

    private void ApplyPlayerLockVisual(int index)
    {
        if (index < 0 || index >= dealAnimator.PlayerButtons.Count) return;

        Button b = dealAnimator.PlayerButtons[index];
        if (b == null) return;

        Graphic[] graphics = b.GetComponentsInChildren<Graphic>(true);
        foreach (Graphic g in graphics)
        {
            if (g == null) continue;

            if (!playerLockedGraphicOriginalColors.ContainsKey(g))
                playerLockedGraphicOriginalColors.Add(g, g.color);

            Color src = playerLockedGraphicOriginalColors[g];
            g.color = new Color(
                src.r * lockedCardTint.r,
                src.g * lockedCardTint.g,
                src.b * lockedCardTint.b,
                src.a * lockedCardTint.a
            );
        }
    }

    private void ClearPlayerLockVisual(int index)
    {
        if (index < 0 || index >= dealAnimator.PlayerButtons.Count) return;

        foreach (KeyValuePair<Graphic, Color> pair in playerLockedGraphicOriginalColors)
        {
            if (pair.Key != null)
                pair.Key.color = pair.Value;
        }
    }
}