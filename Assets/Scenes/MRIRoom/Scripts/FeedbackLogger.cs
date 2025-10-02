using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TMPro;
using UnityEngine;

/// <summary>
/// Logs feedback events with timestamps and exports them as a CSV file,
/// including session metadata (ID, session, date).
/// </summary>
public class FeedbackLogger : MonoBehaviour
{
    [Header("Recording Settings")]
    [Tooltip("Record ID for the current participant or session.")]
    [SerializeField] private string recordID = "";

    [Tooltip("Session identifier.")]
    [SerializeField] private string recordSession = "";

    [Tooltip("If false, recording is disabled even if StartRegister() is called.")]
    [SerializeField] private bool shouldRecord = true;

    private bool isRegistering = false;
    private string currentFeedbackType = null;
    private readonly List<FeedbackEvent> feedbackLog = new List<FeedbackEvent>();
    private DateTime sessionStartTime;

    [Serializable]
    public class FeedbackEvent
    {
        public string feedbackType;
        public float timestamp;

        public FeedbackEvent(string type, float time)
        {
            feedbackType = type;
            timestamp = time;
        }
    }

    private void OnApplicationQuit()
    {
        if (isRegistering)
            StopRegister();
    }

    private void OnApplicationPause(bool pause)
    {
        if (isRegistering)
        {
            CalculateDurations();
            SaveLogToCsv();
        }
    }

    /// <summary>
    /// Sets the record ID used in the CSV filename and header.
    /// </summary>
    public void SetRecordID(string id) => recordID = id;

    /// <summary>
    /// Sets the session number or name used in the CSV filename and header.
    /// </summary>
    public void SetRecordSession(string session) => recordSession = session;

    /// <summary>
    /// Sets the record ID from a TextMeshPro input field.
    /// </summary>
    /// <param name="inputField">The TMP_InputField containing the ID text.</param>
    public void SetRecordIDFromTMP(TMP_InputField inputField)
    {
        if (inputField == null)
        {
            Debug.LogWarning("SetRecordIDFromTMP called with null input field.");
            return;
        }

        recordID = inputField.text.Trim();
        //Debug.Log($"Record ID set to: {recordID}");
    }

    /// <summary>
    /// Sets the session identifier from a TextMeshPro input field.
    /// </summary>
    /// <param name="inputField">The TMP_InputField containing the session text.</param>
    public void SetRecordSessionFromTMP(TMP_InputField inputField)
    {
        if (inputField == null)
        {
            Debug.LogWarning("SetRecordSessionFromTMP called with null input field.");
            return;
        }

        recordSession = inputField.text.Trim();
        //Debug.Log($"Record Session set to: {recordSession}");
    }

    /// <summary>
    /// Enables or disables logging for this run.
    /// </summary>
    public void SetShouldRecord(bool value) => shouldRecord = value;

    /// <summary>
    /// Starts recording feedback events if shouldRecord is enabled.
    /// </summary>
    public void StartRegister()
    {
        if (!shouldRecord) return;

        isRegistering = true;
        feedbackLog.Clear();
        currentFeedbackType = null;
        sessionStartTime = DateTime.Now;
    }

    /// <summary>
    /// Stops recording and saves the feedback log to a CSV file.
    /// </summary>
    public void StopRegister()
    {
        if (!isRegistering) return;

        isRegistering = false;
        CalculateDurations();
        SaveLogToCsv();
    }

    /// <summary>
    /// Registers a feedback event if recording is active and enabled.
    /// </summary>
    public void RegisterFeedback(string feedbackType)
    {
        if (!isRegistering || !shouldRecord) return;

        if (currentFeedbackType != feedbackType)
        {
            feedbackLog.Add(new FeedbackEvent(feedbackType, Time.time));
            currentFeedbackType = feedbackType;
        }
    }

    /// <summary>
    /// Calculates total duration per feedback type (optional).
    /// </summary>
    private Dictionary<string, float> CalculateDurations()
    {
        Dictionary<string, float> durations = new Dictionary<string, float>();

        for (int i = 0; i < feedbackLog.Count; i++)
        {
            string type = feedbackLog[i].feedbackType;
            float startTime = feedbackLog[i].timestamp;
            float endTime = (i + 1 < feedbackLog.Count) ? feedbackLog[i + 1].timestamp : Time.time;

            float duration = endTime - startTime;
            if (!durations.ContainsKey(type))
                durations[type] = 0f;

            durations[type] += duration;
        }

        return durations;
    }

    /// <summary>
    /// Saves the feedback log to a CSV file in persistent data path.
    /// </summary>
    private void SaveLogToCsv()
    {
        if (feedbackLog.Count == 0) return;

        string safeID = string.IsNullOrEmpty(recordID) ? "unknownID" : recordID;
        string safeSession = string.IsNullOrEmpty(recordSession) ? "unknownSession" : recordSession;

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filename = $"{safeID}_{safeSession}__logs_moontrip_{timestamp}.csv";
        string path = Path.Combine(Application.persistentDataPath, filename);

        try
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.WriteLine("ID,session,date,time,feedback");

                string sessionDate = sessionStartTime.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);

                for (int i = 0; i < feedbackLog.Count; i++)
                {
                    var evt = feedbackLog[i];

                    // First row: include ID/session/date, subsequent rows: leave them empty for readability
                    string idColumn = (i == 0) ? safeID : "";
                    string sessionColumn = (i == 0) ? safeSession : "";
                    string dateColumn = (i == 0) ? sessionDate : "";

                    writer.WriteLine($"{idColumn},{sessionColumn},{dateColumn},{evt.timestamp.ToString(CultureInfo.InvariantCulture)},{evt.feedbackType}");
                }
            }

            Debug.Log($"Feedback log saved: {path}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save feedback log: {e.Message}");
        }
    }
}
