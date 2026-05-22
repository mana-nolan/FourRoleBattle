using UnityEngine;

[CreateAssetMenu(menuName = "FourRole/Skills/DrawPlusSkill", fileName = "DrawPlusSkill")]
public class DrawPlusSkill : BaseSkill
{
    private void OnEnable()
    {
        skillName = "DRAW+";
        cost = 2;
    }

    public override bool TryExecute(SkillController controller, FourRoleGameManager gm)
    {
        if (!CanUse(controller, gm)) return false;

        controller.SetDrawPlusArmed(true);
        gm.SetBattleTextExternal("このターン引き分けで +1点！");
        return true;
    }
}