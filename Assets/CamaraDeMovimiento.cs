using UnityEngine;

public class CamaraDeMovimiento : MonoBehaviour
{
    [Header("Referencias")]
    public Transform pivotCamara;   // Arrastra aquí el PivotCamara (el hijo de Tito)
    public Transform personaje;     // Arrastra aquí tu personaje (Tito)

    [Header("Configuración de Cámara")]
    public Vector3 offset = new Vector3(0f, 0f, -3f); // distancia de la cámara al pivot
    public float sensibilidadMouse = 2f;
    public float suavizado = 8f;

    [Header("Límites de Rotación Vertical")]
    public float limiteVerticalMin = -30f;
    public float limiteVerticalMax = 60f;

    private float rotacionX = 0f;
    private float rotacionY = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensibilidadMouse;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidadMouse;

        rotacionY += mouseX;
        rotacionX -= mouseY;
        rotacionX = Mathf.Clamp(rotacionX, limiteVerticalMin, limiteVerticalMax);

        // Rotar el pivot (no la cámara directamente)
        pivotCamara.rotation = Quaternion.Euler(rotacionX, rotacionY, 0f);

        // Mantener el pivot en la posición del personaje (por si se mueve)
        pivotCamara.position = personaje.position + Vector3.up * 1.6f;
        Debug.Log($"MouseX: {Input.GetAxis("Mouse X")} | MouseY: {Input.GetAxis("Mouse Y")}");
    }

    void LateUpdate()
    {
        // Mover la cámara detrás del pivot
        Vector3 posicionDeseada = pivotCamara.position + pivotCamara.rotation * offset;
        transform.position = Vector3.Lerp(transform.position, posicionDeseada, suavizado * Time.deltaTime);

        // Apuntar la cámara hacia el pivot
        transform.LookAt(pivotCamara);
    }
}