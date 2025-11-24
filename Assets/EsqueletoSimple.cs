using UnityEngine;

public class EsqueletoSimple : MonoBehaviour
{
    [Header("Configuración Movimiento")]
    public Transform objetivo;
    public float velocidad = 3f;
    public float rangoDeteccion = 10f;
    public float rangoAtaque = 2f; // Aumentado un poco para que no se pegue tanto

    [Header("Combate")]
    public int vida = 3;
    public float cooldownAtaque = 2f;
    private float tiempoProximoAtaque;

    [Header("Referencias")]
    public GameObject[] corazonesUI;
    public PuertaSalida paredParaAbrir;

    private Animator anim;
    private DanoGolpe scriptDanoPuno;
    private CharacterController enemyController; // <--- NUEVO
    private bool muerto = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        scriptDanoPuno = GetComponentInChildren<DanoGolpe>();
        enemyController = GetComponent<CharacterController>(); // <--- NUEVO

        if (scriptDanoPuno != null) scriptDanoPuno.tagObjetivo = "Player";

        if (objetivo == null && GameObject.FindGameObjectWithTag("Player") != null)
        {
            objetivo = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    void Update()
    {
        if (muerto || objetivo == null) return;

        // Calculamos la distancia ignorando la altura (para que no se confunda si Tito salta)
        Vector3 posicionTitoPlana = new Vector3(objetivo.position.x, 0, objetivo.position.z);
        Vector3 miPosicionPlana = new Vector3(transform.position.x, 0, transform.position.z);
        float distancia = Vector3.Distance(miPosicionPlana, posicionTitoPlana);

        // Aplicar gravedad simple para que no flote
        Vector3 movimientoGravedad = new Vector3(0, -9.81f, 0);
        enemyController.Move(movimientoGravedad * Time.deltaTime);

        // 1. SI ESTÁ DENTRO DEL RANGO DE VISIÓN
        if (distancia < rangoDeteccion)
        {
            // Mirar a Tito (Solo en el eje Y, para que no se incline)
            Vector3 mirarA = new Vector3(objetivo.position.x, transform.position.y, objetivo.position.z);
            transform.LookAt(mirarA);

            // 2. SI ESTÁ LEJOS PARA PEGAR, CAMINA
            if (distancia > rangoAtaque)
            {
                // Calcular dirección hacia Tito
                Vector3 direccion = (objetivo.position - transform.position).normalized;
                // Moverse usando el Controller (Esto respeta paredes)
                Vector3 movimiento = direccion * velocidad * Time.deltaTime;
                enemyController.Move(movimiento);

                anim.SetFloat("Speed", 1);
            }
            // 3. SI ESTÁ CERCA, ATACA
            else
            {
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
    }

    void Atacar()
    {
        anim.SetTrigger("Attack");
        tiempoProximoAtaque = Time.time + cooldownAtaque;
        Invoke("ActivarHitbox", 0.3f);
        Invoke("DesactivarHitbox", 0.8f);
    }

    void ActivarHitbox() { if (scriptDanoPuno != null) scriptDanoPuno.ActivarHitbox(); }
    void DesactivarHitbox() { if (scriptDanoPuno != null) scriptDanoPuno.DesactivarHitbox(); }

    public void RecibirDano(int cantidad)
    {
        if (muerto) return;
        vida -= cantidad;
        anim.SetTrigger("Hit");
        ActualizarCorazones();
        if (vida <= 0) Morir();
    }

    void ActualizarCorazones()
    {
        for (int i = 0; i < corazonesUI.Length; i++)
        {
            if (i < vida) corazonesUI[i].SetActive(true);
            else corazonesUI[i].SetActive(false);
        }
    }

    void Morir()
    {
        muerto = true;
        anim.SetBool("Die", true);
        enemyController.enabled = false; // Apagar controller al morir
        if (paredParaAbrir != null) paredParaAbrir.AbrirPared();
        Destroy(gameObject, 5f);
    }
}