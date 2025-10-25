using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEditor.Experimental.GraphView;

public class GameManager : MonoBehaviour
{
    [Header("Card Holders")]
    public Transform playerCardsHolder;
    public Transform opponentCardsHolder;
    public Transform communityCardsHolder;

    [Header("UI")]
    public Sprite[] cardSprites;
    public UIManager uiManager;
    public TextMeshProUGUI stateText;


    // player objects
    private Player player;
    private Player opponent;
    private List<Player> players;
    private int dealerIndex = 0;
    private int bettorIndex = 0;

    // public cards
    private readonly List<CardData> communityCardList = new();
    private readonly List<CardData> foldedCards = new();

    // managers
    private BettingManager bettingManager;
    private DeckManager deckManager;

    public static GameManager Instance { get; private set; }
    private GameState currentState = GameState.PreFlop;

    private void Awake()
    {
        // singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // initialize managers
        deckManager = new DeckManager(cardSprites);
        bettingManager = new BettingManager();

        // initialize objects
        deckManager.CreateDeck();
        player = new Player("player", 1000, playerCardsHolder, true);
        opponent = new Player("opponent", 1000, opponentCardsHolder, false);
        
        players = new List<Player> { player, opponent }; // to define a dealer, bettor
    }

    void Start()
    {
        StartNewRound();
    }
    private void StartNewRound()
    {
        deckManager.Shuffle();
        DealHoleCards();
        ResetAllPlayerStatus(true); // if the argument is true, reset player.HasAllIn as well
        bettingManager.PostBlind(players, dealerIndex);
        bettorIndex = (dealerIndex + 2) % players.Count;
        currentState = GameState.PreFlop;
        UpdateMoneyUI();
        UpdateButtonStates();
    }


    // ============================ These are fur debugging. we won't need these in future ============================
    public void CheckPlayerHand()
    {
        var res = PokerHandEvaluator.EvaluateBestHand(GetAllCards(player.HoleCards));
        Debug.Log($"You currently have: {res.Description}");
    }
    public void CheckOpponentHand()
    {
        var res = PokerHandEvaluator.EvaluateBestHand(GetAllCards(opponent.HoleCards));
        Debug.Log($"opponent currently have: {res.Description}");
    }
    private void UpdateStateMessage(string msg)
    {
        stateText.text = msg;
    }
    // ================================================================================================================

    // ============================= Update UIs =============================
    private void UpdateMoneyUI()
    {
        uiManager.UpdateMoneyUI(bettingManager.Pot, player.Chips, opponent.Chips);
    }


    // need to modify in future when we add more than 2 ai
    private void UpdateButtonStates()
    {
        uiManager.UpdateButtonStates(bettorIndex, players);
    }
    // ============================= /Update UIs =============================


    // ============================= Buttons you can hook in the Inspector =============================
    public void RestartButton()
    {
        dealerIndex = (dealerIndex + 1) % players.Count;
        ClearAllCardHolders();
        bettingManager.ResetPot();
        UpdateStateMessage("");
        StartNewRound();
    }

    public void PlayerCallButton() => HandleCall(player, opponent);
    public void OpponentCallButton() => HandleCall(opponent, player);
    public void PlayerRaiseButton() => HandleRaise(player, opponent);
    public void OpponentRaiseButton() => HandleRaise(opponent, player);
    public void PlayerFoldButton() => HandleFold(player, playerCardsHolder);
    public void OpponentFoldButton() => HandleFold(opponent, opponentCardsHolder);

    // ============================= /buttons =============================


    // ============================= button functions =============================
    private void HandleCall(Player caller, Player other)
    {
        if (IsPlayerTurn(caller) && !caller.HasFolded)
        {
            int amount = Mathf.Max(0, other.BetThisRound - caller.BetThisRound);
            bettingManager.Call(caller, amount);
            AdvanceTurn();
        }
        else
        {
            Debug.Log("ERROR");
        }
    }

    private void HandleRaise(Player raiser, Player other)
    {
        if (IsPlayerTurn(raiser) && !raiser.HasFolded)
        {
            int amount = other.BetThisRound + Mathf.Max(bettingManager.bigBlind, other.BetThisRound);
            bettingManager.Raise(players, raiser, amount);
            AdvanceTurn();
        }
        else
        {
            Debug.Log("ERROR");
        }
    }

    private void HandleFold(Player folder, Transform cardsHolder)
    {
        foldedCards.AddRange(folder.HoleCards);
        uiManager.ClearCardHolder(cardsHolder);
        folder.HoleCards.Clear();
        folder.HasFolded = true;
        Debug.Log($"{folder.Name} has folded.");

        // check only one player remains / need to be optimized
        Player winner = GetSoleRemainingPlayer();
        if (winner != null)
        {
            uiManager.UpdateButtonStates(-1, players);
            bettingManager.PayoutChips(winner, bettingManager.Pot);
            bettingManager.ResetPot();
            UpdateMoneyUI();
            uiManager.restartButton.interactable = true;
            currentState = GameState.Showdown;
            Debug.Log("game ended by fold");
            return;
        }
    }

    // ============================= /button functions =============================

    // ============================= flow logics =============================

    public void NextPhase()
    {
        if (!AllPlayersActed())
        {
            return;
        }

        switch (currentState)
        {
            case GameState.PreFlop:
                DealCommunityCards(3);
                currentState = GameState.Flop;
                Debug.Log("flop phase");
                break;
            case GameState.Flop:
                DealCommunityCards(1);
                currentState = GameState.Turn;
                Debug.Log("turn phase");
                break;
            case GameState.Turn:
                DealCommunityCards(1);
                currentState = GameState.River;
                Debug.Log("river phase");
                break;
            case GameState.River:
                Showdown();
                currentState = GameState.Showdown;
                Debug.Log("showdown phase");
                return;
        }
        ResetAllPlayerStatus();
    }

    // change bettor to the next player
    private void NextBettor()
    {
        do
        {
            bettorIndex = (bettorIndex + 1) % players.Count;
        } while (players[bettorIndex].HasFolded);
    }
    
    // check whether it is player's turn
    private bool IsPlayerTurn(Player player)
    {
        return players[bettorIndex] == player && !player.HasFolded;
    }

    // check all player.HasActed is true
    private bool AllPlayersActed()
    {
        foreach (var p in players)
        {
            if (!p.HasActed)
            {
                if (!p.HasFolded)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private Player GetSoleRemainingPlayer()
    {
        // when only one player.HasFolded is false, return the player. else, return null
        var activePlayers = players.Where(p => !p.HasFolded).ToList();
        return activePlayers.Count == 1 ? activePlayers[0] : null;
    }

    // set all player.HasActed = false / player.BetThisRound = 0
    private void ResetAllPlayerStatus(bool resetGame = false)
    {
        foreach (var p in players)
        {
            p.ResetStatus(resetGame);
        }
    }

    private void AdvanceTurn()
    {
        NextBettor();
        UpdateMoneyUI();
        UpdateButtonStates();
        NextPhase();
    }
    // ============================= /flow logics =============================

    void ClearAllCardHolders()
    {
        // cards in holders go back to deck (copy the list elements from hole to deck)
        deckManager.ReturnCards(player.HoleCards);
        deckManager.ReturnCards(opponent.HoleCards);
        deckManager.ReturnCards(communityCardList);
        deckManager.ReturnCards(foldedCards);

        // destroying game objects (UI)
        uiManager.ClearCardHolder(playerCardsHolder);
        uiManager.ClearCardHolder(opponentCardsHolder);
        uiManager.ClearCardHolder(communityCardsHolder);

        // clearing the lists
        player.HoleCards.Clear();
        opponent.HoleCards.Clear();
        communityCardList.Clear();
    }

    void DealHoleCards()
    {
        for (int i = 0; i < 2; i++)
        {
            // player
            CardData playerCard = deckManager.DrawCard();
            player.HoleCards.Add(playerCard);
            uiManager.DisplayCard(playerCard, playerCardsHolder);

            // opponent
            CardData opponentCard = deckManager.DrawCard();
            opponent.HoleCards.Add(opponentCard);
            uiManager.DisplayCard(opponentCard, opponentCardsHolder);
        }
    }

    void DealCommunityCards(int num)
    {
        if (communityCardList.Count >= 5)
        {
            return;
        }

        for (int i = 0; i < num && communityCardList.Count < 5; i++)
        {
            CardData card = deckManager.DrawCard();
            communityCardList.Add(card);
            uiManager.DisplayCard(card, communityCardsHolder);
        }
    }


    // ============================= showdown =============================

    public List<CardData> GetAllCards(List<CardData> holeCards)
    {
        List<CardData> allCards = new();
        allCards.AddRange(holeCards);
        allCards.AddRange(communityCardList);
        return allCards;
    }

    public void Showdown()
    {
        uiManager.UpdateButtonStates(-1, players);
        uiManager.RevealCards(opponentCardsHolder);

        var playerResult = PokerHandEvaluator.EvaluateBestHand(GetAllCards(player.HoleCards));
        var opponentResult = PokerHandEvaluator.EvaluateBestHand(GetAllCards(opponent.HoleCards));

        int cmp = PokerHandEvaluator.Compare(playerResult, opponentResult);

        if (cmp > 0)
        {
            UpdateStateMessage("You win");
            Debug.Log($"You win! {playerResult.Description} beats {opponentResult.Description}. +{bettingManager.Pot} chips.");
            bettingManager.PayoutChips(player, bettingManager.Pot);
            bettingManager.ResetPot();
        }
        else if (cmp < 0)
        {
            UpdateStateMessage("Opponent win");
            Debug.Log($"Opponent wins. {opponentResult.Description} beats {playerResult.Description}.");
            bettingManager.PayoutChips(opponent, bettingManager.Pot);
            bettingManager.ResetPot();
        }
        else
        {
            UpdateStateMessage("Tie");
            Debug.Log($"Tie: {playerResult.Description} vs {opponentResult.Description}. Pot split.");
            int split = bettingManager.Pot / 2;
            bettingManager.PayoutChips(player, split);
            bettingManager.PayoutChips(opponent, bettingManager.Pot - split);
            bettingManager.ResetPot();
        }

        UpdateMoneyUI();
    }
    // ============================= showdown =============================
}
