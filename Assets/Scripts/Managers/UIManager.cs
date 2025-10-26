using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI potText;
    public TextMeshProUGUI playerBankText;
    public TextMeshProUGUI oppBankText;

    [Header("Card Prefab")]
    public GameObject cardPrefab;
    public Sprite cardBackSprite;

    [Header("Buttons")]
    public Button[] callButtons;
    public Button[] raiseButtons;
    public Button[] foldButtons;
    public Button restartButton;

    public void UpdateMoneyUI(int pot, int playerChips, int opponentChips)
    {
        if (potText != null) potText.text = $"Pot: {pot}";
        if (playerBankText != null) playerBankText.text = $"You: {playerChips}";
        if (oppBankText != null) oppBankText.text = $"Opponent: {opponentChips}";
    }

    public void DisplayCard(CardData cardData, Transform holder, bool faceDown = false)
    {
        GameObject cardObject = Instantiate(cardPrefab, holder);
        var ui = cardObject.GetComponent<CardUI>();
        ui.cardBackSprite = cardBackSprite;
        ui.Setup(cardData, faceDown);
    }

    public void ClearCardHolder(Transform holder)
    {
        for (int i = holder.childCount - 1; i >= 0; i--)
        {
            Destroy(holder.GetChild(i).gameObject);
        }
    }

    public void RevealCards(Transform holder)
    {
        restartButton.interactable = true;
        for (int i = 0; i < holder.childCount; i++)
        {
            var ui = holder.GetChild(i).GetComponent<CardUI>();
            ui.SetFaceDown(false);
        }
    }

    public void UpdateButtonStates(int activePlayerIndex, List<Player> allPlayers)
    {
        // all buttons are disabled
        if (activePlayerIndex == -1)
        {
            foreach (var btn in callButtons) btn.interactable = false;
            foreach (var btn in raiseButtons) btn.interactable = false;
            foreach (var btn in foldButtons) btn.interactable = false;
            restartButton.interactable = false;
            return;
        }

        // if at least one player all-in, nobody is allowed to raise
        bool anyoneAllIn = false;
        foreach (var player in allPlayers)
        {
            if (player.HasAllIn)
            {
                anyoneAllIn = true;
                break;
            }
        }

        for (int i = 0; i < allPlayers.Count && i < callButtons.Length; i++)
        {
            bool isThisPlayerTurn = (i == activePlayerIndex);
            bool canAct = isThisPlayerTurn && !allPlayers[i].HasFolded;

            callButtons[i].interactable = canAct;
            raiseButtons[i].interactable = canAct && !anyoneAllIn;
            foldButtons[i].interactable = canAct && !anyoneAllIn;
        }
        restartButton.interactable = false;
    }
}