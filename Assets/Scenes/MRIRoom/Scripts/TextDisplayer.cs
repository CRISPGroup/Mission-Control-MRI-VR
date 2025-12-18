using TMPro;
using UnityEngine;

/// <summary>
/// Represents a single text segment containing a title, a short statement,
/// and an optional associated button group.
/// </summary>
[System.Serializable]
public class Segment
{
    [Tooltip("Title displayed at the top of the segment.")]
    public string title;

    [Tooltip("Main short statement or paragraph to display below the title.")]
    [TextArea(3, 10)]
    public string shortStatement;

    [Tooltip("Set of buttons associated with this segment (optional).")]
    public GameObject buttonSet;
}

/// <summary>
/// Displays text-based segments (title + short statement + optional button set)
/// and allows navigation through them sequentially or directly.
/// </summary>
/// <remarks>
/// The system supports bilingual content using <see cref="LanguageManager"/>:
/// English segments are stored in <see cref="segments"/>,
/// and French segments in <see cref="segmentsFR"/>.
/// </remarks>
public class TextDisplayer : MonoBehaviour
{
    [Header("Display Options")]
    [Tooltip("If true, the first segment will be displayed automatically at Start().")]
    public bool DisplayOnStart = false;

    [Header("UI Elements")]
    [Tooltip("UI text field for displaying the segment title.")]
    public TextMeshProUGUI titleText;

    [Tooltip("UI text field for displaying the main short statement.")]
    public TextMeshProUGUI shortStatementText;

    private GameObject currentButtonSet = null;

    [Header("Content Array")]
    [Tooltip("English version of the text segments.")]
    public Segment[] segments;

    [Tooltip("French version of the text segments.")]
    public Segment[] segmentsFR;

    private int currentIndex = -1;

    /// <summary>
    /// Unity lifecycle method called on the first frame.  
    /// Initializes the text display automatically if <see cref="DisplayOnStart"/> is enabled.
    /// </summary>
    void Start()
    {
        if (DisplayOnStart)
        {
            InitText();
        }
    }

    /// <summary>
    /// Sets the current segment index manually (without immediately updating the UI).
    /// </summary>
    /// <param name="index">The index of the segment to select.</param>
    public void SetIndex(int index)
    {
        currentIndex = index;
    }

    /// <summary>
    /// Initializes the text display by showing the first segment (index 0).
    /// </summary>
    public void InitText()
    {
        currentIndex = 0;
        DisableGO();
        UpdateSegment();
    }

    /// <summary>
    /// Advances to the next segment if available.
    /// </summary>
    public void NextSegment()
    {
        var currentSegments = GetCurrentSegments();
        if (currentIndex < currentSegments.Length - 1)
        {
            ChangeSegment(currentIndex + 1);
        }
    }

    /// <summary>
    /// Displays a specific segment by index, if it exists.
    /// </summary>
    /// <param name="index">The segment index to display.</param>
    public void DisplaySpecificSegment(int index)
    {
        ChangeSegment(index);
    }

    /// <summary>
    /// Core method to change the displayed segment, updating all UI elements.
    /// </summary>
    /// <param name="index">The index of the new segment.</param>
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

    /// <summary>
    /// Hides UI components that are empty (text and button set).
    /// </summary>
    public void DisableGO()
    {
        if (titleText != null)
            titleText.gameObject.SetActive(false);
        if (shortStatementText != null)
            shortStatementText.gameObject.SetActive(false);
        if (currentButtonSet != null)
            currentButtonSet.SetActive(false);
    }

    /// <summary>
    /// Updates the displayed text and button set based on the current segment.
    /// </summary>
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

    /// <summary>
    /// Returns the segment array corresponding to the current language.
    /// </summary>
    /// <returns>The active segment array (English or French).</returns>
    private Segment[] GetCurrentSegments()
    {
        return LanguageManager.Instance.CurrentLang == LanguageManager.Lang.French
            ? segmentsFR
            : segments;
    }
}
