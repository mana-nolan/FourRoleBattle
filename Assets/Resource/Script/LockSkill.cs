using UnityEngine;

[CreateAssetMenu(menuName = "FourRole/Skills/LockSkill", fileName = "LockSkill")]
public class LockSkill : BaseSkill
{
    private void OnEnable()
    {
        skillName = "LOCK";
        cost = 4;
    }

    public override bool CanUse(SkillController controller, FourRoleGameManager gm)
    {
        if (!base.CanUse(controller, gm)) return false;

        if (gm.EnemyHandCount <= 1) return false;
        if (gm.HasEnemyLockActive) return false;

        return true;
    }

    public override bool TryExecute(SkillController controller, FourRoleGameManager gm)
    {
        if (!CanUse(controller, gm)) return false;

        bool ok = gm.TryActivateLockForThisTurn();
        if (!ok) return false;

        return true;
    }
}