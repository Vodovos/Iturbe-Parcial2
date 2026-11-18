using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;        // El objeto a seguir (nuestro Player)
    public Vector3 offset = new Vector3(0f, 6f, -6f); // Distancia de la cámara respecto al jugador
    public float smoothSpeed = 5f;  // Velocidad de suavizado del movimiento

    void LateUpdate()
    {
        if (target == null) return;

        // Posición deseada basada en la posición del jugador más el offset
        Vector3 desiredPosition = target.position + offset;

        // Movimiento suavizado (Lerp) hacia la posición deseada
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        // Opcional: hacer que la cámara siempre mire al jugador
        transform.LookAt(target.position + Vector3.up * 1f);
    }
}
