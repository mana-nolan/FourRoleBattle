using UnityEngine;

public class EnemySkillController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private FourRoleGameManager gm;

    [Header("Rule")]
    [SerializeField] private int totalSkillMaxCost = 5;

    [Header("AI Chance")]
    [Range(0f, 1f)]
    [SerializeField] private float lockChance = 0.25f;

    [Range(0f, 1f)]
    [SerializeField] private float shieldChance = 0.20f;

    [Range(0f, 1f)]
    [SerializeField] private float doubleChance = 0.20f;

    [Range(0f, 1f)]
    [SerializeField] private float weakChance = 0.20f;

    private int usedCost = 0;

    // ターン効果
    private bool shieldArmed = false;
    private bool doublePointArmed = false;
    private bool weakArmed = false;

    private void Awake()
    {
        if (gm == null)
            gm = FindObjectOfType<FourRoleGameManager>();
    }

    // ----------------------------
    // Cost
    // ----------------------------
    public int GetRemainingSkillCost()
    {
        return Mathf.Max(0, totalSkillMaxCost - usedCost);
    }

    public bool CanUseCost(int cost)
    {
        return usedCost + cost <= totalSkillMaxCost;
    }

    private void ConsumeCost(int cost)
    {
        usedCost += cost;
        if (usedCost > totalSkillMaxCost)
            usedCost = totalSkillMaxCost;
    }

    // ----------------------------
    // Reset
    // ----------------------------
    public void ResetForNewGame()
    {
        usedCost = 0;
        ClearTurnSkills();
    }

    public void ResetForOvertime()
    {
        usedCost = 0;
        ClearTurnSkills();
    }

    public void ClearTurnSkills()
    {
        shieldArmed = false;
        doublePointArmed = false;
        weakArmed = false;
    }

    // ----------------------------
    // Turn State
    // ----------------------------
    public bool IsShieldActive()
    {
        return shieldArmed;
    }

    public bool IsWeakActive()
    {
        return weakArmed;
    }

    public bool WasDoubleActive()
    {
        return doublePointArmed;
    }

    public int ModifyEnemyGain(int gain)
    {
        if (!doublePointArmed) return gain;

        doublePointArmed = false;
        Debug.Log("[EnemySkillController] ENEMY DOUBLE applied.");
        return gain * 2;
    }

    // ----------------------------
    // AI
    // ----------------------------
    public string TryPrepareEnemyTurn()
    {
        if (gm == null) return "";
        if (gm.IsGameFinished) return "";

        string message = "";

        // まずLOCKを判定（コスト2）
        if (CanUseCost(2) && gm.PlayerHandCount > 1 && !gm.HasPlayerLockActive)
        {
            if (Random.value < lockChance)
            {
                bool ok = gm.TryActivatePlayerLockForThisTurn();
                if (ok)
                {
                    ConsumeCost(2);
                    AppendMessage(ref message, "Enemy LOCK!");
                }
            }
        }

        // 次に 1コスト系を1つだけ使う
        if (CanUseCost(1))
        {
            float roll = Random.value;

            if (roll < doubleChance)
            {
                doublePointArmed = true;
                ConsumeCost(1);
                AppendMessage(ref message, "Enemy DOUBLE!");
            }
            else if (roll < doubleChance + shieldChance)
            {
                shieldArmed = true;
                ConsumeCost(1);
                AppendMessage(ref message, "Enemy SHIELD!");
            }
            else if (roll < doubleChance + shieldChance + weakChance)
            {
                weakArmed = true;
                ConsumeCost(1);
                AppendMessage(ref message, "Enemy WEAK!");
            }
        }

        return message;
    }

    private void AppendMessage(ref string baseMessage, string add)
    {
        if (string.IsNullOrEmpty(add)) return;

        if (string.IsNullOrEmpty(baseMessage))
            baseMessage = add;
        else
            baseMessage += " " + add;
    }
}