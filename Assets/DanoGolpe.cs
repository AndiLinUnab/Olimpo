using UnityEngine;

public class DanoGolpe : MonoBehaviour
{
    public string tagObjetivo; // A quién quiero pegar ("Enemigo" o "Player")
    public int dano = 1;
    private bool puedeGolpear = false;

    // Este método lo llamaremos desde el script principal cuando se lance la animación
    public void ActivarHitbox() { puedeGolpear = true; }
    public void DesactivarHitbox() { puedeGolpear = false; }

    private void OnTriggerEnter(Collider other)
    {
        if (puedeGolpear && other.CompareTag(tagObjetivo))
        {
            // Enviamos el mensaje de daño al objeto que tocamos
            other.SendMessage("RecibirDano", dano, SendMessageOptions.DontRequireReceiver);
            puedeGolpear = false; // Desactivar para no pegar 2 veces en el mismo frame
        }
    }
}