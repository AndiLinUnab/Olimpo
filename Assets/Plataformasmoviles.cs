using UnityEngine;

public class PlataformaMovil : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float distanciaMovimiento = 5f;  // Cuánto se mueve en total
    [SerializeField] private float velocidad = 2f;  // Velocidad de movimiento
    [SerializeField] private bool moverEnX = true;  // Mover en eje X (izquierda-derecha)
    [SerializeField] private bool moverEnZ = false;  // Mover en eje Z (adelante-atrás)

    [Header("Dirección Aleatoria al Iniciar")]
    [SerializeField] private bool direccionAleatoria = true;  // ✅ Dirección random al empezar

    [Header("Visualización")]
    [SerializeField] private bool mostrarRecorrido = true;  // Ver el recorrido en el editor
    [SerializeField] private Color colorRecorrido = Color.cyan;

    private Vector3 posicionInicial;
    private Vector3 puntoA;
    private Vector3 puntoB;
    private Vector3 objetivo;
    private bool moviendoHaciaB = true;

    void Start()
    {
        // Guardar posición inicial
        posicionInicial = transform.position;

        // Calcular los puntos extremos del movimiento
        CalcularPuntos();

        // ✅ Decidir dirección inicial aleatoriamente
        if (direccionAleatoria)
        {
            // 50% de probabilidad de empezar en cada dirección
            moviendoHaciaB = Random.value > 0.5f;

            // Posicionar en el punto inicial aleatorio
            if (!moviendoHaciaB)
            {
                transform.position = puntoB;
                objetivo = puntoA;
            }
            else
            {
                transform.position = puntoA;
                objetivo = puntoB;
            }
        }
        else
        {
            // Empezar siempre desde el punto A hacia B
            transform.position = puntoA;
            objetivo = puntoB;
        }

        Debug.Log($"🎲 Plataforma '{gameObject.name}' empezó moviendo hacia: {(moviendoHaciaB ? "derecha" : "izquierda")}");
    }

    void CalcularPuntos()
    {
        Vector3 direccion = Vector3.zero;

        if (moverEnX)
        {
            direccion.x = distanciaMovimiento / 2f;
        }

        if (moverEnZ)
        {
            direccion.z = distanciaMovimiento / 2f;
        }

        // Punto A (izquierda/atrás)
        puntoA = posicionInicial - direccion;

        // Punto B (derecha/adelante)
        puntoB = posicionInicial + direccion;
    }

    void Update()
    {
        // Mover hacia el objetivo
        transform.position = Vector3.MoveTowards(transform.position, objetivo, velocidad * Time.deltaTime);

        // Verificar si llegó al objetivo
        if (Vector3.Distance(transform.position, objetivo) < 0.01f)
        {
            // Cambiar de dirección
            CambiarDireccion();
        }
    }

    void CambiarDireccion()
    {
        if (moviendoHaciaB)
        {
            // Ahora mover hacia A
            objetivo = puntoA;
            moviendoHaciaB = false;
        }
        else
        {
            // Ahora mover hacia B
            objetivo = puntoB;
            moviendoHaciaB = true;
        }
    }

    // Hacer que el jugador se mueva con la plataforma
    void OnCollisionEnter(Collision collision)
    {
        // Si algo se sube a la plataforma, hacerlo hijo para que se mueva con ella
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.name == "Tito")
        {
            collision.transform.SetParent(transform);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        // Cuando se baja, quitar la relación padre-hijo
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.name == "Tito")
        {
            collision.transform.SetParent(null);
        }
    }

    // Visualización en el editor
    void OnDrawGizmos()
    {
        if (!mostrarRecorrido) return;

        // Si está en Play, usar los puntos calculados
        if (Application.isPlaying)
        {
            // Dibujar el recorrido
            Gizmos.color = colorRecorrido;
            Gizmos.DrawLine(puntoA, puntoB);

            // Dibujar punto A (izquierda)
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(puntoA, 0.3f);

            // Dibujar punto B (derecha)
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(puntoB, 0.3f);

            // Dibujar posición actual
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.4f);

            // Flecha indicando dirección actual
            Gizmos.color = Color.magenta;
            Vector3 direccionFlecha = (objetivo - transform.position).normalized;
            Gizmos.DrawRay(transform.position, direccionFlecha * 1.5f);
        }
        else
        {
            // En el editor (antes de Play), mostrar preview del recorrido
            Vector3 pos = transform.position;
            Vector3 direccion = Vector3.zero;

            if (moverEnX)
            {
                direccion.x = distanciaMovimiento / 2f;
            }

            if (moverEnZ)
            {
                direccion.z = distanciaMovimiento / 2f;
            }

            Vector3 inicio = pos - direccion;
            Vector3 fin = pos + direccion;

            Gizmos.color = new Color(colorRecorrido.r, colorRecorrido.g, colorRecorrido.b, 0.5f);
            Gizmos.DrawLine(inicio, fin);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(inicio, 0.3f);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(fin, 0.3f);
        }
    }
}