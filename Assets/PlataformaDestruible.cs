using UnityEngine;
using System.Collections;

public class PlataformaDestruible : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform jugador;  // ✅ Arrastra a Tito aquí

    [Header("Configuración de Destrucción")]
    [SerializeField] private float tiempoParaDestruir = 2f;  // Segundos antes de destruirse
    [SerializeField] private float tiempoParaReconstruir = 3f;  // Segundos antes de reaparecer

    [Header("Efectos Visuales")]
    [SerializeField] private bool cambiarColorAlActivar = true;
    [SerializeField] private Color colorNormal = Color.white;
    [SerializeField] private Color colorAdvertencia = Color.yellow;
    [SerializeField] private Color colorDestruyendo = Color.red;
    [SerializeField] private bool temblarAlActivar = true;
    [SerializeField] private float intensidadTemblor = 0.05f;

    [Header("Audio (Opcional)")]
    [SerializeField] private AudioClip sonidoActivacion;
    [SerializeField] private AudioClip sonidoDestruccion;

    private Renderer platformRenderer;
    private Collider platformCollider;
    private Vector3 posicionOriginal;
    private bool jugadorEncima = false;
    private bool estaDestruida = false;
    private bool enProceso = false;
    private Color colorOriginal;
    private AudioSource audioSource;

    void Start()
    {
        platformRenderer = GetComponent<Renderer>();
        platformCollider = GetComponent<Collider>();
        posicionOriginal = transform.position;

        // Guardar color original
        if (platformRenderer != null && platformRenderer.material != null)
        {
            colorOriginal = platformRenderer.material.color;
            if (cambiarColorAlActivar && colorNormal == Color.white)
            {
                colorNormal = colorOriginal;
            }
        }

        // Configurar audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (sonidoActivacion != null || sonidoDestruccion != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        if (jugador == null)
        {
            Debug.LogWarning("⚠️ No has asignado al jugador en la plataforma destruible: " + gameObject.name);
        }
    }

    void Update()
    {
        if (jugador == null || estaDestruida || enProceso) return;

        // Verificar si el jugador está encima de la plataforma
        bool jugadorAhoraEncima = EstaJugadorEncima();

        // Si el jugador acaba de subirse
        if (jugadorAhoraEncima && !jugadorEncima)
        {
            IniciarDestruccion();
        }

        jugadorEncima = jugadorAhoraEncima;
    }

    bool EstaJugadorEncima()
    {
        if (platformCollider == null) return false;

        // Verificar si el jugador está sobre la plataforma
        Bounds bounds = platformCollider.bounds;
        Vector3 posJugador = jugador.position;

        // Expandir un poco hacia arriba para detectar mejor
        float margen = 0.5f;

        // Verificar que esté en el área horizontal
        bool dentroX = posJugador.x >= bounds.min.x - margen && posJugador.x <= bounds.max.x + margen;
        bool dentroZ = posJugador.z >= bounds.min.z - margen && posJugador.z <= bounds.max.z + margen;

        // Verificar que esté justo encima (a una altura razonable)
        bool encima = posJugador.y >= bounds.min.y && posJugador.y <= bounds.max.y + 2f;

        return dentroX && dentroZ && encima;
    }

    void IniciarDestruccion()
    {
        if (enProceso) return;

        StartCoroutine(ProcesoDestruccion());
    }

    IEnumerator ProcesoDestruccion()
    {
        enProceso = true;

        // Reproducir sonido de activación
        if (audioSource != null && sonidoActivacion != null)
        {
            audioSource.PlayOneShot(sonidoActivacion);
        }

        // Fase 1: Advertencia (cambio de color y temblor)
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < tiempoParaDestruir)
        {
            // Interpolación de color
            if (cambiarColorAlActivar && platformRenderer != null)
            {
                float progreso = tiempoTranscurrido / tiempoParaDestruir;
                Color colorActual = Color.Lerp(colorNormal, colorDestruyendo, progreso);
                platformRenderer.material.color = colorActual;
            }

            // Temblor
            if (temblarAlActivar)
            {
                float intensidad = intensidadTemblor * (tiempoTranscurrido / tiempoParaDestruir);
                Vector3 desplazamiento = new Vector3(
                    Random.Range(-intensidad, intensidad),
                    0,
                    Random.Range(-intensidad, intensidad)
                );
                transform.position = posicionOriginal + desplazamiento;
            }

            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }

        // Fase 2: Destrucción
        Destruir();

        // Esperar antes de reconstruir
        yield return new WaitForSeconds(tiempoParaReconstruir);

        // Fase 3: Reconstrucción
        Reconstruir();

        enProceso = false;
        jugadorEncima = false;
    }

    void Destruir()
    {
        estaDestruida = true;

        // Reproducir sonido de destrucción
        if (audioSource != null && sonidoDestruccion != null)
        {
            audioSource.PlayOneShot(sonidoDestruccion);
        }

        // Restaurar posición original (por si estaba temblando)
        transform.position = posicionOriginal;

        // Ocultar la plataforma
        if (platformRenderer != null)
        {
            platformRenderer.enabled = false;
        }

        // Desactivar colisión
        if (platformCollider != null)
        {
            platformCollider.enabled = false;
        }

        Debug.Log($"💥 Plataforma '{gameObject.name}' destruida!");
    }

    void Reconstruir()
    {
        estaDestruida = false;

        // Mostrar la plataforma
        if (platformRenderer != null)
        {
            platformRenderer.enabled = true;
            platformRenderer.material.color = colorNormal;
        }

        // Reactivar colisión
        if (platformCollider != null)
        {
            platformCollider.enabled = true;
        }

        // Asegurar posición original
        transform.position = posicionOriginal;

        Debug.Log($"✅ Plataforma '{gameObject.name}' reconstruida!");
    }

    // Método alternativo: detección por colisión (respaldo)
    void OnCollisionEnter(Collision collision)
    {
        if (enProceso || estaDestruida) return;

        if (jugador != null && (collision.transform == jugador || collision.transform.IsChildOf(jugador)))
        {
            // Verificar que el jugador esté encima (no golpeando desde el lado)
            if (collision.contacts.Length > 0)
            {
                Vector3 normal = collision.contacts[0].normal;
                // Si la normal apunta hacia arriba, el jugador está encima
                if (normal.y < -0.5f)
                {
                    IniciarDestruccion();
                }
            }
        }
    }

    // Visualización en el editor
    void OnDrawGizmosSelected()
    {
        if (platformCollider == null) return;

        Bounds bounds = platformCollider.bounds;

        // Área de detección
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        float margen = 0.5f;
        Vector3 size = bounds.size + new Vector3(margen * 2, 2f, margen * 2);
        Vector3 center = new Vector3(bounds.center.x, bounds.center.y + 1f, bounds.center.z);
        Gizmos.DrawCube(center, size);

        // Borde de la plataforma
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
}