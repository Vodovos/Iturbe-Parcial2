using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    private CharacterController controller;
    private Transform camTransform;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        // Si no tienes CharacterController, puedes usar Rigidbody, pero con CharacterController es directo.
        if (controller == null)
        {
            controller = gameObject.AddComponent<CharacterController>();
        }

        if (Camera.main != null)
        {
            camTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        // Leer entradas de teclado (WASD / Flechas)
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 moveDir = new Vector3(moveX, 0f, moveZ).normalized;

        if (moveDir.magnitude >= 0.1f)
        {
            // Rotar suavemente hacia la dirección del movimiento
            float targetAngle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg + camTransform.eulerAngles.y;
            Quaternion rotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);

            Vector3 moveDirRotated = rotation * Vector3.forward;
            controller.Move(moveDirRotated.normalized * moveSpeed * Time.deltaTime);
        }

        // Aplicar gravedad básica para que no flote
        controller.Move(Vector3.down * 9.81f * Time.deltaTime);
    }
}