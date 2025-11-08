using UnityEngine;
using UnityEngine.SceneManagement;

public class Tito : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Configuración de Cámara")]
    [SerializeField] private Transform cameraTransform;

    [Header("Configuración de Gravedad y Salto")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 2f;

    [Header("Límite de Caída")]
    [SerializeField] private float fallLimit = -10f;

    private CharacterController controller;
    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (controller == null)
        {
            Debug.LogError("⚠️ No se encontró un CharacterController en este GameObject.");
        }
    }

    void Update()
    {
        HandleMovementAndJump();
        CheckFallOffMap();
    }

    void HandleMovementAndJump()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 inputDir = new Vector3(horizontal, 0f, vertical).normalized;
        Vector3 moveDir = Vector3.zero;

        // ✅ SOLO procesar rotación SI HAY INPUT
        if (inputDir.magnitude >= 0.1f)
        {
            // Calcular ángulo objetivo
            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;

            // Rotar hacia el ángulo objetivo
            float angle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Mover hacia adelante
            moveDir = transform.forward * speed;
        }
        // ✅ SI NO HAY INPUT: NO TOCAR transform.rotation EN ABSOLUTO

        // --- Gravedad y salto ---
        if (controller.isGrounded)
        {
            if (velocity.y < 0)
                velocity.y = -2f;

            if (Input.GetButtonDown("Jump"))
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -0.6f * gravity);
            }
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        // --- Movimiento combinado (horizontal + vertical) ---
        Vector3 totalMove = (moveDir + new Vector3(0, velocity.y, 0)) * Time.deltaTime;
        controller.Move(totalMove);
    }

    void CheckFallOffMap()
    {
        if (transform.position.y < fallLimit)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}