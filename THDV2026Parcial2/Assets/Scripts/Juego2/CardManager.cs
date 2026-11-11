
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CardGameManager : MonoBehaviour
{
    [Header("Cartas disponibles")]
    [SerializeField] private List<CardDataSO> availableCards = new();

    [Header("Runas disponibles")]
    [SerializeField] private List<RuneDataSO> availableRunes = new();

    [Header("Configuracion")]
    [SerializeField] private int startingHandSize = 4;
    [SerializeField] private int deckSize = 12;
    [SerializeField] private int maxCopiesPerCard = 3;
    [SerializeField] private int maxBoardCards = 12;
    [SerializeField] private int maxTurns = 6;

    [Header("Recompensas")]
    [SerializeField] private int startingCoins = 5;
    [SerializeField] private int winReward = 5;
    [SerializeField] private int drawReward = 2;
    [SerializeField] private int loseReward = 1;

    [Header("Pruebas temporales")]
    [SerializeField] private bool keyboardTesting = true;

    private readonly List<CardInstance> playerDeck = new();
    private readonly List<CardInstance> enemyDeck = new();
    private readonly List<CardInstance> playerHand = new();
    private readonly List<CardInstance> enemyHand = new();
    private readonly List<CardInstance> playerBoard = new();
    private readonly List<CardInstance> enemyBoard = new();

    // Coleccion permanente durante la sesion.
    private readonly List<CardInstance> ownedCards = new();
    private readonly List<RuneDataSO> ownedRunes = new();

    public IReadOnlyList<CardInstance> OwnedCards => ownedCards;
    public IReadOnlyList<RuneDataSO> OwnedRunes => ownedRunes;
    public IReadOnlyList<CardDataSO> AvailableCards => availableCards;
    public IReadOnlyList<RuneDataSO> AvailableRunes => availableRunes;

    private int currentTurn;
    private int currentEnergy;
    private int enemyEnergy;
    private int playerCardsPlayedThisTurn;
    private int enemyCardsPlayedThisTurn;
    private int coins;

    private bool gameFinished;
    private bool gameStarted;

    // Datos para la futura interfaz.
    public int CurrentTurn => currentTurn;
    public int CurrentEnergy => currentEnergy;
    public int EnemyEnergy => enemyEnergy;
    public int Coins => coins;
    public bool GameFinished => gameFinished;

    public IReadOnlyList<CardInstance> PlayerHand => playerHand;
    public IReadOnlyList<CardInstance> PlayerBoard => playerBoard;
    public IReadOnlyList<CardInstance> EnemyBoard => enemyBoard;

    public int PlayerPower => CalculatePower(playerBoard);
    public int EnemyPower => CalculatePower(enemyBoard);

    public event Action OnGameStateChanged;

    private void Start()
    {
        coins = startingCoins;
        ownedCards.Clear();
        if (availableCards != null)
        {
            foreach (CardDataSO data in availableCards)
            {
                if (data != null && CountCopies(ownedCards, data) < maxCopiesPerCard)
                    ownedCards.Add(new CardInstance(data));
            }
        }
    }

    public void StartGame()
    {
        if (availableCards == null || availableCards.Count == 0)
        {
            Debug.LogError("No hay cartas configuradas.");
            return;
        }

        gameFinished = false;
        gameStarted = false;

        playerDeck.Clear();
        enemyDeck.Clear();
        playerHand.Clear();
        enemyHand.Clear();
        playerBoard.Clear();
        enemyBoard.Clear();

        // Por ahora se juega con todas las cartas propias (hasta deckSize).
        // El editor de mazos permitira elegir una seleccion mas adelante.
        foreach (CardInstance ownedCard in ownedCards)
        {
            if (playerDeck.Count >= deckSize)
                break;
            ownedCard.ResetPower();
            playerDeck.Add(ownedCard);
        }

        CreateDeck(enemyDeck);

        // Equipamos runas aleatorias al rival.
        EquipRandomEnemyRunes();

        ShuffleDeck(playerDeck);
        ShuffleDeck(enemyDeck);

        DrawStartingHand(playerDeck, playerHand);
        DrawStartingHand(enemyDeck, enemyHand);

        currentTurn = 0;
        gameStarted = true;

        Debug.Log("===== NUEVA PARTIDA =====");

        StartNextTurn();
    }

    private void CreateDeck(List<CardInstance> deck)
    {
        List<CardDataSO> validCards =
            availableCards.FindAll(card => card != null);

        if (validCards.Count == 0)
            return;

        // Una copia de cada carta, si hay espacio.
        foreach (CardDataSO data in validCards)
        {
            if (deck.Count >= deckSize)
                break;

            if (CountCopies(deck, data) < maxCopiesPerCard)
                deck.Add(new CardInstance(data));
        }

        // Completamos el mazo aleatoriamente.
        int attempts = 0;

        while (deck.Count < deckSize && attempts < 1000)
        {
            attempts++;

            CardDataSO data =
                validCards[UnityEngine.Random.Range(0, validCards.Count)];

            if (CountCopies(deck, data) >= maxCopiesPerCard)
                continue;

            deck.Add(new CardInstance(data));
        }

        if (deck.Count < deckSize)
            Debug.LogWarning("No se pudo completar el mazo.");
    }

    public bool BuyCard(CardDataSO data)
    {
        if (data == null || !availableCards.Contains(data) || coins < data.shopPrice)
            return false;

        if (CountCopies(ownedCards, data) >= maxCopiesPerCard)
            return false;

        coins -= data.shopPrice;
        ownedCards.Add(new CardInstance(data));
        Debug.Log("Compraste carta: " + data.cardName);
        NotifyStateChanged();
        return true;
    }

    public bool BuyRune(RuneDataSO rune)
    {
        if (rune == null || !availableRunes.Contains(rune) ||
            coins < rune.shopPrice || ownedRunes.Contains(rune))
            return false;

        coins -= rune.shopPrice;
        ownedRunes.Add(rune);
        Debug.Log("Desbloqueaste runa: " + rune.runeName);
        NotifyStateChanged();
        return true;
    }

    private int CountCopies(
        List<CardInstance> deck,
        CardDataSO data)
    {
        int count = 0;

        foreach (CardInstance card in deck)
        {
            if (card.cardData == data)
                count++;
        }

        return count;
    }

    private void EquipRandomEnemyRunes()
    {
        if (availableRunes == null || availableRunes.Count == 0)
            return;

        List<RuneDataSO> validRunes =
            availableRunes.FindAll(rune => rune != null);

        foreach (CardInstance card in enemyDeck)
        {
            int slotsToFill = UnityEngine.Random.Range(
                0,
                card.RuneSlots + 1
            );

            List<RuneDataSO> candidates =
                new List<RuneDataSO>(validRunes);

            ShuffleRunes(candidates);

            foreach (RuneDataSO rune in candidates)
            {
                if (card.equippedRunes.Count >= slotsToFill)
                    break;

                card.EquipRune(rune);
            }
        }
    }

    private void ShuffleRunes(List<RuneDataSO> runes)
    {
        for (int i = runes.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);

            (runes[i], runes[j]) = (runes[j], runes[i]);
        }
    }

    private void ShuffleDeck(List<CardInstance> deck)
    {
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);

            (deck[i], deck[j]) = (deck[j], deck[i]);
        }
    }

    private void DrawStartingHand(
        List<CardInstance> deck,
        List<CardInstance> hand)
    {
        for (int i = 0; i < startingHandSize; i++)
            DrawCard(deck, hand);
    }

    private void DrawCard(
        List<CardInstance> deck,
        List<CardInstance> hand)
    {
        if (deck.Count == 0)
            return;

        CardInstance card = deck[0];
        deck.RemoveAt(0);
        hand.Add(card);
    }

    private void StartNextTurn()
    {
        currentTurn++;

        if (currentTurn > maxTurns)
        {
            EndGame();
            return;
        }

        currentEnergy = currentTurn;
        enemyEnergy = currentTurn;

        playerCardsPlayedThisTurn = 0;
        enemyCardsPlayedThisTurn = 0;

        if (currentTurn > 1)
        {
            DrawCard(playerDeck, playerHand);
            DrawCard(enemyDeck, enemyHand);
        }

        Debug.Log(
            "TURNO " + currentTurn +
            " | Energia: " + currentEnergy
        );

        ShowPlayerHand();
        NotifyStateChanged();
    }

    public void PlayCard(int handIndex)
    {
        if (!gameStarted || gameFinished)
            return;

        if (handIndex < 0 || handIndex >= playerHand.Count)
            return;

        if (playerBoard.Count >= maxBoardCards)
            return;

        CardInstance card = playerHand[handIndex];

        if (card.EnergyCost > currentEnergy)
        {
            Debug.Log("No tenes suficiente energia.");
            return;
        }

        currentEnergy -= card.EnergyCost;

        playerHand.RemoveAt(handIndex);
        playerBoard.Add(card);

        playerCardsPlayedThisTurn++;

        Debug.Log("Jugaste: " + card.cardData.cardName);

        ActivateRunes(
            card,
            RuneTrigger.OnReveal,
            true
        );

        ShowScores();
        ShowPlayerHand();
        NotifyStateChanged();
    }

    private void EnemyPlayTurn()
    {
        // IA sencilla: elige entre cartas que puede pagar.
        // Puede reservar el turno si tiene Soledad activa.
        bool hasSolitude = HasActiveSolitude(enemyBoard);

        if (hasSolitude && UnityEngine.Random.value < 0.35f)
        {
            Debug.Log("El rival decide pasar para potenciar sus cartas.");
            return;
        }

        int safety = 0;

        while (enemyHand.Count > 0 &&
               enemyBoard.Count < maxBoardCards &&
               safety < 20)
        {
            safety++;

            List<CardInstance> playable =
                enemyHand.FindAll(
                    card => card.EnergyCost <= enemyEnergy
                );

            if (playable.Count == 0)
                break;

            CardInstance selected =
                playable[UnityEngine.Random.Range(0, playable.Count)];

            enemyEnergy -= selected.EnergyCost;

            enemyHand.Remove(selected);
            enemyBoard.Add(selected);

            enemyCardsPlayedThisTurn++;

            Debug.Log(
                "Rival juega: " + selected.cardData.cardName
            );

            ActivateRunes(
                selected,
                RuneTrigger.OnReveal,
                false
            );
        }
    }

    private bool HasActiveSolitude(List<CardInstance> board)
    {
        foreach (CardInstance card in board)
        {
            foreach (RuneDataSO rune in card.equippedRunes)
            {
                if (rune != null &&
                    rune.effect == RuneEffect.DoublePowerIfNoCardsPlayed)
                    return true;
            }
        }

        return false;
    }

    private void ActivateRunes(
        CardInstance card,
        RuneTrigger trigger,
        bool isPlayer)
    {
        // Copia para evitar problemas si un efecto modifica listas.
        List<RuneDataSO> runes =
            new List<RuneDataSO>(card.equippedRunes);

        foreach (RuneDataSO rune in runes)
        {
            if (rune == null || rune.trigger != trigger)
                continue;

            switch (rune.effect)
            {
                case RuneEffect.DestroyStrongestEnemy:
                    DestroyStrongestEnemy(isPlayer);
                    break;

                case RuneEffect.GainPowerIfTwoCardsPlayed:
                    int cardsPlayed = isPlayer
                        ? playerCardsPlayedThisTurn
                        : enemyCardsPlayedThisTurn;

                    if (cardsPlayed >= 2)
                    {
                        card.AddPower(rune.effectValue);

                        Debug.Log(
                            card.cardData.cardName +
                            " gana +" + rune.effectValue + " poder."
                        );
                    }
                    break;

                case RuneEffect.DoublePowerIfNoCardsPlayed:
                    int played = isPlayer
                        ? playerCardsPlayedThisTurn
                        : enemyCardsPlayedThisTurn;

                    if (played == 0)
                    {
                        card.MultiplyPower(rune.effectValue);

                        Debug.Log(
                            card.cardData.cardName +
                            " duplica su poder: " + card.currentPower
                        );
                    }
                    break;

                case RuneEffect.DrawCard:
                    if (isPlayer)
                        DrawCard(playerDeck, playerHand);
                    else
                        DrawCard(enemyDeck, enemyHand);

                    Debug.Log(
                        (isPlayer ? "Jugador" : "Rival") +
                        " roba una carta por Inspiracion."
                    );
                    break;
            }
        }
    }

    private void DestroyStrongestEnemy(bool isPlayer)
    {
        List<CardInstance> targetBoard =
            isPlayer ? enemyBoard : playerBoard;

        if (targetBoard.Count == 0)
            return;

        CardInstance strongest = targetBoard[0];

        foreach (CardInstance card in targetBoard)
        {
            if (card.currentPower > strongest.currentPower)
                strongest = card;
        }

        targetBoard.Remove(strongest);

        Debug.Log(
            "Ejecucion destruye: " +
            strongest.cardData.cardName
        );
    }

    private void ResolveEndOfTurnRunes()
    {
        // Instantanea del tablero: las cartas deben seguir
        // en juego para activar sus efectos.
        List<CardInstance> playerSnapshot =
            new List<CardInstance>(playerBoard);

        List<CardInstance> enemySnapshot =
            new List<CardInstance>(enemyBoard);

        foreach (CardInstance card in playerSnapshot)
        {
            if (playerBoard.Contains(card))
                ActivateRunes(card, RuneTrigger.EndOfTurn, true);
        }

        foreach (CardInstance card in enemySnapshot)
        {
            if (enemyBoard.Contains(card))
                ActivateRunes(card, RuneTrigger.EndOfTurn, false);
        }
    }

    public void EndTurn()
    {
        if (!gameStarted || gameFinished)
            return;

        EnemyPlayTurn();

        ResolveEndOfTurnRunes();

        ShowScores();

        if (currentTurn >= maxTurns)
        {
            EndGame();
            return;
        }

        StartNextTurn();
    }

    private int CalculatePower(List<CardInstance> board)
    {
        int total = 0;

        foreach (CardInstance card in board)
            total += card.currentPower;

        return total;
    }

    private void ShowScores()
    {
        Debug.Log(
            "PODER | Jugador: " + PlayerPower +
            " | Rival: " + EnemyPower
        );
    }

    private void EndGame()
    {
        if (gameFinished)
            return;

        gameFinished = true;

        Debug.Log("===== FIN DE PARTIDA =====");

        ShowScores();

        if (PlayerPower > EnemyPower)
        {
            coins += winReward;
            Debug.Log("GANASTE | +" + winReward + " monedas");
        }
        else if (PlayerPower < EnemyPower)
        {
            coins += loseReward;
            Debug.Log("PERDISTE | +" + loseReward + " monedas");
        }
        else
        {
            coins += drawReward;
            Debug.Log("EMPATE | +" + drawReward + " monedas");
        }

        Debug.Log("Monedas totales: " + coins);

        NotifyStateChanged();
    }

    private void ShowPlayerHand()
    {
        Debug.Log("===== TU MANO =====");

        for (int i = 0; i < playerHand.Count; i++)
        {
            CardInstance card = playerHand[i];

            Debug.Log(
                (i + 1) + ": " +
                card.cardData.cardName +
                " | Coste: " + card.EnergyCost +
                " | Poder: " + card.currentPower
            );
        }
    }

    private void NotifyStateChanged()
    {
        OnGameStateChanged?.Invoke();
    }

    private void Update()
    {
        if (!keyboardTesting ||
            !gameStarted ||
            gameFinished ||
            Keyboard.current == null)
            return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            PlayCard(0);
        else if (Keyboard.current.digit2Key.wasPressedThisFrame)
            PlayCard(1);
        else if (Keyboard.current.digit3Key.wasPressedThisFrame)
            PlayCard(2);
        else if (Keyboard.current.digit4Key.wasPressedThisFrame)
            PlayCard(3);
        else if (Keyboard.current.digit5Key.wasPressedThisFrame)
            PlayCard(4);
        else if (Keyboard.current.digit6Key.wasPressedThisFrame)
            PlayCard(5);
        else if (Keyboard.current.digit7Key.wasPressedThisFrame)
            PlayCard(6);
        else if (Keyboard.current.digit8Key.wasPressedThisFrame)
            PlayCard(7);
        else if (Keyboard.current.digit9Key.wasPressedThisFrame)
            PlayCard(8);
        else if (Keyboard.current.digit0Key.wasPressedThisFrame)
            PlayCard(9);

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            EndTurn();
    }
}
