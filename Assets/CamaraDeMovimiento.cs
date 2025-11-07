using UnityEngine;

public class CamaraDeMovimiento : MonoBehaviour
{
    [Header("Configuración de Cámara")]
    public Transform player;                  // El jugador
    public Transform cameraTarget;            // Punto de enfoque (por ejemplo, la cabeza del jugador)
    public Vector3 shoulderOffset = new Vector3(0.3f, 1.7f, 0f);
    public float followSpeed = 10f;
    public float rotationSpeed = 5f;
    public float mouseSensitivity = 2f;

    [Header("Órbita (Rotación con el ratón)")]
    public float yaw = 0f;                    // Rotación horizontal
    private float pitch = 0f;                 // Rotación vertical
    [SerializeField] private float minPitch = -30f;   // límite inferior
    [SerializeField] private float maxPitch = 60f;    // límite superior

    [Header("Zoom con la rueda del mouse")]
    public float distance = 3f;               // Distancia inicial de la cámara
    public float minDistance = 2f;            // Mínimo zoom (más cerca)
    public float maxDistance = 6f;            // Máximo zoom (más lejos)
    public float zoomSpeed = 2f;              // Velocidad de zoom

    private Transform mainCamera;

    void Start()
    {
        mainCamera = Camera.main.transform;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        HandleInput();
        UpdateCameraPosition();
    }

    void HandleInput()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Gira con el ratón
        yaw += mouseX * rotationSpeed;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // 🔹 Zoom con la rueda del mouse
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            distance -= scroll * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }
    }

    void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        // 🔹 Aplicar distancia como desplazamiento en Z
        Vector3 offset = shoulderOffset + new Vector3(0, 0, -distance);
        Vector3 targetPosition = cameraTarget.position + rotation * offset;

        // Movimiento suave
        mainCamera.position = Vector3.Lerp(mainCamera.position, targetPosition, followSpeed * Time.deltaTime);
        mainCamera.LookAt(cameraTarget);
    }
}
