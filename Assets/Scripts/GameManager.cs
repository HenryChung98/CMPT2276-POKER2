using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Card Setup")]
    public GameObject cardPrefab;
    public Transform playerCardsHolder;
    public Transform opponentCardsHolder;
    public Transform communityCardsHolder;
    public Sprite[] cardSprites;

    private readonly List<CardData> deck = new();
    private readonly List<CardData> playerCardList = new();
    private readonly List<CardData> opponentCardList = new();
    private readonly List<CardData> communityCardList = new();

    public static GameManager Instance { get; private set; } 

    private void Awake()
    {
        // make the game manager as a singleton
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
        ShuffleDeck();
        DealHoleCards();
    }

    public void ResetButton()
    {
        ClearAllCardHolders();
        ShuffleDeck();
        DealHoleCards();
    }

    public void TestButton()
    {
        DealCommunityCards(1);
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
        deck.AddRange(playerCardList);
        deck.AddRange(opponentCardList);
        deck.AddRange(communityCardList);

        // destroying game objects
        for (int i = playerCardsHolder.childCount - 1; i >= 0; i--)
        {
            Destroy(playerCardsHolder.GetChild(i).gameObject);
        }

        for (int i = opponentCardsHolder.childCount - 1; i >= 0; i--)
        {
            Destroy(opponentCardsHolder.GetChild(i).gameObject);
        }

        for (int i = communityCardsHolder.childCount - 1; i >= 0; i--)
        {
            Destroy(communityCardsHolder.GetChild(i).gameObject);
        }

        // clearing the lists
        playerCardList.Clear();
        opponentCardList.Clear();
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
    void DisplayCard(CardData cardData, Transform holder)
    {
        GameObject cardObject = Instantiate(cardPrefab, holder);
        //CardUI cardComponent = cardObject.GetComponent<CardUI>();
        //if (cardComponent != null)
        //{
        //    cardComponent.Setup(cardData);
        //}
        cardObject.GetComponent<CardUI>().Setup(cardData);
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
            // Player
            CardData playerCard = DrawCard();
            playerCardList.Add(playerCard);
            DisplayCard(playerCard, playerCardsHolder);

            // Opponent
            CardData opponentCard = DrawCard();
            opponentCardList.Add(opponentCard);
            DisplayCard(opponentCard, opponentCardsHolder);
        }
    }

    // deal 3, 1, 1
    void DealCommunityCards(int num)
    {
        if (communityCardList.Count < 5)
        {
            for (int i = 0; i < num; i++)
            {
                CardData card = DrawCard();
                communityCardList.Add(card);
                DisplayCard(card, communityCardsHolder);
            }
        }
        else
        {
            Debug.Log("Community card can have max 5 cards");
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

}