using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    private GameManager gameManager;
    private BettingManager bettingManager;
    private CardDealer cardDealer;
    private UIManager uiManager;

    private List<Player> players;
    private List<CardData> communityCardList;
    private List<CardData> foldedCards;

    private int dealerIndex = 0;
    private int bettorIndex = 0;
    private GameState currentState = GameState.PreFlop;

    public Transform[] playerTransforms;
    public Transform bettorButton;
    public float delay = 0.5f;
    public GameObject gameOverPanel;

    public void Initialize(GameManager gm, BettingManager bm, CardDealer cd, UIManager um,
                          List<Player> playerList, List<CardData> communityCards, List<CardData> folded)
    {
        gameManager = gm;
        bettingManager = bm;
        cardDealer = cd;
        uiManager = um;
        players = playerList;
        communityCardList = communityCards;
        foldedCards = folded;
    }

    public int DealerIndex => dealerIndex;
    public int BettorIndex => bettorIndex;
    public GameState CurrentState => currentState;

    public void StartNewRound()
    {
        gameOverPanel.SetActive(false);
        currentState = GameState.PreFlop;
        bettorIndex = (dealerIndex + 3) % players.Count;
        StartCoroutine(MoveBettorButton(playerTransforms[bettorIndex]));
    }

    public void NextPhase()
    {
        if (!AllPlayersActed())
        {
            return;
        }

        switch (currentState)
        {
            case GameState.PreFlop:
                gameManager.DealCommunityCards(3);
                currentState = GameState.Flop;
                break;
            case GameState.Flop:
                gameManager.DealCommunityCards(1);
                currentState = GameState.Turn;
                break;
            case GameState.Turn:
                gameManager.DealCommunityCards(1);
                currentState = GameState.River;
                break;
            case GameState.River:
                gameManager.Showdown();
                currentState = GameState.Showdown;
                return;
            case GameState.Showdown:
                gameManager.Invoke(nameof(ShowGameOverPanel), 1f);
                return;
        }
        ResetAllPlayerStatus();
    }

    public void AdvanceTurn()
    {
        NextBettor();
        NextPhase();
        StartCoroutine(MoveBettorButton(playerTransforms[bettorIndex]));
    }

    public void NextBettor()
    {
        do
        {
            bettorIndex = (bettorIndex + 1) % players.Count;
        } while (players[bettorIndex].HasFolded);
    }

    public void IncrementDealer()
    {
        dealerIndex = (dealerIndex + 1) % players.Count;
    }

    public bool IsPlayerTurn(Player player)
    {
        return players[bettorIndex] == player && !player.HasFolded;
    }

    public bool AllPlayersActed()
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

    // set all player.HasActed = false / player.BetThisRound = 0
    public void ResetAllPlayerStatus(bool resetGame = false)
    {
        foreach (var p in players)
        {
            p.ResetStatus(resetGame);
        }
    }

    IEnumerator MoveBettorButton(Transform targetPosition)
    {
        float duration = 0.3f;
        float elapsed = 0;
        Vector3 startPos = bettorButton.position;

        while (elapsed < duration)
        {
            bettorButton.position = Vector3.Lerp(startPos, targetPosition.position, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        bettorButton.position = targetPosition.position;
    }
    void ShowGameOverPanel()
    {
        gameOverPanel.SetActive(true);
    }
}