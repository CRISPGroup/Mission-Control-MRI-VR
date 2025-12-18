using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Handles feedback (visual, audio, and data logging) when the XR reticle collides
/// with a specific interactable target in the scene.
/// </summary>
/// <remarks>
/// - Provides immediate visual feedback using color/material changes.  
/// - Triggers UnityEvents for correct/incorrect hits and trajectory corrections.  
/// - Optionally logs feedback events via a <see cref="FeedbackLogger"/>.  
/// - Designed for VR reticle-based training.
/// </remarks>
public class HandleReticleCollision : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The primary target GameObject to detect collisions with.")]
    [SerializeField] private GameObject MoonTarget;

    [Tooltip("Parent GameObject containing all feedback visuals (e.g., highlight rings).")]
    [SerializeField] private GameObject FeedbackGO;

    [Tooltip("The fixation cross visual to recolor based on feedback.")]
    [SerializeField] private GameObject FixationCross;

    [Header("Feedback Materials")]
    [Tooltip("Material applied when the reticle is correctly aligned with the target.")]
    [SerializeField] private Material CorrectFeedbackMaterial;

    [Tooltip("Material applied when the reticle is not aligned with the target.")]
    [SerializeField] private Material IncorrectFeedbackMaterial;

    [Header("Logging (Optional)")]
    [Tooltip("Reference to the FeedbackLogger component used for recording events/data.")]
    public FeedbackLogger feedbackLogger;

    [Header("Events")]
    [SerializeField] UnityEvent OnStart;
    [SerializeField] UnityEvent OnCorrectFeedback;
    [SerializeField] UnityEvent OnIncorrectFeedback;
    [SerializeField] UnityEvent OnTrajectoryCorrected;
    [SerializeField] UnityEvent OnExit;

    private XRRayInteractor rayInteractor;
    private XRSimpleInteractable moonInteractable;
    private Material fixationCross;
    private bool isHoveringMoonTarget = false;
    private bool previousFeedbackWasIncorrect = false;

    private bool isCorrectFeedbackActive = false;
    private bool isIncorrectFeedbackActive = false;
    private bool hasAudioFeedback = true;

    /// <summary>
    /// Enables or disables audio feedback events.
    /// </summary>
    public void SetAudioFeedbackState(bool state)
    {
        this.hasAudioFeedback = state;
    }

    /// <summary>
    /// Initializes the components and subscribes to XR interaction events.
    /// </summary>
    public void Init()
    {
        rayInteractor = GetComponent<XRRayInteractor>();
        if (rayInteractor == null)
        {
            Debug.LogError("XRRayInteractor component not found on the GameObject.");
            return;
        }

        moonInteractable = MoonTarget.GetComponent<XRSimpleInteractable>();
        if (moonInteractable == null)
        {
            Debug.LogError("XRSimpleInteractable component not found on the MoonTarget.");
            return;
        }

        fixationCross = FixationCross.GetComponent<Renderer>().material;
        if (fixationCross == null)
        {
            Debug.LogError("SpriteRenderer component not found on the FixationCross.");
            return;
        }

        OnStart.Invoke();

        // Subscribe to interaction events
        rayInteractor.hoverEntered.AddListener(OnInteractorHoverEntered);
        rayInteractor.hoverExited.AddListener(OnInteractorHoverExited);
    }

    /// <summary>
    /// Called when the XR ray interactor begins hovering over an interactable object.
    /// Provides feedback depending on whether the hovered object is the target.
    /// </summary>
    void OnInteractorHoverEntered(HoverEnterEventArgs args)
    {
        if (args.interactableObject == moonInteractable)
        {
            isHoveringMoonTarget = true;
            HandleCorrectFeedback();
        }
        else
        {
            HandleIncorrectFeedback();
        }
    }

    /// <summary>
    /// Called when the XR ray interactor stops hovering over an interactable object.
    /// Resets feedback when leaving the correct target.
    /// </summary>
    void OnInteractorHoverExited(HoverExitEventArgs args)
    {
        if (args.interactableObject == moonInteractable)
        {
            isHoveringMoonTarget = false;
            HandleIncorrectFeedback();
        }
    }

    /// <summary>
    /// Applies correct visual feedback and triggers corresponding events.
    /// </summary>
    private void HandleCorrectFeedback()
    {
        foreach (Transform child in FeedbackGO.transform)
        {
            Renderer childRenderer = child.GetComponent<Renderer>();
            if (childRenderer != null)
            {
                childRenderer.material = CorrectFeedbackMaterial;
            }
        }
        fixationCross.SetColor("_Color", Color.green);

        if (hasAudioFeedback) HandleCorrectAudio();
    }

    /// <summary>
    /// Applies incorrect visual feedback and triggers corresponding events.
    /// </summary>
    private void HandleIncorrectFeedback()
    {
        foreach (Transform child in FeedbackGO.transform)
        {
            Renderer childRenderer = child.GetComponent<Renderer>();
            if (childRenderer != null)
            {
                childRenderer.material = IncorrectFeedbackMaterial;
            }
        }
        fixationCross.SetColor("_Color", Color.red);

        if (hasAudioFeedback) HandleIncorrectAudio();
    }

    /// <summary>
    /// Handles audio feedback and data logging for correct interactions.
    /// </summary>
    private void HandleCorrectAudio()
    {
        if (isCorrectFeedbackActive) return;

        if (previousFeedbackWasIncorrect)
        {
            OnTrajectoryCorrected.Invoke();
            feedbackLogger?.RegisterFeedback("TrajectoryCorrected");
        }
        OnCorrectFeedback.Invoke();
        previousFeedbackWasIncorrect = false;
        isCorrectFeedbackActive = true;
        isIncorrectFeedbackActive = false;

        feedbackLogger?.RegisterFeedback("Correct");
    }

    /// <summary>
    /// Handles audio feedback and data logging for incorrect interactions.
    /// </summary>
    private void HandleIncorrectAudio()
    {
        if (isIncorrectFeedbackActive) return;
        OnIncorrectFeedback.Invoke();
        previousFeedbackWasIncorrect = true;
        isIncorrectFeedbackActive = true;
        isCorrectFeedbackActive = false;

        feedbackLogger?.RegisterFeedback("Incorrect");
    }

    /// <summary>
    /// Invoked automatically when this component is disabled.
    /// Used to trigger exit-related cleanup or events.
    /// </summary>
    public void OnDisable()
    {
        OnExit.Invoke();
    }
}
