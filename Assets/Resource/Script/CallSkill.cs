using UnityEngine;

[CreateAssetMenu(menuName = "FourRole/Skills/CallSkill", fileName = "CallSkill")]
public class CallSkill : BaseSkill
{
    [Header("GUESS Target")]
    [SerializeField] private CardType targetType = CardType.King;

    private void OnEnable()
    {
        skillName = "GUESS";
        cost = 3;
    }

    public override bool TryExecute(SkillController controller, FourRoleGameManager gm)
    {
        if (!CanUse(controller, gm)) return false;
        if (controller == null) return false;
        if (gm == null) return false;

        controller.SetCall(targetType);
        gm.SetBattleTextExternal($"GUESSÅI {targetType} Çó\ëzÇµÇΩÅI");
        return true;
    }
}