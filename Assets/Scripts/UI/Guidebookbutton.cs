using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [SerializeField] private CanvasGroup menu;

    public void OpenClosePanel()
    {
        // Toggle alpha between 0 and 1
        menu.alpha = menu.alpha > 0 ? 0 : 1;

        // Enable or disable interaction
        menu.blocksRaycasts = menu.alpha > 0;
        menu.interactable = menu.alpha > 0;
    }
}
