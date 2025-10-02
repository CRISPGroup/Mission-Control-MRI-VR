using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;


public class ManualRealigner2 : MonoBehaviour
{
    [SerializeField] private Transform origin;
    [SerializeField] private float movementSpeed = 2f; // Vitesse de d�placement
    [SerializeField] private float rotationSpeed = 45f; // Vitesse de rotation
    [SerializeField] private ControllerInputDetector controllerDetector = null;

    bool leftInitialized = false;
    bool rightInitialized = false;

    private bool performRealignement = false;

    private float cameraRotationAngle = 0f; // Rotation accumul�e de la cam�ra

    private int currentMethod = -1;

    public void Update()
    {
        if (performRealignement)
        {
            HandleRealignment();
        }
    }


    public void SetPerformRealignement(bool performRealignement)
    {
        this.performRealignement = performRealignement;
    }

    private void HandleRealignment()
    {
        Vector2 leftJoystickInput = Vector2.zero;
        Vector2 rightJoystickInput = Vector2.zero;

        // Obtenez les valeurs des joysticks
        if (controllerDetector.leftInitialized && controllerDetector.left.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out Vector2 leftInput))
        {
            if (leftInput.magnitude > 0.1f)
            {
                leftJoystickInput = leftInput;
            }
        }

        if (controllerDetector.rightInitialized && controllerDetector.right.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out Vector2 rightInput))
        {
            if (rightInput.magnitude > 0.1f)
            {
                rightJoystickInput = rightInput;
            }
        }

        // Invoquez l'�v�nement avec les donn�es des joysticks
        if (leftJoystickInput != Vector2.zero || rightJoystickInput != Vector2.zero)
        {
            switch (currentMethod) {
                case 0:
                    HandleUpDown(leftJoystickInput, rightJoystickInput);
                    break;
                case 1:
                    HandleForwardBackward(leftJoystickInput, rightJoystickInput);
                    break;
                case 2:
                    HandleLeftRightRotation(leftJoystickInput, rightJoystickInput);
                    break;
                default:
                    Debug.LogError("No method specified");
                    break;

            }
        }
    }

    public void HandleUpDown(Vector2 leftJoystickInput, Vector2 rightJoystickInput)
    {
        Vector2 currentJoystick = Vector2.zero;

        // R�cup�rer le joystick actif
        if (leftJoystickInput != Vector2.zero) currentJoystick = leftJoystickInput;
        if (rightJoystickInput != Vector2.zero) currentJoystick = rightJoystickInput;

        // D�placement vertical simple (uniquement sur l'axe Y)
        if (currentJoystick.y != 0f)
        {
            // Mouvement uniquement vers le haut ou vers le bas
            Vector3 moveDirection = Vector3.up * currentJoystick.y * movementSpeed * Time.unscaledDeltaTime;

            // D�place l'origine
            origin.position += moveDirection;
        }
    }

    public void HandleForwardBackward(Vector2 leftJoystickInput, Vector2 rightJoystickInput)
    {
        Vector2 currentJoystick = Vector2.zero;
        if (leftJoystickInput != Vector2.zero) currentJoystick = leftJoystickInput;
        if (rightJoystickInput != Vector2.zero) currentJoystick = rightJoystickInput;
        // D�placement avec le joystick gauche
        if (currentJoystick != Vector2.zero)
        {
            // R�cup�re les directions locale de d�placement par rapport � l'angle de rotation de la cam�ra
            Vector3 forward = Quaternion.Euler(0, cameraRotationAngle, 0) * Vector3.forward;
            Vector3 right = Quaternion.Euler(0, cameraRotationAngle, 0) * Vector3.right;

            // Calcule la direction de d�placement
            Vector3 moveDirection = forward * currentJoystick.y + right * currentJoystick.x;
            moveDirection *= movementSpeed * Time.unscaledDeltaTime;

            // D�place l'origine dans la direction calcul�e
            origin.position += moveDirection;
        }

       
    }

    public void HandleLeftRightRotation(Vector2 leftJoystickInput, Vector2 rightJoystickInput)
    {
        Vector2 currentJoystick = Vector2.zero;
        currentJoystick.x = 0f;

        if (leftJoystickInput.x != 0f) currentJoystick = leftJoystickInput;
        if (rightJoystickInput.x != 0f) currentJoystick = rightJoystickInput;

        // Rotation avec le joystick droit
        if (currentJoystick.x != 0f )
        {
            // Ajuste la rotation accumul�e de la cam�ra en fonction du joystick droit
            cameraRotationAngle += currentJoystick.x * rotationSpeed * Time.unscaledDeltaTime;

            // Applique la rotation � l'origine (pivot horizontal)
            origin.rotation = Quaternion.Euler(0, cameraRotationAngle, 0);
        }
    }
}
