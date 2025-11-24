using UnityEngine;

public class EsqueletoSimple : MonoBehaviour
{
    [Header("Configuración Movimiento")]
    public Transform objetivo; // Arrastra a TITO aquí
    public float velocidad = 3f;
    public float rangoDeteccion = 8f; // A qué distancia empieza a seguirte
    public float rangoAtaque = 1.5f;  // A qué distancia se detiene para pegar

    [Header("Combate")]
    public int vida = 3;
    public float cooldownAtaque = 2f;
    private float tiempoProximoAtaque;

    [Header("Referencias")]
    public GameObject[] corazonesUI; // Los 3 corazones de su cabeza
    public PuertaSalida paredParaAbrir; // La pared que sube al morir
    private Animator anim;
    private DanoGolpe scriptDanoPuno; // El script de su mano
    private bool muerto = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        scriptDanoPuno = GetComponentInChildren<DanoGolpe>();

        if (scriptDanoPuno != null) scriptDanoPuno.tagObjetivo = "Player"; // Que pegue a Tito

        // Si olvidaste asignar a Tito, lo busca por su Tag
        if (objetivo == null && GameObject.FindGameObjectWithTag("Player") != null)
        {
            objetivo = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    void Update()
    {
        if (muerto || objetivo == null) return;

        // Calcular distancia entre Esqueleto y Tito
        float distancia = Vector3.Distance(transform.position, objetivo.position);

        // 1. SI ESTÁ DENTRO DEL RANGO DE VISIÓN
        if (distancia < rangoDeteccion)
        {
            // Hacer que el esqueleto mire a Tito (pero sin inclinarse hacia arriba/abajo)
            Vector3 mirarA = new Vector3(objetivo.position.x, transform.position.y, objetivo.position.z);
            transform.LookAt(mirarA);

            // 2. SI ESTÁ LEJOS PARA PEGAR, CAMINA
            if (distancia > rangoAtaque)
            {
                // Moverse hacia el objetivo
                transform.position = Vector3.MoveTowards(transform.position, mirarA, velocidad * Time.deltaTime);
                anim.SetFloat("Speed", 1); // Activar animación caminar
            }
            // 3. SI ESTÁ CERCA, ATACA
            else
            {
                anim.SetFloat("Speed", 0); // Quieto

                if (Time.time >= tiempoProximoAtaque)
                {
                    Atacar();
                }
            }
        }
        else
        {
            // Si Tito está lejos, se queda quieto
            anim.SetFloat("Speed", 0);
        }
    }

    void Atacar()
    {
        anim.SetTrigger("Attack"); // Animación puñetazo
        tiempoProximoAtaque = Time.time + cooldownAtaque;

        // Activar daño justo un momento después (puedes ajustar el 0.3f)
        Invoke("ActivarHitbox", 0.3f);
        Invoke("DesactivarHitbox", 0.8f);
    }

    void ActivarHitbox() { if (scriptDanoPuno != null) scriptDanoPuno.ActivarHitbox(); }
    void DesactivarHitbox() { if (scriptDanoPuno != null) scriptDanoPuno.DesactivarHitbox(); }

    // --- SISTEMA DE VIDA ---
    public void RecibirDano(int cantidad)
    {
        if (muerto) return;

        vida -= cantidad;
        anim.SetTrigger("Hit"); // Animación de dolor
        ActualizarCorazones();

        if (vida <= 0)
        {
            Morir();
        }
    }

    void ActualizarCorazones()
    {
        // Recorremos los corazones de la UI
        for (int i = 0; i < corazonesUI.Length; i++)
        {
            if (i < vida) corazonesUI[i].SetActive(true); // Visible
            else corazonesUI[i].SetActive(false); // Invisible
        }
    }

    void Morir()
    {
        muerto = true;
        anim.SetBool("Die", true);

        // Desactivar colisiones para que no estorbe
        GetComponent<Collider>().enabled = false;

        // Abrir la pared
        if (paredParaAbrir != null) paredParaAbrir.AbrirPared();

        // Destruir el cuerpo luego de 5 segundos
        Destroy(gameObject, 5f);
    }
}