using UnityEngine;

public class Tito : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadMovimiento = 5f;
    public float fuerzaSalto = 7f;
    
    [Header("Detección de Suelo")]
    public float alturaRaycast = 1.1f;
    
    private Rigidbody rb;
    private bool estaEnSuelo;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Configurar Rigidbody
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }
    
    void Update()
    {
        // Verificar si está en el suelo con Raycast
        estaEnSuelo = Physics.Raycast(transform.position, Vector3.down, alturaRaycast);
        
        // Saltar con ESPACIO
        if (Input.GetKeyDown(KeyCode.Space) && estaEnSuelo)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * fuerzaSalto, ForceMode.Impulse);
        }
    }
    
    void FixedUpdate()
    {
        // Movimiento con WASD
        float horizontal = 0f;
        float vertical = 0f;
        
        if (Input.GetKey(KeyCode.W)) vertical = 1f;      // Adelante
        if (Input.GetKey(KeyCode.S)) vertical = -1f;     // Atrás
        if (Input.GetKey(KeyCode.A)) horizontal = -1f;   // Izquierda
        if (Input.GetKey(KeyCode.D)) horizontal = 1f;    // Derecha
        
        // Aplicar movimiento relativo a la rotación del personaje
        Vector3 movimiento = transform.right * horizontal + transform.forward * vertical;
        movimiento = movimiento.normalized * velocidadMovimiento;
        
        rb.linearVelocity = new Vector3(movimiento.x, rb.linearVelocity.y, movimiento.z);
    }
    
    // Visualizar el raycast en el editor
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * alturaRaycast);
    }
}
