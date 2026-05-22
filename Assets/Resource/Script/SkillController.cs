using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private FourRoleGameManager gm;

    [Header("UI")]
    [SerializeField] private Button openSkillButton;
    [SerializeField] private GameObject skillMenuUI;
    [SerializeField] private TextMeshProUGUI skillUsesText;
    [SerializeField] private TextMeshProUGUI skillCostText;

    [Header("Guess UI")]
    [SerializeField] private GameObject guessSelectPanel;

    [Header("Tooltip UI")]
    [SerializeField] private SkillTooltipUI tooltipUI;

    [Header("Rule")]
    [SerializeField] private int totalSkillMaxUses = 2;

    [Header("Skill Cost Rule")]
    [SerializeField] private int totalSkillMaxCost = 5;

    [Header("Skill Assets")]
    [SerializeField] private ChangeSkill changeSkill;
    [SerializeField] private PeekSkill peekSkill;
    [SerializeField] private ShieldSkill shieldSkill;
    [SerializeField] private LockSkill lockSkill;
    [SerializeField] private DoubleSkill doubleSkill;
    [SerializeField] private DrawPlusSkill drawPlusSkill;
    [SerializeField] private WeakSkill weakSkill;
    [SerializeField] private EvenSkill evenSkill;

    [Header("Guess Skill Assets")]
    [SerializeField] private CallSkill guessKingSkill;
    [SerializeField] private CallSkill guessSoldierSkill;
    [SerializeField] private CallSkill guessCommonerSkill;
    [SerializeField] private CallSkill guessAssassinSkill;

    [Header("Audio")]
    [SerializeField] private AudioClip openMenuSE;
    [SerializeField] private AudioClip closeMenuSE;
    [SerializeField] private AudioClip useSkillSE;
    [SerializeField] private AudioClip guessOpenSE;
    [SerializeField] private AudioClip guessSelectSE;
    [SerializeField] private AudioClip changeWaitSE;
    [SerializeField] private AudioClip changeSuccessSE;

    private int usedCount = 0;
    private int usedCost = 0;

    // CHANGE待ち
    private bool waitingPlayerPickForChange = false;
    private ChangeSkill pendingChangeSkill = null;

    // ターン効果
    private bool shieldArmed = false;
    private bool doublePointArmed = false;
    private bool drawPlusArmed = false;
    private bool weakArmed = false;

    private bool callArmed = false;
    private CardType calledType = CardType.King;

    private void Awake()
    {
        if (gm == null)
            gm = FindObjectOfType<FourRoleGameManager>();

        if (skillMenuUI != null)
            skillMenuUI.SetActive(false);

        if (guessSelectPanel != null)
            guessSelectPanel.SetActive(false);

        if (tooltipUI != null)
            tooltipUI.Hide();

        if (openSkillButton != null)
        {
            openSkillButton.onClick.RemoveAllListeners();
            openSkillButton.onClick.AddListener(OpenSkillMenu);
        }

        RefreshUsesText();
        RefreshCostText();
    }

    // ----------------------------
    // Menu
    // ----------------------------
    public void OpenSkillMenu()
    {
        if (skillMenuUI == null) return;
        if (gm == null) return;
        if (gm.IsInputLocked || gm.IsGameFinished) return;

        skillMenuUI.SetActive(true);

        if (guessSelectPanel != null)
            guessSelectPanel.SetActive(false);

        if (tooltipUI != null)
            tooltipUI.Hide();

        AudioManager.Instance?.PlaySE(openMenuSE);
    }

    public void CloseSkillMenu()
    {
        bool didClose = false;

        if (skillMenuUI != null && skillMenuUI.activeSelf)
        {
            skillMenuUI.SetActive(false);
            didClose = true;
        }

        if (guessSelectPanel != null && guessSelectPanel.activeSelf)
        {
            guessSelectPanel.SetActive(false);
            didClose = true;
        }

        if (tooltipUI != null)
            tooltipUI.Hide();

        if (didClose)
            AudioManager.Instance?.PlaySE(closeMenuSE);
    }

    public void ToggleSkillMenu()
    {
        if (skillMenuUI == null) return;

        if (skillMenuUI.activeSelf)
            CloseSkillMenu();
        else
            OpenSkillMenu();
    }

    public void OpenGuessSelectPanel()
    {
        if (gm == null) return;
        if (gm.IsInputLocked || gm.IsGameFinished) return;
        if (guessSelectPanel == null) return;

        guessSelectPanel.SetActive(true);

        if (skillMenuUI != null)
            skillMenuUI.SetActive(false);

        if (tooltipUI != null)
            tooltipUI.Hide();

        AudioManager.Instance?.PlaySE(guessOpenSE);
    }

    public void CloseGuessSelectPanel()
    {
        if (guessSelectPanel != null)
            guessSelectPanel.SetActive(false);

        if (tooltipUI != null)
            tooltipUI.Hide();
    }

    // ----------------------------
    // Use Count / Cost
    // ----------------------------
    public bool CanUseSkillByCount()
    {
        return usedCount < totalSkillMaxUses;
    }

    public bool CanUseSkillCost(int cost)
    {
        return usedCost + cost <= totalSkillMaxCost;
    }

    public int GetRemainingSkillCost()
    {
        return Mathf.Max(0, totalSkillMaxCost - usedCost);
    }

    /// <summary>
    /// スキルを1回使い、指定コストを消費する
    /// </summary>
    public void ConsumeUse(int costAmount)
    {
        usedCount += 1;
        if (usedCount > totalSkillMaxUses)
            usedCount = totalSkillMaxUses;

        usedCost += costAmount;
        if (usedCost > totalSkillMaxCost)
            usedCost = totalSkillMaxCost;

        RefreshUsesText();
        RefreshCostText();
    }

    private void RefreshUsesText()
    {
        if (skillUsesText == null) return;

        int remain = Mathf.Max(0, totalSkillMaxUses - usedCount);
        skillUsesText.text = $"SKILL:{remain}";
    }

    private void RefreshCostText()
    {
        if (skillCostText == null) return;

        int remain = GetRemainingSkillCost();
        skillCostText.text = $"スキルポイント:{remain}/{totalSkillMaxCost}";
    }

    public void ResetForNewGame()
    {
        usedCount = 0;
        usedCost = 0;
        waitingPlayerPickForChange = false;
        pendingChangeSkill = null;

        ClearTurnSkills();

        RefreshUsesText();
        RefreshCostText();

        if (skillMenuUI != null) skillMenuUI.SetActive(false);
        if (guessSelectPanel != null) guessSelectPanel.SetActive(false);
        if (tooltipUI != null) tooltipUI.Hide();
    }

    public void ResetForOvertime()
    {
        usedCount = 0;
        usedCost = 0;
        waitingPlayerPickForChange = false;
        pendingChangeSkill = null;

        ClearTurnSkills();

        RefreshUsesText();
        RefreshCostText();

        if (skillMenuUI != null) skillMenuUI.SetActive(false);
        if (guessSelectPanel != null) guessSelectPanel.SetActive(false);
        if (tooltipUI != null) tooltipUI.Hide();
    }

    // ----------------------------
    // Generic Use
    // ----------------------------
    public void UseSkill(BaseSkill skill)
    {
        if (tooltipUI != null)
            tooltipUI.Hide();

        if (skill == null) return;
        if (gm == null) return;

        if (!skill.CanUse(this, gm))
            return;

        bool ok = skill.TryExecute(this, gm);
        if (!ok)
            return;

        // CHANGE はカードをまだ選んでいないのでここでは消費しない
        if (skill is ChangeSkill)
            return;

        ConsumeUse(skill.cost);
        AudioManager.Instance?.PlaySE(useSkillSE);
        CloseSkillMenu();
    }

    // ----------------------------
    // UI Buttons
    // ----------------------------
    public void OnClickChangeSkill()
    {
        UseSkill(changeSkill);
    }

    public void OnClickPeekSkill()
    {
        UseSkill(peekSkill);
    }

    public void OnClickShieldSkill()
    {
        UseSkill(shieldSkill);
    }

    public void OnClickLockSkill()
    {
        UseSkill(lockSkill);
    }

    public void OnClickDoubleSkill()
    {
        UseSkill(doubleSkill);
    }

    public void OnClickDrawPlusSkill()
    {
        UseSkill(drawPlusSkill);
    }

    public void OnClickWeakSkill()
    {
        UseSkill(weakSkill);
    }

    public void OnClickEvenSkill()
    {
        UseSkill(evenSkill);
    }

    // ----------------------------
    // GUESS
    // ----------------------------
    public void OnClickGuessSkill()
    {
        if (gm == null) return;
        if (gm.IsInputLocked || gm.IsGameFinished) return;
        if (guessSelectPanel == null) return;

        if (!CanUseSkillByCount()) return;

        bool hasAnyGuessSkill =
            guessKingSkill != null ||
            guessSoldierSkill != null ||
            guessCommonerSkill != null ||
            guessAssassinSkill != null;

        if (!hasAnyGuessSkill) return;

        if (skillMenuUI != null)
            skillMenuUI.SetActive(false);

        if (tooltipUI != null)
            tooltipUI.Hide();

        guessSelectPanel.SetActive(true);
        AudioManager.Instance?.PlaySE(guessOpenSE);
    }

    public void OnClickGuessKingSkill()
    {
        if (guessKingSkill == null) return;
        AudioManager.Instance?.PlaySE(guessSelectSE);
        UseSkill(guessKingSkill);
    }

    public void OnClickGuessSoldierSkill()
    {
        if (guessSoldierSkill == null) return;
        AudioManager.Instance?.PlaySE(guessSelectSE);
        UseSkill(guessSoldierSkill);
    }

    public void OnClickGuessCommonerSkill()
    {
        if (guessCommonerSkill == null) return;
        AudioManager.Instance?.PlaySE(guessSelectSE);
        UseSkill(guessCommonerSkill);
    }

    public void OnClickGuessAssassinSkill()
    {
        if (guessAssassinSkill == null) return;
        AudioManager.Instance?.PlaySE(guessSelectSE);
        UseSkill(guessAssassinSkill);
    }

    // ----------------------------
    // CHANGE
    // ----------------------------
    public void StartChangeSelection(ChangeSkill skill)
    {
        pendingChangeSkill = skill;
        waitingPlayerPickForChange = true;

        if (tooltipUI != null)
            tooltipUI.Hide();

        if (skillMenuUI != null) skillMenuUI.SetActive(false);
        if (guessSelectPanel != null) guessSelectPanel.SetActive(false);

        AudioManager.Instance?.PlaySE(changeWaitSE);
    }

    public bool TryConsumeCardClick(int playerIndex)
    {
        if (!waitingPlayerPickForChange) return false;

        if (pendingChangeSkill == null)
        {
            waitingPlayerPickForChange = false;
            return false;
        }

        if (gm == null)
        {
            waitingPlayerPickForChange = false;
            pendingChangeSkill = null;
            return false;
        }

        if (gm.IsInputLocked || gm.IsGameFinished)
            return true;

        if (tooltipUI != null)
            tooltipUI.Hide();

        bool ok = gm.TryChangePlayerCard(playerIndex);

        if (ok)
        {
            ConsumeUse(pendingChangeSkill.cost);
            gm.SetBattleTextExternal("交換したよ！続けてカードを出してね！");
            AudioManager.Instance?.PlaySE(changeSuccessSE);
        }
        else
        {
            gm.SetBattleTextExternal("交換に失敗…");
        }

        waitingPlayerPickForChange = false;
        pendingChangeSkill = null;
        return true;
    }

    public bool IsBlockingCardPlay()
    {
        return waitingPlayerPickForChange;
    }

    // ----------------------------
    // Turn Skill State
    // ----------------------------
    public void SetShieldArmed(bool value)
    {
        shieldArmed = value;
    }

    public bool IsShieldActive()
    {
        return shieldArmed;
    }

    public void SetDoublePointArmed(bool value)
    {
        doublePointArmed = value;
    }

    public int ModifyPlayerGain(int gain)
    {
        if (!doublePointArmed) return gain;

        doublePointArmed = false;
        Debug.Log("[SkillController] DOUBLE applied.");
        return gain * 2;
    }

    public bool WasDoubleActive()
    {
        return doublePointArmed;
    }

    public void SetDrawPlusArmed(bool value)
    {
        drawPlusArmed = value;
    }

    public bool IsDrawPlusActive()
    {
        return drawPlusArmed;
    }

    public void SetWeakArmed(bool value)
    {
        weakArmed = value;
    }

    public bool IsWeakActive()
    {
        return weakArmed;
    }

    public void SetCall(CardType type)
    {
        callArmed = true;
        calledType = type;
    }

    public bool IsCallActive()
    {
        return callArmed;
    }

    public CardType GetCalledType()
    {
        return calledType;
    }

    public void ConsumeCall()
    {
        callArmed = false;
    }

    public void ClearTurnSkills()
    {
        shieldArmed = false;
        doublePointArmed = false;
        drawPlusArmed = false;
        weakArmed = false;
        callArmed = false;
    }

    // ----------------------------
    // Safe hooks
    // ----------------------------
    public void OnDealFinished() { }

    public void OnInputLockChanged(bool locked)
    {
        if (!locked) return;

        if (tooltipUI != null)
            tooltipUI.Hide();

        if (skillMenuUI != null) skillMenuUI.SetActive(false);
        if (guessSelectPanel != null) guessSelectPanel.SetActive(false);

        waitingPlayerPickForChange = false;
        pendingChangeSkill = null;
    }
}