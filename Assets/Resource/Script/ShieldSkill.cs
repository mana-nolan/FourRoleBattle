using UnityEngine;

[CreateAssetMenu(menuName = "FourRole/Skills/ShieldSkill", fileName = "ShieldSkill")]
public class ShieldSkill : BaseSkill
{
    private void OnEnable()
    {
        skillName = "SHIELD";
        cost = 2;
    }

    public override bool TryExecute(SkillController controller, FourRoleGameManager gm)
    {
        if (!CanUse(controller, gm)) return false;

        controller.SetShieldArmed(true);
        gm.SetBattleTextExternal("このターンは負けても失点しない！");
        return true;
    }
}