
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private CardGameManager gameManager;
    [SerializeField] private Transform productsContainer;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private TMP_Text coinsText;

    private void OnEnable()
    {
        if (gameManager != null)
            gameManager.OnGameStateChanged += RefreshShop;

        RefreshShop();
    }

    private void OnDisable()
    {
        if (gameManager != null)
            gameManager.OnGameStateChanged -= RefreshShop;
    }

    private void RefreshShop()
    {
        if (gameManager == null ||
            productsContainer == null ||
            buttonPrefab == null)
            return;

        coinsText.text = "Monedas: " + gameManager.Coins;

        foreach (Transform child in productsContainer)
            Destroy(child.gameObject);

        foreach (CardDataSO card in gameManager.AvailableCards)
        {
            if (card == null)
                continue;

            GameObject item = Instantiate(buttonPrefab, productsContainer);
            Button button = item.GetComponent<Button>();
            TMP_Text label = item.GetComponentInChildren<TMP_Text>();

            label.text = card.cardName + " - " + card.shopPrice + " monedas";

            button.onClick.AddListener(() =>
            {
                gameManager.BuyCard(card);
                RefreshShop();
            });
        }

        foreach (RuneDataSO rune in gameManager.AvailableRunes)
        {
            if (rune == null)
                continue;

            GameObject item = Instantiate(buttonPrefab, productsContainer);
            Button button = item.GetComponent<Button>();
            TMP_Text label = item.GetComponentInChildren<TMP_Text>();

            label.text = rune.runeName + " - " + rune.shopPrice + " monedas";

            button.onClick.AddListener(() =>
            {
                gameManager.BuyRune(rune);
                RefreshShop();
            });
        }
    }
}
