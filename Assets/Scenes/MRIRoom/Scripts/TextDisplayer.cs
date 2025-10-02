using TMPro;
using UnityEngine;

[System.Serializable]
public class Segment
{
    public string title;
    [TextArea(3, 10)]
    public string shortStatement;
    public GameObject buttonSet;
}

public class TextDisplayer : MonoBehaviour
{
    public bool DisplayOnStart = false;

    [Header("UI Elements")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI shortStatementText;
    private GameObject currentButtonSet = null;

    [Header("Content Array")]
    public Segment[] segments;
    public Segment[] segmentsFR;

    private int currentIndex = -1;

    void Start()
    {
        if (DisplayOnStart)
        {
            InitText();
        }
    }

    public void SetIndex(int index)
    {
        currentIndex = index;
    }

    public void InitText()
    {
        currentIndex = 0;
        DisableGO();
        UpdateSegment();
    }

    public void NextSegment()
    {
        var currentSegments = GetCurrentSegments();
        if (currentIndex < currentSegments.Length - 1)
        {
            ChangeSegment(currentIndex + 1);
        }
    }

    public void DisplaySpecificSegment(int index)
    {
        ChangeSegment(index);
    }

    private void ChangeSegment(int index)
    {
        var currentSegments = GetCurrentSegments();
        if (index >= 0 && index < currentSegments.Length)
        {
            currentIndex = index;
            DisableGO();
            UpdateSegment();
        }
    }

    public void DisableGO()
    {
        if (titleText != null)
            titleText.gameObject.SetActive(false);
        if (shortStatementText != null)
            shortStatementText.gameObject.SetActive(false);
        if (currentButtonSet != null)
            currentButtonSet.SetActive(false);
    }

    private void UpdateSegment()
    {
        var currentSegments = GetCurrentSegments();

        if (currentIndex < 0 || currentIndex >= currentSegments.Length) return;

        Segment currentSegment = currentSegments[currentIndex];

        if (!string.IsNullOrEmpty(currentSegment.title))
        {
            titleText.gameObject.SetActive(true);
            titleText.text = currentSegment.title;
        }

        if (!string.IsNullOrEmpty(currentSegment.shortStatement))
        {
            shortStatementText.gameObject.SetActive(true);
            shortStatementText.text = currentSegment.shortStatement;
        }

        if (currentSegment.buttonSet != null)
        {
            currentButtonSet = currentSegment.buttonSet;
            currentButtonSet.SetActive(true);
        }
    }

    private Segment[] GetCurrentSegments()
    {
        return LanguageManager.Instance.CurrentLang == LanguageManager.Lang.French
            ? segmentsFR
            : segments;
    }
}
