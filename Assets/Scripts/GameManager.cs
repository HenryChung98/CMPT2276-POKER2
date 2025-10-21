using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        PreFlop,
        Flop,
        Turn,
        River,
        Showdown
    }

    [Header("Card Setup")]
    public GameObject cardPrefab;
    public Sprite[] cardSprites;
    public Sprite cardBackSprite;

    [Header("Card Holders")]
    public Transform playerCardsHolder;
    public Transform opponentCardsHolder;
    public Transform communityCardsHolder;

    [Header("UI")]
    public UIManager uiManager;

    // player objects
    private Player player;
    private Player opponent;
    private List<Player> players;
    private int dealerIndex = 0;

    // public cards
    private readonly List<CardData> communityCardList = new();

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
        
        players = new List<Player> { player, opponent }; // to define a dealer
    }

    void Start()
    {
        StartNewRound();
    }
    private void StartNewRound()
    {
        deckManager.Shuffle();
        DealHoleCards();
        PostBlind();
        currentState = GameState.PreFlop;
        UpdateMoneyUI();
    }

    private void UpdateMoneyUI()
    {
        uiManager.UpdateMoneyUI(bettingManager.Pot, player.Chips, opponent.Chips);
    }


    // ---------------- Buttons you can hook in the Inspector ----------------
    public void ResetButton()
    {
        ClearAllCardHolders();
        player.Chips = 1000;
        opponent.Chips = 1000;
        bettingManager.ResetPot();
        StartNewRound();
    }

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

    public void PlayerCallButton()
    {
        if (player.HasActed != true)
        {
            int amount = opponent.BetThisRound - player.BetThisRound > 0 ? opponent.BetThisRound - player.BetThisRound : 0;
            Call(player, amount);
        }
        else
        {
            Debug.Log("player has acted.");
        }
    }

    public void OpponentCallButton()
    {
        if (opponent.HasActed != true)
        {
            int amount = player.BetThisRound - opponent.BetThisRound > 0 ? player.BetThisRound - opponent.BetThisRound : 0;
            Call(opponent, amount);
        }
        else
        {
            Debug.Log("opponent has acted.");
        }
    }

    // ----------------------------------------------------------------------

    // ============================= flow logics =============================

    // move to next phase. currently hooked to a button and executed when clicked
    public void NextPhase()
    {
        if (!AllPlayersActed())
        {
            Debug.Log("All players must act before move");
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
        ResetRoundBetting();
    }

    private bool AllPlayersActed()
    {
        foreach (var p in players)
        {
            if (!p.HasActed)
            {
                return false;
            }
        }
        return true;
    }
    private void ResetRoundBetting()
    {
        foreach (var p in players)
        {
            p.ResetStatus();
        }
    }
    // ============================= /flow logics =============================

    void ClearAllCardHolders()
    {
        // cards in holders go back to deck (copy the list elements from hole to deck)
        deckManager.ReturnCards(player.HoleCards);
        deckManager.ReturnCards(opponent.HoleCards);
        deckManager.ReturnCards(communityCardList);

        // destroying game objects (UI)
        uiManager.ClearCardHolder(playerCardsHolder);
        uiManager.ClearCardHolder(opponentCardsHolder);
        uiManager.ClearCardHolder(communityCardsHolder);

        // clearing the lists
        player.HoleCards.Clear();
        opponent.HoleCards.Clear();
        communityCardList.Clear();
    }


    public void Call(Player player, int amount)
    {
        bettingManager.Call(player, amount);
        UpdateMoneyUI();
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
    private void PostBlind()
    {
        bettingManager.PostBlind(players, dealerIndex);
        dealerIndex = (dealerIndex + 1) % players.Count;
    }

    void DealCommunityCards(int num)
    {
        if (communityCardList.Count >= 5)
        {
            Debug.Log("Community card can have max 5 cards");
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
        //uiManager.RevealCards(opponentCardsHolder);

        var playerResult = PokerHandEvaluator.EvaluateBestHand(GetAllCards(player.HoleCards));
        var opponentResult = PokerHandEvaluator.EvaluateBestHand(GetAllCards(opponent.HoleCards));

        int cmp = PokerHandEvaluator.Compare(playerResult, opponentResult);

        if (cmp > 0)
        {
            Debug.Log($"You win! {playerResult.Description} beats {opponentResult.Description}. +{bettingManager.Pot} chips.");
            bettingManager.PayoutChips(player, bettingManager.Pot);
            bettingManager.ResetPot();
        }
        else if (cmp < 0)
        {
            Debug.Log($"Opponent wins. {opponentResult.Description} beats {playerResult.Description}.");
            bettingManager.PayoutChips(opponent, bettingManager.Pot);
            bettingManager.ResetPot();
        }
        else
        {
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
