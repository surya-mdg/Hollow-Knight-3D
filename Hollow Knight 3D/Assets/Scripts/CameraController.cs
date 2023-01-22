using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cam;
    [SerializeField] private Transform orientation;

    [Header("Camera Settings")]
    [SerializeField] private float mouseSensitivityX = 1f;
    [SerializeField] private float mouseSensitivityY = 1f;

    private float mouseX;
    private float mouseY;
    private float rotationX = 0f;
    private float rotationY = 0f;
    private readonly float sensitivityMultiplier = 0.1f;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        CameraInput();
        Rotate();
    }

    private void CameraInput()
    {
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");

        rotationX -= mouseY * mouseSensitivityX * sensitivityMultiplier;
        rotationY += mouseX * mouseSensitivityY * sensitivityMultiplier;

        rotationX = Mathf.Clamp(rotationX, -90f, 90f);
    }

    private void Rotate()
    {
        cam.localRotation = Quaternion.Euler(rotationX, rotationY, 0);
        orientation.localRotation = Quaternion.Euler(0, rotationY, 0);
    }
}
