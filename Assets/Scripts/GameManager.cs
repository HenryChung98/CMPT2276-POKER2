using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{


    [Header("Card Setup")]
    public GameObject cardPrefab;
    public Transform playerCardsHolder;
    public Transform opponentCardsHolder;
    public Transform communityCardsHolder;
    public Sprite[] cardSprites;
    public Sprite cardBackSprite; // NEW: for hiding opponent cards

    [Header("UI")]
    public TextMeshProUGUI statusText;   // drag a TMP Text here
    public TextMeshProUGUI potText;      // drag a TMP Text here
    public TextMeshProUGUI bankText;     // drag a TMP Text here (player bank)
    public TextMeshProUGUI oppBankText;  // drag a TMP Text here (opponent bank)

    // player objects
    private Player player;
    private Player opponent;
    private int pot = 0;

    // public cards
    private readonly List<CardData> deck = new();
    private readonly List<CardData> communityCardList = new();

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
        pot = 0;
        UpdateMoneyUI();
        SetStatus("Reset. New hand.");
    }

    public void TestButton() { DealCommunityCards(1); } // existing
    public void DealFlopButton() { DealCommunityCards(3); }
    public void DealTurnButton() { DealCommunityCards(1); }
    public void DealRiverButton() { DealCommunityCards(1); }
    public void Bet10Button() { PlaceBet(10); }
    public void Bet50Button() { PlaceBet(50); }
    public void Bet100Button() { PlaceBet(100); }

    // Requested ¡§3 buttons¡¨: quick evaluators + showdown
    public void CheckHighCard() { PrintIfRank(HandRank.HighCard); }
    public void CheckOnePair() { PrintIfRank(HandRank.OnePair); }
    public void ShowdownButton() { Showdown(); }
    // ----------------------------------------------------------------------


    // ============================== update UIs ==============================
    private void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
        Debug.Log(msg);
    }

    private void UpdateMoneyUI()
    {
        if (potText != null) potText.text = $"Pot: {pot}";
        if (bankText != null) bankText.text = $"You: {player.Chips}";
        if (oppBankText != null) oppBankText.text = $"Opponent: {opponent.Chips}";
    }
    // ============================== update UIs ==============================

    public void PlaceBet(int amount)
    {
        if (player.Chips < amount)
        {
            SetStatus("Not enough chips.");
            return;
        }
        // player bets, opponent auto-calls (toy logic)
        player.Chips -= amount;
        pot += amount;

        int call = Mathf.Min(opponent.Chips, amount);
        opponent.Chips -= call;
        pot += call;

        UpdateMoneyUI();
        SetStatus($"You bet {amount}. Opponent calls {call}. Pot is now {pot}.");
    }

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

    // this will execute every time when reset
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
            // Player (face up)
            CardData playerCard = DrawCard();
            player.HoleCards.Add(playerCard);
            DisplayCard(playerCard, playerCardsHolder);

            // Opponent (HIDDEN until showdown)
            CardData opponentCard = DrawCard();
            opponent.HoleCards.Add(opponentCard);
            DisplayCard(opponentCard, opponentCardsHolder, true);
        }
    }

    // deal 3, 1, 1
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

    public List<CardData> GetAllCards(List<CardData> holeCards)
    {
        List<CardData> allCards = new();
        allCards.AddRange(holeCards);
        allCards.AddRange(communityCardList);
        return allCards;
    }

    private void RevealOpponentCards()
    {
        for (int i = 0; i < opponentCardsHolder.childCount; i++)
        {
            var ui = opponentCardsHolder.GetChild(i).GetComponent<CardUI>();
            ui.SetFaceDown(false);
        }
    }

    private void PrintIfRank(HandRank target)
    {
        var res = PokerHandEvaluator.EvaluateBestHand(GetAllCards(player.HoleCards));
        if (res.Rank == target)
            SetStatus(res.Description); // e.g., "One Pair (Aces) with kickers K, Q, 9"
        else
            SetStatus($"You currently have: {res.Description}");
    }

    private void PayoutToPlayer(int amount)
    {
        player.Chips += amount;
        UpdateMoneyUI();
    }

    private void PayoutToOpponent(int amount)
    {
        opponent.Chips += amount;
        UpdateMoneyUI();
    }

    public void Showdown()
    {
        // Must have 5 community cards to showdown; but still allow with fewer for testing.
        RevealOpponentCards();

        var playerRes = PokerHandEvaluator.EvaluateBestHand(GetAllCards(player.HoleCards));
        var opponentRes = PokerHandEvaluator.EvaluateBestHand(GetAllCards(opponent.HoleCards));

        int cmp = PokerHandEvaluator.Compare(playerRes, opponentRes);

        if (cmp > 0)
        {
            SetStatus($"You win! {playerRes.Description} beats {opponentRes.Description}. +{pot} chips.");
            PayoutToPlayer(pot);
            pot = 0;
        }
        else if (cmp < 0)
        {
            SetStatus($"Opponent wins. {opponentRes.Description} beats {playerRes.Description}.");
            PayoutToOpponent(pot);
            pot = 0;
        }
        else
        {
            SetStatus($"Tie: {playerRes.Description} vs {opponentRes.Description}. Pot split.");
            int split = pot / 2;
            PayoutToPlayer(split);
            PayoutToOpponent(pot - split);
            pot = 0;
        }

        UpdateMoneyUI();
    }
}
