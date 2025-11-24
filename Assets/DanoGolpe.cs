using UnityEngine;

public class DanoGolpe : MonoBehaviour
{
    public string tagObjetivo; // "Enemigo" para Tito, "Player" para Esqueleto
    public int dano = 1;
    private bool puedeGolpear = false;

    public void ActivarHitbox() { puedeGolpear = true; }
    public void DesactivarHitbox() { puedeGolpear = false; }

    private void OnTriggerEnter(Collider other)
    {
        // Solo procesamos si el hitbox está activo
        if (puedeGolpear)
        {
            // Chequeo de seguridad por si te golpeas a ti mismo
            if (other.gameObject == transform.root.gameObject) return;

            if (other.CompareTag(tagObjetivo))
            {
                Debug.Log("¡GOLPE CONECTADO! He pegado a: " + other.name); // <--- MIRA LA CONSOLA

                other.SendMessage("RecibirDano", dano, SendMessageOptions.DontRequireReceiver);
                puedeGolpear = false; // Desactivar para no dar doble golpe
            }
        }
    }
}