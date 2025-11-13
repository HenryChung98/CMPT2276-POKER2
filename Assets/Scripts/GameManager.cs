using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEditor.Experimental.GraphView;
using System.Collections;

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
    public GuidebookUI guidebookUI;             //reference to your guidebook panel


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


    private void UpdateStateMessage(string msg)
    {
        stateText.text = msg;
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
        cardDealer.Initialize(deckManager, uiManager, delay);

    }

    void Start()
    {
        gameFlowManager.Initialize(this, bettingManager, cardDealer, uiManager, players, communityCardList, foldedCards);
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
        // update cards
        deckManager.Shuffle();
        StartCoroutine(DealHoleCards());

        // update status and post blinds
        gameFlowManager.ResetAllPlayerStatus(true);
        bettingManager.PostBlind(players, gameFlowManager.DealerIndex);
        gameFlowManager.StartNewRound();

        // update UIs
        UpdateMoneyUI();
        UpdateButtonStates();
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
    // ============================= /Update UIs =============================

    public List<CardData> GetPlayerAllCardsForUI()
    {
        return GetAllCards(player.HoleCards);
    }

    // Tells the guidebook to update highlight/description
    private void UpdateGuidebook()
    {
        if (guidebookUI != null)
            guidebookUI.Refresh(GetPlayerAllCardsForUI());
    }



    // ============================= Buttons you can hook in the Inspector =============================
    public void RestartButton()
    {
        gameFlowManager.IncrementDealer();
        ClearAllCardHolders();
        UpdateStateMessage("");
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
            AdvanceTurn();
        }
        else
        {
            Debug.Log($"{raiser} cannot raise");
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
            UpdateMoneyUI();
            uiManager.restartButton.interactable = true;
            gameFlowManager.currentState = GameState.Showdown;
            gameFlowManager.ShowGameOverPanel();
            Debug.Log("game ended by fold");
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
        }
        else if (cmp < 0)
        {
            UpdateStateMessage("Opponent win");
            Debug.Log($"Opponent wins. {opponentResult.Description} beats {playerResult.Description}.");
            bettingManager.PayoutChips(opponent, bettingManager.Pot);
        }
        else
        {
            UpdateStateMessage("Tie");
            Debug.Log($"Tie: {playerResult.Description} vs {opponentResult.Description}. Pot split.");
            int split = bettingManager.Pot / 2;
            bettingManager.PayoutChips(player, split);
            bettingManager.PayoutChips(opponent, bettingManager.Pot - split);
        }

        UpdateMoneyUI();
        UpdateGuidebook();
    }

}