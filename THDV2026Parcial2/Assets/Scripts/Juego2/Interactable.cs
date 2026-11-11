
using UnityEngine;
using UnityEngine.InputSystem;

public class Interactable : MonoBehaviour
{
    public enum InteractionType
    {
        Shop,
        CardTable,
        Exit
    }

    [Header("Interaccion")]
    [SerializeField] private InteractionType interactionType;
    [SerializeField] private Transform player;
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private BarUIManager uiManager;

    private bool wasNearby;

    private void Update()
    {
        if (player == null || uiManager == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        bool nearby = distance <= interactionDistance;

        if (nearby && !wasNearby && !uiManager.IsBusy)
            uiManager.SetNearbyAction(GetActionName());

        if (!nearby && wasNearby)
            uiManager.ClearNearbyAction();

        wasNearby = nearby;

        if (!nearby || uiManager.IsBusy || Keyboard.current == null)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
            Interact();
    }

    private string GetActionName()
    {
        switch (interactionType)
        {
            case InteractionType.Shop:
                return "Tienda";

            case InteractionType.CardTable:
                return "Jugar partida";

            case InteractionType.Exit:
                return "Salir";

            default:
                return "Interactuar";
        }
    }

    private void Interact()
    {
        switch (interactionType)
        {
            case InteractionType.Shop:
                uiManager.OpenShop();
                break;

            case InteractionType.CardTable:
                uiManager.OpenCardGame();
                break;

            case InteractionType.Exit:
                uiManager.OpenMainMenu();
                break;
        }
    }
}
