using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class DIalougeBox : MonoBehaviour
{
    [SerializeField]
    [TextArea]
    private List<string> _dialougeLine;
    private int _lineIndex;

    private TMP_Text _text;

    private void Start()
    {
        _text = GetComponent<TMP_Text>();
    }
    private void Update()
    {
        _text.SetText(_dialougeLine[_lineIndex++]);
    }
}
