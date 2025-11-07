using UnityEngine;

public class ColisionSuelo : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnCollisionEnter(Collision collision)
{
    if (collision.gameObject.CompareTag("Suelo"))
    {
        Debug.Log("Tocando el suelo");
    }
}

void OnCollisionExit(Collision collision)
{
    if (collision.gameObject.CompareTag("Suelo"))
    {
        Debug.Log("Dejó de tocar el suelo");
    }
}
}
