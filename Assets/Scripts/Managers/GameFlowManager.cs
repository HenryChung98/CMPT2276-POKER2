using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    private GameManager gameManager;

    private List<Player> players;

    private int dealerIndex = 0;
    private int bettorIndex = 0;
    private BettingManager bettingManager;
    private DeckManager deckManager;
    public GameState currentState = GameState.PreFlop;

    public Transform[] playerTransforms;
    public Transform bettorButton;
    public float delay = 0.5f;
    public GameObject gameOverPanel;
    public TextMeshProUGUI resultText;

    public void Initialize(GameManager gm, BettingManager bm, DeckManager dm, List<Player> playerList)
    {
        gameManager = gm;
        bettingManager = bm;
        deckManager = dm;
        players = playerList;

    }

    public int DealerIndex => dealerIndex;
    public int BettorIndex => bettorIndex;
    public GameState CurrentState => currentState;

    public void StartNewRound()
    {
        deckManager.Shuffle();
        ResetAllPlayerStatus(true);
        bettingManager.PostBlind(players, DealerIndex);

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
    public void ShowGameOverPanel()
    {
        gameOverPanel.SetActive(true);
    }
}