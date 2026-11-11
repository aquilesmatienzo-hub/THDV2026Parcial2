
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardInstance
{
    // Datos originales de la carta
    public CardDataSO cardData;

    // Runas equipadas en esta copia
    public List<RuneDataSO> equippedRunes = new List<RuneDataSO>();

    // Poder que puede cambiar durante la partida
    public int currentPower;

    public CardInstance(CardDataSO data)
    {
        cardData = data;
        currentPower = data.basePower;
    }

    // Coste de energia
    public int EnergyCost => cardData.energyCost;

    // Cantidad maxima de runas
    public int RuneSlots => cardData.runeSlots;

    // Equipar una runa
    public bool EquipRune(RuneDataSO rune)
    {
        if (rune == null)
            return false;

        if (equippedRunes.Count >= RuneSlots)
            return false;

        // No permite repetir la misma runa
        if (equippedRunes.Contains(rune))
            return false;

        equippedRunes.Add(rune);
        return true;
    }

    // Aumentar el poder
    public void AddPower(int amount)
    {
        currentPower += amount;
    }

    // Multiplicar el poder
    public void MultiplyPower(int multiplier)
    {
        currentPower *= multiplier;
    }

    // Reiniciar el poder para una nueva partida
    public void ResetPower()
    {
        currentPower = cardData.basePower;
    }
}
