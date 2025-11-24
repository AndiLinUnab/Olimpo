using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class Tito : MonoBehaviour
{
    [Header("Componentes")]
    private CharacterController controller;
    private Animator anim;
    [SerializeField] private TextMeshProUGUI textoContador;
    [SerializeField] private GameObject mensajeLlave;
    [SerializeField] private Transform cameraTransform;

    [Header("COMBATE (NUEVO)")]
    public Transform puntoAtaque; // <--- Arrastra el objeto vacio aqui
    public float radioAtaque = 1.0f; // Tamaño de la bola de daño
    public LayerMask capaEnemigos; // Selecciona "Default" o la capa de los enemigos

    [Header("Configuración Movimiento")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float climbSpeed = 3f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float fallLimit = -10f;

    private Vector3 velocity;
    private bool isBusy = false;
    private bool isClimbing = false;
    private Escalera escaleraActual;
    private GameObject objetoCercano;
    private int cristalesRecolectados = 0;
    private int cristalesTotales = 3;
    private bool tieneLlave = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        ActualizarUI();
        if (mensajeLlave != null) mensajeLlave.SetActive(false);
        if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        if (isBusy) return;

        if (isClimbing) HandleClimbing();
        else
        {
            HandleMovementAndJump();
            HandleInteraction();
        }
        CheckFallOffMap();
    }

    // --- NUEVO SISTEMA DE ATAQUE ---
    void Atacar()
    {
        anim.SetTrigger("Attack");

        // Detectar enemigos en el rango matemáticamente
        if (puntoAtaque == null) return;

        // Crea una bola invisible y obtiene todo lo que toca
        Collider[] enemigosGolpeados = Physics.OverlapSphere(puntoAtaque.position, radioAtaque, capaEnemigos);

        foreach (Collider enemigo in enemigosGolpeados)
        {
            if (enemigo.CompareTag("Enemigo")) // Si tiene el tag Enemigo
            {
                Debug.Log("¡Golpeaste al esqueleto!");
                enemigo.SendMessage("RecibirDano", 1, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    // Dibujar la bola roja en el editor para que veas el rango
    void OnDrawGizmosSelected()
    {
        if (puntoAtaque == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(puntoAtaque.position, radioAtaque);
    }
    // --------------------------------

    void HandleMovementAndJump()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 inputDir = new Vector3(horizontal, 0f, vertical).normalized;
        Vector3 moveDir = Vector3.zero;

        if (anim != null) anim.SetFloat("Speed", inputDir.magnitude);

        if (inputDir.magnitude >= 0.1f)
        {
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
        else velocity.y += gravity * Time.deltaTime;

        if (anim != null)
        {
            anim.SetBool("IsGrounded", controller.isGrounded);
            anim.SetFloat("VerticalSpeed", velocity.y);
        }

        Vector3 totalMove = (moveDir + new Vector3(0, velocity.y, 0)) * Time.deltaTime;
        controller.Move(totalMove);

        // ATAQUE
        if (Input.GetButtonDown("Fire1") || Input.GetKeyDown(KeyCode.F))
        {
            Atacar(); // Llamada al nuevo sistema
        }
    }

    // ... (El resto del código de escalar y objetos se mantiene igual abajo) ...
    void HandleInteraction()
    {
        if (objetoCercano != null && Input.GetKeyDown(KeyCode.E))
        {
            string tag = objetoCercano.tag;
            if (tag == "Escalera" && !isClimbing) StartClimbing();
            else if (tag == "Cristal") StartCoroutine(RecogerObjeto(objetoCercano));
            else if (tag == "Cofre") IntentarAbrirCofre();
        }
    }
    IEnumerator RecogerObjeto(GameObject objeto)
    {
        isBusy = true; anim.SetTrigger("Take");
        Vector3 mirarHacia = new Vector3(objeto.transform.position.x, transform.position.y, objeto.transform.position.z);
        transform.LookAt(mirarHacia);
        yield return new WaitForSeconds(0.5f);
        if (objeto.CompareTag("Cristal")) { cristalesRecolectados++; ActualizarUI(); Destroy(objeto); objetoCercano = null; }
        yield return new WaitForSeconds(0.5f); isBusy = false;
    }
    void IntentarAbrirCofre() { if (cristalesRecolectados >= cristalesTotales) StartCoroutine(AbrirCofreAnimacion()); else Debug.Log("Faltan cristales"); }
    IEnumerator AbrirCofreAnimacion()
    {
        isBusy = true; anim.SetTrigger("Take");
        Vector3 mirarHacia = new Vector3(objetoCercano.transform.position.x, transform.position.y, objetoCercano.transform.position.z);
        transform.LookAt(mirarHacia);
        yield return new WaitForSeconds(0.5f); tieneLlave = true; if (mensajeLlave != null) mensajeLlave.SetActive(true);
        yield return new WaitForSeconds(0.5f); isBusy = false;
    }
    void ActualizarUI() { if (textoContador != null) textoContador.text = cristalesRecolectados + " / " + cristalesTotales; }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Escalera")) { escaleraActual = other.GetComponent<Escalera>(); objetoCercano = other.gameObject; }
        else if (other.CompareTag("Cristal") || other.CompareTag("Cofre")) objetoCercano = other.gameObject;
    }
    private void OnTriggerExit(Collider other) { if (objetoCercano == other.gameObject) { objetoCercano = null; escaleraActual = null; if (isClimbing) StopClimbing(); } }
    void StartClimbing()
    {
        isClimbing = true; velocity = Vector3.zero; anim.SetBool("IsClimbing", true); anim.SetTrigger("startscaling");
        if (escaleraActual != null)
        {
            controller.enabled = false;
            transform.position = new Vector3(escaleraActual.puntoBase.position.x, transform.position.y, escaleraActual.puntoBase.position.z);
            controller.enabled = true;
            transform.rotation = Quaternion.LookRotation(escaleraActual.transform.forward);
        }
    }
    void HandleClimbing()
    {
        float verticalInput = Input.GetAxis("Vertical"); Vector3 climbMove = new Vector3(0, verticalInput * climbSpeed, 0);
        controller.Move(climbMove * Time.deltaTime);
        if (verticalInput > 0 && escaleraActual != null) { if (transform.position.y >= escaleraActual.puntoCima.position.y - 0.5f) StartCoroutine(FinishClimbingRoutine()); }
        if (verticalInput < 0 && controller.isGrounded) StopClimbing();
    }
    IEnumerator FinishClimbingRoutine()
    {
        isBusy = true; anim.SetTrigger("ClimbTop"); Vector3 startPos = transform.position; Vector3 targetPos = escaleraActual.puntoCima.position;
        Vector3 dir = (targetPos - startPos); dir.y = 0; Quaternion rot = transform.rotation; if (dir != Vector3.zero) rot = Quaternion.LookRotation(dir);
        controller.enabled = false; float dur = 1.2f; float elap = 0f;
        while (elap < dur) { transform.position = Vector3.Lerp(startPos, targetPos, elap / dur); transform.rotation = rot; elap += Time.deltaTime; yield return null; }
        transform.position = targetPos; transform.rotation = rot; controller.enabled = true; StopClimbing(); isBusy = false;
    }
    void StopClimbing() { isClimbing = false; anim.SetBool("IsClimbing", false); }
    void CheckFallOffMap() { if (transform.position.y < fallLimit) SceneManager.LoadScene(SceneManager.GetActiveScene().name); }

    // --- FUNCIÓN NUEVA PARA LA PUERTA FINAL ---
    public bool PuedeGanar()
    {
        // Devuelve VERDADERO solo si tiene la llave Y los 3 cristales
        if (tieneLlave && cristalesRecolectados >= cristalesTotales)
        {
            return true;
        }
        else
        {
            Debug.Log("¡No puedes escapar aún! Tienes llave: " + tieneLlave + " | Cristales: " + cristalesRecolectados + "/3");
            return false;
        }
    }
}