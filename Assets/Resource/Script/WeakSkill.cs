using UnityEngine;

[CreateAssetMenu(menuName = "FourRole/Skills/WeakSkill", fileName = "WeakSkill")]
public class WeakSkill : BaseSkill
{
    private void OnEnable()
    {
        skillName = "WEAK";
        cost = 2;
    }

    public override bool TryExecute(SkillController controller, FourRoleGameManager gm)
    {
        if (!CanUse(controller, gm)) return false;

        controller.SetWeakArmed(true);
        gm.SetBattleTextExternal("このターン相手の得点 -1！");
        return true;
    }
}