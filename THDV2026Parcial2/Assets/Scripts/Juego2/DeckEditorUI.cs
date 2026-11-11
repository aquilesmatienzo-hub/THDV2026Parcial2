using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class DeckEditorUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private CardGameManager gameManager;
    [Header("Contenedores")]
    [SerializeField] private Transform cardsContainer;
    [SerializeField] private Transform runesContainer;
    [Header("Prefab")]
    [SerializeField] private GameObject buttonPrefab;
    [Header("Textos")]
    [SerializeField] private TMP_Text deckCountText;

    private CardInstance focusedCard;
    public IReadOnlyList<CardInstance> SelectedCards => gameManager != null ? gameManager.SelectedDeck : null;

    private void OnEnable()
    {
        if (gameManager != null)
            gameManager.OnGameStateChanged += RefreshUI;
        RefreshUI();
    }

    private void OnDisable()
    {
        if (gameManager != null)
            gameManager.OnGameStateChanged -= RefreshUI;
    }

    private void RefreshUI()
    {
        if (gameManager == null || buttonPrefab == null) return;
        if (focusedCard == null || !gameManager.OwnedCards.Contains(focusedCard))
            focusedCard = gameManager.OwnedCards.Count > 0 ? gameManager.OwnedCards[0] : null;
        if (deckCountText != null)
        {
            int count = gameManager.SelectedDeck.Count;
            deckCountText.text = "Mazo: " + count + "/" + gameManager.MaxDeckCards +
                (count < gameManager.MinDeckCards ? " | Faltan cartas" : " | Mazo listo");
        }
        RefreshCards();
        RefreshRunes();
    }

    private void RefreshCards()
    {
        if (cardsContainer == null) return;
        ClearContainer(cardsContainer);
        foreach (CardInstance card in gameManager.OwnedCards)
        {
            if (card == null || card.cardData == null) continue;
            GameObject item = Instantiate(buttonPrefab, cardsContainer);
            Button button = item.GetComponent<Button>();
            TMP_Text label = item.GetComponentInChildren<TMP_Text>();
            bool inDeck = gameManager.SelectedDeck.Contains(card);
            if (label != null)
                label.text = (focusedCard == card ? "> " : "") + card.cardData.cardName +
                    "\n" + (inDeck ? "[EN MAZO]" : "[FUERA]") +
                    " | Poder: " + card.cardData.basePower +
                    "\nRunas: " + card.equippedRunes.Count + "/" + card.RuneSlots;
            if (button != null)
            {
                CardInstance captured = card;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => SelectCard(captured));
            }
        }
    }

    private void SelectCard(CardInstance card)
    {
        // Primera pulsacion: enfoca la carta. Pulsarla de nuevo cambia su inclusion.
        if (focusedCard != card)
        {
            focusedCard = card;
            RefreshUI();
            return;
        }
        if (!gameManager.ToggleDeckCard(card))
            Debug.Log("No se pudo modificar el mazo (minimo 6, maximo 12, o partida activa).");
        RefreshUI();
    }

    private void RefreshRunes()
    {
        if (runesContainer == null) return;
        ClearContainer(runesContainer);
        foreach (RuneDataSO rune in gameManager.OwnedRunes)
        {
            if (rune == null) continue;
            GameObject item = Instantiate(buttonPrefab, runesContainer);
            Button button = item.GetComponent<Button>();
            TMP_Text label = item.GetComponentInChildren<TMP_Text>();
            bool equipped = focusedCard != null && focusedCard.equippedRunes.Contains(rune);
            if (label != null)
                label.text = rune.runeName + (equipped ? "\n[EQUIPADA]" : "\n[DISPONIBLE]");
            if (button != null)
            {
                RuneDataSO captured = rune;
                button.onClick.RemoveAllListeners();
                button.interactable = focusedCard != null && gameManager.SelectedDeck.Contains(focusedCard);
                button.onClick.AddListener(() => ToggleRune(captured));
            }
        }
    }

    private void ToggleRune(RuneDataSO rune)
    {
        if (focusedCard == null || !gameManager.SelectedDeck.Contains(focusedCard)) return;
        if (!gameManager.ToggleCardRune(focusedCard, rune))
            Debug.Log("No se pudo cambiar la runa: revisa los espacios disponibles.");
        RefreshUI();
    }

    private void ClearContainer(Transform container)
    {
        foreach (Transform child in container)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }
    }
}
