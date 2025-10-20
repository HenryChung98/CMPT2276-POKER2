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
    public TextMeshProUGUI potText;      
    public TextMeshProUGUI playerBankText;     
    public TextMeshProUGUI oppBankText; 
    // player objects
    private Player player;
    private Player opponent;
    private int pot = 0;

    // public cards
    private readonly List<CardData> deck = new();
    private readonly List<CardData> communityCardList = new();

    // blinds
    private readonly int smallBlind = 5;
    private readonly int bigBlind = 10;

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

        CreateDeck();
    }

    void Start()
    {
        player = new Player(1000, playerCardsHolder, true);
        opponent = new Player(1000, opponentCardsHolder, false);
        ShuffleDeck();
        DealHoleCards();
        UpdateMoneyUI();
        SetStatus("New hand. Place a bet or deal community cards.");
    }

    // ---------------- Buttons you can hook in the Inspector ----------------
    public void ResetButton()
    {
        ClearAllCardHolders();
        ShuffleDeck();
        DealHoleCards();

        player.ResetStatus();
        opponent.ResetStatus();
        
        pot = 0;
        UpdateMoneyUI();
        SetStatus("Reset. New hand.");
    }
    public void InitialCallButton() { PlaceBet(bigBlind - smallBlind); }

    public void TestDealCommunityCard() { DealCommunityCards(1); }
    public void CheckHand()
    {
        var res = PokerHandEvaluator.EvaluateBestHand(GetAllCards(player.HoleCards));
        SetStatus($"You currently have: {res.Description}");
    }

    // ----------------------------------------------------------------------


    // for debugging
    private void SetStatus(string msg)
    {
        Debug.Log(msg);
    }

    private void UpdateMoneyUI()
    {
        if (potText != null) potText.text = $"Pot: {pot}";
        if (playerBankText != null) playerBankText.text = $"You: {player.Chips}";
        if (oppBankText != null) oppBankText.text = $"Opponent: {opponent.Chips}";
    }

    // ============================== logics for handling deck or reset cards ==============================

    // this will execute only one time at the very beginning
    void CreateDeck()
    {
        int spriteIndex = 0;
        foreach (Suit suit in System.Enum.GetValues(typeof(Suit)))
        {
            foreach (Rank rank in System.Enum.GetValues(typeof(Rank)))
            {
                CardData cardData = ScriptableObject.CreateInstance<CardData>();
                cardData.suit = suit;
                cardData.rank = rank;
                cardData.cardImage = cardSprites[spriteIndex];
                deck.Add(cardData);
                spriteIndex++;
            }
        }
    }

    void ClearAllCardHolders()
    {
        // cards in holders go back to deck
        deck.AddRange(player.HoleCards);
        deck.AddRange(opponent.HoleCards);
        deck.AddRange(communityCardList);

        // destroying game objects
        for (int i = playerCardsHolder.childCount - 1; i >= 0; i--)
            Destroy(playerCardsHolder.GetChild(i).gameObject);

        for (int i = opponentCardsHolder.childCount - 1; i >= 0; i--)
            Destroy(opponentCardsHolder.GetChild(i).gameObject);

        for (int i = communityCardsHolder.childCount - 1; i >= 0; i--)
            Destroy(communityCardsHolder.GetChild(i).gameObject);

        // clearing the lists
        player.HoleCards.Clear();
        opponent.HoleCards.Clear();
        communityCardList.Clear();
    }

    void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            CardData temp = deck[i];
            int randomIndex = Random.Range(i, deck.Count);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }
    // ============================== /logics for handling deck or reset cards ==============================

    // ============================== deal cards ==============================

    // render cards
    void DisplayCard(CardData cardData, Transform holder, bool faceDown = false)
    {
        GameObject cardObject = Instantiate(cardPrefab, holder);
        var ui = cardObject.GetComponent<CardUI>();
        ui.cardBackSprite = cardBackSprite;
        ui.Setup(cardData, faceDown);
    }

    private CardData DrawCard()
    {
        CardData card = deck[0];
        deck.RemoveAt(0);
        return card;
    }

    // pre-flop
    void DealHoleCards()
    {
        for (int i = 0; i < 2; i++)
        {
            // player
            CardData playerCard = DrawCard();
            player.HoleCards.Add(playerCard);
            DisplayCard(playerCard, playerCardsHolder);

            // opponent
            CardData opponentCard = DrawCard();
            opponent.HoleCards.Add(opponentCard);
            DisplayCard(opponentCard, opponentCardsHolder);
        }
    }

    void DealCommunityCards(int num)
    {
        if (communityCardList.Count >= 5)
        {
            SetStatus("Community card can have max 5 cards");
            return;
        }

        for (int i = 0; i < num && communityCardList.Count < 5; i++)
        {
            CardData card = DrawCard();
            communityCardList.Add(card);
            DisplayCard(card, communityCardsHolder);
        }
    }
    // ============================== /deal cards ==============================

    //private void RevealOpponentCards()
    //{
    //    for (int i = 0; i < opponentCardsHolder.childCount; i++)
    //    {
    //        var ui = opponentCardsHolder.GetChild(i).GetComponent<CardUI>();
    //        ui.SetFaceDown(false);
    //    }
    //}

    // ============================== betting logics ==============================
    private void PayoutChips(Player player, int amount)
    {
        player.Chips += amount;
        UpdateMoneyUI();
    }

    public void PlaceBet(int amount)
    {
        if (!player.CanBet(amount))
        {
            SetStatus("Not enough chips.");
            return;
        }
        // player bets, opponent auto-calls (toy logic)
        int playerBet = player.Bet(amount);
        pot += playerBet;

        int opponentBet = opponent.Bet(amount);
        pot += opponentBet;

        UpdateMoneyUI();
        SetStatus($"You bet {playerBet}. Opponent calls {opponentBet}. Pot is now {pot}.");
    }

    // ============================== /betting logics ==============================


    public List<CardData> GetAllCards(List<CardData> holeCards)
    {
        List<CardData> allCards = new();
        allCards.AddRange(holeCards);
        allCards.AddRange(communityCardList);
        return allCards;
    }

    public void Showdown()
    {
        //RevealOpponentCards();

        var playerResult = PokerHandEvaluator.EvaluateBestHand(GetAllCards(player.HoleCards));
        var opponentResult = PokerHandEvaluator.EvaluateBestHand(GetAllCards(opponent.HoleCards));

        int cmp = PokerHandEvaluator.Compare(playerResult, opponentResult);

        if (cmp > 0)
        {
            SetStatus($"You win! {playerResult.Description} beats {opponentResult.Description}. +{pot} chips.");
            PayoutChips(player, pot);
            pot = 0;
        }
        else if (cmp < 0)
        {
            SetStatus($"Opponent wins. {opponentResult.Description} beats {playerResult.Description}.");
            PayoutChips(opponent, pot);
            pot = 0;
        }
        else
        {
            SetStatus($"Tie: {playerResult.Description} vs {opponentResult.Description}. Pot split.");
            int split = pot / 2;
            PayoutChips(player, split);
            PayoutChips(opponent, pot - split);
            pot = 0;
        }

        UpdateMoneyUI();
    }
}
