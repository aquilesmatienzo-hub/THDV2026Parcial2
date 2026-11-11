
using UnityEngine;

public enum CardRarity
{
    Common,
    Rare,
    Epic
}

[CreateAssetMenu(fileName = "NewCard", menuName = "Game/Cards/Card")]
public class CardDataSO : ScriptableObject
{
    [Header("Identidad")]
    public string cardName;
    public Sprite artwork;
    public CardRarity rarity;

    [Header("Estadisticas")]
    [Min(1)] public int energyCost = 1;
    [Min(0)] public int basePower = 1;

    [Header("Runas")]
    [Range(1, 3)] public int runeSlots = 1;

    [Header("Tienda")]
    [Min(0)] public int shopPrice = 2;
}
