using UnityEngine;

public class CardArtDatabase : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite backSprite;
    public Sprite kingSprite;
    public Sprite soldierSprite;
    public Sprite commonerSprite;
    public Sprite assassinSprite;

    public Sprite GetFront(CardType type)
    {
        switch (type)
        {
            case CardType.King: return kingSprite;
            case CardType.Soldier: return soldierSprite;
            case CardType.Commoner: return commonerSprite;
            case CardType.Assassin: return assassinSprite;
            default: return kingSprite;
        }
    }
}
