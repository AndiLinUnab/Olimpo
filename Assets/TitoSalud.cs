using UnityEngine;
using UnityEngine.SceneManagement; // IMPORTANTE: Esto permite reiniciar la escena

public class TitoSalud : MonoBehaviour
{
    [Header("Configuración Salud")]
    public int vida = 4; // Los golpes que aguanta
    private bool estaMuerto = false;

    private Animator anim;
    private Tito scriptMovimiento; // Referencia al script de movimiento

    void Start()
    {
        anim = GetComponent<Animator>();
        scriptMovimiento = GetComponent<Tito>();
    }

    public void RecibirDano(int cantidad)
    {
        if (estaMuerto) return;

        vida -= cantidad;
        Debug.Log("Tito recibió daño. Vida restante: " + vida);

        if (vida > 0)
        {
            anim.SetTrigger("Hurt"); // Animación de dolor
        }
        else
        {
            Morir();
        }
    }

    void Morir()
    {
        if (estaMuerto) return; // Evitar morir dos veces
        estaMuerto = true;

        // 1. Activar animación de muerte
        anim.SetBool("IsDead", true);

        // 2. Desactivar el movimiento para que no puedas caminar muerto
        if (scriptMovimiento != null) scriptMovimiento.enabled = false;

        // 3. Desactivar el CharacterController para que los enemigos dejen de empujarte
        GetComponent<CharacterController>().enabled = false;

        Debug.Log("¡Tito ha muerto! Reiniciando en 3 segundos...");

        // 4. Llamar a la función de reinicio con 3 segundos de retraso
        Invoke("ReiniciarNivel", 3f);
    }

    void ReiniciarNivel()
    {
        // Carga la escena actual de nuevo (resetea todo)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}