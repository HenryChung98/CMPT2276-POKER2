using UnityEngine;
using System.Collections;

public class GuidebookButton : MonoBehaviour
{
    public GameObject guidebook;
    public float duration = 0.3f;
    private bool isOpen = false;
    private bool isAnimating = false;

    public void ToggleGuidebook()
    {
        if (guidebook != null && !isAnimating)
        {
            StartCoroutine(AnimateGuidebook());
        }
    }

    IEnumerator AnimateGuidebook()
    {
        isAnimating = true;
        RectTransform rect = guidebook.GetComponent<RectTransform>();
        Vector3 startPos = rect.anchoredPosition;
        Vector3 targetPos = startPos;

        targetPos.x = isOpen ? 320f : -320f;
        isOpen = !isOpen;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            rect.anchoredPosition = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rect.anchoredPosition = targetPos;
        isAnimating = false;
    }
}