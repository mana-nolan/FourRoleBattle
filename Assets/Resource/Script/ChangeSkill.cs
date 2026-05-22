using UnityEngine;

[CreateAssetMenu(menuName = "FourRole/Skills/ChangeSkill", fileName = "ChangeSkill")]
public class ChangeSkill : BaseSkill
{
    private void OnEnable()
    {
        skillName = "CHANGE";
        cost = 1;
    }

    public override bool TryExecute(SkillController controller, FourRoleGameManager gm)
    {
        if (!CanUse(controller, gm)) return false;

        controller.StartChangeSelection(this);
        gm.SetBattleTextExternal("交換したい自分のカードを1枚クリックしてね！");
        return true;
    }
}