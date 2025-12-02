using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    // messages
    [SerializeField]
    [TextArea]
    private List<string> dialogueLines;

    // page
    private int currentPage = 0;
    private const int first = 0;
    private const int last = 9;

    [Header("UI Elements")]
    public Transform[] objectTransforms;
    public GameObject speechBubble;
    public Image speechBubbleImage;
    public TextMeshProUGUI speechBubbleText;
    public Image handRankImage;
    private Vector3 orginalScale;
    public GameObject Playerhand, CommunityCard;
    public Button PlayButton;

    [Header("Audio Handling")]
    public Button prevButton;
    public Button nextButton;
    public AudioSource audioSource;
    public AudioClip buttonSound;

    private void Start()
    {
        orginalScale = speechBubbleImage.transform.localScale;
        RenderTutorial();
    }
    private void RenderTutorial()
    {
        handRankImage.gameObject.SetActive(false);
        Playerhand.gameObject.SetActive(false);
        CommunityCard.gameObject.SetActive(false);
        PlayButton.gameObject.SetActive(false);
        speechBubbleText.SetText(dialogueLines[currentPage]);

        switch (currentPage)
        {
            case 0:
                Debug.Log("welcome");
                speechBubbleImage.transform.localScale = orginalScale;
                break;
            case 1:
                speechBubbleImage.transform.localScale = new Vector3(1f,1f,0);
                speechBubbleImage.transform.localScale *= 1.5f;
                Debug.Log("second page");
                break;
            case 2:
                //display hand combination
                speechBubbleImage.transform.localScale = new Vector3(1.2f,1.3f,0);
                Debug.Log("third page");
                handRankImage.gameObject.SetActive(true);
                break;
            case 3:
                //display the call button meaning
                speechBubbleImage.transform.localScale = new Vector3(1.3f, 1.1f, 0);
                handRankImage.gameObject.SetActive(false);
                Debug.Log("fourth page");
                break;
            case 4:
                //Example of call
                speechBubbleImage.transform.localScale = new Vector3(1.3f, 1.1f, 0);
                Debug.Log("fifth page");
                break;
            case 5:
                //display raise button 
                speechBubbleImage.transform.localScale = new Vector3(1.4f, 1.2f, 0);
                Debug.Log("sixth page");
                break;
            case 6:
                //display fold button  meaning
                speechBubbleImage.transform.localScale = new Vector3(1.3f, 1.1f, 0);
                Debug.Log("seventh page");
                break;
            case 7:
                //hand card
                Debug.Log("eight page");
                Playerhand.gameObject.SetActive(true);
                
                break;
            case 8:
                //community card
                Debug.Log("nineth page");
                CommunityCard.gameObject.SetActive(true);
                break;
            case 9:
                //display combination message
                Debug.Log("tenth page");
                Playerhand.gameObject.SetActive(true);
                CommunityCard.gameObject.SetActive(true);
                PlayButton.gameObject.SetActive(true);
                break;

        }
        prevButton.interactable = currentPage > 0;
        nextButton.interactable = currentPage < last;
        speechBubble.transform.position = objectTransforms[currentPage].position;
    }

    public void NextState()
    {
        if (currentPage < last)
        {
            currentPage++;
            audioSource.PlayOneShot(buttonSound);
            RenderTutorial();
        }
        else
        {
            Debug.Log("Already at last page");

        }
    }

    public void PreviousState()
    {
        if (currentPage > first)
        {
            currentPage--;
            audioSource.PlayOneShot(buttonSound);
            RenderTutorial();
        }
        else
        {
            Debug.Log("Already at first page");
        }
    }
}
