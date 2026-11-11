using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimpleEnemyController : MonoBehaviour
{
    [Header("Ajustes de Enemigo (Editables)")]
    [Tooltip("Velocidad de persecución del enemigo")]
    [SerializeField] private float speed = 4f;

    [Tooltip("Daño infligido al jugador por contacto")]
    [SerializeField] private float contactDamage = 10f;

    [Tooltip("Vida máxima del enemigo")]
    [SerializeField] private float maxHealth = 30f;

    public float ContactDamage => contactDamage;

    private float currentHealth;
    private Transform playerTransform;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Bloqueo de altura Y y rotaciones físicas
        rb.constraints = RigidbodyConstraints.FreezePositionY |
                         RigidbodyConstraints.FreezeRotation;
        rb.useGravity = false;

        currentHealth = maxHealth;

        // Búsqueda del transform del jugador mediante Tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    void FixedUpdate()
    {
        if (playerTransform == null) return;

        // Vector directo hacia el jugador ignorando la altura en Y
        Vector3 direction = (playerTransform.position - transform.position);
        direction.y = 0f;
        direction.Normalize();

        rb.linearVelocity = direction * speed;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        // Acá más adelante se puede instanciar partículas o sumar score
        Destroy(gameObject);
    }
}