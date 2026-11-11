using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CardGameUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private CardGameManager gameManager;
    [SerializeField] private BarUIManager barUIManager;
    [Header("Contenedores")]
    [SerializeField] private Transform playerHandContainer;
    [SerializeField] private Transform playerBoardContainer;
    [SerializeField] private Transform enemyBoardContainer;
    [Header("Prefab")]
    [SerializeField] private GameObject cardButtonPrefab;
    [Header("Textos")]
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private TMP_Text energyText;
    [SerializeField] private TMP_Text playerScoreText;
    [SerializeField] private TMP_Text enemyScoreText;
    [SerializeField] private TMP_Text resultText;
    [Header("Botones")]
    [SerializeField] private Button endTurnButton;
    [SerializeField] private Button exitButton;
    [Header("Pantalla de resultado")]
    [SerializeField] private GameObject resultPanel;

    private void OnEnable()
    {
        if (gameManager == null) { Debug.LogError("Falta asignar CardGameManager en CardGameUI."); return; }
        gameManager.OnGameStateChanged += RefreshUI;
        if (endTurnButton != null)
        {
            endTurnButton.onClick.RemoveListener(gameManager.EndTurn);
            endTurnButton.onClick.AddListener(gameManager.EndTurn);
        }
        if (exitButton != null)
        {
            exitButton.onClick.RemoveListener(ExitGame);
            exitButton.onClick.AddListener(ExitGame);
        }
        RefreshUI();
    }

    private void OnDisable()
    {
        if (gameManager != null)
        {
            gameManager.OnGameStateChanged -= RefreshUI;
            if (endTurnButton != null) endTurnButton.onClick.RemoveListener(gameManager.EndTurn);
        }
        if (exitButton != null) exitButton.onClick.RemoveListener(ExitGame);
    }

    private void ExitGame()
    {
        if (gameManager == null || !gameManager.GameFinished) return;
        if (barUIManager != null) barUIManager.ClosePanel();
        else Debug.LogWarning("Asigna BarUIManager en CardGameUI para poder salir.");
    }

    private void RefreshUI()
    {
        if (gameManager == null) return;
        bool finished = gameManager.GameFinished;
        if (turnText != null) turnText.text = "Turno " + gameManager.CurrentTurn + " / 6";
        if (energyText != null) energyText.text = "Energia: " + gameManager.CurrentEnergy;
        if (playerScoreText != null) playerScoreText.text = "Tu poder: " + gameManager.PlayerPower;
        if (enemyScoreText != null) enemyScoreText.text = "Rival: " + gameManager.EnemyPower;
        if (!finished && cardButtonPrefab != null)
        {
            RefreshCards(playerHandContainer, gameManager.PlayerHand, true);
            RefreshCards(playerBoardContainer, gameManager.PlayerBoard, false);
            RefreshCards(enemyBoardContainer, gameManager.EnemyBoard, false);
        }
        if (endTurnButton != null) endTurnButton.interactable = !finished;
        if (resultText != null && finished)
        {
            string outcome = gameManager.PlayerPower > gameManager.EnemyPower ? "¡GANASTE!" :
                gameManager.PlayerPower < gameManager.EnemyPower ? "PERDISTE" : "EMPATE";
            resultText.text = outcome + "\n\nTu poder: " + gameManager.PlayerPower +
                "\nPoder rival: " + gameManager.EnemyPower +
                "\n\nMonedas totales: " + gameManager.Coins;
        }
        if (resultPanel != null) resultPanel.SetActive(finished);
        else
        {
            if (resultText != null) resultText.gameObject.SetActive(finished);
            if (exitButton != null) exitButton.gameObject.SetActive(finished);
        }
    }

    private void RefreshCards(Transform container, IReadOnlyList<CardInstance> cards, bool playable)
    {
        if (container == null) return;
        foreach (Transform child in container)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }
        for (int i = 0; i < cards.Count; i++)
        {
            CardInstance card = cards[i];
            GameObject cardObject = Instantiate(cardButtonPrefab, container);
            // Child 'Artwork' is optional: old prefab continues to work without it.
            Transform artTransform = cardObject.transform.Find("Artwork");
            if (artTransform != null)
            {
                Image artImage = artTransform.GetComponent<Image>();
                if (artImage != null)
                {
                    artImage.sprite = card.cardData.artwork;
                    artImage.enabled = artImage.sprite != null;
                    artImage.raycastTarget = false;
                }
            }
            // Explicit child avoids accidentally editing text in the art area.
            Transform labelTransform = cardObject.transform.Find("CardLabel");
            TMP_Text label = labelTransform != null ? labelTransform.GetComponent<TMP_Text>() :
                cardObject.GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                string runeNames = "";
                foreach (RuneDataSO rune in card.equippedRunes)
                    if (rune != null) runeNames += "\n" + rune.runeName;
                label.text = card.cardData.cardName + "\nCoste: " + card.EnergyCost +
                    "\nPoder: " + card.currentPower + runeNames;
            }
            Button button = cardObject.GetComponent<Button>();
            if (button == null) continue;
            button.onClick.RemoveAllListeners();
            if (playable)
            {
                int index = i;
                button.onClick.AddListener(() => gameManager.PlayCard(index));
                button.interactable = !gameManager.GameFinished && card.EnergyCost <= gameManager.CurrentEnergy;
            }
            else button.interactable = false;
        }
    }
}
