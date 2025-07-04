using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Transforms")]
    [SerializeField] Transform PlayerTransform;
    [SerializeField] Transform POVTransform;
    [Header("Sensitivity")]
    [SerializeField] float cameraSensitivityX = 1;
    [SerializeField] float cameraSensitivityY = 1;
    [SerializeField] TMP_Text sensDisplay;

    float xRotation, yRotation;



    private void Awake()
    {
        //Lock cursor and make it invisible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    private void Update()
    {
        //Get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * cameraSensitivityX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * cameraSensitivityY;


        //Temporary sensitivity changer
        if (Input.GetKeyDown(KeyCode.G)) cameraSensitivityX -= 50;
        if (Input.GetKeyDown(KeyCode.H)) cameraSensitivityX += 50;
        if (Input.GetKeyDown(KeyCode.J)) cameraSensitivityY -= 50;
        if (Input.GetKeyDown(KeyCode.K)) cameraSensitivityY += 50;
        sensDisplay.text = $"X sensitivity: {cameraSensitivityX} \nY sensitivity: {cameraSensitivityY} ";

        //Calculate rotation
        yRotation += mouseX;
        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //Rotate camera and player
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        transform.position = POVTransform.position;
        PlayerTransform.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    private void Look()
    {

    }

}
