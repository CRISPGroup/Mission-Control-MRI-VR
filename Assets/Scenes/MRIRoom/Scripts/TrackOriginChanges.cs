using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using UnityEngine.XR.Management;

/// <summary>
/// Detects when the XR tracking origin (or reference space) is updated —
/// typically when the user manually recenters their view (e.g., "Reset View" or recenter event).
/// </summary>
/// <remarks>
/// This component listens for <see cref="XRInputSubsystem.trackingOriginUpdated"/>  
/// and triggers a UnityEvent when a recenter action is detected.
/// </remarks>
public class TrackOriginChanges : MonoBehaviour
{

    [Header("Configuration")]
    [Tooltip("Event triggered when the user recenters their view or when the tracking origin changes.")]
    public UnityEvent OnRecenterDetected;

    private XRInputSubsystem _inputSubsystem;

    /// <summary>
    /// Unity lifecycle method — called on the first frame.
    /// Initializes the XR input subsystem and subscribes to tracking origin updates.
    /// </summary>
    private void Start()
    {
        GetXRInputSubsystem();
        SubscribeToTrackingChanges();
    }

    /// <summary>
    /// Locates the active <see cref="XRInputSubsystem"/> from the current XR setup.
    /// </summary>
    /// <remarks>
    /// This is necessary to access tracking-related events such as recentering or origin changes.
    /// </remarks>
    private void GetXRInputSubsystem()
    {
        var xrInputSubsystems = new List<XRInputSubsystem>();
        SubsystemManager.GetSubsystems(xrInputSubsystems);

        if (xrInputSubsystems.Count > 0)
        {
            _inputSubsystem = xrInputSubsystems[0];
        }
        else
        {
            Debug.LogWarning("No XRInputSubsystem found.");
        }
    }

    /// <summary>
    /// Subscribes to tracking origin change events from the XR input subsystem.
    /// </summary>
    private void SubscribeToTrackingChanges()
    {
        if (_inputSubsystem != null)
        {
            _inputSubsystem.trackingOriginUpdated += OnTrackingOriginUpdated;
        }
    }

    /// <summary>
    /// Callback invoked when the XR system detects a tracking origin update (i.e., recentering).
    /// </summary>
    /// <param name="inputSubsystem">The XR input subsystem that triggered the event.</param>
    private void OnTrackingOriginUpdated(XRInputSubsystem inputSubsystem)
    {
        //Debug.Log("Tracking origin updated (Recenter/Reset View detected)");
        OnRecenterDetected?.Invoke();
    }

    /// <summary>
    /// Unity lifecycle method — ensures event unsubscription when the object is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        if (_inputSubsystem != null)
        {
            _inputSubsystem.trackingOriginUpdated -= OnTrackingOriginUpdated;
        }
    }
}