using UnityEngine;
using UnityEngine.SceneManagement;

public class TitoSalud : MonoBehaviour
{
    public int vidaMaxima = 4;
    public int vidaActual;
    public GameObject[] corazonesUI; // Arrastra aquí las 4 imágenes del Canvas

    private Animator anim;

    void Start()
    {
        vidaActual = vidaMaxima;
        anim = GetComponent<Animator>();
        ActualizarCorazones();
    }

    // Este nombre "RecibirDano" es importante para que el puño lo encuentre
    public void RecibirDano(int cantidad)
    {
        if (vidaActual <= 0) return;

        vidaActual -= cantidad;
        anim.SetTrigger("Hurt"); // Asegúrate de tener este trigger en Tito también
        ActualizarCorazones();

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    void ActualizarCorazones()
    {
        for (int i = 0; i < corazonesUI.Length; i++)
        {
            // Si el índice es menor que la vida, encendemos el corazón, si no, lo apagamos
            if (i < vidaActual) corazonesUI[i].SetActive(true);
            else corazonesUI[i].SetActive(false);
        }
    }

    void Morir()
    {
        anim.SetBool("IsDead", true);
        GetComponent<Tito>().enabled = false; // Desactivar movimiento
        Debug.Log("GAME OVER");
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reiniciar nivel
    }
}