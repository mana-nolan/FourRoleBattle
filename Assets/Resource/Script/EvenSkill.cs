using UnityEngine;

[CreateAssetMenu(menuName = "FourRole/Skills/EvenSkill", fileName = "EvenSkill")]
public class EvenSkill : BaseSkill
{
    private void OnEnable()
    {
        skillName = "EVEN";
        cost = 3;
    }

    public override bool TryExecute(SkillController controller, FourRoleGameManager gm)
    {
        if (!CanUse(controller, gm)) return false;

        gm.ApplyEvenSkill();
        return true;
    }
}