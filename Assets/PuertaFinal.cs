using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para cambiar de escena

public class PuertaFinal : MonoBehaviour
{
    [Header("Configuración")]
    public string nombreEscenaMenu = "MenuPrincipal"; // El nombre exacto de tu escena
    private bool jugadorCerca = false;
    private Tito scriptTito; // Para conectar con el jugador

    void Update()
    {
        // Si el jugador está cerca y presiona E
        if (jugadorCerca && Input.GetKeyDown(KeyCode.E))
        {
            IntentarEscapar();
        }
    }

    void IntentarEscapar()
    {
        if (scriptTito != null)
        {
            // Preguntamos a Tito si tiene todo lo necesario
            if (scriptTito.PuedeGanar())
            {
                Debug.Log("¡JUEGO COMPLETADO! Cargando menú...");
                // Cargar la escena del menú
                SceneManager.LoadScene(nombreEscenaMenu);
            }
            else
            {
                Debug.Log("La puerta está cerrada con magia... necesitas la llave y los cristales.");
            }
        }
    }

    // Detectar cuando Tito se acerca
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = true;
            scriptTito = other.GetComponent<Tito>();
            Debug.Log("Presiona E para escapar (si tienes la llave)");
        }
    }

    // Detectar cuando Tito se aleja
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = false;
            scriptTito = null;
        }
    }
}