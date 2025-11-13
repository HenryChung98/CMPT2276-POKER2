using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public GameManager gameManager;
    
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
        // cards go from deck to holder
        GameObject cardObject = Instantiate(cardPrefab, deckTransform);
        var ui = cardObject.GetComponent<CardUI>();
        ui.cardBackSprite = cardBackSprite;
        ui.Setup(cardData, faceDown);

        StartCoroutine(MoveCardToHolder(cardObject, holder));
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
            callButton.interactable = true && !gameManager.isAnimating;
            raiseButton.interactable = true && !gameManager.isAnimating && !anyoneAllIn;
            foldButton.interactable = true && !gameManager.isAnimating && !anyoneAllIn;
        }
        else {
            callButton.interactable = false;
            raiseButton.interactable = false;
            foldButton.interactable = false;
        }
    }

    // animation - move card from deck to holder
    private IEnumerator MoveCardToHolder(GameObject card, Transform holder)
    {
        Vector3 startPos = card.transform.position;

        card.transform.SetParent(holder);
        Canvas.ForceUpdateCanvases();
        yield return null; // Layout Group needs time to calculate (wait 1 frame)

        Vector3 endPos = card.transform.position;
        card.transform.position = startPos;

        float duration = 0.5f;
        float elapsed = 0f;

        // animation
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            card.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        card.transform.position = endPos;
    }
}