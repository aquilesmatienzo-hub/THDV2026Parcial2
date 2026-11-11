
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class BarUIManager : MonoBehaviour
{
    [Header("Jugador")]
    [SerializeField] private MonoBehaviour playerMovement;

    [Header("Paneles")]
    [SerializeField] private GameObject cardGamePanel;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject mainMenuPanel;

    [Header("Interaccion")]
    [SerializeField] private TMP_Text interactionText;

    [Header("Juego de cartas")]
    [SerializeField] private CardGameManager cardGameManager;

    private GameObject currentPanel;
    private string nearbyAction = "";

    public bool IsBusy => currentPanel != null;

    private void Start()
    {
        if (cardGamePanel != null) cardGamePanel.SetActive(false);
        if (shopPanel != null) shopPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);

        if (interactionText != null)
            interactionText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (currentPanel == pausePanel)
                ClosePanel();
            else if (currentPanel == null)
                OpenPanel(pausePanel);
        }
    }

    public void SetNearbyAction(string action)
    {
        nearbyAction = action;

        if (interactionText != null && !IsBusy)
        {
            interactionText.text = "E - " + action;
            interactionText.gameObject.SetActive(true);
        }
    }

    public void ClearNearbyAction()
    {
        nearbyAction = "";

        if (interactionText != null)
            interactionText.gameObject.SetActive(false);
    }

    public void OpenCardGame()
    {
        OpenPanel(cardGamePanel);

        if (cardGameManager != null)
            cardGameManager.StartGame();
    }

    public void OpenShop()
    {
        OpenPanel(shopPanel);
    }


    public void OpenMainMenu()
    {
        if (currentPanel != null)
        {
            currentPanel.SetActive(false);
            currentPanel = null;
        }

        OpenPanel(mainMenuPanel);
    }


    public void OpenPanel(GameObject panel)
    {
        if (panel == null || IsBusy)
            return;

        currentPanel = panel;
        panel.SetActive(true);

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (interactionText != null)
            interactionText.gameObject.SetActive(false);
    }

    public void ClosePanel()
    {
        if (currentPanel == null)
            return;

        currentPanel.SetActive(false);
        currentPanel = null;

        if (playerMovement != null)
            playerMovement.enabled = true;

        if (interactionText != null && nearbyAction != "")
        {
            interactionText.text = "E - " + nearbyAction;
            interactionText.gameObject.SetActive(true);
        }
    }
}
