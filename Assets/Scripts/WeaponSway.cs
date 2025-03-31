using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    [Header("Sway Settings")]
    public float swayAmount = 0.02f; // Intensité du sway
    public float maxSwayAmount = 0.06f; // Amplitude max du sway
    public float swaySmoothness = 4f; // Fluidité du mouvement

    [Header("Rotation Settings")]
    public float rotationAmount = 2f;
    public float maxRotationAmount = 5f;
    public float rotationSmoothness = 4f;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Start()
    {
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
    }

    void Update()
    {
        ApplySway();
    }

    void ApplySway()
    {
        // Récupération de l'entrée de la souris
        float mouseX = Input.GetAxis("Mouse X") * swayAmount;
        float mouseY = Input.GetAxis("Mouse Y") * swayAmount;

        // Clamp pour éviter un déplacement excessif
        mouseX = Mathf.Clamp(mouseX, -maxSwayAmount, maxSwayAmount);
        mouseY = Mathf.Clamp(mouseY, -maxSwayAmount, maxSwayAmount);

        // Calcul de la nouvelle position avec un Lerp pour adoucir
        Vector3 targetPosition = new Vector3(-mouseX, -mouseY, 0) + initialPosition;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * swaySmoothness);

        // Calcul de la nouvelle rotation
        float rotX = Mathf.Clamp(mouseX * rotationAmount, -maxRotationAmount, maxRotationAmount);
        float rotY = Mathf.Clamp(mouseY * rotationAmount, -maxRotationAmount, maxRotationAmount);
        Quaternion targetRotation = Quaternion.Euler(rotY, rotX, 0) * initialRotation;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * rotationSmoothness);
    }
}
