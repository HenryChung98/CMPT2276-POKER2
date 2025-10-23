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
        ResetAllPlayerStatus();
        PostBlind();
        currentState = GameState.PreFlop;
        UpdateMoneyUI();
    }

    private void UpdateMoneyUI()
    {
        uiManager.UpdateMoneyUI(bettingManager.Pot, player.Chips, opponent.Chips);
    }
    private void UpdateStateMessage(string msg) // this is just displaying debugging message on the scene. we won't need in the future
    {
        stateText.text = msg;
    }


    // ---------------- Buttons you can hook in the Inspector ----------------
    public void ResetButton()
    {
        ClearAllCardHolders();
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
            bettingManager.Call(player, amount);
            UpdateMoneyUI();
            UpdateStateMessage("Player called");
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
            bettingManager.Call(opponent, amount);
            UpdateMoneyUI();
            UpdateStateMessage("Opponent called");
        }
        else
        {
            Debug.Log("opponent has acted.");
        }
    }

    public void PlayerRaiseButton()
    {
        if (player.HasActed != true)
        {
            int amount = opponent.BetThisRound * 2 > 0 ? opponent.BetThisRound * 2 : 5;
            bettingManager.Raise(players, player, amount);
            UpdateStateMessage($"Player raised {amount}");
            UpdateMoneyUI();
        }
        else
        {
            Debug.Log("player has acted.");
        }
    }

    public void OpponentRaiseButton()
    {
        if (opponent.HasActed != true)
        {
            int amount = player.BetThisRound * 2 > 0 ? player.BetThisRound * 2 : 5;
            bettingManager.Raise(players, opponent, amount);
            UpdateStateMessage($"opponent raised {amount}");
            UpdateMoneyUI();
        }
        else
        {
            Debug.Log("opponent has acted.");
        }
    }

    public void PlayerFoldButton()
    {
        deckManager.ReturnCards(player.HoleCards); // this will be modified later
        uiManager.ClearCardHolder(playerCardsHolder);
        player.HoleCards.Clear();

        player.HasActed = true;
        Debug.Log("Player has folded.");

        if (player.HasActed && opponent.HasActed)
        {
            ResetAllPlayerStatus();
        }
    }

    public void OpponentFoldButton()
    {
        deckManager.ReturnCards(opponent.HoleCards); // this will be modified later
        uiManager.ClearCardHolder(opponentCardsHolder);
        opponent.HoleCards.Clear();

        opponent.HasActed = true;
        Debug.Log("Opponent has folded.");
        
        if (opponent.HasActed && player.HasActed)
        {
            ResetAllPlayerStatus();
        }
    }
    
    // ----------------------------------------------------------------------

    // ============================= flow logics =============================

    // move to next phase. currently hooked to a button and executed when clicked
    public void NextPhase()
    {
        if (!AllPlayersActed())
        {
            UpdateStateMessage("All players must act before move");
            Debug.Log("All players must act before move");
            return;
        }

        switch (currentState)
        {
            case GameState.PreFlop:
                DealCommunityCards(3);
                currentState = GameState.Flop;
                UpdateStateMessage("flop phase");
                Debug.Log("flop phase");
                break;
            case GameState.Flop:
                DealCommunityCards(1);
                currentState = GameState.Turn;
                UpdateStateMessage("turn phase");
                Debug.Log("turn phase");
                break;
            case GameState.Turn:
                DealCommunityCards(1);
                currentState = GameState.River;
                UpdateStateMessage("river phase");
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
    private void ResetAllPlayerStatus()
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
