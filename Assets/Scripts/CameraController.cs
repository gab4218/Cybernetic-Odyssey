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

    float xRotation, yRotation, mouseX, mouseY;



    private void Awake()
    {
        //Bloquear cursor y hacerlo invisible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    private void Update()
    {
        transform.position = POVTransform.position;
        if (DialogueManager.instance != null)
        {
            if (DialogueManager.instance.inDialogue)
            {
                return;
            }
        }
        //Obtener input de mouse
        mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * cameraSensitivityX;
        mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * cameraSensitivityY;


        //Modificador de sensibilidad temporal
        if (Input.GetKeyDown(KeyCode.G)) cameraSensitivityX -= 50;
        if (Input.GetKeyDown(KeyCode.H)) cameraSensitivityX += 50;
        if (Input.GetKeyDown(KeyCode.J)) cameraSensitivityY -= 50;
        if (Input.GetKeyDown(KeyCode.K)) cameraSensitivityY += 50;

        //Calcular rotacion
        yRotation += mouseX;
        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //Rotar camara y jugador
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        PlayerTransform.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public IEnumerator Shake(float length, float intensity)
    {
        while (length > 0)
        {
            float randomx = Random.Range(0, intensity), randomy = Random.Range(0, intensity);
            Camera.main.transform.localPosition = new Vector3(randomx, randomy, 0);
            length -= Time.deltaTime;
            yield return null;
        }
        Camera.main.transform.localPosition = Vector3.zero;
    }

}
