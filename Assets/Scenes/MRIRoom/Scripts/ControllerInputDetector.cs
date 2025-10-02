using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class ControllerInputDetector : MonoBehaviour
{
    public UnityEngine.XR.InputDevice left;
    public UnityEngine.XR.InputDevice right;

    public bool leftInitialized = false;
    public bool rightInitialized = false;

    private float holdTimeThreshold = 3f;
    private float buttonHoldTimeL = 0.0f;
    private float buttonHoldTimeR = 0.0f;
    private bool holdingDetected = false;

    private bool holdingEnabled = true;

    private bool wasPrimaryButtonPressedR = false;
    private bool wasPrimaryButtonPressedL = false;
    private bool wasMenuButtonPressedL = false;

    [SerializeField] UnityEvent OnPrimaryButtonPressed;
    [SerializeField] private AudioSource audioHold;
    [SerializeField] private GameObject holdingCanvas;
    [SerializeField] UnityEvent OnTriggerButtonHeld;
    [SerializeField] UnityEvent OnYLeftCombo;
    [SerializeField] UnityEvent OnYRightCombo;

    // XR Device Simulator support via InputActionReference
    [Header("XR Device Simulator Input Actions")]
    public GameObject XRDeviceSimulator;
    public InputActionReference leftPrimaryButtonAction;
    public InputActionReference rightPrimaryButtonAction;
    public InputActionReference leftTriggerAction;
    public InputActionReference rightTriggerAction;
    public InputActionReference leftMenuAction;
    [SerializeField] private InputActionReference leftSecondaryButtonAction;
    [SerializeField] private InputActionReference rightSecondaryButtonAction;
    [SerializeField] private InputActionReference leftPrimary2DAxisAction;
    [SerializeField] private InputActionReference rightPrimary2DAxisAction;

    private bool wasSimPrimaryButtonL = false;
    private bool wasSimPrimaryButtonR = false;
    private bool wasSimMenuButtonL = false;

    private bool isHoldingLoadingActiveL = false;
    private bool isHoldingLoadingActiveR = false;

    private bool _comboLockL = false;
    private bool _comboLockR = false;

    public void SetHoldingEnabled(bool enabled)
    {
        holdingEnabled = enabled;
    }


    void Start()
    {
        //DisableLeftJoystick();
        //DisableSimulatorLeftJoystick();
        InitializeDevices();

        if (XRDeviceSimulator.activeInHierarchy)
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

    /*
    void DisableSimulatorLeftJoystick()
    {
        if (leftJoystickActionSimulator != null)
        {
            leftJoystickActionSimulator.action.Disable();
        }
        else
        {
            Debug.LogError("Left joystick simulator action reference is not set.");
        }
    }

    void DisableLeftJoystick()
    {
        if (leftJoystickAction != null)
        {
            leftJoystickAction.action.Disable();
        }
        else
        {
            Debug.LogError("Left joystick action reference is not set.");
        }
    }
    */
    void Update()
    {
        if (!leftInitialized || !rightInitialized)
        {
            InitializeDevices();
        }

        HandleDefaultInput();
        HandleSimulatorInput();

    }

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
    private void HandleDefaultInput()
    {
        // Vérifiez les entrées du contrôleur droit
        if (rightInitialized)
        {
            right.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out bool isPressedR);

            // Détecte une transition de "relâché" à "pressé" pour le bouton droit
            if (isPressedR && !wasPrimaryButtonPressedR)
            {
                OnPrimaryButtonPressed.Invoke(); // Appelle l'événement une seule fois
            }
            wasPrimaryButtonPressedR = isPressedR; // Met à jour l'état précédent

            right.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out bool isPressedRT);
            HandleHoldInput(isPressedRT, false);


            // Test secret button combo
            bool yPressed = false;
            Vector2 stick = Vector2.zero;

            right.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out yPressed);
            right.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out stick);

            DetectSecretButtonCombo(yPressed, stick, false);


        }

        // Vérifiez les entrées du contrôleur gauche
        if (leftInitialized)
        {
            left.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out bool isPressedL);

            // Détecte une transition de "relâché" à "pressé" pour le bouton gauche
            if (isPressedL && !wasPrimaryButtonPressedL)
            {
                OnPrimaryButtonPressed.Invoke(); // Appelle l'événement une seule fois
            }
            wasPrimaryButtonPressedL = isPressedL; // Met à jour l'état précédent

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

    private void HandleSimulatorInput()
    {
        if (!XRDeviceSimulator.activeInHierarchy) return;
        // Bouton principal gauche
        bool simPrimaryButtonL = leftPrimaryButtonAction != null && leftPrimaryButtonAction.action.IsPressed();
        if (simPrimaryButtonL && !wasSimPrimaryButtonL)
        {
            OnPrimaryButtonPressed.Invoke();
        }
        wasSimPrimaryButtonL = simPrimaryButtonL;

        // Bouton principal droit
        bool simPrimaryButtonR = rightPrimaryButtonAction != null && rightPrimaryButtonAction.action.IsPressed();
        if (simPrimaryButtonR && !wasSimPrimaryButtonR)
        {
            OnPrimaryButtonPressed.Invoke();
        }
        wasSimPrimaryButtonR = simPrimaryButtonR;

        // Trigger gauche
        bool simTriggerL = leftTriggerAction != null && leftTriggerAction.action.IsPressed();
        HandleHoldInput(simTriggerL, true);

        // Trigger droit
        bool simTriggerR = rightTriggerAction != null && rightTriggerAction.action.IsPressed();
        HandleHoldInput(simTriggerR, false);

        // Menu gauche
        bool simMenuL = leftMenuAction != null && leftMenuAction.action.IsPressed();
        if (simMenuL && !wasSimMenuButtonL)
        {
            OnTriggerButtonHeld.Invoke();
        }
        wasSimMenuButtonL = simMenuL;

        // Test secret button combo dans le simulateur

        // Bouton Y gauche
        bool simYPressedL = leftSecondaryButtonAction != null && leftSecondaryButtonAction.action.IsPressed();
        Vector2 simStickL = Vector2.zero;
        if (leftPrimary2DAxisAction != null)
        {
            simStickL = leftPrimary2DAxisAction.action.ReadValue<Vector2>();
        }

        DetectSecretButtonCombo(simYPressedL, simStickL, true);

        // Bouton Y droit
        bool simYPressedR = rightSecondaryButtonAction != null && rightSecondaryButtonAction.action.IsPressed();
        Vector2 simStickR = Vector2.zero;
        if (rightPrimary2DAxisAction != null)
        {
            simStickR = rightPrimary2DAxisAction.action.ReadValue<Vector2>();
        }

        DetectSecretButtonCombo(simYPressedR, simStickR, false);

    }
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
                    //Debug.Log(">> Activer audio + canvas L");
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
                    //Debug.Log(">> Activer audio + canvas R");
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

            // Si les deux boutons sont relâchés, on stoppe tout
            if (!IsAnyHandPressed())
            {
                holdingDetected = false;
                audioHold.Stop();
                holdingCanvas.gameObject.GetComponent<Canvas>().enabled = false;
            }
        }
    }

    private bool IsAnyHandPressed()
    {
        bool isPressedL = false;
        bool isPressedR = false;

        // Input device direct (fallback si pas simulateur)
        if (leftInitialized)
            left.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out isPressedL);
        if (rightInitialized)
            right.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out isPressedR);

        // En plus, si simulateur actif
        if (XRDeviceSimulator.activeInHierarchy)
        {
            if (leftTriggerAction != null)
                isPressedL |= leftTriggerAction.action.IsPressed();
            if (rightTriggerAction != null)
                isPressedR |= rightTriggerAction.action.IsPressed();
        }

        return isPressedL || isPressedR;
    }

    private bool _wasYPressed = false;
    private Vector2 _lastStickValue = Vector2.zero;

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

    private void OnHoldTriggered()
    {
        OnTriggerButtonHeld.Invoke();
        holdingDetected = true;
    }
}