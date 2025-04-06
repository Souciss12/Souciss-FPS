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
    private AutomaticGun gunController;

    void Start()
    {
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
        gunController = GetComponent<AutomaticGun>();
    }

    void LateUpdate()
    {
        // Apply sway after GunController has positioned the weapon
        ApplySway();
    }

    void ApplySway()
    {
        bool isAiming = Input.GetMouseButton(1);

        Vector3 targetPos = isAiming ? gunController.weaponAimingPosition : gunController.weaponPosition;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        if (isAiming)
        {
            mouseX *= swayAmount;
            mouseY *= swayAmount;
        }
        else
        {
            mouseX *= swayAmount;
            mouseY *= swayAmount;
        }

        mouseX = Mathf.Clamp(mouseX, -maxSwayAmount, maxSwayAmount);
        mouseY = Mathf.Clamp(mouseY, -maxSwayAmount, maxSwayAmount);

        Vector3 finalPosition = new Vector3(-mouseX, -mouseY, 0) + targetPos;
        transform.localPosition = Vector3.Lerp(transform.localPosition, finalPosition, Time.deltaTime * swaySmoothness);

        float rotX = Mathf.Clamp(mouseX * rotationAmount, -maxRotationAmount, maxRotationAmount);
        float rotY = Mathf.Clamp(mouseY * rotationAmount, -maxRotationAmount, maxRotationAmount);

        Quaternion targetRot = isAiming ? gunController.weaponAimingRotationQuaternion : gunController.weaponRotationQuaternion;

        Quaternion finalRotation = Quaternion.Euler(rotY, rotX, 0) * targetRot;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, finalRotation, Time.deltaTime * rotationSmoothness);
    }
}
