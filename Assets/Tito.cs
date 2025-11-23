using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro; // Necesario para el Texto

public class Tito : MonoBehaviour
{
    [Header("Componentes")]
    private CharacterController controller;
    private Animator anim;
    [SerializeField] private TextMeshProUGUI textoContador; // Arrastra aquí tu texto UI
    [SerializeField] private GameObject mensajeLlave; // Opcional: Texto de "Llave Obtenida"

    [Header("Configuración de Cámara")] // <--- ¡ESTO FALTABA!
    [SerializeField] private Transform cameraTransform; // <--- ¡ESTA ERA LA VARIABLE QUE FALTABA!

    [Header("Configuración Movimiento")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float climbSpeed = 3f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float fallLimit = -10f;

    // Variables internas
    private Vector3 velocity;
    private bool isBusy = false; // Para bloquear movimiento al atacar, recoger o escalar final

    // --- SISTEMA DE ESCALERA ---
    private bool isClimbing = false;
    private Escalera escaleraActual;

    // --- SISTEMA DE INTERACCIÓN ---
    private GameObject objetoCercano; // ¿Qué tengo enfrente?
    private int cristalesRecolectados = 0;
    private int cristalesTotales = 3;
    private bool tieneLlave = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        ActualizarUI();
        if (mensajeLlave != null) mensajeLlave.SetActive(false);

        // Seguridad por si olvidaste asignar la cámara
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        if (isBusy) return; // Si está ocupado (animación final o recogiendo), no hacer nada más

        if (isClimbing)
        {
            HandleClimbing();
        }
        else
        {
            HandleMovementAndJump();
            HandleInteraction(); // Detectar tecla E
        }

        CheckFallOffMap();
    }

    // --------------------------------------------------------
    // LÓGICA DE INTERACCIÓN (CRISTALES Y COFRE)
    // --------------------------------------------------------
    void HandleInteraction()
    {
        // Si hay un objeto cerca y presiono E
        if (objetoCercano != null && Input.GetKeyDown(KeyCode.E))
        {
            string tag = objetoCercano.tag;

            if (tag == "Escalera" && !isClimbing)
            {
                StartClimbing();
            }
            else if (tag == "Cristal")
            {
                StartCoroutine(RecogerObjeto(objetoCercano));
            }
            else if (tag == "Cofre")
            {
                IntentarAbrirCofre();
            }
        }
    }

    IEnumerator RecogerObjeto(GameObject objeto)
    {
        isBusy = true; // Bloquear movimiento
        anim.SetTrigger("Take"); // Reproducir animación

        // Rotar hacia el objeto para que se vea natural
        Vector3 mirarHacia = new Vector3(objeto.transform.position.x, transform.position.y, objeto.transform.position.z);
        transform.LookAt(mirarHacia);

        // Esperar 0.5 segundos (o lo que tarde la mano en bajar en tu animación)
        yield return new WaitForSeconds(0.5f);

        // Lógica según qué recogimos
        if (objeto.CompareTag("Cristal"))
        {
            cristalesRecolectados++;
            ActualizarUI();
            Destroy(objeto); // Desaparecer cristal
            objetoCercano = null; // Limpiar referencia
        }

        // Esperar a que termine la animación de levantarse
        yield return new WaitForSeconds(0.5f);

        isBusy = false; // Desbloquear movimiento
    }

    void IntentarAbrirCofre()
    {
        if (cristalesRecolectados >= cristalesTotales)
        {
            // Tiene los cristales, abrir cofre
            StartCoroutine(AbrirCofreAnimacion());
        }
        else
        {
            Debug.Log("¡Faltan cristales! Tienes " + cristalesRecolectados + "/" + cristalesTotales);
        }
    }

    IEnumerator AbrirCofreAnimacion()
    {
        isBusy = true;
        anim.SetTrigger("Take");

        Vector3 mirarHacia = new Vector3(objetoCercano.transform.position.x, transform.position.y, objetoCercano.transform.position.z);
        transform.LookAt(mirarHacia);

        yield return new WaitForSeconds(0.5f);

        Debug.Log("¡Cofre Abierto! Llave conseguida.");
        tieneLlave = true;
        if (mensajeLlave != null) mensajeLlave.SetActive(true);

        // Opcional: Destruir cofre o cambiar sprite
        // Destroy(objetoCercano);

        yield return new WaitForSeconds(0.5f);
        isBusy = false;
    }

    void ActualizarUI()
    {
        if (textoContador != null)
            textoContador.text = cristalesRecolectados + " / " + cristalesTotales;
    }

    // --------------------------------------------------------
    // DETECCIÓN (TRIGGERS)
    // --------------------------------------------------------
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Escalera"))
        {
            escaleraActual = other.GetComponent<Escalera>();
            objetoCercano = other.gameObject;
        }
        else if (other.CompareTag("Cristal") || other.CompareTag("Cofre"))
        {
            objetoCercano = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Si me alejo del objeto que tenía en la mira, lo olvido
        if (objetoCercano == other.gameObject)
        {
            objetoCercano = null;
            escaleraActual = null;
            if (isClimbing) StopClimbing();
        }
    }

    // --------------------------------------------------------
    // MOVIMIENTO Y ESCALADA
    // --------------------------------------------------------
    void HandleMovementAndJump()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 inputDir = new Vector3(horizontal, 0f, vertical).normalized;
        Vector3 moveDir = Vector3.zero;

        if (anim != null) anim.SetFloat("Speed", inputDir.magnitude);

        if (inputDir.magnitude >= 0.1f)
        {
            // AQUI ES DONDE DABAS EL ERROR, AHORA YA EXISTE cameraTransform
            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float angle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            moveDir = transform.forward * speed;
        }

        if (controller.isGrounded)
        {
            if (velocity.y < 0) velocity.y = -2f;
            if (Input.GetButtonDown("Jump")) velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        if (anim != null)
        {
            anim.SetBool("IsGrounded", controller.isGrounded);
            anim.SetFloat("VerticalSpeed", velocity.y);
        }

        Vector3 totalMove = (moveDir + new Vector3(0, velocity.y, 0)) * Time.deltaTime;
        controller.Move(totalMove);

        if (Input.GetButtonDown("Fire1") || Input.GetKeyDown(KeyCode.F)) anim.SetTrigger("Attack");
    }

    void StartClimbing()
    {
        isClimbing = true;
        velocity = Vector3.zero; // Resetear gravedad
        anim.SetBool("IsClimbing", true);
        anim.SetTrigger("startscaling"); // Disparar animación inicial

        // (Opcional) Alinear personaje con la escalera y ponerlo en la base
        if (escaleraActual != null)
        {
            transform.rotation = Quaternion.LookRotation(escaleraActual.transform.forward); // Mirar hacia la escalera
            // Mover al jugador a la posición X y Z de la base, manteniendo su Y (o usar Lerp para que sea suave)
            controller.enabled = false; // Desactivar controller un momento para teletransportar
            transform.position = new Vector3(escaleraActual.puntoBase.position.x, transform.position.y, escaleraActual.puntoBase.position.z);
            controller.enabled = true;
        }
    }

    void HandleClimbing()
    {
        float verticalInput = Input.GetAxis("Vertical"); // W/S o Flechas Arriba/Abajo

        // Moverse solo Arriba/Abajo
        Vector3 climbMove = new Vector3(0, verticalInput * climbSpeed, 0);
        controller.Move(climbMove * Time.deltaTime);

        // Chequear si llegamos a la cima
        if (verticalInput > 0 && escaleraActual != null)
        {
            // Si la distancia vertical entre los pies y la cima es pequeña
            if (transform.position.y >= escaleraActual.puntoCima.position.y - 0.5f)
            {
                StartCoroutine(FinishClimbingRoutine());
            }
        }

        // Chequear si bajamos al suelo para salir de la escalera
        if (verticalInput < 0 && controller.isGrounded)
        {
            StopClimbing();
        }
    }

    IEnumerator FinishClimbingRoutine()
    {
        isBusy = true;
        anim.SetTrigger("ClimbTop");
        Vector3 startPos = transform.position;
        Vector3 targetPos = escaleraActual.puntoCima.position;

        Vector3 direccionHaciaCima = (targetPos - startPos);
        direccionHaciaCima.y = 0;
        Quaternion rotacionFija = transform.rotation;
        if (direccionHaciaCima != Vector3.zero) rotacionFija = Quaternion.LookRotation(direccionHaciaCima);

        controller.enabled = false;
        float duration = 1.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            transform.rotation = rotacionFija;
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = rotacionFija;
        controller.enabled = true;
        StopClimbing();
        isBusy = false;
    }

    void StopClimbing()
    {
        isClimbing = false;
        anim.SetBool("IsClimbing", false);
    }

    void CheckFallOffMap()
    {
        if (transform.position.y < fallLimit) SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}