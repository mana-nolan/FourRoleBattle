using UnityEngine;

[CreateAssetMenu(menuName = "FourRole/Skills/PeekSkill", fileName = "PeekSkill")]
public class PeekSkill : BaseSkill
{
    [SerializeField] private float revealDuration = 1.2f;

    private void OnEnable()
    {
        skillName = "PEEK";
        cost = 1;
    }

    public override bool TryExecute(SkillController controller, FourRoleGameManager gm)
    {
        if (!CanUse(controller, gm)) return false;
        if (gm.EnemyHandCount <= 0) return false;

        int enemyIndex = Random.Range(0, gm.EnemyHandCount);
        CardType cardType = gm.GetEnemyCardType(enemyIndex);
        Sprite front = gm.GetCardFrontSprite(cardType);

        if (front == null) return false;

        gm.RevealEnemyCardSprite(front, revealDuration);
        gm.SetBattleTextExternal($"PEEKI ‘ŠŽè‚Ì {cardType} ‚ðŒ©‚½I");
        return true;
    }
}