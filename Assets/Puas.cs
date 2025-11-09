using UnityEngine;
using UnityEngine.SceneManagement;

public class Puas : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform jugador;  // ✅ Arrastra a Tito aquí (opcional)

    [Header("Configuración")]
    [SerializeField] private bool usarTrigger = true;  // Si el collider es trigger o sólido
    [SerializeField] private float areaDeteccion = 0.5f;  // Margen de detección adicional

    [Header("Efectos Visuales")]
    [SerializeField] private bool mostrarMensaje = true;
    [SerializeField] private string mensajeMuerte = "💀 ¡Tito fue empalado por los pinchos!";
    [SerializeField] private Color colorPeligro = Color.red;
    [SerializeField] private bool brillarAlTocar = true;

    [Header("Audio (Opcional)")]
    [SerializeField] private AudioClip sonidoMuerte;

    private Renderer pinchosRenderer;
    private Color colorOriginal;
    private AudioSource audioSource;
    private bool jugadorMuerto = false;

    void Start()
    {
        // Obtener Renderer para efectos visuales
        pinchosRenderer = GetComponent<Renderer>();
        if (pinchosRenderer != null)
        {
            colorOriginal = pinchosRenderer.material.color;
        }

        // Configurar audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && sonidoMuerte != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Verificar que tenga collider
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogWarning("⚠️ Los pinchos necesitan un Collider. Añade un Box Collider o Mesh Collider.");
        }
        else
        {
            // Configurar si es trigger o no
            col.isTrigger = usarTrigger;
        }

        // Buscar a Tito si no está asignado
        if (jugador == null)
        {
            GameObject titoObj = GameObject.Find("Tito");
            if (titoObj != null)
            {
                jugador = titoObj.transform;
            }
        }
    }

    void Update()
    {
        // Detección continua por proximidad (más confiable que solo triggers)
        if (jugador != null && !jugadorMuerto)
        {
            float distancia = Vector3.Distance(transform.position, jugador.position);

            if (distancia <= areaDeteccion)
            {
                MatarJugador();
            }
        }
    }

    // Método 1: Detección por Trigger
    void OnTriggerEnter(Collider other)
    {
        if (jugadorMuerto) return;

        // Verificar si es el jugador
        bool esJugador = false;

        if (jugador != null)
        {
            esJugador = (other.transform == jugador || other.transform.IsChildOf(jugador));
        }
        else
        {
            // Fallback: buscar por nombre o tag
            esJugador = (other.gameObject.name == "Tito" ||
                        other.CompareTag("Player") ||
                        other.GetComponent<CharacterController>() != null);
        }

        if (esJugador)
        {
            MatarJugador();
        }
    }

    // Método 2: Detección por Colisión (si no es trigger)
    void OnCollisionEnter(Collision collision)
    {
        if (jugadorMuerto) return;

        bool esJugador = false;

        if (jugador != null)
        {
            esJugador = (collision.transform == jugador || collision.transform.IsChildOf(jugador));
        }
        else
        {
            esJugador = (collision.gameObject.name == "Tito" ||
                        collision.gameObject.CompareTag("Player") ||
                        collision.gameObject.GetComponent<CharacterController>() != null);
        }

        if (esJugador)
        {
            MatarJugador();
        }
    }

    void MatarJugador()
    {
        if (jugadorMuerto) return;

        jugadorMuerto = true;

        // Mostrar mensaje
        if (mostrarMensaje)
        {
            Debug.Log(mensajeMuerte);
        }

        // Efecto visual de muerte
        if (brillarAlTocar)
        {
            StartCoroutine(EfectoMuerte());
        }
        else
        {
            ReiniciarEscena();
        }
    }

    System.Collections.IEnumerator EfectoMuerte()
    {
        // Cambiar color de los pinchos brevemente
        if (pinchosRenderer != null)
        {
            pinchosRenderer.material.color = colorPeligro;
        }

        // Reproducir sonido
        if (audioSource != null && sonidoMuerte != null)
        {
            audioSource.PlayOneShot(sonidoMuerte);
            yield return new WaitForSeconds(sonidoMuerte.length);
        }
        else
        {
            yield return new WaitForSeconds(0.3f);
        }

        ReiniciarEscena();
    }

    void ReiniciarEscena()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Validación en el editor
    void OnValidate()
    {
        // Asegurar que tenga un collider
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogWarning($"⚠️ '{gameObject.name}' necesita un Collider para funcionar como pinchos.");
        }
    }

    // Visualización en el editor
    void OnDrawGizmos()
    {
        // Dibujar área de peligro
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            if (col is BoxCollider)
            {
                BoxCollider box = col as BoxCollider;
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (col is SphereCollider)
            {
                SphereCollider sphere = col as SphereCollider;
                Gizmos.DrawSphere(sphere.center, sphere.radius);
            }
            else if (col is CapsuleCollider)
            {
                CapsuleCollider capsule = col as CapsuleCollider;
                Gizmos.DrawSphere(capsule.center, capsule.radius);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Cuando está seleccionado, mostrar más detalles
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            // Borde rojo brillante
            Gizmos.color = Color.red;
            Gizmos.matrix = transform.localToWorldMatrix;

            if (col is BoxCollider)
            {
                BoxCollider box = col as BoxCollider;
                Gizmos.DrawWireCube(box.center, box.size);

                // Área expandida de detección
                if (areaDeteccion > 0)
                {
                    Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
                    Vector3 expandedSize = box.size + Vector3.one * (areaDeteccion * 2);
                    Gizmos.DrawWireCube(box.center, expandedSize);
                }
            }
        }

        // Mostrar conexión con el jugador si está asignado
        if (jugador != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, jugador.position);

            // Mostrar distancia
            float dist = Vector3.Distance(transform.position, jugador.position);
            if (dist <= areaDeteccion)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(jugador.position, 0.5f);
            }
        }
    }
}