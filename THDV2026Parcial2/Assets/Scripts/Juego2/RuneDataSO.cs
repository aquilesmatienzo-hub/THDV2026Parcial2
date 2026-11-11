
using UnityEngine;

public enum RuneTrigger
{
    OnReveal,
    EndOfTurn
}

public enum RuneEffect
{
    DestroyStrongestEnemy,
    GainPowerIfTwoCardsPlayed,
    DoublePowerIfNoCardsPlayed,
    DrawCard
}

[CreateAssetMenu(fileName = "NewRune", menuName = "Game/Runes/Rune")]
public class RuneDataSO : ScriptableObject
{
    [Header("Identidad")]
    public string runeName;

    [TextArea(2, 4)]
    public string description;

    public Sprite artwork;

    [Header("Funcionamiento")]
    public RuneTrigger trigger;
    public RuneEffect effect;

    [Header("Valores")]
    public int effectValue = 2;
    public int shopPrice = 3;
}
