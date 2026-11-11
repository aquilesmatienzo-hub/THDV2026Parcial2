
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CardGameUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private CardGameManager gameManager;

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

    [Header("Botones")]
    [SerializeField] private Button endTurnButton;

    private void OnEnable()
    {
        if (gameManager != null)
            gameManager.OnGameStateChanged += RefreshUI;
    }

    private void Start()
    {
        if (gameManager == null)
        {
            Debug.LogError("Falta asignar CardGameManager en CardGameUI.");
            return;
        }

        if (endTurnButton != null)
            endTurnButton.onClick.AddListener(gameManager.EndTurn);

        RefreshUI();
    }

    private void OnDisable()
    {
        if (gameManager != null)
            gameManager.OnGameStateChanged -= RefreshUI;

        if (endTurnButton != null && gameManager != null)
            endTurnButton.onClick.RemoveListener(gameManager.EndTurn);
    }

    private void RefreshUI()
    {
        if (gameManager == null || cardButtonPrefab == null)
            return;

        if (turnText != null)
            turnText.text = "Turno " + gameManager.CurrentTurn + " / 6";

        if (energyText != null)
            energyText.text = "Energia: " + gameManager.CurrentEnergy;

        if (playerScoreText != null)
            playerScoreText.text = "Tu poder: " + gameManager.PlayerPower;

        if (enemyScoreText != null)
            enemyScoreText.text = "Rival: " + gameManager.EnemyPower;

        RefreshCards(playerHandContainer, gameManager.PlayerHand, true);
        RefreshCards(playerBoardContainer, gameManager.PlayerBoard, false);
        RefreshCards(enemyBoardContainer, gameManager.EnemyBoard, false);

        if (endTurnButton != null)
            endTurnButton.interactable = !gameManager.GameFinished;
    }

    private void RefreshCards(
        Transform container,
        IReadOnlyList<CardInstance> cards,
        bool playable)
    {
        if (container == null)
            return;

        foreach (Transform child in container)
            Destroy(child.gameObject);

        for (int i = 0; i < cards.Count; i++)
        {
            CardInstance card = cards[i];
            GameObject cardObject = Instantiate(cardButtonPrefab, container);

            TMP_Text label = cardObject.GetComponentInChildren<TMP_Text>();

            if (label != null)
            {
                string runeNames = "";

                foreach (RuneDataSO rune in card.equippedRunes)
                {
                    if (rune != null)
                        runeNames += "\n" + rune.runeName;
                }

                label.text =
                    card.cardData.cardName +
                    "\nCoste: " + card.EnergyCost +
                    "\nPoder: " + card.currentPower +
                    runeNames;
            }

            Button button = cardObject.GetComponent<Button>();

            if (button != null)
            {
                button.onClick.RemoveAllListeners();

                if (playable)
                {
                    int index = i;
                    button.onClick.AddListener(() => gameManager.PlayCard(index));

                    button.interactable =
                        !gameManager.GameFinished &&
                        card.EnergyCost <= gameManager.CurrentEnergy;
                }
                else
                {
                    button.interactable = false;
                }
            }
        }
    }
}
