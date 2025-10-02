using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using UnityEngine.XR.Management;

public class TrackOriginChanges : MonoBehaviour
{

    [Header("Configuration")]
    public UnityEvent OnRecenterDetected;

    private XRInputSubsystem _inputSubsystem;

    private void Start()
    {
        GetXRInputSubsystem();
        SubscribeToTrackingChanges();
    }

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

    private void SubscribeToTrackingChanges()
    {
        if (_inputSubsystem != null)
        {
            _inputSubsystem.trackingOriginUpdated += OnTrackingOriginUpdated;
        }
    }

    private void OnTrackingOriginUpdated(XRInputSubsystem inputSubsystem)
    {
        //Debug.Log("Tracking origin updated (Recenter/Reset View detected)");
        OnRecenterDetected?.Invoke();
    }

    private void OnDestroy()
    {
        if (_inputSubsystem != null)
        {
            _inputSubsystem.trackingOriginUpdated -= OnTrackingOriginUpdated;
        }
    }
}