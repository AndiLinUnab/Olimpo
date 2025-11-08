using UnityEngine;
using UnityEngine.SceneManagement;

public class Lava : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private Transform jugador;  // ✅ Arrastra a Tito aquí
    [SerializeField] private float margenDeteccion = 0.3f;  // Margen de detección (más pequeño = más preciso)

    [Header("Detección Mejorada")]
    [SerializeField] private LayerMask layerLava;  // Opcional: Layer específico para la lava
    [SerializeField] private bool usarDeteccionVertical = true;  // Verificar que esté cayendo en la lava

    [Header("Visual (Opcional)")]
    [SerializeField] private bool mostrarMensaje = true;
    [SerializeField] private string mensajeMuerte = "🔥 ¡Tito cayó en la lava!";
    [SerializeField] private bool mostrarDebugRayos = false;  // Ver los rayos de detección

    private bool jugadorMuerto = false;
    private Collider lavaCollider;

    void Start()
    {
        lavaCollider = GetComponent<Collider>();

        if (lavaCollider == null)
        {
            Debug.LogError("⚠️ La lava necesita un Collider!");
        }

        if (jugador == null)
        {
            Debug.LogError("⚠️ No has asignado al jugador en el script de Lava!");
        }

        // Asegurar que el collider sea trigger
        if (lavaCollider != null)
        {
            lavaCollider.isTrigger = true;
        }
    }

    void Update()
    {
        if (jugador == null || jugadorMuerto) return;

        // Verificar si Tito está tocando la lava
        if (EstaTocandoLava())
        {
            MatarJugador();
        }
    }

    bool EstaTocandoLava()
    {
        if (lavaCollider == null) return false;

        Vector3 posJugador = jugador.position;
        Bounds bounds = lavaCollider.bounds;

        // Verificación vertical: está a la altura de la lava o por debajo
        float alturaLava = bounds.max.y;  // Parte superior de la lava

        if (posJugador.y <= alturaLava + margenDeteccion)
        {
            // Verificar que esté dentro del área horizontal (X y Z)
            Vector3 posHorizontal = new Vector3(posJugador.x, alturaLava, posJugador.z);

            // Expandir los bounds un poco para el margen
            Bounds expandedBounds = bounds;
            expandedBounds.Expand(margenDeteccion * 2);

            if (expandedBounds.Contains(posHorizontal))
            {
                if (mostrarDebugRayos)
                {
                    Debug.DrawLine(jugador.position, posHorizontal, Color.red, 0.1f);
                    Debug.DrawLine(posHorizontal, new Vector3(posHorizontal.x, bounds.min.y, posHorizontal.z), Color.yellow, 0.1f);
                }
                return true;
            }
        }

        if (mostrarDebugRayos)
        {
            Debug.DrawLine(jugador.position, new Vector3(posJugador.x, alturaLava, posJugador.z), Color.green, 0.1f);
        }

        return false;
    }

    // Trigger como método de respaldo
    void OnTriggerEnter(Collider other)
    {
        if (jugadorMuerto) return;

        // Verificar si es el jugador
        if (jugador != null && (other.transform == jugador || other.transform.IsChildOf(jugador)))
        {
            MatarJugador();
        }
    }

    // Trigger continuo (mientras está dentro)
    void OnTriggerStay(Collider other)
    {
        if (jugadorMuerto) return;

        if (jugador != null && (other.transform == jugador || other.transform.IsChildOf(jugador)))
        {
            MatarJugador();
        }
    }

    void MatarJugador()
    {
        if (jugadorMuerto) return;

        jugadorMuerto = true;

        if (mostrarMensaje)
        {
            Debug.Log(mensajeMuerte);
        }

        // Reiniciar la escena actual
        Invoke("ReiniciarEscena", 0.1f);  // Pequeño delay para evitar bugs
    }

    void ReiniciarEscena()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Validación en el editor
    void OnValidate()
    {
        if (jugador == null)
        {
            Debug.LogWarning("⚠️ No has asignado al jugador en el script de Lava.");
        }
    }

    // Visualización mejorada en el editor
    void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        // Área de la lava (naranja sólido)
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.7f);
        Gizmos.matrix = transform.localToWorldMatrix;

        if (col is BoxCollider)
        {
            BoxCollider box = col as BoxCollider;
            Gizmos.DrawCube(box.center, box.size);
        }

        // Área de detección (amarillo transparente)
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.3f);
        if (col is BoxCollider)
        {
            BoxCollider box = col as BoxCollider;
            Vector3 expandedSize = box.size + Vector3.one * (margenDeteccion * 2);
            Gizmos.DrawCube(box.center, expandedSize);
        }

        // Dibujar borde del área
        Gizmos.color = Color.red;
        if (col is BoxCollider)
        {
            BoxCollider box = col as BoxCollider;
            Gizmos.DrawWireCube(box.center, box.size);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Cuando está seleccionada, mostrar más detalles
        if (jugador != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(jugador.position, 0.5f);

            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                Vector3 puntoMasCercano = col.ClosestPoint(jugador.position);
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(jugador.position, puntoMasCercano);
                Gizmos.DrawWireSphere(puntoMasCercano, 0.2f);
            }
        }
    }
}