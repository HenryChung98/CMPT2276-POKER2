using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEditor.Experimental.GraphView;
using System.Collections;
using static UnityEditor.U2D.ScriptablePacker;

public class GameManager : MonoBehaviour
{
    [Header("Card Holders")]
    public Transform playerCardsHolder;
    public Transform opponentCardsHolder;
    public Transform communityCardsHolder;

    [Header("UI")]
    public Sprite[] cardSprites;
    public UIManager uiManager;
    public GuidebookUI guidebookUI;
    public TextMeshProUGUI callButtonText;
    public TextMeshProUGUI callAmountText;
    public TextMeshProUGUI raiseAmountText;


    // --- Auto Play (opponent only) ---
    [Header("Auto Play")]
    public bool autoPlay = true;                 // current state
    public float delay = 0.5f;                // delay between auto decisions
    public TextMeshProUGUI autoPlayButtonText;    // drag the Button's TMP text here


    // player objects
    private Player player;
    private Player opponent;
    private List<Player> players;


    // public cards
    private readonly List<CardData> communityCardList = new();
    private readonly List<CardData> foldedCards = new();

    // managers
    private BettingManager bettingManager;
    private DeckManager deckManager;
    private CardDealer cardDealer;
    public GameFlowManager gameFlowManager;

    // audios
    public AudioSource audioSource;
    public AudioClip callSound;
    public AudioClip raiseSound;
    public AudioClip foldSound;
    public AudioClip buttonSound;
    public AudioClip cardSound;

    // etc
    public bool isAnimating = false;
    public static GameManager Instance { get; private set; }

    // ============================ These are for debugging. we won't need these in the end ============================
    private void UpdateAutoPlayUI()
    {
        if (autoPlayButtonText == null) return;

        autoPlayButtonText.text = autoPlay ? "Stop Auto: ON" : "Auto Play: OFF";
    }

    public void ToggleAutoPlayButton()
    {
        autoPlay = !autoPlay;
        Invoke(nameof(AIBehavior), delay);
        autoPlayButtonText.text = autoPlay ? "Auto: ON" : "Auto: OFF";
    }

    public void CheckPlayerHand()
    {
        var res = PokerHandEvaluator.EvaluateBestHand(GetAllCards(player.HoleCards));
        Debug.Log($"You currently have: {res.Description}");

        UpdateGuidebook();
    }
    public void CheckOpponentHand()
    {
        var res = PokerHandEvaluator.EvaluateBestHand(GetAllCards(opponent.HoleCards));
        Debug.Log($"opponent currently have: {res.Description}");
    }

    public void OpponentCallButton() => HandleCall(opponent, player);
    public void OpponentRaiseButton() => HandleRaise(opponent, player);
    public void OpponentFoldButton() => HandleFold(opponent, opponentCardsHolder);

    // ================================================================================================================


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
        
        // initialize dealer
        cardDealer = gameObject.AddComponent<CardDealer>(); 
        cardDealer.Initialize(deckManager, uiManager, delay, cardSound);

        // initialize winning probability
        uiManager.InitializeProbabilityCalculator(deckManager);

    }

    void Start()
    {
        gameFlowManager.Initialize(this, bettingManager, deckManager, players);
        autoPlay = true;
        StartNewRound();
        UpdateAutoPlayUI();
    }

    private void AIBehavior()
    {
        if (autoPlay && gameFlowManager.IsPlayerTurn(opponent) && gameFlowManager.currentState != GameState.Showdown && !isAnimating)
        {
            bool tryRaise = !opponent.HasAllIn && !player.HasAllIn && Random.value < 0.5f;
            bool tryFold = !opponent.HasAllIn && !player.HasAllIn && Random.value < 0.2f;
            if (tryRaise) HandleRaise(opponent, player);
            else if (tryFold) HandleFold(opponent, opponentCardsHolder);
            else HandleCall(opponent, player);
        }
    }

    private void StartNewRound()
    {
        StartCoroutine(DealHoleCards());
        gameFlowManager.StartNewRound();

        // update UIs
        UpdateMoneyUI();
        UpdateGuidebook();
        UpdateButtonStates();
        uiManager.ResetWinProbability();
        Invoke(nameof(AIBehavior), delay);
    }
    // ============================= Update UIs =============================
    private void UpdateMoneyUI()
    {
        uiManager.UpdateMoneyUI(bettingManager.Pot, player.Chips, opponent.Chips);
    }


    // need to modify in future when we add more than 2 ai
    private void UpdateButtonStates()
    {
        uiManager.UpdateButtonStates(gameFlowManager.BettorIndex, players);
    }

    public List<CardData> GetPlayerAllCardsForUI()
    {
        return GetAllCards(player.HoleCards);
    }

    // Tells the guidebook to update highlight/description
    private void UpdateGuidebook()
    {
        if (guidebookUI != null)
            guidebookUI.Refresh(GetPlayerAllCardsForUI());

        // Update win probability after pre-flop 
        if (player.HoleCards.Count == 2 && gameFlowManager.currentState != GameState.PreFlop)
        {
            uiManager.UpdateWinProbability(player.HoleCards, communityCardList);
        }
    }



    // ============================= Buttons you can hook in the Inspector =============================
    public void RestartButton()
    {
        gameFlowManager.IncrementDealer();
        ClearAllCardHolders();
        audioSource.PlayOneShot(buttonSound);
        StartNewRound();
    }

    public void PlayerCallButton() => HandleCall(player, opponent);
    public void PlayerRaiseButton() => HandleRaise(player, opponent);
    public void PlayerFoldButton() => HandleFold(player, playerCardsHolder);



    // ============================= button functions =============================
    private void HandleCall(Player caller, Player other)
    {
        if (gameFlowManager.IsPlayerTurn(caller) && !caller.HasFolded)
        {
            int amount = Mathf.Max(0, other.BetThisRound - caller.BetThisRound);
            bettingManager.Call(caller, amount);
            audioSource.PlayOneShot(callSound);
            AdvanceTurn();
        }
        else
        {
            Debug.Log($"{caller} cannot call");
        }
    }

    private void HandleRaise(Player raiser, Player other)
    {
        if (gameFlowManager.IsPlayerTurn(raiser) && !raiser.HasFolded)
        {
            int amount = other.BetThisRound + Mathf.Max(bettingManager.bigBlind, other.BetThisRound);
            bettingManager.Raise(players, raiser, amount);
            audioSource.PlayOneShot(raiseSound);
            AdvanceTurn();
        }
        else
        {
            Debug.Log($"{raiser} cannot raise");
        }
    }

    private void HandleFold(Player folder, Transform cardsHolder)
    {
        // move folded cards away & clear UI
        foldedCards.AddRange(folder.HoleCards);
        uiManager.ClearCardHolder(cardsHolder);
        audioSource.PlayOneShot(foldSound);
        folder.HoleCards.Clear();
        folder.HasFolded = true;
        Debug.Log($"{folder.Name} has folded.");

        // check if only one active player is left
        Player winner = GetSoleRemainingPlayer();
        if (winner != null)
        {
            uiManager.UpdateButtonStates(-1, players);
            bettingManager.PayoutChips(winner, bettingManager.Pot);
            UpdateMoneyUI();
            gameFlowManager.currentState = GameState.Showdown;

            //New detailed end-of-round messages
            if (folder == opponent && winner == player)
            {
                // opponent folded, human wins
                gameFlowManager.resultText.text = "Opponent folded, you win";
            }
            else if (folder == player && winner == opponent)
            {
                // player folded, opponent wins
                gameFlowManager.resultText.text = "You folded, opponent wins";
            }
            else
            {
                // fallback (just in case)
                gameFlowManager.resultText.text = winner == player ? "You win" : "Opponent wins";
            }

            gameFlowManager.ShowGameOverPanel();
            return;
        }
    }

    // when only one player.HasFolded is false, return the player. else, return null
    private Player GetSoleRemainingPlayer()
    {
        var activePlayers = players.Where(p => !p.HasFolded).ToList();
        return activePlayers.Count == 1 ? activePlayers[0] : null;
    }


    private void AdvanceTurn()
    {
        gameFlowManager.AdvanceTurn();
        UpdateMoneyUI();
        UpdateButtonStates();
        Invoke(nameof(AIBehavior), delay);

        int callAmount = Mathf.Max(0, opponent.BetThisRound - player.BetThisRound);
        callButtonText.text = callAmount == 0 ? "Check" : "Call";
        callAmountText.text = $"{callAmount}";
        raiseAmountText.text = $"{opponent.BetThisRound + Mathf.Max(bettingManager.bigBlind, opponent.BetThisRound)}";
    }


    // ============================= handle cards =============================
    void ClearAllCardHolders()
    {
        cardDealer.ClearAllCards(player, opponent, communityCardList, foldedCards,
                                 playerCardsHolder, opponentCardsHolder, communityCardsHolder);
    }
    IEnumerator DealHoleCards()
    {
        isAnimating = true;
        UpdateButtonStates();

        //uiManager.playerCards.Clear();
        //uiManager.opponentCards.Clear();

        yield return StartCoroutine(cardDealer.DealHoleCards(player, playerCardsHolder, opponent, opponentCardsHolder));

        isAnimating = false;
        UpdateGuidebook();
        UpdateButtonStates();
        Invoke(nameof(AIBehavior), delay);
    }

    public void DealCommunityCards(int num)
    {
        UpdateButtonStates();
        cardDealer.DealCommunityCards(communityCardList, communityCardsHolder, num);
        UpdateGuidebook();
        UpdateButtonStates();
        Invoke(nameof(AIBehavior), delay);
    }

    // ============================= showdown =============================

    public List<CardData> GetAllCards(List<CardData> holeCards)
    {
        List<CardData> allCards = new();
        allCards.AddRange(holeCards);
        allCards.AddRange(communityCardList);
        return allCards;
    }

    // Spades > Hearts > Diamonds > Clubs
    private int CompareBySuit(List<CardData> playerCards, List<CardData> opponentCards)
    {
        var playerSorted = playerCards
            .OrderByDescending(c => (int)c.rank)
            .ThenByDescending(c => (int)c.suit)
            .ToList();

        var opponentSorted = opponentCards
            .OrderByDescending(c => (int)c.rank)
            .ThenByDescending(c => (int)c.suit)
            .ToList();

        int n = Mathf.Min(playerSorted.Count, opponentSorted.Count);

        for (int i = 0; i < n; i++)
        {
            if (playerSorted[i].rank != opponentSorted[i].rank)
            {
                // higher rank wins
                return playerSorted[i].rank.CompareTo(opponentSorted[i].rank);
            }

            if (playerSorted[i].suit != opponentSorted[i].suit)
            {
                // same rank ¡÷ higher suit wins (Spades>Hearts>Diamonds>Clubs)
                return playerSorted[i].suit.CompareTo(opponentSorted[i].suit);
            }
        }

        // absolute tie (very rare)
        return 0;
    }

    public void Showdown()
    {
        uiManager.UpdateButtonStates(-1, players);
        uiManager.RevealCards(opponentCardsHolder);

        uiManager.playerCards.Clear();
        uiManager.opponentCards.Clear();

        foreach (Transform child in playerCardsHolder)
        {
            var cardUI = child.GetComponent<CardUI>();
            if (cardUI != null)
                uiManager.playerCards.Add(cardUI);
        }

        foreach (Transform child in opponentCardsHolder)
        {
            var cardUI = child.GetComponent<CardUI>();
            if (cardUI != null)
                uiManager.opponentCards.Add(cardUI);
        }

        foreach (Transform child in communityCardsHolder)
        {
            var cardUI = child.GetComponent<CardUI>();
            if (cardUI != null)
            {
                uiManager.playerCards.Add(cardUI);
                uiManager.opponentCards.Add(cardUI);
            }
        }

        var playerAllCards = GetAllCards(player.HoleCards);
        var opponentAllCards = GetAllCards(opponent.HoleCards);

        var playerResult = PokerHandEvaluator.EvaluateBestHand(playerAllCards);
        var opponentResult = PokerHandEvaluator.EvaluateBestHand(opponentAllCards);


        bool sameCategory = playerResult.Rank == opponentResult.Rank;

        // primary comparison
        int cmp = PokerHandEvaluator.Compare(playerResult, opponentResult);

        // if same rank + same rank tiebreakers, use suit order to decide
        if (cmp == 0)
        {
            int suitCmp = CompareBySuit(playerAllCards, opponentAllCards);
            if (suitCmp != 0)
            {
                cmp = suitCmp;   // suit actually decides the winner
            }
        }

        //  show yellow kickers 
        bool showKickersForPlayer = sameCategory && cmp > 0;
        bool showKickersForOpponent = sameCategory && cmp < 0;

        uiManager.ClearAllHighlight();

        if (cmp > 0)
        {
            gameFlowManager.resultText.text = "You win";
            Debug.Log($"You win! {playerResult.Description} beats {opponentResult.Description}. +{bettingManager.Pot} chips.");
            bettingManager.PayoutChips(player, bettingManager.Pot);

            uiManager.HighlightHand(player, playerResult, showKickersForPlayer);
        }
        else if (cmp < 0)
        {
            gameFlowManager.resultText.text = "Opponent win";
            Debug.Log($"Opponent wins. {opponentResult.Description} beats {playerResult.Description}.");
            bettingManager.PayoutChips(opponent, bettingManager.Pot);

            uiManager.HighlightHand(opponent, opponentResult, showKickersForOpponent);
        }
        else
        {
            //split pot, no yellow
            gameFlowManager.resultText.text = "Tie";
            Debug.Log($"Tie: {playerResult.Description} vs {opponentResult.Description}. Pot split.");
            int split = bettingManager.Pot / 2;
            bettingManager.PayoutChips(player, split);
            bettingManager.PayoutChips(opponent, bettingManager.Pot - split);

            uiManager.HighlightHand(player, playerResult, false);
            uiManager.HighlightHand(opponent, opponentResult, false);
        }

        UpdateMoneyUI();
        UpdateGuidebook();
        gameFlowManager.ShowGameOverPanel();
    }

}