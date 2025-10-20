using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
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
    private readonly List<CardData> deck = new();
    private readonly List<CardData> communityCardList = new();

    // managers
    private BettingManager bettingManager;
    private DeckManager deckManager;

    public static GameManager Instance { get; private set; }

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
        deckManager.Shuffle();
        DealHoleCards();
        PostBlind();
        UpdateMoneyUI();
    }

    // ============================= rendering UIs =============================
    private void UpdateMoneyUI()
    {
        uiManager.UpdateMoneyUI(bettingManager.Pot, player.Chips, opponent.Chips);
    }


    // ---------------- Buttons you can hook in the Inspector ----------------
    public void ResetButton()
    {
        ClearAllCardHolders();
        deckManager.Shuffle();
        DealHoleCards();

        player.ResetStatus();
        opponent.ResetStatus();
        bettingManager.ResetPot();

        PostBlind();
        player.Chips = 1000;
        opponent.Chips = 1000;

        UpdateMoneyUI();

    }

    public void TestDealCommunityCard() { DealCommunityCards(1); }
    public void CheckHand()
    {
        var res = PokerHandEvaluator.EvaluateBestHand(GetAllCards(player.HoleCards));
        Debug.Log($"You currently have: {res.Description}");
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


    // ============================= pre-flop =============================
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
    // ============================= /pre-flop =============================



    // ============================= flop =============================
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
    // ============================= /flop =============================


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
