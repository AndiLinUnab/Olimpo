using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Boton : MonoBehaviour
{
    [Header("Configuración del Botón")]
    [SerializeField] private KeyCode teclaActivacion = KeyCode.E;
    [SerializeField] private float distanciaInteraccion = 3f;
    [SerializeField] private Transform jugador;  // ✅ Ahora arrastra directamente a Tito aquí

    [Header("Configuración de la Pared")]
    [SerializeField] private Transform pared;  // Arrastra aquí la pared en el Inspector
    [SerializeField] private float alturaSubida = 5f;  // Cuánto sube la pared
    [SerializeField] private float velocidadMovimiento = 2f;  // Velocidad de subida/bajada
    [SerializeField] private float tiempoArriba = 3f;  // Segundos que permanece arriba
    [SerializeField] private bool usarRigidbody = false;  // ✅ Marcar si la pared tiene Rigidbody
    [SerializeField] private bool matarAlBajar = true;  // ✅ Si aplasta al jugador cuando baja

    [Header("Visual (Opcional)")]
    [SerializeField] private Color colorNormal = Color.gray;
    [SerializeField] private Color colorActivo = Color.green;
    [SerializeField] private Renderer rendererBoton;  // Opcional: para cambiar color del botón

    private Vector3 posicionInicial;
    private Vector3 posicionArriba;
    private bool jugadorCerca = false;
    private bool enMovimiento = false;
    private Rigidbody rbPared;  // ✅ Para mover con física si es necesario

    void Start()
    {
        if (pared == null)
        {
            Debug.LogError("⚠️ No se asignó la pared en el Inspector!");
            return;
        }

        // Verificar si tiene Rigidbody
        rbPared = pared.GetComponent<Rigidbody>();
        if (rbPared != null && usarRigidbody)
        {
            rbPared.isKinematic = true;  // Asegurar que sea kinematic
        }

        // Guardar posiciones
        posicionInicial = pared.position;
        posicionArriba = posicionInicial + Vector3.up * alturaSubida;

        // Configurar color inicial del botón
        if (rendererBoton != null)
        {
            rendererBoton.material.color = colorNormal;
        }
    }

    void Update()
    {
        DetectarJugador();

        // Si el jugador está cerca y presiona E
        if (jugadorCerca && Input.GetKeyDown(teclaActivacion) && !enMovimiento)
        {
            ActivarBoton();
        }
    }

    void DetectarJugador()
    {
        // Verificar si el jugador está asignado
        if (jugador == null)
        {
            Debug.LogWarning("⚠️ No se asignó el jugador en el Inspector del botón!");
            jugadorCerca = false;
            return;
        }

        float distancia = Vector3.Distance(transform.position, jugador.position);
        jugadorCerca = distancia <= distanciaInteraccion;
    }

    void ActivarBoton()
    {
        if (pared == null) return;

        // Cambiar color del botón (opcional)
        if (rendererBoton != null)
        {
            rendererBoton.material.color = colorActivo;
        }

        // Iniciar secuencia: subir → esperar → bajar
        StartCoroutine(SecuenciaPared());
    }

    IEnumerator SecuenciaPared()
    {
        enMovimiento = true;

        // 1. SUBIR la pared
        yield return StartCoroutine(MoverPared(posicionArriba));

        // 2. ESPERAR arriba
        yield return new WaitForSeconds(tiempoArriba);

        // 3. BAJAR la pared (detectando colisión con el jugador)
        yield return StartCoroutine(MoverParedYDetectarJugador(posicionInicial));

        // Resetear estado
        enMovimiento = false;

        // Restaurar color del botón
        if (rendererBoton != null)
        {
            rendererBoton.material.color = colorNormal;
        }
    }

    IEnumerator MoverPared(Vector3 destino)
    {
        // Si tiene Rigidbody, usar MovePosition para física
        if (rbPared != null && usarRigidbody)
        {
            while (Vector3.Distance(pared.position, destino) > 0.01f)
            {
                Vector3 nuevaPos = Vector3.MoveTowards(pared.position, destino, velocidadMovimiento * Time.deltaTime);
                rbPared.MovePosition(nuevaPos);
                yield return new WaitForFixedUpdate();
            }
        }
        else
        {
            // Movimiento directo sin física
            while (Vector3.Distance(pared.position, destino) > 0.01f)
            {
                pared.position = Vector3.MoveTowards(pared.position, destino, velocidadMovimiento * Time.deltaTime);
                yield return null;
            }
        }

        // Asegurar que llegue exactamente al destino
        if (rbPared != null && usarRigidbody)
        {
            rbPared.MovePosition(destino);
        }
        else
        {
            pared.position = destino;
        }
    }

    IEnumerator MoverParedYDetectarJugador(Vector3 destino)
    {
        // Si tiene Rigidbody, usar MovePosition para física
        if (rbPared != null && usarRigidbody)
        {
            while (Vector3.Distance(pared.position, destino) > 0.01f)
            {
                Vector3 nuevaPos = Vector3.MoveTowards(pared.position, destino, velocidadMovimiento * Time.deltaTime);
                rbPared.MovePosition(nuevaPos);

                // ✅ Verificar si el jugador está debajo de la pared
                if (matarAlBajar)
                {
                    VerificarColisionConJugador();
                }

                yield return new WaitForFixedUpdate();
            }
        }
        else
        {
            // Movimiento directo sin física
            while (Vector3.Distance(pared.position, destino) > 0.01f)
            {
                pared.position = Vector3.MoveTowards(pared.position, destino, velocidadMovimiento * Time.deltaTime);

                // ✅ Verificar si el jugador está debajo de la pared
                if (matarAlBajar)
                {
                    VerificarColisionConJugador();
                }

                yield return null;
            }
        }

        // Asegurar que llegue exactamente al destino
        if (rbPared != null && usarRigidbody)
        {
            rbPared.MovePosition(destino);
        }
        else
        {
            pared.position = destino;
        }

        // Verificación final
        if (matarAlBajar)
        {
            VerificarColisionConJugador();
        }
    }

    void VerificarColisionConJugador()
    {
        if (jugador == null || pared == null) return;

        // Obtener el Collider de la pared
        Collider colliderPared = pared.GetComponent<Collider>();
        if (colliderPared == null) return;

        // Obtener el Collider del jugador
        Collider colliderJugador = jugador.GetComponent<Collider>();
        if (colliderJugador == null)
        {
            colliderJugador = jugador.GetComponentInChildren<Collider>();
        }
        if (colliderJugador == null) return;

        // ✅ Verificar si los colliders están en contacto
        if (colliderPared.bounds.Intersects(colliderJugador.bounds))
        {
            Debug.Log("💀 ¡El muro aplastó a Tito! Reiniciando escena...");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    // Visualizar el rango de interacción en el Editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaInteraccion);

        if (pared != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 posInicial = Application.isPlaying ? posicionInicial : pared.position;
            Vector3 posArriba = posInicial + Vector3.up * alturaSubida;
            Gizmos.DrawLine(posInicial, posArriba);
            Gizmos.DrawWireCube(posArriba, pared.localScale);
        }
    }
}