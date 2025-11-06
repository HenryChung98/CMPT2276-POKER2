using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class DIalougeBox : MonoBehaviour
{
    [SerializeField]
    [TextArea]
    private List<string> _dialogueLines;
    private int _lineIndex;

    private TMP_Text _text;

    public TutorialManager tutorialManager;

    private void Start()
    {
        _text = GetComponent<TMP_Text>();
    }

    public void UpdateDialogue(int currentPage)
    {
        if (currentPage - 1 < _dialogueLines.Count)
        {
            _text.SetText(_dialogueLines[currentPage - 1]);
        }
    }
}
