using UnityEngine;

public abstract class BaseSkill : ScriptableObject
{
    [Header("Skill Info")]
    public string skillName = "Skill";

    [Min(1)]
    public int cost = 1;

    public virtual bool CanUse(SkillController controller, FourRoleGameManager gm)
    {
        if (controller == null)
        {
            Debug.LogWarning($"[{skillName}] CanUse failed: controller is null");
            return false;
        }

        if (gm == null)
        {
            Debug.LogWarning($"[{skillName}] CanUse failed: gm is null");
            return false;
        }

        if (gm.IsGameFinished)
        {
            Debug.LogWarning($"[{skillName}] CanUse failed: game finished");
            return false;
        }

        if (gm.IsInputLocked)
        {
            Debug.LogWarning($"[{skillName}] CanUse failed: input locked");
            return false;
        }

        if (!controller.CanUseSkillByCount())
        {
            Debug.LogWarning($"[{skillName}] CanUse failed: no skill use left");
            return false;
        }

        if (!controller.CanUseSkillCost(cost))
        {
            Debug.LogWarning($"[{skillName}] CanUse failed: not enough cost. need={cost}");
            return false;
        }

        return true;
    }

    public abstract bool TryExecute(SkillController controller, FourRoleGameManager gm);
}