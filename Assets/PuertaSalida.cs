using UnityEngine;

public class PuertaSalida : MonoBehaviour
{
    public float velocidadSubida = 2f;
    public float alturaMeta = 5f; // Cuánto subirá
    private bool abrir = false;
    private float alturaInicialY;

    void Start()
    {
        alturaInicialY = transform.position.y;
    }

    void Update()
    {
        if (abrir && transform.position.y < alturaInicialY + alturaMeta)
        {
            transform.Translate(Vector3.up * velocidadSubida * Time.deltaTime);
        }
    }

    public void AbrirPared()
    {
        abrir = true;
    }
}