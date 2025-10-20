using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI potText;
    public TextMeshProUGUI playerBankText;
    public TextMeshProUGUI oppBankText;

    [Header("Card Prefab")]
    public GameObject cardPrefab;
    public Sprite cardBackSprite;

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
        for (int i = 0; i < holder.childCount; i++)
        {
            var ui = holder.GetChild(i).GetComponent<CardUI>();
            ui.SetFaceDown(false);
        }
    }
}