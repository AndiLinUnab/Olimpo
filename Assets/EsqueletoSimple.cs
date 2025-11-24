using UnityEngine;

public class EsqueletoSimple : MonoBehaviour
{
    [Header("Configuración Movimiento")]
    public Transform objetivo;
    public float velocidad = 2.5f;
    public float rangoDeteccion = 10f;
    public float rangoAtaque = 3.0f; // Mantén esto en 2.0 para que no te encime

    [Header("COMBATE")]
    public Transform puntoAtaque;
    public float radioGolpe = 1.5f;
    public LayerMask capaJugador;

    [Header("Vida")]
    public int vida = 3;
    public float cooldownAtaque = 2f;
    private float tiempoProximoAtaque;
    public PuertaSalida paredParaAbrir;

    private Animator anim;
    private CharacterController enemyController;
    private bool muerto = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        enemyController = GetComponent<CharacterController>();

        if (objetivo == null && GameObject.FindGameObjectWithTag("Player") != null)
            objetivo = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (muerto) return; // Si está muerto, no hace NADA más.

        if (objetivo == null) return;

        Vector3 movimientoFinal = Vector3.zero;

        // 1. Calcular distancias (Plano XZ)
        Vector3 miPos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 titoPos = new Vector3(objetivo.position.x, 0, objetivo.position.z);
        float distancia = Vector3.Distance(miPos, titoPos);

        // 2. GRAVEDAD (Siempre se aplica para que no flote)
        if (!enemyController.isGrounded)
        {
            movimientoFinal.y = -9.81f;
        }

        // 3. LÓGICA DE IA
        if (distancia < rangoDeteccion)
        {
            // Rotar hacia Tito
            Vector3 direccion = (titoPos - miPos).normalized;
            if (direccion != Vector3.zero)
            {
                Quaternion rotacion = Quaternion.LookRotation(direccion);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotacion, 5f * Time.deltaTime);
            }

            // DECISIÓN: ¿CAMINAR O ATACAR?
            if (distancia > rangoAtaque)
            {
                // CAMINAR: Añadimos velocidad al movimiento
                movimientoFinal.x = direccion.x * velocidad;
                movimientoFinal.z = direccion.z * velocidad;
                anim.SetFloat("Speed", 1);
            }
            else
            {
                // ATACAR: No sumamos nada a X ni Z, se queda quieto.
                anim.SetFloat("Speed", 0);

                if (Time.time >= tiempoProximoAtaque)
                {
                    Atacar();
                }
            }
        }
        else
        {
            anim.SetFloat("Speed", 0);
        }

        // 4. APLICAR MOVIMIENTO FINAL
        // Movemos el controller una sola vez por frame con todos los cálculos
        enemyController.Move(movimientoFinal * Time.deltaTime);
    }

    void Atacar()
    {
        anim.SetTrigger("Attack");
        tiempoProximoAtaque = Time.time + cooldownAtaque;
        Invoke("EjecutarGolpe", 0.4f);
    }

    void EjecutarGolpe()
    {
        if (muerto || puntoAtaque == null) return;

        Collider[] jugadorgolpeado = Physics.OverlapSphere(puntoAtaque.position, radioGolpe, capaJugador);
        foreach (Collider jugador in jugadorgolpeado)
        {
            // Buscamos el script de salud directamente
            TitoSalud saludTito = jugador.GetComponentInParent<TitoSalud>();
            if (saludTito != null)
            {
                saludTito.RecibirDano(1);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (puntoAtaque != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(puntoAtaque.position, radioGolpe);
        }
    }

    public void RecibirDano(int cantidad)
    {
        if (muerto) return;
        vida -= cantidad;
        anim.SetTrigger("Hit");
        if (vida <= 0) Morir();
    }

    void Morir()
    {
        muerto = true; // Esto bloquea el Update inmediatamente
        anim.SetBool("Die", true);

        // Desactivar el controller para que Tito pueda atravesar el cadáver y no se choque
        enemyController.enabled = false;

        if (paredParaAbrir != null) paredParaAbrir.AbrirPared();
        Destroy(gameObject, 5f);
    }
}