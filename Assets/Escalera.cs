using UnityEngine;



public class Escalera : MonoBehaviour
{
    // Punto donde el personaje debe terminar al subir (La cima)
    public Transform puntoCima;
    // Punto donde el personaje se debe colocar para empezar a subir (La base)
    public Transform puntoBase;

    private void OnDrawGizmos()
    {
        // Esto dibuja unas bolitas en el editor para que veas los puntos
        if (puntoCima != null) { Gizmos.color = Color.red; Gizmos.DrawSphere(puntoCima.position, 0.3f); }
        if (puntoBase != null) { Gizmos.color = Color.blue; Gizmos.DrawSphere(puntoBase.position, 0.3f); }
    }
}