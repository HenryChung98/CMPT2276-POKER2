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
    public Button callButton;
    public Button raiseButton;
    public Button foldButton;
    public Button restartButton;

    [Header("Transforms")]
    public Transform deckTransform;

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

        if (activePlayerIndex == 0) {
            callButton.interactable = true;
            raiseButton.interactable = true && !anyoneAllIn;
            foldButton.interactable = true && !anyoneAllIn;
        }
        else {
            callButton.interactable = false;
            raiseButton.interactable = false;
            foldButton.interactable = false;
        }

        restartButton.interactable = false;
    }



}