using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR;

/// <summary>
/// Detects and manages VR controller input for both real devices and the XR Device Simulator.
/// Handles primary, trigger, and menu buttons, as well as combo inputs (Y + joystick direction).
/// Includes support for long press detection with visual and audio feedback.
/// </summary>
public class ControllerInputDetector : MonoBehaviour
{
    [Header("XR Controller References")]
    [Tooltip("Left-hand XR controller device.")]
    public UnityEngine.XR.InputDevice left;

    [Tooltip("Right-hand XR controller device.")]
    public UnityEngine.XR.InputDevice right;

    [Header("Initialization Flags")]
    public bool leftInitialized = false;
    public bool rightInitialized = false;

    [Header("Hold Settings")]
    [Tooltip("Duration in seconds required to trigger a hold event.")]
    private float holdTimeThreshold = 3f;

    private float buttonHoldTimeL = 0.0f;
    private float buttonHoldTimeR = 0.0f;
    private bool holdingDetected = false;

    private bool holdingEnabled = true;

    private bool wasPrimaryButtonPressedR = false;
    private bool wasPrimaryButtonPressedL = false;
    private bool wasMenuButtonPressedL = false;
    private bool wasMenuButtonPressedR = false;

    [Header("Unity Events")]
    [Tooltip("Event invoked when a primary button (A/X) is pressed.")]
    [SerializeField] UnityEvent OnPrimaryButtonPressed;

    [Tooltip("Event invoked when a trigger button is held for the required duration.")]
    [SerializeField] UnityEvent OnTriggerButtonHeld;

    [Tooltip("Event invoked when Y button is held while joystick is tilted left.")]
    [SerializeField] UnityEvent OnYLeftCombo;

    [Tooltip("Event invoked when Y button is held while joystick is tilted right.")]
    [SerializeField] UnityEvent OnYRightCombo;

    [Header("Audio & UI Feedback")]
    [SerializeField] private AudioSource audioHold;
    [SerializeField] private GameObject holdingCanvas;

    [Header("XR Device Simulator Input Actions")]
    [Tooltip("Root object of the XR Device Simulator.")]
    public GameObject XRDeviceSimulator;

    [Tooltip("Simulator primary button (left hand).")]
    public InputActionReference leftPrimaryButtonAction;

    [Tooltip("Simulator primary button (right hand).")]
    public InputActionReference rightPrimaryButtonAction;

    [Tooltip("Simulator trigger (left hand).")]
    public InputActionReference leftTriggerAction;

    [Tooltip("Simulator trigger (right hand).")]
    public InputActionReference rightTriggerAction;

    [Tooltip("Simulator menu button (left hand).")]
    public InputActionReference leftMenuAction;

    [Tooltip("Simulator menu button (right hand).")]
    public InputActionReference rightMenuAction;

    [SerializeField] private InputActionReference leftSecondaryButtonAction;
    [SerializeField] private InputActionReference rightSecondaryButtonAction;
    [SerializeField] private InputActionReference leftPrimary2DAxisAction;
    [SerializeField] private InputActionReference rightPrimary2DAxisAction;

    private bool wasSimPrimaryButtonL = false;
    private bool wasSimPrimaryButtonR = false;
    private bool wasSimMenuButtonL = false;
    private bool wasSimMenuButtonR = false;

    private bool isHoldingLoadingActiveL = false;
    private bool isHoldingLoadingActiveR = false;

    private bool _comboLockL = false;
    private bool _comboLockR = false;

    /// <summary>
    /// Enables or disables hold detection behavior globally.
    /// </summary>
    public void SetHoldingEnabled(bool enabled)
    {
        holdingEnabled = enabled;
    }


    void Start()
    {
        InitializeDevices();

        // Enable simulator input actions
        if (XRDeviceSimulator != null && XRDeviceSimulator.activeInHierarchy)
        {
            // Enable input actions if assigned
            leftPrimaryButtonAction?.action.Enable();
            rightPrimaryButtonAction?.action.Enable();
            leftTriggerAction?.action.Enable();
            rightTriggerAction?.action.Enable();
            leftMenuAction?.action.Enable();
            leftSecondaryButtonAction?.action.Enable();
            rightSecondaryButtonAction?.action.Enable();
            leftPrimary2DAxisAction?.action.Enable();
            rightPrimary2DAxisAction?.action.Enable();
        }
    }

    /// <summary>
    /// Updates input state each frame.
    /// Ensures controllers are initialized and processes input for both real XR devices and the XR Device Simulator.
    /// </summary>
    void Update()
    {
        if (!leftInitialized || !rightInitialized)
        {
            InitializeDevices();
        }

        HandleDefaultInput();
        HandleSimulatorInput();

    }

    /// <summary>
    /// Initializes XR input devices for left and right hands.
    /// </summary>
    void InitializeDevices()
    {
        List<UnityEngine.XR.InputDevice> devices = new List<UnityEngine.XR.InputDevice>();

        InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, devices);
        if (devices.Count > 0)
        {
            left = devices[0];
            leftInitialized = true;
        }

        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);
        if (devices.Count > 0)
        {
            right = devices[0];
            rightInitialized = true;
        }
    }

    /// <summary>
    /// Handles hardware controller inputs for both hands.
    /// </summary>
    private void HandleDefaultInput()
    {
        // RIGHT CONTROLLER
        if (rightInitialized)
        {
            right.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out bool isPressedR);

            if (isPressedR && !wasPrimaryButtonPressedR)
            {
                OnPrimaryButtonPressed.Invoke();
            }
            wasPrimaryButtonPressedR = isPressedR;

            right.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out bool isPressedRT);
            HandleHoldInput(isPressedRT, false);

            // Right menu button
            right.TryGetFeatureValue(UnityEngine.XR.CommonUsages.menuButton, out bool isPressedRM);
            if (isPressedRM && !wasMenuButtonPressedR)
            {
                OnTriggerButtonHeld.Invoke();
            }
            wasMenuButtonPressedR = isPressedRM;

            // Test secret button combo
            bool yPressed = false;
            Vector2 stick = Vector2.zero;

            right.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out yPressed);
            right.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out stick);

            DetectSecretButtonCombo(yPressed, stick, false);


        }

        // LEFT CONTROLLER
        if (leftInitialized)
        {
            left.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out bool isPressedL);

            if (isPressedL && !wasPrimaryButtonPressedL)
            {
                OnPrimaryButtonPressed.Invoke();
            }
            wasPrimaryButtonPressedL = isPressedL;

            left.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out bool isPressedLT);
            HandleHoldInput(isPressedLT, true);

            // Left Controller detecting Menu Button Press
            left.TryGetFeatureValue(UnityEngine.XR.CommonUsages.menuButton, out bool isPressedLM);
            if (isPressedLM && !wasMenuButtonPressedL)
            {
                OnTriggerButtonHeld.Invoke();
            }
            wasMenuButtonPressedL = isPressedLM;

            // Test secret button combo
            bool yPressed = false;
            Vector2 stick = Vector2.zero;

            left.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out yPressed);
            left.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out stick);

            DetectSecretButtonCombo(yPressed, stick, true);
        }
    }
    /// <summary>
    /// Handles XR Device Simulator inputs (keyboard/mouse-based testing).
    /// </summary>
    private void HandleSimulatorInput()
    {
        if (!XRDeviceSimulator.activeInHierarchy) return;
        // Primary buttons
        bool simPrimaryButtonL = leftPrimaryButtonAction != null && leftPrimaryButtonAction.action.IsPressed();
        if (simPrimaryButtonL && !wasSimPrimaryButtonL)
        {
            OnPrimaryButtonPressed.Invoke();
        }
        wasSimPrimaryButtonL = simPrimaryButtonL;

        bool simPrimaryButtonR = rightPrimaryButtonAction != null && rightPrimaryButtonAction.action.IsPressed();
        if (simPrimaryButtonR && !wasSimPrimaryButtonR)
        {
            OnPrimaryButtonPressed.Invoke();
        }
        wasSimPrimaryButtonR = simPrimaryButtonR;

        // Triggers
        bool simTriggerL = leftTriggerAction != null && leftTriggerAction.action.IsPressed();
        HandleHoldInput(simTriggerL, true);

        bool simTriggerR = rightTriggerAction != null && rightTriggerAction.action.IsPressed();
        HandleHoldInput(simTriggerR, false);

        // Menu buttons
        bool simMenuL = leftMenuAction != null && leftMenuAction.action.IsPressed();
        if (simMenuL && !wasSimMenuButtonL)
        {
            OnTriggerButtonHeld.Invoke();
        }
        wasSimMenuButtonL = simMenuL;

        bool simMenuR = rightMenuAction != null && rightMenuAction.action.IsPressed();
        if (simMenuR && !wasSimMenuButtonR)
        {
            OnTriggerButtonHeld.Invoke();
        }
        wasSimMenuButtonR = simMenuR;

        // Secret combos
        bool simYPressedL = leftSecondaryButtonAction != null && leftSecondaryButtonAction.action.IsPressed();
        Vector2 simStickL = Vector2.zero;
        if (leftPrimary2DAxisAction != null)
        {
            simStickL = leftPrimary2DAxisAction.action.ReadValue<Vector2>();
        }
        DetectSecretButtonCombo(simYPressedL, simStickL, true);

        bool simYPressedR = rightSecondaryButtonAction != null && rightSecondaryButtonAction.action.IsPressed();
        Vector2 simStickR = Vector2.zero;
        if (rightPrimary2DAxisAction != null)
        {
            simStickR = rightPrimary2DAxisAction.action.ReadValue<Vector2>();
        }
        DetectSecretButtonCombo(simYPressedR, simStickR, false);
    }
    /// <summary>
    /// Handles trigger button hold detection and associated visual/audio feedback.
    /// </summary>
    private void HandleHoldInput(bool isPressed, bool isLeft)
    {
        if (!holdingEnabled)
        {
            buttonHoldTimeL = 0.0f;
            buttonHoldTimeR = 0.0f;
            return;
        }

        if (isPressed && !holdingDetected)
        {
            if (isLeft)
            {
                buttonHoldTimeL += Time.unscaledDeltaTime;
                //Debug.Log("Left button hold time: " + buttonHoldTimeL);

                if (buttonHoldTimeL > 0.5f && !isHoldingLoadingActiveL)
                {
                    //Debug.Log(">> Playing Audio + canvas L");
                    //holdingCanvas.SetActive(true);
                    holdingCanvas.gameObject.GetComponent<Canvas>().enabled = true;
                    holdingCanvas.GetComponent<LoadingFillImage>().StartLoading(holdTimeThreshold - buttonHoldTimeL);
                    audioHold.Play();
                    isHoldingLoadingActiveL = true;
                }

                if (buttonHoldTimeL >= holdTimeThreshold)
                {
                    OnHoldTriggered();
                    holdingDetected = true;
                    buttonHoldTimeL = 0.0f;
                    isHoldingLoadingActiveL = false;
                }
            }
            else
            {
                buttonHoldTimeR += Time.unscaledDeltaTime;
                //Debug.Log("Right button hold time: " + buttonHoldTimeR);

                if (buttonHoldTimeR > 0.5f && !isHoldingLoadingActiveR)
                {
                    //Debug.Log(">> Playing audio + canvas R");
                    //holdingCanvas.SetActive(true);
                    holdingCanvas.gameObject.GetComponent<Canvas>().enabled = true;
                    holdingCanvas.GetComponent<LoadingFillImage>().StartLoading(holdTimeThreshold - buttonHoldTimeR);
                    audioHold.Play();
                    isHoldingLoadingActiveR = true;
                }

                if (buttonHoldTimeR >= holdTimeThreshold)
                {
                    OnHoldTriggered();
                    holdingDetected = true;
                    buttonHoldTimeR = 0.0f;
                    isHoldingLoadingActiveR = false;
                }
            }
        }
        else if (!isPressed)
        {
            if (isLeft)
            {
                buttonHoldTimeL = 0.0f;
                isHoldingLoadingActiveL = false;
            }
            else
            {
                buttonHoldTimeR = 0.0f;
                isHoldingLoadingActiveR = false;
            }

            // If both buttons are released, stop everything
            if (!IsAnyHandPressed())
            {
                holdingDetected = false;
                audioHold.Stop();
                holdingCanvas.gameObject.GetComponent<Canvas>().enabled = false;
            }
        }
    }

    /// <summary>
    /// Determines whether any hand (left or right) currently has its trigger button pressed.
    /// Includes checks for both XR devices and XR Device Simulator input actions.
    /// </summary>
    /// <returns>True if at least one hand trigger is pressed; otherwise, false.</returns>
    private bool IsAnyHandPressed()
    {
        bool isPressedL = false;
        bool isPressedR = false;

        if (leftInitialized)
            left.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out isPressedL);
        if (rightInitialized)
            right.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out isPressedR);

        if (XRDeviceSimulator.activeInHierarchy)
        {
            if (leftTriggerAction != null)
                isPressedL |= leftTriggerAction.action.IsPressed();
            if (rightTriggerAction != null)
                isPressedR |= rightTriggerAction.action.IsPressed();
        }

        return isPressedL || isPressedR;
    }

    /// <summary>
    /// Detects a hidden button combination using the Y button and joystick direction.
    /// Invokes <see cref="OnYLeftCombo"/> or <see cref="OnYRightCombo"/> when the combo is valid and prevents repeated triggering while held.
    /// </summary>
    /// <param name="yPressed">True if the Y button is pressed.</param>
    /// <param name="stick">Current joystick axis value.</param>
    /// <param name="isLeft">True if the input is from the left controller; false if from the right.</param>
    private void DetectSecretButtonCombo(bool yPressed, Vector2 stick, bool isLeft)
    {
        ref bool comboLock = ref isLeft ? ref _comboLockL : ref _comboLockR;

        bool isComboValid = yPressed && Mathf.Abs(stick.x) > 0.5f;

        if (isComboValid && !comboLock)
        {
            if (stick.x < -0.5f)
            {
                //Debug.Log("Invoking L");
                OnYLeftCombo.Invoke();
            }
            else if (stick.x > 0.5f)
            {
                //Debug.Log("Invoking R");
                OnYRightCombo.Invoke();
            }

            comboLock = true;
        }
        else if (!yPressed || Mathf.Abs(stick.x) < 0.3f)
        {
            comboLock = false;
        }
    }

    /// <summary>
    /// Invokes the configured UnityEvent when a hold action has been successfully detected.
    /// Prevents multiple invocations until all triggers are released.
    /// </summary>
    private void OnHoldTriggered()
    {
        OnTriggerButtonHeld.Invoke();
        holdingDetected = true;
    }
}