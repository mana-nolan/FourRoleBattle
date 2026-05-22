using UnityEngine;

[CreateAssetMenu(menuName = "FourRole/Skills/DoubleSkill", fileName = "DoubleSkill")]
public class DoubleSkill : BaseSkill
{
    private void OnEnable()
    {
        skillName = "DOUBLE";
        cost = 1;
    }

    public override bool TryExecute(SkillController controller, FourRoleGameManager gm)
    {
        if (!CanUse(controller, gm)) return false;

        controller.SetDoublePointArmed(true);
        gm.SetBattleTextExternal("このターンだけ得点2倍！");
        return true;
    }
}